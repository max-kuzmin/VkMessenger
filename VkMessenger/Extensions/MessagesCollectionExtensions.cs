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
        public static async Task Update(
            this CustomObservableCollection<Message> collection,
            int dialogId,
            int? offset = null)
        {
            var newMessages = await MessagesClient.GetMessages(dialogId, offset);
            if (newMessages.Any())
            {
                collection.AddUpdate(newMessages);
                _ = DurableCacheManager.SaveMessages(dialogId, collection);
            }
        }

        /// <summary>
        /// Update messages from API. Can be used during setup of page or with <see cref="LongPolling"/>
        /// </summary>
        public static async Task UpdateByIds(
            this CustomObservableCollection<Message> collection,
            IReadOnlyCollection<int> messagesIds,
            int dialogId)
        {
            var newMessages = await MessagesClient.GetMessagesByIds(messagesIds);
            if (newMessages.Any())
            {
                collection.AddUpdate(newMessages);
                _ = DurableCacheManager.SaveMessages(dialogId, collection);
            }
        }

        public static void AddUpdate(
            this CustomObservableCollection<Message> collection,
            IReadOnlyCollection<Message> newMessages)
        {
            lock (collection)
            {
                var newestToInsertId = newMessages.Last().Id;
                var oldestExistingId = collection.First().Id;
                bool isOldMessages = oldestExistingId >= newestToInsertId;

                var messagesToInsert = new List<Message>();

                foreach (var newMessage in newMessages)
                {
                    var foundMessage = collection.FirstOrDefault(m => m.Id == newMessage.Id);
                    if (foundMessage != null)
                    {
                        UpdateMessage(newMessage, foundMessage);
                    }
                    else
                    {
                        messagesToInsert.Add(newMessage);
                    }
                }

                if (isOldMessages)
                {
                    foreach (var newMessage in messagesToInsert)
                    {
                        newMessage.SetRead();
                    }

                    collection.AddRange(messagesToInsert);
                }
                else
                {
                    collection.InsertRange(0, messagesToInsert);
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
