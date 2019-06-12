using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class MessagesClient
    {
        private static readonly MD5 md5Hasher = MD5.Create();

        public async static Task<IReadOnlyCollection<Message>> GetMessages(int dialogId, uint offset, IReadOnlyCollection<uint> messagesIds)
        {
            try
            {
                Logger.Info($"Updating messages {JsonConvert.SerializeObject(messagesIds)} in dialog {dialogId}");

                var json = JObject.Parse(messagesIds != null ?
                    await GetMessagesJson(messagesIds) :
                    await GetMessagesJson(dialogId, offset));

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

        private static List<Message> FromJsonArray(JArray source, IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups)
        {
            var result = new List<Message>();

            foreach (var item in source)
            {
                result.Add(FromJson(item as JObject, profiles, groups));
            }

            return result;
        }

        public static Message FromJson(JObject source, IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups)
        {
            var dialogId = source["from_id"].Value<int>();

            ParseAttachments(source, out string text, out Uri attachmentImage);

            var result = new Message(
                source["id"].Value<uint>(),
                text.Length > Message.MaxLength ? text.Substring(0, Message.MaxLength) + "..." : text,
                new DateTime(source["date"].Value<uint>(), DateTimeKind.Utc),
                profiles?.FirstOrDefault(p => p.Id == dialogId),
                groups?.FirstOrDefault(p => p.Id == Math.Abs(dialogId)),
                attachmentImage);

            return result;
        }

        private static void ParseAttachments(JObject source, out string text, out Uri attachmentImage)
        {
            text = source["text"].Value<string>();
            attachmentImage = null;

            var firstAttachment = (source["attachments"] as JArray)?.FirstOrDefault();
            if (firstAttachment != null)
            {
                if (firstAttachment["type"].Value<string>() == "photo")
                {
                    attachmentImage = new Uri(firstAttachment["photo"]["sizes"]
                        .Single(i => i["type"].Value<string>() == "s")["url"].Value<string>());
                }
                else
                {
                    if (text != string.Empty) text += "\n";

                    if (firstAttachment["type"].Value<string>() == "link")
                        text += $"<{firstAttachment["link"]["url"].Value<string>()}>";
                    else
                        text += $"<{firstAttachment["type"].Value<string>()}>";
                }
            }

            var forwardMessages = (source["fwd_messages"] as JArray)?.Select(i => i["text"])?.ToArray();
            if (forwardMessages != null)
            {
                foreach (var item in forwardMessages)
                {
                    if (text != string.Empty) text += "\n";
                    text += $"\"{item.Value<string>()}\"";
                }
            }
        }

        private async static Task<string> GetMessagesJson(int dialogId, uint offset)
        {
            var url =
                "https://api.vk.com/method/messages.getHistory" +
                "?v=5.92" +
                "&extended=1" +
                "&offset=" + offset +
                "&peer_id=" + dialogId +
                "&access_token=" + Authorization.Token;

            using (var client = new ProxiedWebClient())
            {
                var json = await client.DownloadStringTaskAsync(url);
                Logger.Debug(json.ToString());
                return json;
            }
        }

        public async static Task<bool> Send(string text, int dialogId)
        {
            try
            {
                var url =
                    "https://api.vk.com/method/messages.send" +
                    "?v=5.92" +
                    "&random_id=" + BitConverter.ToInt32(md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(text)), 0) +
                    "&peer_id=" + dialogId +
                    "&message=" + text +
                    "&access_token=" + Authorization.Token;

                using (var client = new ProxiedWebClient())
                {
                    var json = await client.DownloadStringTaskAsync(url);
                    Logger.Debug(json.ToString());
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        private async static Task<string> GetMessagesJson(IReadOnlyCollection<uint> messagesIds)
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
                Logger.Debug(json.ToString());
                return json;
            }
        }
    }
}
