using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ru.MaxKuzmin.VkMessenger.Extensions
{
    public static class DialogsCollectionExtensions
    {
        /// <summary>
        /// Update dialogs from API. Can be used during setup of page or with <see cref="LongPolling"/>
        /// </summary>
        /// <param name="dialogIds">Dialog id collection or null</param>
        public static async Task<bool> Update(this ObservableCollection<Dialog> collection, IReadOnlyCollection<int> dialogIds)
        {
            var newDialogs = await DialogsClient.GetDialogs(dialogIds);
            if (newDialogs == null)
            {
                return false;
            }
            else if (newDialogs.Any())
            {
                collection.AddUpdate(newDialogs);
            }
            return true;
        }

        public static void AddUpdate(this ObservableCollection<Dialog> collection,
            IReadOnlyCollection<Dialog> newDialogs)
        {
            lock (collection)
            {
                foreach (var newDialog in newDialogs.AsEnumerable().Reverse())
                {
                    var foundDialog = collection.FirstOrDefault(m => m.Id == newDialog.Id);
                    if (foundDialog != null)
                    {
                        var oldLastMessage = foundDialog.Messages.Last();

                        UpdateDialog(newDialog, foundDialog);

                        // Move dialog to top, because it was updated
                        if (collection.IndexOf(foundDialog) != collection.Count - 1
                            && oldLastMessage != foundDialog.Messages.Last())
                        {
                            collection.Remove(foundDialog);
                            collection.Insert(0, foundDialog);
                        }
                    }
                    else
                    {
                        collection.Insert(0, newDialog);
                    }
                }
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
            foundDialog.Messages.AddUpdate(newDialog.Messages);
            foundDialog.SetUnreadCount(newDialog.UnreadCount);
        }
    }
}
