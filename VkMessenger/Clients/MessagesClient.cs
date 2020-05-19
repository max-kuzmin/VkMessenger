using Newtonsoft.Json.Linq;
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
using Xamarin.Forms;
using Group = ru.MaxKuzmin.VkMessenger.Models.Group;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class MessagesClient
    {
        private static readonly MD5 Md5Hasher = MD5.Create();

        public static async Task<IReadOnlyCollection<Message>> GetMessages(
            int dialogId,
            uint? offset = null,
            IReadOnlyCollection<uint>? messagesIds = null)
        {
            try
            {
                Logger.Info($"Updating messages {messagesIds.ToJson()} in dialog {dialogId}");

                var json = JObject.Parse(messagesIds != null
                    ? await GetMessagesJson(messagesIds)
                    : await GetMessagesJson(dialogId, offset));

                var response = json["response"]!;
                var profiles = ProfilesClient.FromJsonArray((JArray)response["profiles"]!);
                var groups = GroupsClient.FromJsonArray((JArray)response["groups"]!);
                return FromJsonArray((JArray)response["items"]!, profiles, groups);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        private static List<Message> FromJsonArray(
            JArray source,
            IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups)
        {
            return source.Select(item => FromJson((JObject)item, profiles, groups)).ToList();
        }

        public static Message FromJson(
            JObject source,
            IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups)
        {
            var dialogId = (uint)source["from_id"]!.Value<int>();
            var messageId = source["id"]!.Value<uint>();
            var date = new DateTime(source["date"]!.Value<uint>(), DateTimeKind.Utc);
            var fullText = source["text"]!.Value<string>();

            var attachmentImages = new List<ImageSource>();
            var attachmentUris = new List<Uri>();
            var otherAttachments = new List<string>();

            var attachmentMessages = (source["fwd_messages"] as JArray)?
                .Select(i =>
                (
                    profiles.Single(e => e.Id == i["from_id"]!.Value<uint>()),
                    i["text"]!.Value<string>()
                )).ToArray();

            if (source["attachments"] is JArray attachments)
            {
                foreach (var item in attachments)
                {
                    switch (item["type"]!.Value<string>())
                    {
                        case "photo":
                            attachmentImages
                                .Add(new Uri(item["photo"]!["sizes"]!
                                .Single(i => i["type"]!.Value<string>() == "q")["url"]!.Value<string>()));
                            break;

                        case "link":
                            attachmentUris.Add(new Uri(item["link"]!["url"]!.Value<string>()));
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

                        default:
                            otherAttachments.Add(item["type"]!.Value<string>());
                            break;
                    }
                }
            }


            if (!attachmentUris.Any())
            {
                var matches = Regex.Matches(fullText, @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
                foreach (Match match in matches)
                {
                    if (Uri.TryCreate(match.Value, UriKind.Absolute, out Uri parsed))
                    {
                        attachmentUris.Add(parsed);
                    }
                }
            }

            return new Message(
                messageId,
                fullText,
                date,
                profiles?.FirstOrDefault(p => p.Id == dialogId),
                groups?.FirstOrDefault(p => p.Id == dialogId),
                attachmentImages,
                attachmentUris,
                attachmentMessages,
                otherAttachments);
        }

        private static async Task<string> GetMessagesJson(int dialogId, uint? offset = null)
        {
            var url =
                "https://api.vk.com/method/messages.getHistory" +
                "?v=5.92" +
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
                    "?v=5.92" +
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

        private static async Task<string> GetMessagesJson(IReadOnlyCollection<uint> messagesIds)
        {
            var url =
                "https://api.vk.com/method/messages.getById" +
                "?v=5.92" +
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
