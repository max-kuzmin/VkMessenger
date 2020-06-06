﻿using ru.MaxKuzmin.VkMessenger.Extensions;
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
            IReadOnlyCollection<Message> lastMessages)
        {
            return dialogs
                .Select(item => FromDto(item, profiles, groups, lastMessages))
                .ToList();
        }

        private static Dialog FromDto(
            DialogDto dialog,
            IReadOnlyCollection<Profile> profiles,
            IReadOnlyCollection<Group> groups,
            IReadOnlyCollection<Message> lastMessages)
        {
            var conversation = dialog.conversation;
            var peerId = Math.Abs(conversation.peer.id); // dialogs with groups have negative ids
            var dialogType = Enum.Parse<DialogType>(conversation.peer.type, true);
            var unreadCount = conversation.unread_count ?? 0;

            var lastMessage = new[] {
                dialog.last_message != null
                    ? MessagesClient.FromDto(dialog.last_message, profiles, groups)
                    : lastMessages.First(e => e.Id == conversation.last_message_id)
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
                        var chatSettings = conversation.chat_settings!;
                        var chat = new Chat
                        {
                            Title = chatSettings.title,
                            Id = peerId,
                            Photo = chatSettings.photo != null
                                ? ImageSource.FromUri(chatSettings.photo.photo_50)
                                : null
                        };

                        var dialogProfiles = chatSettings.active_ids
                            .Select(id => profiles.First(p => p.Id == id))
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

                var json = JsonConvert.DeserializeObject<JsonDto<DialogsResponseDto>>(dialogIds == null
                    ? await GetDialogsJson()
                    : await GetDialogsJson(dialogIds));

                var response = json.response;
                var responseItems = response.items;

                var profiles = ProfilesClient.FromDtoArray(response.profiles);
                var groups = GroupsClient.FromDtoArray(response.groups);

                var lastMessagesIds = responseItems
                    .Where(e => e.last_message == null && e.conversation.last_message_id.HasValue)
                    .Select(e => e.conversation.last_message_id!.Value).ToList();

                var lastMessages = lastMessagesIds.Any()
                    ? await MessagesClient.GetMessages(0, 0, lastMessagesIds)
                    : Array.Empty<Message>();

                return FromDtoArray(responseItems, profiles, groups, lastMessages);
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
                var json = JsonConvert.DeserializeObject<JsonDto<int>>(await client.DownloadStringTaskAsync(url));
                Logger.Debug(json.ToString());
                return json.response == 1;
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
