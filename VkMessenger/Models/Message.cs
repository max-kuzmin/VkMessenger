﻿using ru.MaxKuzmin.VkMessenger.Localization;
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
        private const string TrashCan = "🗑";

        /// <summary>
        /// Positive number
        /// </summary>
        public int Id { get; set; }

        public int ConversationMessageId { get; set; }
        public string Text { get; set; }
        public bool? Read { get; set; }
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
        public bool Deleted { get; set; }

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
#pragma warning restore CS8618

        public Message(
            int id,
            int conversationMessageId,
            string fullText,
            DateTime date,
            Profile? profile,
            Group? group,
            IReadOnlyCollection<AttachmentImage>? attachmentImages,
            IReadOnlyCollection<Uri>? attachmentUris,
            IReadOnlyCollection<AttachmentMessage>? attachmentMessages,
            (Uri, int)? voiceMessage,
            IReadOnlyCollection<string> otherAttachments,
            bool deleted)
        {
            Id = id;
            ConversationMessageId = conversationMessageId;
            Date = date;
            Group = group;
            Profile = profile;
            AttachmentImages = attachmentImages ?? Array.Empty<AttachmentImage>();
            AttachmentUris = attachmentUris ?? Array.Empty<Uri>();
            VoiceMessage = voiceMessage?.Item1;
            VoiceMessageDuration = voiceMessage?.Item2;
            AttachmentMessages = attachmentMessages ?? Array.Empty<AttachmentMessage>();
            FullText = fullText;
            TimeFormatted = date.ToString("HH:mm");
            Deleted = deleted;

            if (fullText.Length > MaxLength)
            {
                Text = fullText.Substring(0, MaxLength) + "...";
                FullScreenAllowed = true;
            }
            else
            {
                Text = fullText;
            }

            if (Deleted)
                Text = $"{TrashCan} {Text}";

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

        public void SetRead(bool value)
        {
            if (Read != value)
            {
                Read = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Read)));
            }
        }

        public void SetText(string text)
        {
            if (Text != text)
            {
                Text = text;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }

        public void SetDeleted(bool value)
        {
            if (Deleted != value)
            {
                Deleted = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Deleted)));

                if (value && !Text.StartsWith(TrashCan))
                    SetText($"{TrashCan} {Text}");
            }
        }
    }
}
