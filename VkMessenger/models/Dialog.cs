using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Dialog : INotifyPropertyChanged
    {
        public List<Profile> Profiles { get; set; }
        public Group Group { get; set; }
        public Chat Chat { get; set; }

        public Message LastMessage { get; set; }
        public DialogType Type { get; set; }
        public uint UnreadCount { get; set; }
        public string Text => LastMessage.Text;
        public bool IsOnline => Type == DialogType.User ? Profiles.First().IsOnline : false;

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
        public void InvokePropertyChanged() => PropertyChanged(this, new PropertyChangedEventArgs(null));
    }
}
