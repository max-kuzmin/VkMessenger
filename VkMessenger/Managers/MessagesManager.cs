using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Models;

namespace ru.MaxKuzmin.VkMessenger.Managers
{
    public class MessagesManager
    {
        private readonly IReadOnlyCollection<Dialog> dialogsCollection;

        public IReadOnlyCollection<Message>? GetMessages(int dialogId) =>
            FirstOrDefaultWithLock(dialogId)?.Messages;

        public MessagesManager(IReadOnlyCollection<Dialog> dialogsCollection)
        {
            this.dialogsCollection = dialogsCollection;
        }

        public async Task UpdateMessagesFromApi(int dialogId, int? offset = null)
        {
            var dialog = FirstOrDefaultWithLock(dialogId);
            if (dialog == null)
                return;

            var collection = dialog.Messages;
            var newMessages = await MessagesClient.GetMessages(dialog.Id, offset);
            if (newMessages.Any())
            {
                var isNewestMessagesBatch = offset == null;
                AddUpdateMessagesInCollection(dialogId, newMessages, dialog.UnreadCount, isNewestMessagesBatch);
                await DurableCacheManager.SaveMessages(dialog.Id, collection).ConfigureAwait(false);
            }
        }

        public async Task UpdateMessagesFromApiByIds(int dialogId, IReadOnlyCollection<int> messagesIds)
        {
            var dialog = FirstOrDefaultWithLock(dialogId);
            if (dialog == null)
                return;

            var collection = dialog.Messages;
            var newMessages = await MessagesClient.GetMessagesByIds(messagesIds);
            if (newMessages.Any())
            {
                AddUpdateMessagesInCollection(dialogId, newMessages, dialog.UnreadCount, false);
                await DurableCacheManager.SaveMessages(dialog.Id, collection).ConfigureAwait(false);
            }
        }

        public void AddUpdateMessagesInCollection(int dialogId, IReadOnlyCollection<Message> newMessages, int? unreadCount, bool isNewestMessagesBatch)
        {
            var dialog = FirstOrDefaultWithLock(dialogId);
            if (dialog == null)
                return;

            var collection = dialog.Messages;
            lock (collection)
            {
                var newestExistingId = collection.First().Id;
                var oldestExistingId = collection.Last().Id;

                var oldMessagesToAppend = new List<Message>();
                var newMessagesToPrepend = new List<Message>();

                foreach (var newMessage in newMessages)
                {
                    // Delete found message
                    var foundMessage = collection.FirstOrDefault(m => m.Id == newMessage.Id);
                    if (newMessage.Deleted)
                    {
                        if (foundMessage != null)
                            collection.Remove(foundMessage);
                    }
                    // Update found message
                    else if (foundMessage != null)
                        UpdateMessage(newMessage, foundMessage);
                    // Prepend new message
                    else if (newestExistingId < newMessage.Id)
                        newMessagesToPrepend.Add(newMessage);
                    // Append old message
                    else if (oldestExistingId > newMessage.Id)
                        oldMessagesToAppend.Add(newMessage);
                    // Find message place in collection
                    else
                        for (int newIndex = 0; newIndex < collection.Count; newIndex++)
                        {
                            if (collection[newIndex].Id < newMessage.Id)
                            {
                                collection.Insert(newIndex, newMessage);
                                break;
                            }
                        }
                }

                // Delete messages, that was not returned by update of last messages in dialog
                if (isNewestMessagesBatch)
                {
                    var oldestNewId = newMessages.Last().Id;
                    for (int id = oldestNewId + 1; id <= newestExistingId; id++)
                    {
                        var newMessage = newMessages.FirstOrDefault(e => e.Id == id);
                        if (newMessage != null)
                            continue;

                        var oldMessage = collection.FirstOrDefault(e => e.Id == id);
                        if (oldMessage != null)
                            collection.Remove(oldMessage);
                    }
                }

                collection.AddRange(oldMessagesToAppend);
                collection.PrependRange(newMessagesToPrepend);
                if (unreadCount.HasValue)
                    UpdateMessagesRead(dialogId, unreadCount.Value);

                //Trim to batch size to prevent skipping new messages between cached and 20 loaded on init
                if (isNewestMessagesBatch)
                    collection.Trim(Consts.BatchSize);
            }
        }

        public void UpdateMessagesRead(int dialogId, int unreadCount)
        {
            var dialog = FirstOrDefaultWithLock(dialogId);
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

        public async Task DeleteMessage(int dialogId, int messageId)
        {
            await MessagesClient.DeleteMessage(messageId);
            var dialog = FirstOrDefaultWithLock(dialogId);
            if (dialog == null)
                return;

            var collection = dialog.Messages;
            lock (collection)
            {
                var message = collection.FirstOrDefault(e => e.Id == messageId);
                if (message != null)
                    collection.Remove(message);
            }
        }

        public void DeleteMessagesFromCollectionOnly(int dialogId, int[] messageIds)
        {
            var dialog = FirstOrDefaultWithLock(dialogId);
            if (dialog == null)
                return;

            var collection = dialog.Messages;
            lock (collection)
            {
                foreach (var messageId in messageIds)
                {
                    var message = collection.FirstOrDefault(e => e.Id == messageId);
                    if (message != null)
                        collection.Remove(message);
                }
            }
        }

        public async Task SendMessage(int dialogId, string? text, string? voiceMessagePath)
        {
            var messageId = await MessagesClient.Send(dialogId, text, voiceMessagePath);

            var myProfile = new Profile
            {
                Id = AuthorizationManager.UserId,
                Name = string.Empty,
                Photo = AuthorizationManager.Photo,
                Online = true,
                Surname = string.Empty
            };
            var fullText = text?.Trim() ?? $"{Consts.PaperClip} {LocalizedStrings.VoiceMessage}";
            var newMessage = new Message(
                messageId,
                //Will be loaded on next update
                fullText,
                fullText,
                null,
                DateTime.Now, 
                DateTime.Now,
                false,
                myProfile,
                null,
                // All next fields can't be set by our app
                null,
                null,
                null);
            newMessage.SetRead(true);

            AddUpdateMessagesInCollection(dialogId, new []{ newMessage }, null, false);
        }

        public int GetMessagesCount(int dialogId)
        {
            var dialog = FirstOrDefaultWithLock(dialogId);
            if (dialog == null)
                return 0;

            var collection = dialog.Messages;
            lock (collection)
            {
                return collection.Count;
            }
        }

        public bool IsMessageOlderThanAll(int dialogId, int messageId)
        {
            var dialog = FirstOrDefaultWithLock(dialogId);
            if (dialog == null)
                return false;

            var collection = dialog.Messages;
            lock (collection)
            {
                return collection.All(i => i.Id >= messageId);
            }
        }

        /// <summary>
        /// Update message data without recreating it
        /// </summary>
        private static void UpdateMessage(Message newMessage, Message foundMessage)
        {
            if (foundMessage.UpdateTime != newMessage.UpdateTime)
            {
                foundMessage.SetFullText(newMessage.FullText);
                foundMessage.SetText(newMessage.Text);
                foundMessage.SetVoiceMessage(newMessage.VoiceMessage);
                foundMessage.SetDate(newMessage.Date);
                foundMessage.SetUpdateTime(newMessage.UpdateTime);
                foundMessage.SetProfile(newMessage.Profile);
                foundMessage.SetGroup(newMessage.Group);
            }
        }

        private Dialog? FirstOrDefaultWithLock(int dialogId)
        {
            lock (dialogsCollection)
            {
                return dialogsCollection.FirstOrDefault(e => e.Id == dialogId);
            }
        }
    }
}
