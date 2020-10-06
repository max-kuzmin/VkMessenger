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

        /// <summary>
        /// Positive number
        /// </summary>
        public int Id { get; }
        public string Text { get; private set; }
        public bool Read { get; private set; }
        public DateTime Date { get; }
        public string TimeFormatted { get; }
        public Profile? Profile { get; }
        public Group? Group { get; }
        public string FullText { get; }
        public IReadOnlyCollection<(ImageSource Url, bool IsSticker)> AttachmentImages { get; }
        public IReadOnlyCollection<Uri> AttachmentUris { get; }
        public Uri? AudioMessage { get; }
        public IReadOnlyCollection<(Profile? Profile, string Text)> AttachmentMessages { get; }
        public bool FullScreenAllowed { get; }

        public int SenderId
        {
            get
            {
                if (Group != null)
                    return -Group.Id;
                else if (Profile != null)
                    return Profile.Id;
                else
                    return Authorization.UserId;
            }
        }

        public ImageSource? Photo =>
            Profile?.Photo ??
            Group?.Photo ??
            Authorization.Photo;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Message(
            int id,
            string fullText,
            DateTime date,
            Profile? profile,
            Group? group,
            IReadOnlyCollection<(ImageSource Url, bool IsSticker)>? attachmentImages,
            IReadOnlyCollection<Uri>? attachmentUris,
            IReadOnlyCollection<(Profile? Profile, string Text)>? attachmentMessages,
            Uri? audioMessage,
            IReadOnlyCollection<string> otherAttachments)
        {
            Id = id;
            Date = date;
            Group = group;
            Profile = profile;
            AttachmentImages = attachmentImages ?? Array.Empty<(ImageSource Url, bool IsSticker)>();
            AttachmentUris = attachmentUris ?? Array.Empty<Uri>();
            AudioMessage = audioMessage;
            AttachmentMessages = attachmentMessages ?? Array.Empty<(Profile? Profile, string Text)>();
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

            if (AttachmentMessages.Any())
            {
                Text += $"\n{PaperClip} {LocalizedStrings.Message}";
                FullScreenAllowed = true;
            }

            if (AttachmentUris.Any())
            {
                Text += $"\n{PaperClip} {LocalizedStrings.Link}";
                FullScreenAllowed = true;
            }

            if (AttachmentImages.Any(e => !e.IsSticker))
            {
                Text += $"\n{PaperClip} {LocalizedStrings.Image}";
                FullScreenAllowed = true;
            }

            if (AttachmentImages.Any(e => e.IsSticker))
            {
                Text += $"\n{PaperClip} {LocalizedStrings.Sticker}";
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
