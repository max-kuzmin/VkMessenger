using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ru.MaxKuzmin.VkMessenger.Extensions
{
    public static class MessagesCollectionExtensions
    {
        /// <summary>
        /// Update messages from API. Can be used during setup of page or with <see cref="LongPolling"/>
        /// </summary>
        /// <param name="messagesIds">Message id collection or null</param>
        public static async Task<bool> Update(this ObservableCollection<Message> collection, int dialogId,
            uint offset, IReadOnlyCollection<uint> messagesIds)
        {
            var newMessages = await MessagesClient.GetMessages(dialogId, offset, messagesIds);
            if (newMessages == null)
            {
                return false;
            }
            else if (newMessages.Any())
            {
                collection.AddUpdate(newMessages);
            }
            return true;
        }

        public static void AddUpdate(this ObservableCollection<Message> collection,
            IReadOnlyCollection<Message> newMessages)
        {
            lock (collection)
            {
                var toAppend = newMessages.AsEnumerable();
                var smallestToAppendId = toAppend.Last().Id;
                var biggestExistingId = collection.Last().Id;
                bool isOldMessages = biggestExistingId >= smallestToAppendId;

                foreach (var newMessage in toAppend)
                {
                    var foundMessage = collection.FirstOrDefault(m => m.Id == newMessage.Id);
                    if (foundMessage != null)
                    {
                        UpdateMessage(newMessage, foundMessage);
                    }
                    else
                    {
                        if (isOldMessages)
                            collection.Add(newMessage);
                        else
                            collection.Insert(0, newMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Update message data without recreating it
        /// </summary>
        private static void UpdateMessage(Message newMessage, Message foundMessage)
        {
            foundMessage.SetText(newMessage.Text);
        }
    }
}
