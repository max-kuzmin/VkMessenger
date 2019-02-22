using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Sender { get; set; }
        public DateTime Date { get; set; }

        public static List<Message> GetMessages(int peerId)
        {
            var json = JObject.Parse(Api.GetMessagesJson(peerId));
            return Message.FromJsonArray(json["response"]["items"] as JArray);
        }

        public static List<Message> FromJsonArray(JArray source)
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
    }
}
