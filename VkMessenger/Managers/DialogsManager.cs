using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Models;

namespace ru.MaxKuzmin.VkMessenger.Managers
{
    public static class DialogsManager
    {
        private static readonly ObservableCollection<Dialog> collection = new ObservableCollection<Dialog>();

        public static IReadOnlyCollection<Dialog> Collection => collection;

        public static async Task UpdateDialogsFromCache()
        {
            var cached = await DurableCacheManager.GetDialogs();
            if (cached != null)
                AddUpdateDialogsInCollection(cached);
        }

        public static async Task UpdateDialogsFromApi()
        {
            var newDialogs = await DialogsClient.GetDialogs();
            if (newDialogs.Any())
            {
                AddUpdateDialogsInCollection(newDialogs);
                _ = DurableCacheManager.SaveDialogs(collection);
            }
        }

        public static async Task UpdateDialogsFromApiByIds(IReadOnlyCollection<int> dialogIds)
        {
            var newDialogs = await DialogsClient.GetDialogsByIds(dialogIds);
            if (newDialogs.Any())
            {
                AddUpdateDialogsInCollection(newDialogs);
                _ = DurableCacheManager.SaveDialogs(collection);
            }
        }

        /// <summary>
        /// Update user status in every dialog
        /// </summary>
        public static void SetDialogsOnline(IReadOnlyCollection<(int UserId, bool Status)> updates)
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

        public static void TrimDialogs()
        {
            lock (collection)
            {
                collection.Trim(Consts.BatchSize);
            }
        }

        public static void SetDialogAndMessagesRead(Dialog dialog)
        {
            MessagesManager.UpdateMessagesRead(dialog, 0);
            dialog.SetUnreadCount(0);
        }

        public static async Task SetDialogAndMessagesReadAndPublish(Dialog dialog)
        {
            if (dialog.UnreadCount != 0)
            {
                SetDialogAndMessagesRead(dialog);

                await DialogsClient.MarkAsRead(dialog.Id);
                await DurableCacheManager.SaveDialog(dialog).ConfigureAwait(false);
            }
        }

        private static void AddUpdateDialogsInCollection(IReadOnlyCollection<Dialog> newDialogs)
        {
            lock (collection)
            {
                var dialogsToInsert = new List<Dialog>();

                foreach (var newDialog in newDialogs)
                {
                    var foundDialog = collection.FirstOrDefault(m => m.Id == newDialog.Id);
                    if (foundDialog != null)
                    {
                        var oldLastMessage = foundDialog.Messages.First();
                        var newLastMessage = newDialog.Messages.First();

                        UpdateDialog(newDialog, foundDialog);

                        // Move dialog to top, because it was updated
                        if (collection.IndexOf(foundDialog) != collection.Count - 1
                            && oldLastMessage.Id != newLastMessage.Id)
                        {
                            collection.Remove(foundDialog);
                            dialogsToInsert.Add(foundDialog);
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
        private static void UpdateDialog(Dialog newDialog, Dialog foundDialog)
        {
            foreach (var newProfile in newDialog.Profiles)
            {
                foundDialog.SetOnline(newProfile.Id, newDialog.Online);
            }
            MessagesManager.AddUpdateMessagesInCollection(foundDialog, newDialog.Messages, newDialog.UnreadCount);
            foundDialog.SetUnreadCount(newDialog.UnreadCount);
        }
    }
}
