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

        public async static Task<IReadOnlyCollection<Message>> GetMessages(int dialogId, IReadOnlyCollection<uint> messagesIds)
        {
            Logger.Info($"Updating messages {JsonConvert.SerializeObject(messagesIds)} in dialog {dialogId}");

            var json = JObject.Parse(messagesIds == null ?
                await GetMessagesJson(dialogId) :
                await GetMessagesJson(messagesIds));
            var profiles = ProfilesClient.FromJsonArray(json["response"]["profiles"] as JArray);
            var groups = GroupsClient.FromJsonArray(json["response"]["groups"] as JArray);
            return FromJsonArray(json["response"]["items"] as JArray, profiles, groups);
        }

        private static List<Message> FromJsonArray(JArray source, IReadOnlyCollection<Profile> profiles, IReadOnlyCollection<Group> groups)
        {
            var result = new List<Message>();

            foreach (var item in source)
            {
                result.Add(FromJson(item as JObject, profiles, groups));
            }

            return result;
        }

        public static Message FromJson(JObject source, IReadOnlyCollection<Profile> profiles, IReadOnlyCollection<Group> groups)
        {
            var text = source["text"].Value<string>();
            var dialogId = source["from_id"].Value<int>();

            var result = new Message();
            result.SetId(source["id"].Value<uint>());
            result.SetText(text.Length > Message.MaxLength ? text.Substring(0, Message.MaxLength) + "..." : text);
            result.SetDate(new DateTime(source["date"].Value<uint>(), DateTimeKind.Utc));
            result.SetProfile(profiles?.FirstOrDefault(p => p.Id == dialogId));
            result.SetGroup(groups?.FirstOrDefault(p => p.Id == Math.Abs(dialogId)));

            return result;
        }

        private async static Task<string> GetMessagesJson(int dialogId)
        {
            var url =
                "https://api.vk.com/method/messages.getHistory" +
                "?v=5.92" +
                "&extended=1" +
                "&peer_id=" + dialogId +
                "&access_token=" + Authorization.Token;

            using (var client = new ProxiedWebClient())
            {
                return await client.DownloadStringTaskAsync(url);
            }
        }

        public async static Task Send(string text, int dialogId)
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
                await client.DownloadStringTaskAsync(url);
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
                return await client.DownloadStringTaskAsync(url);
            }
        }
    }
}
