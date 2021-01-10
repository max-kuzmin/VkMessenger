using ru.MaxKuzmin.VkMessenger.Collections;
using System;
using System.Collections.Generic;
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
        public CustomObservableCollection<Profile> Profiles { get; set; }
        public Group? Group { get; set; }
        public Chat? Chat { get; set; }
        public CustomObservableCollection<Message> Messages { get; set; }
        public DialogType Type { get; set; }
        public int UnreadCount { get; set; }
        [JsonIgnore]
        public string Text => Messages.FirstOrDefault()?.PreviewText ?? string.Empty;
        [JsonIgnore]
        public bool Online => Type == DialogType.User && Profiles.First().Online;

        [JsonIgnore]
        public string Title
        {
            get
            {
                return Type switch
                {
                    DialogType.User => (Profiles.First().Name + " " + Profiles.First().Surname),
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
        public Dialog(DialogType type, Group? group, Chat? chat, int unreadCount,
            IReadOnlyCollection<Profile>? profiles, IReadOnlyCollection<Message>? messages)
        {
            Type = type;
            Group = group;
            Chat = chat;
            UnreadCount = unreadCount;

            Messages = new CustomObservableCollection<Message>(messages ?? Array.Empty<Message>());

            Messages.CollectionChanged += (s, e) =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            };

            Profiles = new CustomObservableCollection<Profile>(profiles ?? Array.Empty<Profile>());
        }

        public void SetReadWithMessages()
        {
            foreach (var message in Messages.ToArray()) //To prevent enumeration exception
            {
                message.SetRead(true);
            }

            SetUnreadCount(0);
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
            var profile = Profiles?.FirstOrDefault(p => p.Id == userId);
            if (profile != null && profile.Online != online)
            {
                profile.Online = online;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Online)));
            }
        }
    }
}
