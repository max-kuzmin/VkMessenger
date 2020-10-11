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
using Xamarin.Forms;
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

                var json = await HttpHelpers.RetryIfEmptyResponse<JsonDto<MessagesResponseDto>>(
                    () => GetMessagesJson(dialogId, offset), e => e?.response != null);

                var response = json.response;
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

                var json = await HttpHelpers.RetryIfEmptyResponse<JsonDto<MessagesResponseDto>>(
                    () => GetMessagesJsonByIds(messagesIds), e => e?.response != null);

                var response = json.response;
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
                var peerId = message.from_id;
                var date = DateTimeOffset
                    .FromUnixTimeSeconds(message.date).UtcDateTime
                    .Add(TimeZoneInfo.Local.BaseUtcOffset);
                var fullText = message.text;

                var attachmentImages = new List<(ImageSource Url, bool IsSticker)>();
                var attachmentUris = new List<Uri>();
                (Uri, int)? voiceMessage = null;
                var otherAttachments = new List<string>();

                var attachmentMessages = message.fwd_messages?
                    .Select(i =>
                    (
                        // ReSharper disable once RedundantCast
                        (Profile?)profiles.SingleOrDefault(e => e.Id == i.from_id),
                        i.text
                    )).ToArray();

                if (message.attachments?.Any() == true)
                {
                    foreach (var item in message.attachments)
                    {
                        switch (item.type)
                        {
                            case "photo":
                                var photoUri = item.photo?.sizes.SingleOrDefault(i => i.type == "q");
                                if (photoUri == null)
                                    Logger.Error("Uri for photo attachment is null. Attachment: " + item.ToJson());
                                else
                                    attachmentImages.Add((photoUri.url, false));
                                break;

                            case "link":
                                Uri.TryCreate(item.link?.url, UriKind.RelativeOrAbsolute, out Uri? uriResult);
                                if (uriResult != null)
                                    attachmentUris.Add(uriResult);
                                break;

                            case "wall":
                                otherAttachments.Add(LocalizedStrings.WallPost);
                                break;

                            case "video":
                                otherAttachments.Add(LocalizedStrings.Video);
                                break;

                            case "doc":
                                otherAttachments.Add(LocalizedStrings.File);
                                break;

                            case "album":
                                otherAttachments.Add(LocalizedStrings.Album);
                                break;

                            case "sticker":
                                var stickerUri = item.sticker?.images.FirstOrDefault(i => i.height == 256)
                                        ?? item.sticker?.images.OrderBy(i => i.height).FirstOrDefault();
                                if (stickerUri == null)
                                    Logger.Error("Uri for sticker attachment is null. Attachment: " + item.ToJson());
                                else
                                    attachmentImages.Add((stickerUri.url, true));
                                break;

                            case "audio_message":
                                voiceMessage = item.audio_message != null
                                    ? (item.audio_message.link_mp3, item.audio_message.duration)
                                    : ((Uri, int)?)null;
                                break;

                            default:
                                otherAttachments.Add(item.type);
                                break;
                        }
                    }
                }

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

                return new Message(
                    message.id,
                    fullText,
                    date,
                    profiles.FirstOrDefault(p => p.Id == peerId),
                    groups.FirstOrDefault(p => p.Id == peerId),
                    attachmentImages,
                    attachmentUris,
                    attachmentMessages,
                    voiceMessage,
                    otherAttachments);

            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        private static async Task<string> GetMessagesJson(int dialogId, int? offset)
        {
            var url =
                "https://api.vk.com/method/messages.getHistory" +
                "?v=5.124" +
                "&extended=1" +
                "&offset=" + (offset ?? 0) +
                "&peer_id=" + dialogId +
                "&access_token=" + Authorization.Token;

            using var client = new ProxiedWebClient();
            var json = await client.DownloadStringTaskAsync(url);
            Logger.Debug(json);
            ExceptionHelpers.ThrowIfInvalidSession(json);
            return json;
        }

        public static async Task Send(string text, int dialogId)
        {
            try
            {
                var url =
                    "https://api.vk.com/method/messages.send" +
                    "?v=5.124" +
                    "&random_id=" + BitConverter.ToInt32(Md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(text)), 0) +
                    "&peer_id=" + dialogId +
                    "&message=" + text +
                    "&access_token=" + Authorization.Token;

                using var client = new ProxiedWebClient();
                var json = await client.DownloadStringTaskAsync(url);
                Logger.Debug(json);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        private static async Task<string> GetMessagesJsonByIds(IReadOnlyCollection<int> messagesIds)
        {
            var url =
                "https://api.vk.com/method/messages.getById" +
                "?v=5.124" +
                "&extended=1" +
                "&message_ids=" + messagesIds.Aggregate(string.Empty, (seed, item) => seed + "," + item).Substring(1) +
                "&access_token=" + Authorization.Token;

            using var client = new ProxiedWebClient();
            var json = await client.DownloadStringTaskAsync(url);
            Logger.Debug(json);
            ExceptionHelpers.ThrowIfInvalidSession(json);
            return json;
        }
    }
}
