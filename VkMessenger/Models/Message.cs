using ru.MaxKuzmin.VkMessenger.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    /// <summary>
    /// Older messages with smaller Id are at the end of list
    /// </summary>
    public class Message : INotifyPropertyChanged
    {
        private const int MaxLength = 150;
        private const string PaperClip = "📎";

        /// <summary>
        /// Positive number
        /// </summary>
        public int Id { get; private set; }

        public int ConversationMessageId { get; private set; }
        public string Text { get; private set; }
        public bool? Read { get; private set; }
        public DateTime Date { get; private set; }
        public DateTime? UpdateTime { get; private set; }
        public Profile? Profile { get; private set; }
        public Group? Group { get; private set; }
        public string FullText { get; private set; }
        public IReadOnlyCollection<AttachmentImage> AttachmentImages { get; private set; }
        public IReadOnlyCollection<Uri> AttachmentUris { get; private set; }
        public Uri? VoiceMessage { get; private set; }
        public int? VoiceMessageDuration { get; private set; }
        public IReadOnlyCollection<AttachmentMessage> AttachmentMessages { get; private set; }
        public IReadOnlyCollection<string> OtherAttachments { get; private set; }
        public bool FullScreenAllowed { get; private set; }
        public bool Deleted { get; private set; }

        [JsonIgnore]
        public string TimeFormatted => Date.ToString("HH:mm");

        [JsonIgnore]
        public string PreviewText => VoiceMessage != null
             ? $"{PaperClip} {LocalizedStrings.VoiceMessage}"
             : Text.Replace('\n', ' ');

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
                    return 0;
            }
        }

        [JsonIgnore]
        public ImageSource? Photo =>
            Profile?.Photo ??
            Group?.Photo;

        public event PropertyChangedEventHandler? PropertyChanged;

        [JsonConstructor]
#pragma warning disable CS8618
        public Message() { }

        public Message(
#pragma warning restore CS8618
            int id,
            int conversationMessageId,
            string fullText,
            (Uri, int)? voiceMessage,
            DateTime date,
            DateTime? updateTime,
            bool deleted,
            Profile? profile,
            Group? group,
            IReadOnlyCollection<AttachmentImage>? attachmentImages,
            IReadOnlyCollection<Uri>? attachmentUris,
            IReadOnlyCollection<AttachmentMessage>? attachmentMessages,
            IReadOnlyCollection<string>? otherAttachments)
        {
            Id = id;
            ConversationMessageId = conversationMessageId;
            Date = date;
            UpdateTime = updateTime;
            Group = group;
            Profile = profile;
            AttachmentImages = attachmentImages ?? Array.Empty<AttachmentImage>();
            AttachmentUris = attachmentUris ?? Array.Empty<Uri>();
            VoiceMessage = voiceMessage?.Item1;
            VoiceMessageDuration = voiceMessage?.Item2;
            AttachmentMessages = attachmentMessages ?? Array.Empty<AttachmentMessage>();
            OtherAttachments = otherAttachments ?? Array.Empty<string>();
            Deleted = deleted;
            ComposeText(fullText);
        }

        public void SetRead(bool value)
        {
            if (Read != value)
            {
                Read = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Read)));
            }
        }

        public void SetFullText(string fullText)
        {
            if (FullText != fullText)
            {
                ComposeText(fullText);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }

        public void SetVoiceMessage(Uri? voiceMessage)
        {
            if (VoiceMessage != voiceMessage)
            {
                VoiceMessage = voiceMessage;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VoiceMessage)));
            }
        }

        private void ComposeText(string fullText)
        {
            FullText = fullText;
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

            foreach (var other in OtherAttachments.Distinct())
            {
                Text += $"\n{PaperClip} {other}";
            }

            Text = Text.Trim('\n');
        }

        public void SetDate(DateTime date)
        {
            if (Date != date)
            {
                Date = date;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Date)));
            }
        }

        public void SetUpdateTime(DateTime? updateTime)
        {
            if (UpdateTime != updateTime)
            {
                UpdateTime = updateTime;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpdateTime)));
            }
        }
    }
}
