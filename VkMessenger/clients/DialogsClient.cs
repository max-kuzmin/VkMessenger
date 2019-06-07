using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var dialogId = conversation["peer"]["id"].Value<int>();
            var dialogType = Enum.Parse<DialogType>(conversation["peer"]["type"].Value<string>(), true);
            var unreadCount = conversation["unread_count"]?.Value<uint>() ?? 0u;

            var lastMessage = new[] { dialog.ContainsKey("last_message") ?
                MessagesClient.FromJson(dialog["last_message"] as JObject, profiles, groups) :
                lastMessages.First(e => e.Id == dialog["last_message_id"].Value<uint>()) };

            Dialog result = null;

            if (dialogType == DialogType.User)
            {
                var dialogProfiles = new[] { profiles.First(p => p.Id == dialogId) };
                result = new Dialog(dialogType, null, null, unreadCount, dialogProfiles, lastMessage);
            }
            else if (dialogType == DialogType.Group)
            {
                var group = groups.First(g => g.Id == Math.Abs(dialogId));
                result = new Dialog(dialogType, group, null, unreadCount, null, lastMessage);
            }
            else if (dialogType == DialogType.Chat)
            {
                var chatSettings = conversation["chat_settings"];
                var chat = new Chat
                {
                    Title = chatSettings["title"].Value<string>(),
                    Id = (uint)dialogId,
                    Photo = chatSettings["photo"] != null ?
                        ImageSource.FromUri(new Uri(chatSettings["photo"]["photo_50"].Value<string>())) : null
                };

                var dialogProfiles = new List<Profile>();
                foreach (var id in chatSettings["active_ids"] as JArray)
                {
                    dialogProfiles.Add(profiles.First(p => p.Id == id.Value<uint>()));
                }

                result = new Dialog(dialogType, null, chat, unreadCount, dialogProfiles, lastMessage);
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
            Logger.Info($"Updating dialogs {JsonConvert.SerializeObject(dialogIds)}");

            var json = JObject.Parse(dialogIds == null ?
                await GetDialogsJson() :
                await GetDialogsJson(dialogIds));
            Logger.Verbose(json.ToString());
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
            var lastMessages = lastMessagesIds.Any() ?
                await MessagesClient.GetMessages(0, lastMessagesIds) :
                Array.Empty<Message>();

            return FromJsonArray(json["response"]["items"] as JArray, profiles, groups, lastMessages);
        }

        private async static Task<string> GetDialogsJson()
        {
            var url =
                "https://api.vk.com/method/messages.getConversations" +
                "?v=5.92" +
                "&extended=1" +
                "&access_token=" + Authorization.Token;

            using (var client = new ProxiedWebClient())
            {
                return await client.DownloadStringTaskAsync(url);
            }
        }

        public async static Task<bool> MarkAsRead(int dialogId)
        {
            var url =
                "https://api.vk.com/method/messages.markAsRead" +
                "?v=5.92" +
                "&peer_id=" + dialogId +
                "&access_token=" + Authorization.Token;

            using (var client = new ProxiedWebClient())
            {
                var json = JObject.Parse(await client.DownloadStringTaskAsync(url));
                return json["response"].Value<int>() == 1;
            }
        }

        private async static Task<string> GetDialogsJson(IReadOnlyCollection<int> dialogIds)
        {
            var url =
                "https://api.vk.com/method/messages.getConversationsById" +
                "?v=5.92" +
                "&extended=1" +
                "&peer_ids=" + dialogIds.Aggregate(string.Empty, (seed, item) => seed + "," + item).Substring(1) +
                "&access_token=" + Authorization.Token;

            using (var client = new ProxiedWebClient())
            {
                return await client.DownloadStringTaskAsync(url);
            }
        }
    }
}
