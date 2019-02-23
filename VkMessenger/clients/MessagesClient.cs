using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Net;
using Tizen.Applications;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class MessagesClient
    {
        public static List<Message> GetMessages(int peerId)
        {
            var json = JObject.Parse(GetMessagesJson(peerId));
            return FromJsonArray(json["response"]["items"] as JArray);
        }

        private static List<Message> FromJsonArray(JArray source)
        {
            var result = new List<Message>();

            foreach (var item in source)
            {
                result.Add(FromJson(item as JObject));
            }

            return result;
        }

        public static Message FromJson(JObject source)
        {
            return new Message
            {
                Id = source["id"].Value<int>(),
                Sender = source["from_id"].Value<int>(),
                Text = source["text"].Value<string>(),
                Date = new DateTime(source["date"].Value<int>(), DateTimeKind.Utc)
            };
        }

        private static string GetMessagesJson(int peerId)
        {
            var url =
                "https://api.vk.com/method/messages.getHistory" +
                "?v=5.92" +
                "&peer_id=" + peerId +
                "&access_token=" + Preference.Get<string>(Setting.TokenKey);

            using (var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }
}
