using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Collections;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Collections.Generic;
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
        public static async Task<bool> Update(
            this CustomObservableCollection<Message> collection,
            int dialogId,
            uint? offset = null,
            IReadOnlyCollection<uint> messagesIds = null)
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

        public static void AddUpdate(
            this CustomObservableCollection<Message> collection,
            IReadOnlyCollection<Message> newMessages)
        {
            lock (collection)
            {
                var smallestToAppendId = newMessages.Last().Id;
                var biggestExistingId = collection.First().Id;
                bool isOldMessages = biggestExistingId >= smallestToAppendId;

                var toAppend = newMessages.ToList();
                foreach (var newMessage in newMessages)
                {
                    var foundMessage = collection.FirstOrDefault(m => m.Id == newMessage.Id);
                    if (foundMessage != null)
                    {
                        UpdateMessage(newMessage, foundMessage);
                        toAppend.Remove(newMessage);
                    }
                }

                if (isOldMessages)
                {
                    foreach (var newMessage in toAppend)
                        newMessage.SetRead();
                    collection.InsertRange(0, toAppend);
                }
                else
                    collection.AddRange(toAppend);
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
