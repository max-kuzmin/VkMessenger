using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Models;
using ru.MaxKuzmin.VkMessenger.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var peerId = Math.Abs(conversation.peer.id); // id in peerDto can be negative
            var dialogType = Enum.Parse<DialogType>(conversation.peer.type, true);
            var unreadCount = conversation.unread_count ?? 0;

            var lastMessage = dialog.last_message != null
                ? MessagesClient.FromDto(dialog.last_message, profiles, groups)
                : lastMessages?.SingleOrDefault(e => e.Id == conversation.last_message_id);
            var messages = lastMessage != null
                ? new[] { lastMessage }
                : null;

            if (lastMessage != null && unreadCount == 0)
            {
                lastMessage.SetRead(true);
            }

            Dialog result = default!;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (dialogType)
            {
                case DialogType.User:
                    {
                        var dialogProfiles = profiles.Where(p => p.Id == peerId).ToArray();
                        result = new Dialog(dialogType, null, null, unreadCount, dialogProfiles, messages);
                        break;
                    }
                case DialogType.Group:
                    {
                        var group = groups.FirstOrDefault(g => g.Id == peerId);
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
                                : (chatSettings.photo?.photo_100 != null
                                    ? ImageSource.FromUri(chatSettings.photo.photo_100)
                                    : null)
                        };

                        if (chatSettings.photo != null && chatSettings.photo.photo_50 == null && chatSettings.photo.photo_100 == null)
                            Logger.Error("chatSettings.photo.photo_50 and 100 is not found. Dialog:" + dialog.ToJson());

                        var dialogProfiles = profiles
                            .Where(p => chatSettings.active_ids?.Any(i => Math.Abs(i) == p.Id) == true)
                            .ToArray();

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
#if DEBUG
                Logger.Info("Updating dialogs");
#endif

                var json = await HttpHelpers.RetryIfEmptyResponse<JsonDto<DialogsResponseDto>>(
                    GetDialogsJson, e => e?.response != null);

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
#if DEBUG
                Logger.Info($"Updating dialogs {dialogIds.ToJson()}");
#endif

                var json = await HttpHelpers.RetryIfEmptyResponse<JsonDto<DialogsByIdsResponseDto>>(
                    () => GetDialogsJsonByIds(dialogIds), e => e?.response != null);
                
                var response = json.response;
                var responseItems = response.items;

                var profiles = ProfilesClient.FromDtoArray(response.profiles);
                var groups = GroupsClient.FromDtoArray(response.groups);

                var lastMessagesIds = responseItems
                    .Where(e => e.last_message_id.HasValue)
                    .Select(e => e.last_message_id!.Value).ToList();

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
                "?v=5.124" +
                "&extended=1" +
                "&access_token=" + Authorization.Token;

            using var client = new ProxiedWebClient();
            var json = await client.DownloadStringTaskAsync(url);
#if DEBUG
            Logger.Debug(json);
#endif
            ExceptionHelpers.ThrowIfInvalidSession(json);
            return json;
        }

        public static async Task<bool> MarkAsRead(int dialogId)
        {
            try
            {
                var url =
                    "https://api.vk.com/method/messages.markAsRead" +
                    "?v=5.124" +
                    "&peer_id=" + dialogId +
                    "&access_token=" + Authorization.Token;

                using var client = new ProxiedWebClient();
                var json = await HttpHelpers.RetryIfEmptyResponse<JsonDto<int>>(
                    () => client.DownloadStringTaskAsync(url), e => e?.response != null);
#if DEBUG
                Logger.Debug(json.ToString());
#endif
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
                "?v=5.124" +
                "&extended=1" +
                "&peer_ids=" + dialogIds.Aggregate(string.Empty, (seed, item) => seed + "," + item).Substring(1) +
                "&access_token=" + Authorization.Token;

            using var client = new ProxiedWebClient();
            var json = await client.DownloadStringTaskAsync(url);
#if DEBUG
            Logger.Debug(json);
#endif
            ExceptionHelpers.ThrowIfInvalidSession(json);
            return json;
        }
    }
}
