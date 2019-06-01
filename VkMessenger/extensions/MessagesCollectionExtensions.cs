using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
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
        /// <returns>Null means update successfull</returns>
        public static async Task<Exception> Update(this ObservableCollection<Message> messages, int dialogId, IReadOnlyCollection<uint> messagesIds)
        {
            try
            {
                var newMessages = await MessagesClient.GetMessages(dialogId, messagesIds);

                foreach (var item in newMessages.AsEnumerable().Reverse())
                {
                    var foundMessage = messages.FirstOrDefault(d => d.Id == item.Id);

                    if (foundMessage == null)
                        messages.Add(item);
                    else
                    {
                        foundMessage.Text = item.Text;
                        foundMessage.ApplyChanges();
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return e;
            }
        }

        public static void UpdateUnreadProperty(this ObservableCollection<Message> messages, uint unreadCount)
        {
            if (unreadCount > 0)
            {
                var orderedMessages = messages.OrderByDescending(m => m.Date);
                var unreadMessages = orderedMessages.Take((int)unreadCount);
                foreach (var message in unreadMessages)
                {
                    if (!message.Unread)
                    {
                        message.Unread = true;
                        message.ApplyChanges();
                    }
                }

                var readMessages = orderedMessages.Skip((int)unreadCount);
                foreach (var message in unreadMessages)
                {
                    if (message.Unread)
                    {
                        message.Unread = false;
                        message.ApplyChanges();
                    }
                }
            }
        }
    }
}
