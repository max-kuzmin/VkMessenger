using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Extensions;
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
            IReadOnlyCollection<uint> messagesIds = null)
        {
            try
            {
                Logger.Info($"Updating messages {messagesIds.ToJson()} in dialog {dialogId}");

                var json = JObject.Parse(messagesIds != null
                    ? await GetMessagesJson(messagesIds)
                    : await GetMessagesJson(dialogId, offset));

                var profiles = ProfilesClient.FromJsonArray(json["response"]["profiles"] as JArray);
                var groups = GroupsClient.FromJsonArray(json["response"]["groups"] as JArray);
                return FromJsonArray(json["response"]["items"] as JArray, profiles, groups);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private static List<Message> FromJsonArray(
            JArray source,
            IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups)
        {
            var result = new List<Message>();

            foreach (var item in source)
            {
                result.Add(FromJson(item as JObject, profiles, groups));
            }

            return result;
        }

        public static Message FromJson(
            JObject source,
            IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups)
        {
            var dialogId = source["from_id"].Value<int>();

            ParseMessageBody(source, out var text, out var fullText, out var attachmentImages, out var attachmentUri);

            var result = new Message(
                source["id"].Value<uint>(),
                text,
                fullText,
                new DateTime(source["date"].Value<uint>(), DateTimeKind.Utc),
                profiles?.FirstOrDefault(p => p.Id == dialogId),
                groups?.FirstOrDefault(p => p.Id == Math.Abs(dialogId)),
                attachmentImages,
                attachmentUri);

            return result;
        }

        private static void ParseMessageBody(
            JObject source,
            out string text,
            out string fullText,
            out IReadOnlyCollection<ImageSource> attachmentImages,
            out Uri attachmentUri)
        {
            fullText = source["text"].Value<string>();

            var forwardMessages = (source["fwd_messages"] as JArray)?.Select(i => i["text"]).ToArray();
            if (forwardMessages != null)
            {
                foreach (var item in forwardMessages)
                {
                    if (fullText != string.Empty) fullText += "\n";
                    fullText += $"\"{item.Value<string>()}\"";
                }
            }

            text = fullText.Length > Message.MaxLength
                ? fullText.Substring(0, Message.MaxLength) + "..."
                : fullText;

            var attachmentImagesList = new List<ImageSource>();
            attachmentImages = attachmentImagesList;
            attachmentUri = null;

            if (source["attachments"] is JArray attachments)
            {
                foreach (var item in attachments)
                {
                    if (item["type"].Value<string>() == "photo")
                    {
                        attachmentImagesList
                            .Add(new Uri(item["photo"]["sizes"]
                            .Single(i => i["type"].Value<string>() == "q")["url"].Value<string>()));
                    }
                    else if (attachmentUri == null && item["type"].Value<string>() == "link")
                    {
                        attachmentUri = new Uri(item["link"]["url"].Value<string>());
                    }

                    if (text != string.Empty) text += "\n";
                    text += $"<{item["type"].Value<string>()}>";
                }
            }


            if (attachmentUri == null)
            {
                var match = Regex.Match(fullText, @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
                if (Uri.TryCreate(match.Value, UriKind.Absolute, out Uri parsed))
                {
                    attachmentUri = parsed;
                }
            }
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

            using (var client = new ProxiedWebClient())
            {
                var json = await client.DownloadStringTaskAsync(url);
                Logger.Debug(json);
                return json;
            }
        }

        public static async Task<bool> Send(string text, int dialogId)
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

                using (var client = new ProxiedWebClient())
                {
                    var json = await client.DownloadStringTaskAsync(url);
                    Logger.Debug(json);
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
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

            using (var client = new ProxiedWebClient())
            {
                var json = await client.DownloadStringTaskAsync(url);
                Logger.Debug(json);
                return json;
            }
        }
    }
}
