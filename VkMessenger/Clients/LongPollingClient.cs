using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Models;
using ru.MaxKuzmin.VkMessenger.Net;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Dtos;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class LongPollingClient
    {
        public static event EventHandler<MessageEventArgs>? OnMessageUpdate;

        public static event EventHandler<DialogEventArgs>? OnDialogUpdate;

        public static event EventHandler<UserStatusEventArgs>? OnUserStatusUpdate;

        public static event EventHandler? OnFullReset;

        private static TimeSpan currentRequestInterval = LongPolling.RequestInterval;
        private static readonly Timer timer = new Timer(
            async obj => await MainLoop(),
            null,
            TimeSpan.FromMilliseconds(-1),
            TimeSpan.FromMilliseconds(-1));

        private static async Task GetLongPollServer()
        {
            var url =
                "https://api.vk.com/method/messages.getLongPollServer" +
                "?v=5.124" +
                "&lp_version=3" +
                "&access_token=" + Authorization.Token;

            using var client = new ProxiedWebClient();
            var json = await HttpHelpers.RetryIfEmptyResponse<JsonDto<LongPollingInitResponseDto>>(
                () => client.DownloadStringTaskAsync(url), e => e?.response != null);
#if DEBUG
            Logger.Debug(json.ToString());
#endif

            var response = json.response;

            if (response?.key == null || response.server == null || response.ts == 0)
                Logger.Error("One of LongPollingInitResponseDto props is null. Response: " + response.ToJson());

            LongPolling.Key = response.key;
            LongPolling.Server = response.server;
            LongPolling.Ts ??= response.ts;
        }

        private static async Task<LongPollingUpdatesJsonDto> SendLongRequest()
        {
            using var client = new ProxiedWebClient();
            var url = "https://" + LongPolling.Server +
                "?act=a_check" +
                "&key=" + LongPolling.Key +
                "&ts=" + LongPolling.Ts +
                "&wait=" + LongPolling.WaitTime +
                "&version=3";

            var json = await HttpHelpers.RetryIfEmptyResponse<LongPollingUpdatesJsonDto>(
                () => client.DownloadStringTaskAsync(url), e => e != null);
#if DEBUG
            Logger.Debug(json.ToString());
#endif

            return json;
        }

        private static void ParseLongPollingJson(LongPollingUpdatesJsonDto json)
        {
            LongPolling.Ts = json.ts;

            var messageEventArgs = new MessageEventArgs();
            var dialogEventArgs = new DialogEventArgs();
            var userStatusEventArgs = new UserStatusEventArgs();

            var updates = json.updates?.Where(e => e.Length >= 2) ?? Array.Empty<JToken[]>();
            foreach (var update in updates)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (update[0].Value<int>())
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        {
                            if (update.Length >= 4)
                                messageEventArgs.Data.Add((update[1].Value<int>(), update[3].Value<int>()));
                            break;
                        }
                    case 6:
                    case 7:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 51:
                    case 52:
                        {
                            dialogEventArgs.DialogIds.Add(update[1].Value<int>());
                            break;
                        }
                    case 8:
                        {
                            userStatusEventArgs.Data.Add((Math.Abs(update[1].Value<int>()), true)); // User ids send as negative values
                            break;
                        }
                    case 9:
                        {
                            userStatusEventArgs.Data.Add((Math.Abs(update[1].Value<int>()), false));
                            break;
                        }
                }
            }

            if (messageEventArgs.Data.Any())
            {
#if DEBUG
                Logger.Info("Messages update: " + messageEventArgs.Data.Select(i => i.MessageId).ToJson());
#endif
                Device.BeginInvokeOnMainThread(() =>
                    OnMessageUpdate?.Invoke(null, messageEventArgs));
            }

            if (dialogEventArgs.DialogIds.Any())
            {
#if DEBUG
                Logger.Info("Dialogs update: " + dialogEventArgs.DialogIds.ToJson());
#endif
                Device.BeginInvokeOnMainThread(() =>
                    OnDialogUpdate?.Invoke(null, dialogEventArgs));
            }

            if (userStatusEventArgs.Data.Any())
            {
#if DEBUG
                Logger.Info("Online status changed for users: " + userStatusEventArgs.Data.Select(i => i.UserId).ToJson());
#endif
                Device.BeginInvokeOnMainThread(() =>
                    OnUserStatusUpdate?.Invoke(null, userStatusEventArgs));
            }
        }

        public static async void Start()
        {
            await Task.Delay(500);
#if DEBUG
            Logger.Info("Long polling started");
#endif
            currentRequestInterval = LongPolling.RequestInterval;
            timer.Change(LongPolling.RequestInterval, TimeSpan.FromMilliseconds(-1));
        }

        public static void Stop()
        {
#if DEBUG
            Logger.Info("Long polling stopped");
#endif
            currentRequestInterval = TimeSpan.FromMilliseconds(-1);
        }

        /// <summary>
        /// <see cref="LongPolling"/> main loop
        /// </summary>
        private static async Task MainLoop()
        {
            if (Authorization.Token != null)
            {
                try
                {
                    if (LongPolling.Ts == null || LongPolling.Server == null || LongPolling.Key == null)
                    {
                        await GetLongPollServer();
                    }

                    var json = await SendLongRequest();

                    if (json.failed != null)
                    {
                        Reset();
                    }
                    else
                    {
                        ParseLongPollingJson(json);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }

            timer.Change(currentRequestInterval, TimeSpan.FromMilliseconds(-1));
        }

        private static void Reset()
        {
            LongPolling.Ts = null;
            OnMessageUpdate = null;
            OnDialogUpdate = null;
            OnUserStatusUpdate = null;

            Device.BeginInvokeOnMainThread(() =>
                OnFullReset?.Invoke(null, null));
        }
    }
}
