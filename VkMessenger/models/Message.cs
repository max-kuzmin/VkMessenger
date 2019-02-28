using System;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Message
    {
        public const int MaxLength = 200;

        public uint Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public Profile Profile { get; set; }
        public Group Group { get; set; }

        public int SenderId
        {
            get
            {
                if (Group != null)
                    return -(int)Group.Id;
                else if (Profile != null)
                    return (int)Profile.Id;
                else
                    return (int)Authorization.UserId;
            }
        }
        public UriImageSource Photo => Profile?.Photo ?? Group?.Photo ?? Authorization.Photo;
    }
}
