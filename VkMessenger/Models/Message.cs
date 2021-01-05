using ru.MaxKuzmin.VkMessenger.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
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
        public int Id { get; set; }
        public string Text { get; set; }
        public bool Read { get; set; }
        public DateTime Date { get; set; }
        public string TimeFormatted { get; set; }
        public Profile? Profile { get; set; }
        public Group? Group { get; set; }
        public string FullText { get; set; }
        public string PreviewText { get; set; }
        public IReadOnlyCollection<AttachmentImage> AttachmentImages { get; set; }
        public IReadOnlyCollection<Uri> AttachmentUris { get; set; }
        public Uri? VoiceMessage { get; set; }
        public int? VoiceMessageDuration { get; set; }
        public IReadOnlyCollection<AttachmentMessage> AttachmentMessages { get; set; }
        public bool FullScreenAllowed { get; set; }

        [JsonIgnore]
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

        [JsonIgnore]
        public ImageSource? Photo =>
            Profile?.Photo ??
            Group?.Photo ??
            Authorization.Photo;

        public event PropertyChangedEventHandler? PropertyChanged;

        [JsonConstructor]
        public Message() { }

        public Message(
            int id,
            string fullText,
            DateTime date,
            Profile? profile,
            Group? group,
            IReadOnlyCollection<AttachmentImage>? attachmentImages,
            IReadOnlyCollection<Uri>? attachmentUris,
            IReadOnlyCollection<AttachmentMessage>? attachmentMessages,
            (Uri, int)? voiceMessage,
            IReadOnlyCollection<string> otherAttachments)
        {
            Id = id;
            Date = date;
            Group = group;
            Profile = profile;
            AttachmentImages = attachmentImages ?? Array.Empty<AttachmentImage>();
            AttachmentUris = attachmentUris ?? Array.Empty<Uri>();
            VoiceMessage = voiceMessage?.Item1;
            VoiceMessageDuration = voiceMessage?.Item2;
            AttachmentMessages = attachmentMessages ?? Array.Empty<AttachmentMessage>();
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

            PreviewText = voiceMessage != null
                ? $"{PaperClip} {LocalizedStrings.VoiceMessage}"
                : Text.Replace('\n', ' ');
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
