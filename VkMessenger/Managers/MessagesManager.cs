using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Models;

namespace ru.MaxKuzmin.VkMessenger.Managers
{
    public class MessagesManager
    {
        private readonly IReadOnlyCollection<Dialog> dialogsCollection;

        public IReadOnlyCollection<Message> GetMessages(int dialogId) =>
            dialogsCollection.FirstOrDefault(e => e.Id == dialogId)?.Messages ?? Array.Empty<Message>();

        public MessagesManager(IReadOnlyCollection<Dialog> dialogsCollection)
        {
            this.dialogsCollection = dialogsCollection;
        }

        public async Task UpdateMessagesFromApi(int dialogId, int? offset = null)
        {
            var dialog = dialogsCollection.FirstOrDefault(e => e.Id == dialogId);
            if (dialog == null)
                return;

            var collection = dialog.Messages;
            var newMessages = await MessagesClient.GetMessages(dialog.Id, offset);
            if (newMessages.Any())
            {
                AddUpdateMessagesInCollection(dialogId, newMessages, dialog.UnreadCount);
                await DurableCacheManager.SaveMessages(dialog.Id, collection).ConfigureAwait(false);
            }
        }

        public async Task UpdateMessagesFromApiByIds(int dialogId, IReadOnlyCollection<int> messagesIds)
        {
            var dialog = dialogsCollection.FirstOrDefault(e => e.Id == dialogId);
            if (dialog == null)
                return;

            var collection = dialog.Messages;
            var newMessages = await MessagesClient.GetMessagesByIds(messagesIds);
            if (newMessages.Any())
            {
                AddUpdateMessagesInCollection(dialogId, newMessages, dialog.UnreadCount);
                await DurableCacheManager.SaveMessages(dialog.Id, collection).ConfigureAwait(false);
            }
        }

        public void AddUpdateMessagesInCollection(int dialogId, IReadOnlyCollection<Message> newMessages, int unreadCount)
        {
            var dialog = dialogsCollection.FirstOrDefault(e => e.Id == dialogId);
            if (dialog == null)
                return;

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
                UpdateMessagesRead(dialogId, unreadCount);
            }
        }

        public void UpdateMessagesRead(int dialogId, int unreadCount)
        {
            var dialog = dialogsCollection.FirstOrDefault(e => e.Id == dialogId);
            if (dialog == null)
                return;

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

        public void TrimMessages(int dialogId)
        {
            var dialog = dialogsCollection.FirstOrDefault(e => e.Id == dialogId);
            if (dialog == null)
                return;

            var collection = dialog.Messages as Collection<Message>;
            if (collection == null)
                return;

            lock (collection)
            {
                collection.Trim(Consts.BatchSize);
            }
        }

        public async Task DeleteMessage(int dialogId, int messageId)
        {
            var dialog = dialogsCollection.FirstOrDefault(e => e.Id == dialogId);
            if (dialog == null)
                return;
            
            var collection = dialog.Messages as Collection<Message>;
            if (collection == null)
                return;

            await MessagesClient.DeleteMessage(messageId);

            lock (collection)
            {
                var message = collection.FirstOrDefault(e => e.Id == messageId);
                message?.SetDeleted(true);
            }

            await DurableCacheManager.SaveMessages(dialog.Id, collection).ConfigureAwait(false);
        }

        /// <summary>
        /// Update message data without recreating it
        /// </summary>
        private static void UpdateMessage(Message newMessage, Message foundMessage)
        {
            foundMessage.SetText(newMessage.Text);
            foundMessage.SetDeleted(newMessage.Deleted);
        }
    }
}
