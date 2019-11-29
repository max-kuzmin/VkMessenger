using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Message : INotifyPropertyChanged
    {
        private const int MaxLength = 150;

        public uint Id { get; }
        public string Text { get; private set; }
        public bool Read { get; private set; }
        public DateTime Date { get; }
        public Profile Profile { get; }
        public Group Group { get; }
        public string FullText { get; }
        public IReadOnlyCollection<ImageSource> AttachmentImages { get; }
        public IReadOnlyCollection<Uri> AttachmentUris { get; }

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

        public Message(
            uint id,
            string fullText,
            DateTime date,
            Profile profile,
            Group group,
            IReadOnlyCollection<ImageSource> attachmentImages,
            IReadOnlyCollection<Uri> attachmentUris,
            IReadOnlyCollection<string> forwardedMessages,
            IReadOnlyCollection<string> otherAttachments)
        {
            Id = id;
            Date = date;
            Group = group;
            Profile = profile;
            AttachmentImages = attachmentImages;
            AttachmentUris = attachmentUris;
            Read = Profile?.Id == Authorization.UserId;

            foreach (var forwarded in forwardedMessages)
            {
               fullText += $"\n \"{forwarded}\"";
            }

            foreach (var other in otherAttachments)
            {
                fullText += $"\n📎 {other}";
            }


            FullText = fullText;
            Text = fullText.Length > MaxLength
                ? fullText.Substring(0, MaxLength) + "..."
                : fullText;
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
