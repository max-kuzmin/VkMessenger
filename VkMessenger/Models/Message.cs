using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Message : INotifyPropertyChanged
    {
        public const int MaxLength = 150;

        public uint Id { get; }
        public string Text { get; private set; }
        public bool Read { get; private set; }
        public DateTime Date { get; }
        public Profile Profile { get; }
        public Group Group { get; }
        public string FullText { get; }
        public IReadOnlyCollection<ImageSource> AttachmentImages { get; }
        public Uri AttachmentUri { get; }

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

        public Message(uint id, string text, string fullText, DateTime date, Profile profile, Group group,
            IReadOnlyCollection<ImageSource> attachmentImages, Uri attachmentUri)
        {
            Id = id;
            Text = text;
            FullText = fullText;
            Date = date;
            Group = group;
            Profile = profile;
            AttachmentImages = attachmentImages;
            AttachmentUri = attachmentUri;
            Read = Profile?.Id == Authorization.UserId;
        }

        public void SetRead()
        {
            Read = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Read)));
        }

        public void SetText(string text)
        {
            Text = text;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
        }
    }
}
