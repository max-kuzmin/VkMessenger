using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Dialog : INotifyPropertyChanged
    {
        public ObservableCollection<Profile> Profiles { get; }
        public Group Group { get; }
        public Chat Chat { get; }
        public CustomObservableCollection<Message> Messages { get; }
        public DialogType Type { get; }
        public uint UnreadCount { get; private set; }
        public string Text => Messages.LastOrDefault()?.Text?.Replace('\n', ' ') ?? string.Empty;
        public bool Online => Type == DialogType.User ? Profiles.First().Online : false;

        public string Title
        {
            get
            {
                switch (Type)
                {
                    case DialogType.User:
                        return Profiles.First().Name + " " + Profiles.First().Surname;
                    case DialogType.Group:
                        return Group.Name;
                    case DialogType.Chat:
                        return Chat.Title;
                    default:
                        return string.Empty;
                }
            }
        }

        public int Id
        {
            get
            {
                switch (Type)
                {
                    case DialogType.User:
                        return (int)Profiles.First().Id;
                    case DialogType.Chat:
                        return (int)Chat.Id;
                    case DialogType.Group:
                        return -(int)Group.Id;
                    default:
                        return 0;
                }
            }
        }

        public ImageSource Photo
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

        public event PropertyChangedEventHandler PropertyChanged;

        public Dialog(DialogType type, Group group, Chat chat, uint unreadCount,
            IReadOnlyCollection<Profile> profiles, IReadOnlyCollection<Message> messages)
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

            Profiles = new ObservableCollection<Profile>(profiles ?? Array.Empty<Profile>());
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
