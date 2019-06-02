using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Message : INotifyPropertyChanged
    {
        public const int MaxLength = 200;

        public uint Id { get; }
        public string Text { get; private set; }
        public bool Read { get; private set; }
        public DateTime Date { get; }
        public Profile Profile { get; }
        public Group Group { get; }

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

        public ImageSource Photo =>
            Profile?.Photo ??
            Group?.Photo ??
            Authorization.Photo;

        public event PropertyChangedEventHandler PropertyChanged;

        public Message(uint id, string text, DateTime date, Profile profile, Group group, bool read)
        {
            Id = id;
            Text = text;
            Date = date;
            Group = group;
            Profile = profile;
            Read = read;
        }

        public void SetRead(bool read)
        {
            Read = read;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Read)));
        }

        public void SetText(string text)
        {
            Text = text;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
        }

        public void Notify()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
