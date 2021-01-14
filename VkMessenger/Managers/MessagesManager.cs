using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Models;

namespace ru.MaxKuzmin.VkMessenger.Managers
{
    public static class MessagesManager
    {
        public static async Task UpdateMessagesFromApi(Dialog dialog, int? offset = null)
        {
            var collection = dialog.Messages;
            var newMessages = await MessagesClient.GetMessages(dialog.Id, offset);
            if (newMessages.Any())
            {
                AddUpdateMessagesInCollection(dialog, newMessages, dialog.UnreadCount);
                _ = DurableCacheManager.SaveMessages(dialog.Id, collection).ConfigureAwait(false);
            }
        }

        public static async Task UpdateMessagesFromApiByIds(Dialog dialog, IReadOnlyCollection<int> messagesIds)
        {
            var collection = dialog.Messages;
            var newMessages = await MessagesClient.GetMessagesByIds(messagesIds);
            if (newMessages.Any())
            {
                AddUpdateMessagesInCollection(dialog, newMessages, dialog.UnreadCount);
                _ = DurableCacheManager.SaveMessages(dialog.Id, collection).ConfigureAwait(false);
            }
        }

        public static void AddUpdateMessagesInCollection(Dialog dialog, IReadOnlyCollection<Message> newMessages, int unreadCount)
        {
            var collection = dialog.Messages as Collection<Message>;
            if (collection == null)
                return;

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
                                break;
                            }
                        }
                }

                collection.AddRange(oldMessagesToAppend);
                collection.PrependRange(newMessagesToPrepend);
                UpdateMessagesRead(dialog, unreadCount);
            }
        }

        public static void UpdateMessagesRead(Dialog dialog, int unreadCount)
        {
            var collection = dialog.Messages;
            lock (collection) //To prevent enumeration exception
            {
                var leastUnread = unreadCount;
                foreach (var message in collection)
                {
                    // If it's current user message or there are must be more unread messages in dialog
                    if (message.SenderId == AuthorizationManager.UserId || leastUnread == 0)
                        message.SetRead(true);
                    // If message hasn't Read property set to true
                    else if (message.Read != true)
                    {
                        leastUnread--;
                        message.SetRead(false);
                    }
                }
            }
        }

        public static void TrimMessages(Dialog dialog)
        {
            var collection = dialog.Messages as Collection<Message>;
            if (collection == null)
                return;

            lock (collection)
            {
                collection.Trim(Consts.BatchSize);
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
