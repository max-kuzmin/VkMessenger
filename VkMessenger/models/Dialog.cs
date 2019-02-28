using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Dialog
    {
        public List<Profile> Profiles { get; set; }
        public Group Group { get; set; }
        public Chat Chat { get; set; }

        public Message LastMessage { get; set; }
        public DialogType Type { get; set; }
        public uint UnreadCount { get; set; }
        public string Text => LastMessage.Text;

        public string Name
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
                        return Chat.Name;
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

        public UriImageSource Photo
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
    }
}
