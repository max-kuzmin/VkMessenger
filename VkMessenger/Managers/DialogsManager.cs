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
                AddUpdateDialogsInCollection(cached);
        }

        public async Task UpdateDialogsFromApi()
        {
            var newDialogs = await DialogsClient.GetDialogs();
            if (newDialogs.Any())
            {
                AddUpdateDialogsInCollection(newDialogs);

                Dialog[] copy;
                lock (collection)
                {
                    copy = collection.ToArray();
                }
                await DurableCacheManager.SaveDialogs(copy);
            }
        }

        /// <summary>
        /// Returns updated messages ids
        /// </summary>
        public async Task<int[]> UpdateDialogsFromApiByIds(IReadOnlyCollection<int> dialogIds)
        {
            var newDialogs = await DialogsClient.GetDialogsByIds(dialogIds);
            if (newDialogs.Any())
            {
                AddUpdateDialogsInCollection(newDialogs);

                Dialog[] copy;
                lock (collection)
                {
                    copy = collection.ToArray();
                }
                await DurableCacheManager.SaveDialogs(copy);
            }

            return newDialogs.SelectMany(e => e.Messages.Select(m => m.Id)).Distinct().ToArray();
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

        public void TrimDialogs()
        {
            lock (collection)
            {
                collection.Trim(Consts.BatchSize);
            }
        }

        public void SetDialogAndMessagesRead(int dialogId)
        {
            Dialog? dialog;
            lock (collection)
            {
                dialog = collection.FirstOrDefault(e => e.Id == dialogId);
            }

            if (dialog == null)
                return;

            messagesManager.UpdateMessagesRead(dialogId, 0);
            dialog.SetUnreadCount(0);
        }

        public async Task SetDialogAndMessagesReadAndPublish(int dialogId)
        {
            Dialog? dialog;
            lock (collection)
            {
                dialog = collection.FirstOrDefault(e => e.Id == dialogId);
            }

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
            lock (collection)
            {
                return collection.FirstOrDefault(e => e.Id == dialogId)?.CanWrite == true;
            }
        }

        public Dialog? First()
        {
            lock (collection)
            {
                return collection.FirstOrDefault();
            }
        }

        private void AddUpdateDialogsInCollection(IReadOnlyCollection<Dialog> newDialogs)
        {
            lock (collection)
            {
                var dialogsToInsert = new List<Dialog>();

                foreach (var newDialog in newDialogs)
                {
                    var foundDialog = collection.FirstOrDefault(m => m.Id == newDialog.Id);
                    if (foundDialog != null)
                    {
                        // It's important to get first message before UpdateDialog
                        var oldLastMessage = foundDialog.Messages.FirstOrDefault();
                        var newLastMessage = newDialog.Messages.FirstOrDefault();

                        UpdateDialog(newDialog, foundDialog);

                        // Move dialog to top, because it was updated
                        if (collection.IndexOf(foundDialog) != collection.Count - 1
                            && oldLastMessage != null
                            && newLastMessage != null
                            && oldLastMessage.Id != newLastMessage.Id)
                        {
                            collection.Remove(foundDialog);

                            for (int newIndex = 0; newIndex < collection.Count; newIndex++)
                            {
                                var dialogLastMessage = collection[newIndex].Messages.FirstOrDefault();
                                if (dialogLastMessage != null && dialogLastMessage.Date < newLastMessage.Date)
                                {
                                    collection.Insert(newIndex, foundDialog);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        dialogsToInsert.Add(newDialog);
                    }
                }

                collection.PrependRange(dialogsToInsert);
            }
        }

        /// <summary>
        /// Update dialog data without recreating it
        /// </summary>
        private void UpdateDialog(Dialog newDialog, Dialog foundDialog)
        {
            foreach (var newProfile in newDialog.Profiles)
            {
                foundDialog.SetOnline(newProfile.Id, newDialog.Online);
            }
            messagesManager.AddUpdateMessagesInCollection(foundDialog.Id, newDialog.Messages, newDialog.UnreadCount, false);
            foundDialog.SetUnreadCount(newDialog.UnreadCount);
        }
    }
}
