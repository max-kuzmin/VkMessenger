using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace vk_messenger.models
{
    public class Dialog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Message LastMessage { get; set; }

        public static List<Dialog> FromJsonArray(JArray source)
        {
            var result = new List<Dialog>();

            foreach (var item in source)
            {
                result.Add(FromJson(item as JObject));
            }

            return result;
        }

        public static Dialog FromJson(JObject source)
        {
            return new Dialog
            {
                Id = source["peer"]["id"].Value<int>(),
                LastMessage = Message.FromJson(source["last_message"] as JObject),
                Name = GetDialogName(source)
            };
        }

        private static string GetDialogName(JObject source)
        {
            if (source["chat_settings"].HasValues)
            {
                return source["chat_settings"]["title"].Value<string>();
            }
            else
            {
                var id = source["peer"]["id"].Value<int>();
                var user = source["profiles"].Where(o => o["id"].Value<int>() == id).First();
                return user["first_name"].Value<string>() + " " + user["last_name"].Value<string>();
            }
        }
    }
}
