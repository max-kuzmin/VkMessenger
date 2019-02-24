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
            return FromJsonArray(json["response"]["items"] as JArray, profiles);
        }

        private static List<Message> FromJsonArray(JArray source, List<Profile> profiles)
        {
            var result = new List<Message>();

            foreach (var item in source)
            {
                result.Add(FromJson(item as JObject, profiles));
            }

            return result;
        }

        public static Message FromJson(JObject source, List<Profile> profiles)
        {
            return new Message
            {
                Id = source["id"].Value<int>(),
                Text = source["text"].Value<string>(),
                Date = new DateTime(source["date"].Value<int>(), DateTimeKind.Utc),
                Profile = profiles.FirstOrDefault(p => p.Id == source["from_id"].Value<int>())
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
