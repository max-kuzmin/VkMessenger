using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class MessagesClient
    {
        public static List<Message> GetMessages(int dialogId)
        {
            var json = JObject.Parse(GetMessagesJson(dialogId));
            var profiles = ProfilesClient.FromJsonArray(json["response"]["profiles"] as JArray);
            var groups = GroupsClient.FromJsonArray(json["response"]["groups"] as JArray);
            return FromJsonArray(json["response"]["items"] as JArray, profiles, groups);
        }

        private static List<Message> FromJsonArray(JArray source, List<Profile> profiles, List<Group> groups)
        {
            var result = new List<Message>();

            foreach (var item in source)
            {
                result.Add(FromJson(item as JObject, profiles, groups));
            }

            return result;
        }

        public static Message FromJson(JObject source, List<Profile> profiles, List<Group> groups)
        {
            var text = source["text"].Value<string>();
            var dialogId = source["from_id"].Value<int>();
            return new Message
            {
                Id = source["id"].Value<uint>(),
                Text = text.Length > 200 ? text.Substring(0, 200) + "..." : text,
                Date = new DateTime(source["date"].Value<uint>(), DateTimeKind.Utc),
                Profile = profiles.FirstOrDefault(p => p.Id == dialogId),
                Group = groups.FirstOrDefault(p => p.Id == Math.Abs(dialogId))
            };
        }

        private static string GetMessagesJson(int dialogId)
        {
            var url =
                "https://api.vk.com/method/messages.getHistory" +
                "?v=5.92" +
                "&extended=1" +
                "&peer_id=" + dialogId +
                "&access_token=" + Models.Authorization.Token;

            using (var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }
}
