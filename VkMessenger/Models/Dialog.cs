using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    /// <summary>
    /// Older dialogs with smaller date are at the end of list
    /// </summary>
    public class Dialog : INotifyPropertyChanged
    {
        // Can be empty for chats (channels)
        public IReadOnlyCollection<Profile> Profiles { get; set; }
        public Group? Group { get; set; }
        public Chat? Chat { get; set; }
        public IReadOnlyCollection<Message> Messages { get; set; }
        public DialogType Type { get; set; }
        public int UnreadCount { get; set; }
        public bool CanWrite { get; set; }
        /// <summary>
        /// Indicates that dialog will be fully updated when messages page open next time
        /// </summary>
        [JsonIgnore]
        public bool IsInitRequired = true;
        [JsonIgnore]
        public string Text => Messages.FirstOrDefault()?.PreviewText ?? string.Empty;
        [JsonIgnore]
        public bool Online => Type == DialogType.User && Profiles.FirstOrDefault()?.Online == true;

        /// <summary>
        /// Dialog always has at least one message
        /// </summary>
        [JsonIgnore]
        public Message FirstMessage
        {
            get
            {
                lock (Messages)
                {
                    return Messages.First();
                }
            }
        }

        [JsonIgnore]
        public string Title
        {
            get
            {
                return Type switch
                {
                    DialogType.User => Profiles.First().Name + " " + Profiles.First().Surname,
                    DialogType.Group when Group != null => Group.Name,
                    DialogType.Chat when Chat != null => Chat.Title,
                    _ => string.Empty
                };
            }
        }

        /// <summary>
        /// Dialogs with users and chats have positive id, dialogs with groups - negative id
        /// </summary>
        [JsonIgnore]
        public int Id
        {
            get
            {
                return Type switch
                {
                    DialogType.User => Profiles.First().Id,
                    DialogType.Chat when Chat != null => Chat.Id,
                    DialogType.Group when Group != null => -Group.Id,
                    _ => 0
                };
            }
        }

        [JsonIgnore]
        public ImageSource? Photo
        {
            get
            {
                if (Chat?.Photo != null)
                    return Chat.Photo;
                else if (Group != null)
                    return Group.Photo;
                else if (Profiles.Any())
                    return Profiles.First().Photo;
                else
                    return null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [JsonConstructor]
        public Dialog(DialogType type, Group? group, Chat? chat, int unreadCount, bool canWrite,
            IReadOnlyCollection<Profile>? profiles, IReadOnlyCollection<Message>? messages)
        {
            Type = type;
            Group = group;
            Chat = chat;
            UnreadCount = unreadCount;
            CanWrite = canWrite;

            var messagesCollection = new ObservableCollection<Message>(messages ?? Array.Empty<Message>());
            messagesCollection.CollectionChanged += (s, e) =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            };
            Messages = messagesCollection;

            Profiles = new ObservableCollection<Profile>(profiles ?? Array.Empty<Profile>());
        }

        public void SetUnreadCount(int unreadCount)
        {
            if (UnreadCount != unreadCount)
            {
                UnreadCount = unreadCount;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnreadCount)));
            }
        }

        public void SetOnline(int userId, bool online)
        {
            var profile = Profiles.FirstOrDefault(p => p.Id == userId);
            if (profile != null && profile.Online != online)
            {
                profile.Online = online;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Online)));
            }
        }
    }
}
