using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Models;
using ru.MaxKuzmin.VkMessenger.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ru.MaxKuzmin.VkMessenger.Dtos;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class DialogsClient
    {
        private static IReadOnlyCollection<Dialog> FromDtoArray(
            DialogDto[] dialogs,
            IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups,
            IReadOnlyCollection<Message>? lastMessages)
        {
            return dialogs
                .Select(item => FromDto(item, profiles, groups, lastMessages))
                .ToList();
        }

        private static Dialog FromDto(
            DialogDto dialog,
            IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups,
            IReadOnlyCollection<Message>? lastMessages)
        {
            var conversation = dialog.conversation;
            var peerId = Math.Abs(conversation.peer.id); // dialogs with groups have negative ids
            var dialogType = Enum.Parse<DialogType>(conversation.peer.type, true);
            var unreadCount = conversation.unread_count ?? 0;

            var lastMessage = dialog.last_message != null
                ? MessagesClient.FromDto(dialog.last_message, profiles, groups)
                : lastMessages?.SingleOrDefault(e => e.Id == conversation.last_message_id);
            var messages = lastMessage != null
                ? new[] { lastMessage }
                : null;

            Dialog result = default!;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (dialogType)
            {
                case DialogType.User:
                    {
                        var dialogProfiles = profiles.Where(p => Math.Abs(p.Id) == peerId).ToArray(); // Compare absolute values to prevent null ex
                        if (dialogProfiles.Any(e => e.Id < 0))
                            Logger.Error("DialogType.User parse error, negative profile id. Dialog:" + dialog.ToJson());
                        if (!dialogProfiles.Any())
                            Logger.Error("DialogType.User parse error, profile not found. Dialog:" + dialog.ToJson());
                        
                        result = new Dialog(dialogType, null, null, unreadCount, dialogProfiles, messages);
                        break;
                    }
                case DialogType.Group:
                    {
                        var group = groups.FirstOrDefault(g => Math.Abs(g.Id) == peerId); // Compare absolute values to prevent null ex
                        if (group?.Id < 0)
                            Logger.Error("DialogType.Group parse error, negative group id. Dialog:" + dialog.ToJson());
                        if (group == null)
                            Logger.Error("DialogType.Group parse error, group not found. Dialog:" + dialog.ToJson());

                        result = new Dialog(dialogType, group, null, unreadCount, null, messages);
                        break;
                    }
                case DialogType.Chat:
                    {
                        var chatSettings = conversation.chat_settings!;
                        var chat = new Chat
                        {
                            Title = chatSettings.title,
                            Id = peerId,
                            Photo = chatSettings.photo?.photo_50 != null
                                ? ImageSource.FromUri(chatSettings.photo.photo_50)
                                : null
                        };

                        if (chatSettings.photo != null && chatSettings.photo.photo_50 == null)
                            Logger.Error("chatSettings.photo.photo_50 is not found. Dialog:" + dialog.ToJson());

                        var dialogProfiles = profiles
                            .Where(p => chatSettings.active_ids?.Any(i => Math.Abs(i) == Math.Abs(p.Id)) == true) // Compare absolute values to prevent null ex
                            .ToArray();

                        if (dialogProfiles.Any(e => e.Id < 0))
                            Logger.Error("DialogType.Chat parse error, negative profile id. Dialog:" + dialog.ToJson());
                        if (chatSettings.active_ids?.Any(e => e < 0) == true)
                            Logger.Error("DialogType.Chat parse error, negative chat id. Dialog:" + dialog.ToJson());
                        if (!dialogProfiles.Any())
                            Logger.Error("DialogType.Chat parse error, profile not found. Dialog:" + dialog.ToJson());

                        result = new Dialog(dialogType, null, chat, unreadCount, dialogProfiles, messages);
                        break;
                    }
            }

            return result;
        }

        public static async Task<IReadOnlyCollection<Dialog>> GetDialogs()
        {
            try
            {
                Logger.Info("Updating dialogs");

                var json = JsonConvert.DeserializeObject<JsonDto<DialogsResponseDto>>(await GetDialogsJson());

                var response = json.response;
                var responseItems = response.items;

                var profiles = ProfilesClient.FromDtoArray(response.profiles);
                var groups = GroupsClient.FromDtoArray(response.groups);

                return FromDtoArray(responseItems, profiles, groups, null);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        public static async Task<IReadOnlyCollection<Dialog>> GetDialogsByIds(IReadOnlyCollection<int> dialogIds)
        {
            try
            {
                Logger.Info($"Updating dialogs {dialogIds.ToJson()}");

                var json = await HttpHelpers.RetryIfEmptyResponse<JsonDto<DialogsByIdsResponseDto>>(
                    () => GetDialogsJsonByIds(dialogIds), e => e?.response != null);
                
                var response = json.response;
                var responseItems = response.items;

                var profiles = ProfilesClient.FromDtoArray(response.profiles);
                var groups = GroupsClient.FromDtoArray(response.groups);

                var lastMessagesIds = responseItems
                    .Where(e => e.last_message_id.HasValue)
                    .Select(e => e.last_message_id.Value).ToList();

                var lastMessages = lastMessagesIds.Any()
                    ? await MessagesClient.GetMessagesByIds(lastMessagesIds)
                    : null;

                var dialogs = responseItems
                    .Select(e => new DialogDto { conversation = e })
                    .ToArray();

                return FromDtoArray(dialogs, profiles, groups, lastMessages);
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
                var json = await HttpHelpers.RetryIfEmptyResponse<JsonDto<int>>(
                    () => client.DownloadStringTaskAsync(url), e => e?.response != null);
                Logger.Debug(json.ToString());
                return json.response == 1;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        private static async Task<string> GetDialogsJsonByIds(IReadOnlyCollection<int> dialogIds)
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
