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

        public int Id { get; set; }
        public Message LastMessage { get; set; }
        public DialogType Type { get; set; }
        public int UnreadCount { get; set; }
        public Color TextColor => UnreadCount > 0 ? Color.Yellow : Color.White;
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

        public int PeerId
        {
            get
            {
                switch (Type)
                {
                    case DialogType.User:
                    case DialogType.Chat:
                        return Id;
                    case DialogType.Group:
                        return -Id;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public ImageSource Photo
        {
            get
            {
                if (Chat.Photo != null)
                    return Chat.Photo;
                else if (Group != null)
                    return Group.Photo;
                else if (Profiles.Any())
                    return Profiles.First().Photo;
                else
                    return string.Empty;
            }
        }
    }
}
