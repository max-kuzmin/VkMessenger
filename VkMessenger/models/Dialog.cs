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
        public ObservableCollection<Message> Messages { get; }
        public DialogType Type { get; }
        public uint UnreadCount { get; private set; }
        public string Text => Messages.Last()?.Text ?? string.Empty;
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

            Messages = new ObservableCollection<Message>(messages ?? Array.Empty<Message>());
            Messages.CollectionChanged += (s, e) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Messages)));

            Profiles = new ObservableCollection<Profile>(profiles ?? Array.Empty<Profile>());
            Profiles.CollectionChanged += (s, e) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Profiles)));
        }

        public void MarkReadWithMessages()
        {
            foreach (var message in Messages)
            {
                message.MarkRead(true);
            }

            SetUnreadCount(0);
        }

        public void SetUnreadCount(uint unreadCount)
        {
            if (UnreadCount != unreadCount)
            {
                UnreadCount = unreadCount;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnreadCount)));
            }
        }

        public void AddMessages(IReadOnlyCollection<Message> messages)
        {
            foreach (var message in messages)
            {
                if (Messages.All(m => m.Id != message.Id))
                {
                    Messages.Add(message);
                    Messages.OrderBy(m => m.Date);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                }
            }
        }

        public void SetOnline(uint userId, bool online)
        {
            var profile = Profiles?.FirstOrDefault(p => p.Id == userId);
            if (profile != null && profile.Online != online)
            {
                profile.Online = online;
                new PropertyChangedEventArgs(nameof(Online));
            }
        }
    }
}
