using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class DialogsClient
    {
        private static IReadOnlyCollection<Dialog> FromJsonArray(
            JArray dialogs,
            IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups,
            IReadOnlyCollection<Message> lastMessages)
        {
            var result = new List<Dialog>();

            foreach (var item in dialogs)
            {
                result.Add(FromJson(item as JObject, profiles, groups, lastMessages));
            }

            return result;
        }

        private static Dialog FromJson(
            JObject dialog,
            IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups,
            IReadOnlyCollection<Message> lastMessages)
        {
            var conversation = dialog["conversation"] ?? dialog;

            var result = new Dialog
            {
                UnreadCount = conversation["unread_count"]?.Value<uint>() ?? 0u,
                LastMessage = dialog.ContainsKey("last_message") ?
                    MessagesClient.FromJson(dialog["last_message"] as JObject, profiles, groups) :
                    lastMessages.First(e => e.Id == dialog["last_message_id"].Value<uint>())
            };

            var dialogId = conversation["peer"]["id"].Value<int>();
            var peerType = conversation["peer"]["type"].Value<string>();
            if (peerType == "user")
            {
                result.Type = DialogType.User;
                result.Profiles = new List<Profile> { profiles.First(p => p.Id == dialogId) };
            }
            else if (peerType == "group")
            {
                result.Type = DialogType.Group;
                result.Group = groups.First(g => g.Id == Math.Abs(dialogId));
            }
            else if (peerType == "chat")
            {
                var chatSettings = conversation["chat_settings"];
                result.Type = DialogType.Chat;
                result.Chat = new Chat
                {
                    Title = chatSettings["title"].Value<string>(),
                    Id = (uint)dialogId
                };

                result.Profiles = new List<Profile>();
                var ids = chatSettings["active_ids"] as JArray;
                foreach (var id in ids)
                {
                    result.Profiles.Add(profiles.First(p => p.Id == id.Value<uint>()));
                }

                if (chatSettings["photo"] != null)
                {
                    result.Chat.Photo = new UriImageSource
                    {
                        Uri = new Uri(chatSettings["photo"]["photo_50"].Value<string>()),
                        CachingEnabled = true,
                        CacheValidity = TimeSpan.FromDays(1)
                    };
                }
            }

            return result;
        }

        private static Profile GetFriend(JObject dialog, JArray profiles)
        {
            var conversation = dialog["conversation"] ?? dialog;

            var dialogId = conversation["peer"]["id"].Value<int>();
            var profile = profiles.Where(o => o["id"].Value<uint>() == dialogId).FirstOrDefault();
            if (profile != null)
            {
                return ProfilesClient.FromJson(profile as JObject);
            }
            else return null;
        }

        public async static Task<IReadOnlyCollection<Dialog>> GetDialogs(IReadOnlyCollection<int> dialogIds)
        {
            var json = JObject.Parse(dialogIds == null ? await GetDialogsJson() : await GetDialogsJson(dialogIds));
            var profiles = ProfilesClient.FromJsonArray(json["response"]["profiles"] as JArray);
            var groups = GroupsClient.FromJsonArray(json["response"]["groups"] as JArray);

            var lastMessagesIds = new List<uint>();
            foreach (JObject item in json["response"]["items"])
            {
                if (!item.ContainsKey("last_message"))
                {
                    lastMessagesIds.Add(item["last_message_id"].Value<uint>());
                }
            }
            var lastMessages = lastMessagesIds.Any() ? await MessagesClient.GetMessages(0, lastMessagesIds) : new Message[] { };

            return FromJsonArray(json["response"]["items"] as JArray, profiles, groups, lastMessages);
        }

        private async static Task<string> GetDialogsJson()
        {
            var url =
                "https://api.vk.com/method/messages.getConversations" +
                "?v=5.92" +
                "&extended=1" +
                "&access_token=" + Models.Authorization.Token;

            using (var client = new WebClient())
            {
                return await client.DownloadStringTaskAsync(url);
            }
        }

        public async static Task MarkAsRead(int dialogId)
        {
            var url =
                "https://api.vk.com/method/messages.markAsRead" +
                "?v=5.92" +
                "&peer_id=" + dialogId +
                "&access_token=" + Models.Authorization.Token;

            using (var client = new WebClient())
            {
                await client.DownloadStringTaskAsync(url);
            }
        }

        private async static Task<string> GetDialogsJson(IReadOnlyCollection<int> dialogIds)
        {
            var url =
                "https://api.vk.com/method/messages.getConversationsById" +
                "?v=5.92" +
                "&extended=1" +
                "&peer_ids=" + dialogIds.Aggregate(string.Empty, (seed, item) => seed + "," + item).Substring(1) +
                "&access_token=" + Models.Authorization.Token;

            using (var client = new WebClient())
            {
                return await client.DownloadStringTaskAsync(url);
            }
        }
    }
}
