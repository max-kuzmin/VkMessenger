using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
#if DEBUG
using ru.MaxKuzmin.VkMessenger.Loggers;
#endif

namespace ru.MaxKuzmin.VkMessenger.Extensions
{
    public static class DialogsCollectionExtensions
    {
        /// <summary>
        /// Update dialogs from API. Can be used during setup of page or with <see cref="LongPolling"/>
        /// </summary>
        public static async Task Update(
            this ObservableCollection<Dialog> collection)
        {
            var newDialogs = await DialogsClient.GetDialogs();
            if (newDialogs.Any())
            {
                collection.AddUpdate(newDialogs);
                _ = DurableCacheManager.SaveDialogs(collection);
            }
        }

        /// <summary>
        /// Update dialogs from API. Can be used during setup of page or with <see cref="LongPolling"/>
        /// </summary>
        public static async Task UpdateByIds(
            this ObservableCollection<Dialog> collection,
            IReadOnlyCollection<int> dialogIds)
        {
            var newDialogs = await DialogsClient.GetDialogsByIds(dialogIds);
            if (newDialogs.Any())
            {
                collection.AddUpdate(newDialogs);
                _ = DurableCacheManager.SaveDialogs(collection);
            }
        }

        public static async Task GetFromCache(this ObservableCollection<Dialog> collection)
        {
            var cached = await DurableCacheManager.GetDialogs();
            if (cached != null)
                collection.AddUpdate(cached);
        }

        private static void AddUpdate(
            this ObservableCollection<Dialog> collection,
            IReadOnlyCollection<Dialog> newDialogs)
        {
#if DEBUG
            Logger.Debug("Try to lock Dialogs " + collection.GetHashCode());
#endif
            lock (collection)
            {
#if DEBUG
                Logger.Debug("Locked Dialogs " + collection.GetHashCode());
#endif
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
#if DEBUG
            Logger.Debug("Unlocked Dialogs " + collection.GetHashCode());
#endif
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
            foundDialog.Messages.AddUpdate(newDialog.Messages, newDialog.UnreadCount);
            foundDialog.SetUnreadCount(newDialog.UnreadCount);
        }

        /// <summary>
        /// Update user status in every dialog
        /// </summary>
        public static void SetOnline(this ObservableCollection<Dialog> collection, ISet<(int UserId, bool Status)> updates)
        {
#if DEBUG
            Logger.Debug("Try to lock Dialogs " + collection.GetHashCode());
#endif
            lock (collection) //To prevent enumeration exception
            {
#if DEBUG
                Logger.Debug("Locked Dialogs " + collection.GetHashCode());
#endif
                foreach (var dialog in collection)
                {
                    foreach (var (userId, status) in updates)
                    {
                        dialog.SetOnline(userId, status);
                    }
                }
            }
#if DEBUG
            Logger.Debug("Unlocked Dialogs " + collection.GetHashCode());
#endif
        }
    }
}
