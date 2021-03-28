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
        /// <summary>
        /// Positive number
        /// </summary>
        public int Id { get; set; }
        public string Text { get; set; }
        public bool? Read { get; set; }
        public DateTime Date { get; set; }
        public DateTime? UpdateTime { get; set; }
        public Profile? Profile { get; set; }
        public Group? Group { get; set; }
        public string FullText { get; set; }
        public IReadOnlyCollection<AttachmentImage> AttachmentImages { get; set; }
        public IReadOnlyCollection<Uri> AttachmentUris { get; set; }
        public Uri? VoiceMessage { get; set; }
        public int? VoiceMessageDuration { get; set; }
        public IReadOnlyCollection<AttachmentMessage> AttachmentMessages { get; set; }
        public IReadOnlyCollection<string> OtherAttachments { get; set; }
        public bool FullScreenAllowed { get; set; }
        public bool Deleted { get; set; }

        [JsonIgnore]
        public string TimeFormatted => Date.ToString("HH:mm");

        [JsonIgnore]
        public string PreviewText => VoiceMessage != null
             ? $"{Consts.PaperClip} {LocalizedStrings.VoiceMessage}"
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
            if (fullText.Length > Consts.MaxMessagePreviewLength)
            {
                Text = fullText.Substring(0, Consts.MaxMessagePreviewLength) + "...";
                FullScreenAllowed = true;
            }
            else
            {
                Text = fullText;
            }

            if (AttachmentMessages.Any())
            {
                Text += $"\n{Consts.PaperClip} {LocalizedStrings.Message}";
                FullScreenAllowed = true;
            }

            if (AttachmentUris.Any())
            {
                Text += $"\n{Consts.PaperClip} {LocalizedStrings.Link}";
                FullScreenAllowed = true;
            }

            if (AttachmentImages.Any(e => !e.IsSticker))
            {
                Text += $"\n{Consts.PaperClip} {LocalizedStrings.Image}";
                FullScreenAllowed = true;
            }

            if (AttachmentImages.Any(e => e.IsSticker))
            {
                Text += $"\n{Consts.PaperClip} {LocalizedStrings.Sticker}";
                FullScreenAllowed = true;
            }

            foreach (var other in OtherAttachments.Distinct())
            {
                Text += $"\n{Consts.PaperClip} {other}";
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

        public void SetProfile(Profile? profile)
        {
            if (Profile != profile)
            {
                Profile = profile;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Profile)));
            }
        }

        public void SetGroup(Group? group)
        {
            if (Group != group)
            {
                Group = group;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Group)));
            }
        }
    }
}
