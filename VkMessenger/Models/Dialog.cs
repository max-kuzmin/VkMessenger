using ru.MaxKuzmin.VkMessenger.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Dialog : INotifyPropertyChanged
    {
        public CustomObservableCollection<Profile> Profiles { get; }
        public Group? Group { get; }
        public Chat? Chat { get; }
        public CustomObservableCollection<Message> Messages { get; }
        public DialogType Type { get; }
        public uint UnreadCount { get; private set; }
        public string Text => Messages.FirstOrDefault()?.Text?.Replace('\n', ' ') ?? string.Empty;
        public bool Online => Type == DialogType.User && Profiles.First().Online;

        public string Title
        {
            get
            {
                return Type switch
                {
                    DialogType.User => (Profiles.First().Name + " " + Profiles.First().Surname),
                    DialogType.Group => Group!.Name,
                    DialogType.Chat => Chat!.Title,
                    _ => string.Empty
                };
            }
        }

        /// <summary>
        /// Dialogs with users and chats have positive id, dialogs with groups - negative id
        /// </summary>
        public int Id
        {
            get
            {
                return Type switch
                {
                    DialogType.User => (int)Profiles.First().Id,
                    DialogType.Chat => (int)Chat!.Id,
                    DialogType.Group => -(int)Group!.Id,
                    _ => 0
                };
            }
        }

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

        public Dialog(DialogType type, Group? group, Chat? chat, uint unreadCount,
            IReadOnlyCollection<Profile>? profiles, IReadOnlyCollection<Message> messages)
        {
            Type = type;
            Group = group;
            Chat = chat;
            UnreadCount = unreadCount;

            Messages = messages == null
                ? new CustomObservableCollection<Message>()
                : new CustomObservableCollection<Message>(messages);

            Messages.CollectionChanged += (s, e) =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            };

            Profiles = new CustomObservableCollection<Profile>(profiles ?? Array.Empty<Profile>());
        }

        public void SetReadWithMessages()
        {
            foreach (var message in Messages)
            {
                message.SetRead();
            }

            SetUnreadCount(0);
        }

        public void SetUnreadCount(uint unreadCount)
        {
            UnreadCount = unreadCount;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnreadCount)));
        }

        public void SetOnline(uint userId, bool online)
        {
            var profile = Profiles?.FirstOrDefault(p => p.Id == userId);
            if (profile != null)
            {
                profile.Online = online;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Online)));
            }
        }
    }
}
