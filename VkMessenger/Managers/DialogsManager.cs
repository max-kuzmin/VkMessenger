using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Models;

namespace ru.MaxKuzmin.VkMessenger.Managers
{
    public class DialogsManager
    {
        private readonly ObservableCollection<Dialog> collection;
        private readonly MessagesManager messagesManager;

        // ReSharper disable once InconsistentlySynchronizedField
        public IReadOnlyCollection<Dialog> Collection => collection;

        public DialogsManager(ObservableCollection<Dialog> collection, MessagesManager messagesManager)
        {
            this.collection = collection;
            this.messagesManager = messagesManager;
        }

        public async Task UpdateDialogsFromCache()
        {
            var cached = await DurableCacheManager.GetDialogs();
            if (cached != null)
                AddUpdateDialogsInCollection(cached, false);
        }

        public async Task UpdateDialogsFromApi()
        {
            var newDialogs = await DialogsClient.GetDialogs();
            if (newDialogs.Any())
            {
                AddUpdateDialogsInCollection(newDialogs, true);
                // ReSharper disable once InconsistentlySynchronizedField
                await DurableCacheManager.SaveDialogs(collection);
            }
        }

        /// <summary>
        /// Returns updated messages ids
        /// </summary>
        public async Task UpdateDialogsFromApiByIds(IReadOnlyCollection<int> dialogIds)
        {
            var newDialogs = await DialogsClient.GetDialogsByIds(dialogIds);
            if (newDialogs.Any())
            {
                AddUpdateDialogsInCollection(newDialogs, false);
                // ReSharper disable once InconsistentlySynchronizedField
                await DurableCacheManager.SaveDialogs(collection);
            }
        }

        /// <summary>
        /// Update user status in every dialog
        /// </summary>
        public void SetDialogsOnline(IReadOnlyCollection<(int UserId, bool Status)> updates)
        {
            lock (collection) //To prevent enumeration exception
            {
                foreach (var dialog in collection)
                {
                    foreach (var (userId, status) in updates)
                    {
                        dialog.SetOnline(userId, status);
                    }
                }
            }
        }

        public void SetDialogAndMessagesRead(int dialogId)
        {
            var dialog = FirstOrDefaultWithLock(dialogId);
            if (dialog == null)
                return;

            messagesManager.UpdateMessagesRead(dialogId, 0);
            dialog.SetUnreadCount(0);
        }

        public bool GetIsInitRequired(int dialogId)
        {
            var dialog = FirstOrDefaultWithLock(dialogId);
            return dialog != null && dialog.IsInitRequired;
        }

        public void SetIsInitRequiredToFalse(int dialogId)
        {
            var dialog = FirstOrDefaultWithLock(dialogId);
            if (dialog != null)
                dialog.IsInitRequired = false;
        }

        public async Task SetDialogAndMessagesReadAndPublish(int dialogId)
        {
            var dialog = FirstOrDefaultWithLock(dialogId);
            if (dialog == null)
                return;

            if (dialog.UnreadCount != 0)
            {
                SetDialogAndMessagesRead(dialogId);

                await DialogsClient.MarkAsRead(dialog.Id);
                await DurableCacheManager.SaveDialog(dialog).ConfigureAwait(false);
            }
        }

        public void SetAllDialogsInitRequired()
        {
            lock (collection)
            {
                foreach (var dialog in collection)
                {
                    dialog.IsInitRequired = true;
                }
            }
        }

        public bool CanWrite(int dialogId)
        {
            return FirstOrDefaultWithLock(dialogId)?.CanWrite == true;
        }

        public Dialog? First()
        {
            lock (collection)
            {
                return collection.FirstOrDefault();
            }
        }

        private void AddUpdateDialogsInCollection(IReadOnlyCollection<Dialog> newDialogs, bool isNewestMessagesBatch)
        {
            lock (collection)
            {
                var newDialogsToAppend = new List<Dialog>();
                var oldDialogsToPrepend = new List<Dialog>();

                var newestExistingDialogTime = collection.FirstOrDefault()?.FirstMessage.Date;
                var oldestExistingDialogTime = collection.LastOrDefault()?.FirstMessage.Date;

                // If collection is empty, just add all dialogs to it
                if (newestExistingDialogTime == null || oldestExistingDialogTime == null)
                {
                    collection.PrependRange(newDialogs);
                    return;
                }

                foreach (var newDialog in newDialogs)
                {
                    var newDialogTime = newDialog.FirstMessage.Date;
                    var foundDialog = collection.FirstOrDefault(m => m.Id == newDialog.Id);
                    Dialog newOrFoundDialog = newDialog;

                    // Update found dialog and remove it from collection
                    if (foundDialog != null)
                    {
                        UpdateDialog(newDialog, foundDialog);
                        collection.Remove(foundDialog);
                        newOrFoundDialog = foundDialog;
                    }

                    // Append new or found dialog
                    if (newestExistingDialogTime < newDialogTime)
                        newDialogsToAppend.Add(newOrFoundDialog);
                    // Prepend old or found dialog
                    else if (oldestExistingDialogTime > newDialogTime)
                        oldDialogsToPrepend.Add(newOrFoundDialog);
                    // Find dialog place in collection
                    else
                        for (int newIndex = 0; newIndex < collection.Count; newIndex++)
                        {
                            var dialogTime = collection[newIndex].FirstMessage.Date;
                            if (dialogTime < newDialogTime)
                            {
                                collection.Insert(newIndex, newOrFoundDialog);
                                break;
                            }
                        }
                }

                collection.AddRange(oldDialogsToPrepend);
                collection.PrependRange(newDialogsToAppend);

                //Trim to batch size to prevent skipping new dialogs between cached and 20 loaded on init
                if (isNewestMessagesBatch)
                    collection.Trim(Consts.BatchSize);
            }
        }

        public void ReorderDialogs(int[] dialogIds)
        {
            var dialogs = new List<Dialog>();
            lock (collection)
            {
                foreach (var dialogId in dialogIds)
                {
                    var dialog = collection.FirstOrDefault(e => e.Id == dialogId);
                    if (dialog != null)
                        dialogs.Add(dialog);
                }
            }

            if (!dialogs.Any())
                return;

            AddUpdateDialogsInCollection(dialogs, false);
        }

        /// <summary>
        /// Update dialog data without recreating it
        /// </summary>
        private void UpdateDialog(Dialog newDialog, Dialog foundDialog)
        {
            messagesManager.AddUpdateMessagesInCollection(foundDialog.Id, newDialog.Messages, newDialog.UnreadCount, false);
            foundDialog.SetUnreadCount(newDialog.UnreadCount);
            foundDialog.SetProfiles(newDialog.Profiles);
            foundDialog.SetChat(newDialog.Chat);
            foundDialog.SetGroup(newDialog.Group);
        }

        private Dialog? FirstOrDefaultWithLock(int dialogId)
        {
            lock (collection)
            {
                return collection.FirstOrDefault(e => e.Id == dialogId);
            }
        }
    }
}
