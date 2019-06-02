using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
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
        /// <returns>Null means update successfull</returns>
        public static async Task<Exception> Update(this ObservableCollection<Dialog> collection, IReadOnlyCollection<int> dialogIds)
        {
            try
            {
                var newDialogs = await DialogsClient.GetDialogs(dialogIds);
                collection.AddUpdate(newDialogs);
                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return e;
            }
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
                        if (collection.IndexOf(foundDialog) != collection.Count - 1)
                        {
                            collection.Remove(foundDialog);
                            collection.Insert(0, foundDialog);
                        }
                        else
                        {
                            UpdateDialog(newDialog, foundDialog);
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
