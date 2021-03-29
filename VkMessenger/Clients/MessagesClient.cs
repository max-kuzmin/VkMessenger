using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Models;
using ru.MaxKuzmin.VkMessenger.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Dtos;
using ru.MaxKuzmin.VkMessenger.Managers;
using Group = ru.MaxKuzmin.VkMessenger.Models.Group;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class MessagesClient
    {
        private static readonly MD5 Md5Hasher = MD5.Create();

        private const string LinkRegex =
            @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";

        public static async Task<IReadOnlyCollection<Message>> GetMessages(int dialogId, int? offset)
        {
            try
            {
                Logger.Info($"Updating messages in dialog {dialogId}");

                var response = await GetMessagesJson(dialogId, offset);
                var profiles = ProfilesClient.FromDtoArray(response.profiles);
                var groups = GroupsClient.FromDtoArray(response.groups);
                return FromDtoArray(response.items, profiles, groups);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        public static async Task<IReadOnlyCollection<Message>> GetMessagesByIds(IReadOnlyCollection<int> messagesIds)
        {
            try
            {
                Logger.Info($"Updating messages {messagesIds.ToJson()}");

                var response = await GetMessagesJsonByIds(messagesIds);
                var profiles = ProfilesClient.FromDtoArray(response.profiles);
                var groups = GroupsClient.FromDtoArray(response.groups);
                return FromDtoArray(response.items, profiles, groups);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        private static List<Message> FromDtoArray(
            MessageDto[] messages,
            IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups)
        {
            return messages.Select(item => FromDto(item, profiles, groups)).ToList();
        }

        public static Message FromDto(
            MessageDto message,
            IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups)
        {
            try
            {
                var peerId = Math.Abs(message.from_id); // FromId can be negative
                var date = DateTimeOffset
                    .FromUnixTimeSeconds(message.date).UtcDateTime
                    .Add(TimeZoneInfo.Local.BaseUtcOffset);
                var updateTime = message.update_time != null
                    ? DateTimeOffset
                        .FromUnixTimeSeconds(message.update_time.Value).UtcDateTime
                        .Add(TimeZoneInfo.Local.BaseUtcOffset)
                    : (DateTime?)null;
                var fullText = message.text;

                var attachmentImages = new List<AttachmentImage>();
                var attachmentUris = new List<Uri>();
                (Uri, int)? voiceMessage = null;
                var attachmentsNames = new HashSet<string>();

                var attachmentMessages = message.fwd_messages?
                    .Select(i =>
                    new AttachmentMessage
                    {
                        Profile = profiles.SingleOrDefault(e => e.Id == Math.Abs(i.from_id)), // FromId can be negative
                        Text = i.text
                    }).ToArray();

                if (message.attachments != null)
                {
                    foreach (var item in message.attachments)
                    {
                        switch (item.type)
                        {
                            case "photo":
                                var photoUri = item.photo?.sizes.SingleOrDefault(i => i.type == "q")
                                               ?? item.photo?.sizes.FirstOrDefault();

                                if (photoUri != null)
                                    attachmentImages.Add(new AttachmentImage { Url = photoUri.url, IsSticker = false });

                                attachmentsNames.Add(LocalizedStrings.Image);
                                break;

                            case "link":
                                Uri.TryCreate(item.link?.url, UriKind.RelativeOrAbsolute, out Uri? uriResult);
                                if (uriResult != null)
                                    attachmentUris.Add(uriResult);

                                attachmentsNames.Add(LocalizedStrings.Link);
                                break;

                            case "wall":
                                attachmentsNames.Add(LocalizedStrings.WallPost);
                                break;

                            case "video":
                                attachmentsNames.Add(LocalizedStrings.Video);
                                break;

                            case "doc":
                                attachmentsNames.Add(LocalizedStrings.File);
                                break;

                            case "album":
                                attachmentsNames.Add(LocalizedStrings.Album);
                                break;

                            case "sticker":
                                var stickerUri = item.sticker?.images.FirstOrDefault(i => i.height == 256)
                                        ?? item.sticker?.images.OrderBy(i => i.height).FirstOrDefault();
                                if (stickerUri == null)
                                    Logger.Error("Uri for sticker attachment is null. Attachment: " + item.ToJson());
                                else
                                    attachmentImages.Add(new AttachmentImage { Url = stickerUri.url, IsSticker = true });

                                attachmentsNames.Add(LocalizedStrings.Sticker);
                                break;

                            case "audio_message":
                                voiceMessage = item.audio_message != null
                                    ? (item.audio_message.link_mp3, item.audio_message.duration)
                                    : ((Uri, int)?)null;

                                attachmentsNames.Add(LocalizedStrings.VoiceMessage);
                                break;

                            default:
                                attachmentsNames.Add(item.type);
                                break;
                        }
                    }
                }

                ParseUrisFromText(fullText, attachmentUris);

                return new Message(
                    message.id,
                    fullText,
                    ComposeText(fullText, attachmentsNames, message.action),
                    voiceMessage,
                    date,
                    updateTime,
                    message.deleted > 0,
                    profiles.FirstOrDefault(p => p.Id == peerId),
                    groups.FirstOrDefault(p => p.Id == peerId),
                    attachmentImages,
                    attachmentUris,
                    attachmentMessages);

            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        private static string ComposeText(string fullText, HashSet<string> attachmentsNames, MessageActionDto? action)
        {
            var text = fullText.Length > Consts.MaxMessagePreviewLength
                ? fullText.Substring(0, Consts.MaxMessagePreviewLength) + "..."
                : fullText;

            if (action != null)
                text = action.type switch
                {
                    "chat_title_update" => $"\n{LocalizedStrings.ChatRename}: {action.text}",
                    _ => ""
                };

            foreach (var attachment in attachmentsNames)
            {
                text += $"\n{Consts.PaperClip} {attachment}";
            }

            return text.Trim('\n');
        }

        private static void ParseUrisFromText(string fullText, List<Uri> attachmentUris)
        {
            if (!attachmentUris.Any())
            {
                var matches = Regex.Matches(fullText, LinkRegex);
                foreach (Match match in matches)
                {
                    if (Uri.TryCreate(match.Value, UriKind.Absolute, out Uri parsed))
                    {
                        attachmentUris.Add(parsed);
                    }
                }
            }
        }

        private static async Task<MessagesResponseDto> GetMessagesJson(int dialogId, int? offset)
        {
            var url =
                "https://api.vk.com/method/messages.getHistory" +
                "?v=5.124" +
                "&extended=1" +
                "&offset=" + (offset ?? 0) +
                "&peer_id=" + dialogId +
                "&access_token=" + AuthorizationManager.Token;

            using var client = new ProxiedWebClient();
            var json = await HttpHelpers.RetryIfEmptyResponse<JsonDto<MessagesResponseDto>>(
                () => client.GetAsync(new Uri(url)), e => e?.response != null);
            return json.response;
        }

        public static async Task<int> Send(int dialogId, string? text, string? voiceMessagePath)
        {
            try
            {
                Logger.Info($"Sending message in dialog {dialogId}");

                var url =
                    "https://api.vk.com/method/messages.send" +
                    "?v=5.124" +
                    "&peer_id=" + dialogId +
                    "&access_token=" + AuthorizationManager.Token;

                if (text != null)
                {
                    url += "&message=" + text;
                    url += "&random_id=" + BitConverter.ToInt32(Md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(text)), 0);
                }
                else if (voiceMessagePath != null)
                {
                    var id = await DocumentsClient.UploadAudioFile(voiceMessagePath);
                    url += "&attachment=audio_message" + AuthorizationManager.UserId + "_" + id;
                    url += "&random_id=" + BitConverter.ToInt32(Md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(voiceMessagePath)), 0);
                }
                else
                {
                    throw new ArgumentNullException(nameof(voiceMessagePath));
                }

                using var client = new ProxiedWebClient();
                var result = await HttpHelpers.RetryIfEmptyResponse<JsonDto<int>>(
                    () => client.GetAsync(new Uri(url)),
                    e => e?.response != null);

                return result.response;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        public static async Task DeleteMessage(int messageId)
        {
            try
            {
                Logger.Info($"Removing message {messageId}");

                var url =
                    "https://api.vk.com/method/messages.delete" +
                    "?v=5.124" +
                    "&message_ids=" + messageId +
                    "&access_token=" + AuthorizationManager.Token +
                    "&delete_for_all=1";

                using var client = new ProxiedWebClient();
                await HttpHelpers.RetryIfEmptyResponse<JsonDto<object>>(
                    () => client.GetAsync(new Uri(url)),
                    e => e?.response != null || e?.error?.error_code == 924); // Already deleted
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        private static async Task<MessagesResponseDto> GetMessagesJsonByIds(IReadOnlyCollection<int> messagesIds)
        {
            var url =
                "https://api.vk.com/method/messages.getById" +
                "?v=5.124" +
                "&extended=1" +
                "&message_ids=" + messagesIds.Aggregate(string.Empty, (seed, item) => seed + "," + item).Substring(1) +
                "&access_token=" + AuthorizationManager.Token;

            using var client = new ProxiedWebClient();
            var json = await HttpHelpers.RetryIfEmptyResponse<JsonDto<MessagesResponseDto>>(
                () => client.GetAsync(new Uri(url)), e => e?.response != null);

            return json.response;
        }
    }
}
