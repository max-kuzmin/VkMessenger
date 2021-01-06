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
                var newestExistingId = collection.First().ConversationMessageId;
                var oldestExistingId = collection.Last().ConversationMessageId;

                var oldMessagesToAppend = new List<Message>();
                var newMessagesToPrepend = new List<Message>();

                foreach (var newMessage in newMessages)
                {
                    var foundMessage = collection.FirstOrDefault(m => m.Id == newMessage.Id);
                    if (foundMessage != null)
                        UpdateMessage(newMessage, foundMessage);
                    else if (newestExistingId < newMessage.ConversationMessageId)
                        newMessagesToPrepend.Add(newMessage);
                    else if (oldestExistingId > newMessage.ConversationMessageId)
                        oldMessagesToAppend.Add(newMessage);
                    else
                        for (int i = 0; i < collection.Count; i++)
                        {
                            if (collection[i].ConversationMessageId < newMessage.ConversationMessageId)
                            {
                                collection.Insert(i, newMessage);
                                newMessage.SetRead();
                                break;
                            }
                        }
                }

                if (oldMessagesToAppend.Any())
                {
                    foreach (var newMessage in oldMessagesToAppend)
                    {
                        newMessage.SetRead();
                    }

                    collection.AddRange(oldMessagesToAppend);
                }
                
                if (newMessagesToPrepend.Any())
                    collection.InsertRange(0, newMessagesToPrepend);
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
