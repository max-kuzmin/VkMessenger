using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Models;
using ru.MaxKuzmin.VkMessenger.Net;
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
            return dialogs
                .Select(item => FromJson((JObject)item, profiles, groups, lastMessages))
                .ToList();
        }

        private static Dialog FromJson(
            JObject dialog,
            IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups,
            IReadOnlyCollection<Message> lastMessages)
        {
            var conversation = dialog["conversation"] ?? dialog;
            var peerId = Math.Abs(conversation["peer"]!["id"]!.Value<int>()); // dialogs with groups have negative ids
            var dialogType = Enum.Parse<DialogType>(conversation["peer"]!["type"]!.Value<string>(), true);
            var unreadCount = conversation["unread_count"]?.Value<int>() ?? 0;

            var lastMessage = new[] {
                dialog.ContainsKey("last_message")
                    ? MessagesClient.FromJson((JObject)dialog["last_message"]!, profiles, groups)
                    : lastMessages.First(e => e.Id == dialog["last_message_id"]!.Value<int>())
            };

            Dialog result = default!;

            switch (dialogType)
            {
                case DialogType.User:
                    {
                        var dialogProfiles = new[] { profiles.First(p => p.Id == peerId) };
                        result = new Dialog(dialogType, null, null, unreadCount, dialogProfiles, lastMessage);
                        break;
                    }
                case DialogType.Group:
                    {
                        var group = groups.First(g => g.Id == peerId);
                        result = new Dialog(dialogType, group, null, unreadCount, null, lastMessage);
                        break;
                    }
                case DialogType.Chat:
                    {
                        var chatSettings = conversation["chat_settings"]!;
                        var chat = new Chat
                        {
                            Title = chatSettings["title"]!.Value<string>(),
                            Id = peerId,
                            Photo = chatSettings["photo"] != null
                                ? ImageSource.FromUri(new Uri(chatSettings["photo"]!["photo_50"]!.Value<string>()))
                                : null
                        };

                        var dialogProfiles = ((JArray)chatSettings["active_ids"]!)
                            .Select(id => profiles.First(p => p.Id == id.Value<int>()))
                            .ToArray();

                        result = new Dialog(dialogType, null, chat, unreadCount, dialogProfiles, lastMessage);
                        break;
                    }
            }

            return result;
        }

        public static async Task<IReadOnlyCollection<Dialog>> GetDialogs(IReadOnlyCollection<int>? dialogIds = null)
        {
            try
            {
                Logger.Info($"Updating dialogs {dialogIds.ToJson()}");

                var json = JObject.Parse(dialogIds == null
                    ? await GetDialogsJson()
                    : await GetDialogsJson(dialogIds));

                var response = json["response"]!;
                var responseItems = response["items"]!;

                var profiles = ProfilesClient.FromJsonArray((JArray)response["profiles"]!);
                var groups = GroupsClient.FromJsonArray((JArray)response["groups"]!);

                var lastMessagesIds = responseItems
                    .Select(jToken => (JObject)jToken)
                    .Where(e => !e.ContainsKey("last_message"))
                    .Select(e => e["last_message_id"]!.Value<int>()).ToList();

                var lastMessages = lastMessagesIds.Any()
                    ? await MessagesClient.GetMessages(0, 0, lastMessagesIds)
                    : Array.Empty<Message>();

                return FromJsonArray((JArray)responseItems, profiles, groups, lastMessages);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        private static async Task<string> GetDialogsJson()
        {
            var url =
                "https://api.vk.com/method/messages.getConversations" +
                "?v=5.92" +
                "&extended=1" +
                "&access_token=" + Authorization.Token;

            using var client = new ProxiedWebClient();
            var json = await client.DownloadStringTaskAsync(url);
            Logger.Debug(json);
            ExceptionHelpers.ThrowIfInvalidSession(json);
            return json;
        }

        public static async Task<bool> MarkAsRead(int dialogId)
        {
            try
            {
                var url =
                    "https://api.vk.com/method/messages.markAsRead" +
                    "?v=5.92" +
                    "&peer_id=" + dialogId +
                    "&access_token=" + Authorization.Token;

                using var client = new ProxiedWebClient();
                var json = JObject.Parse(await client.DownloadStringTaskAsync(url));
                Logger.Debug(json.ToString());
                return json["response"]!.Value<int>() == 1;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        private static async Task<string> GetDialogsJson(IReadOnlyCollection<int> dialogIds)
        {
            var url =
                "https://api.vk.com/method/messages.getConversationsById" +
                "?v=5.92" +
                "&extended=1" +
                "&peer_ids=" + dialogIds.Aggregate(string.Empty, (seed, item) => seed + "," + item).Substring(1) +
                "&access_token=" + Authorization.Token;

            using var client = new ProxiedWebClient();
            var json = await client.DownloadStringTaskAsync(url);
            Logger.Debug(json);
            ExceptionHelpers.ThrowIfInvalidSession(json);
            return json;
        }
    }
}
