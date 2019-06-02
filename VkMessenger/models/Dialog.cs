using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Dialog : INotifyPropertyChanged
    {
        public ObservableCollection<Profile> Profiles { get; private set; }
        public Group Group { get; private set; }
        public Chat Chat { get; private set; }
        public ObservableCollection<Message> Messages { get; private set; }
        public DialogType Type { get; private set; }
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

        public Dialog()
        {
            Messages = new ObservableCollection<Message>();
            Messages.CollectionChanged += (s, e) =>
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Messages)));

            Profiles = new ObservableCollection<Profile>();
            Profiles.CollectionChanged += (s, e) =>
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Profiles)));
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
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(UnreadCount)));
            }
        }

        public void AddMessage(Message message)
        {
            if (Messages.All(m => m.Id != message.Id))
            {
                Messages.Add(message);
                Messages.OrderBy(m => m.Date);
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Text)));
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

        public void SetGroup(Group group)
        {
            Group = group;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Group)));
        }

        public void SetChat(Chat chat)
        {
            Chat = chat;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Chat)));
        }

        public void SetType(DialogType type)
        {
            Type = type;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Type)));
        }
    }
}
