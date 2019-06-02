using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Message : INotifyPropertyChanged
    {
        public const int MaxLength = 200;

        public uint Id { get; private set; }
        public string Text { get; private set; }
        public bool Read { get; private set; }
        public DateTime Date { get; private set; }
        public Profile Profile { get; private set; }
        public Group Group { get; private set; }

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

        public void MarkRead(bool read)
        {
            if (Read != read)
            {
                Read = read;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Read)));
            }
        }

        public void SetId(uint id)
        {
            Id = id;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Id)));
        }

        public void SetDate(DateTime date)
        {
            Date = date;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Date)));
        }

        public void SetProfile(Profile profile)
        {
            Profile = profile;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Profile)));
        }

        public void SetText(string text)
        {
            Text = text;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Text)));
        }

        public void SetGroup(Group group)
        {
            Group = group;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Group)));
        }
    }
}
