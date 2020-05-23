using ru.MaxKuzmin.VkMessenger.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Message : INotifyPropertyChanged
    {
        private const int MaxLength = 150;
        private const string PaperClip = "📎";

        public uint Id { get; }
        public string Text { get; private set; }
        public bool Read { get; private set; }
        public DateTime Date { get; }
        public string TimeFormatted { get; }
        public Profile? Profile { get; }
        public Group? Group { get; }
        public string FullText { get; }
        public IReadOnlyCollection<ImageSource> AttachmentImages { get; }
        public IReadOnlyCollection<Uri> AttachmentUris { get; }
        public IReadOnlyCollection<(Profile Profile, string Text)> AttachmentMessages { get; }
        public bool FullScreenAllowed { get; }

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

        public ImageSource? Photo =>
            Profile?.Photo ??
            Group?.Photo ??
            Authorization.Photo;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Message(
            uint id,
            string fullText,
            DateTime date,
            Profile? profile,
            Group? group,
            IReadOnlyCollection<ImageSource>? attachmentImages,
            IReadOnlyCollection<Uri>? attachmentUris,
            IReadOnlyCollection<(Profile Profile, string Text)>? attachmentMessages,
            IReadOnlyCollection<string> otherAttachments)
        {
            Id = id;
            Date = date;
            Group = group;
            Profile = profile;
            AttachmentImages = attachmentImages ?? Array.Empty<ImageSource>();
            AttachmentUris = attachmentUris ?? Array.Empty<Uri>();
            AttachmentMessages = attachmentMessages ?? Array.Empty<(Profile Profile, string Text)>();
            Read = Profile?.Id == Authorization.UserId;
            FullText = fullText;
            TimeFormatted = date.ToString("HH:mm");

            if (fullText.Length > MaxLength)
            {
                Text = fullText.Substring(0, MaxLength) + "...";
                FullScreenAllowed = true;
            }
            else
            {
                Text = fullText;
            }

            foreach (var _ in attachmentMessages.Select(e => e.Profile.Name).Distinct())
            {
                Text += $"\n{PaperClip} {LocalizedStrings.Message}";
                FullScreenAllowed = true;
            }

            if (attachmentUris.Any())
            {
                Text += $"\n{PaperClip} {LocalizedStrings.Link}";
                FullScreenAllowed = true;
            }

            if (attachmentImages.Any())
            {
                Text += $"\n{PaperClip} {LocalizedStrings.Image}";
                FullScreenAllowed = true;
            }

            foreach (var other in otherAttachments.Distinct())
            {
                Text += $"\n{PaperClip} {other}";
            }

            Text = Text.Trim('\n');
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
