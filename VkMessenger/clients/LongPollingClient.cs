using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class LongPollingClient
    {
        public static event EventHandler<MessageEventArgs> OnMessageUpdate;

        public static event EventHandler<DialogEventArgs> OnDialogUpdate;

        public static event EventHandler<UserStatusEventArgs> OnUserStatusUpdate;

        public static event EventHandler OnFullReset;

        private static TimeSpan currentRequestInterval = LongPolling.RequestInterval;
        private static Timer timer = new Timer(new TimerCallback(
            async (obj) => await MainLoop()),
            null,
            TimeSpan.FromMilliseconds(-1),
            TimeSpan.FromMilliseconds(-1));

        private async static Task GetLongPollServer()
        {
            var url =
                "https://api.vk.com/method/messages.getLongPollServer" +
                "?v=5.92" +
                "&lp_version=3" +
                "&access_token=" + Authorization.Token;

            using (var client = new ProxiedWebClient())
            {
                var json = JObject.Parse(await client.DownloadStringTaskAsync(url));
                Logger.Debug(json.ToString());

                LongPolling.Key = json["response"]["key"].Value<string>();
                LongPolling.Server = json["response"]["server"].Value<string>();
                LongPolling.Ts = LongPolling.Ts ?? json["response"]["ts"].Value<uint>();
            }
        }

        private async static Task<JObject> SendLongRequest()
        {
            using (var client = new ProxiedWebClient())
            {
                var url = "https://" + LongPolling.Server +
                    "?act=a_check" +
                    "&key=" + LongPolling.Key +
                    "&ts=" + LongPolling.Ts +
                    "&wait=" + LongPolling.WaitTime +
                    "&version=3";

                var json = JObject.Parse(await client.DownloadStringTaskAsync(url));
                Logger.Debug(json.ToString());

                return json;
            }
        }

        private static void ParseLongPollingJson(JObject json)
        {
            LongPolling.Ts = json["ts"].Value<uint>();

            var messageEventArgs = new MessageEventArgs();
            var dialogEventArgs = new DialogEventArgs();
            var userStatusEventArgs = new UserStatusEventArgs();

            foreach (JArray update in json["updates"] as JArray)
            {
                switch (update[0].Value<uint>())
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        {
                            messageEventArgs.Data.Add((update[1].Value<uint>(), update[3].Value<int>()));
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
                            userStatusEventArgs.Data.Add(((uint)update[1].Value<int>(), true));
                            break;
                        }
                    case 9:
                        {
                            userStatusEventArgs.Data.Add(((uint)update[1].Value<int>(), false));
                            break;
                        }
                    default:
                        break;
                }
            }

            if (messageEventArgs.Data.Any())
            {
                Logger.Info("Messages update: " +
                    JsonConvert.SerializeObject(messageEventArgs.Data.Select(i => i.MessageId)));
                OnMessageUpdate?.Invoke(null, messageEventArgs);
            }
            if (dialogEventArgs.DialogIds.Any())
            {
                Logger.Info("Dialogs update: " +
                    JsonConvert.SerializeObject(dialogEventArgs.DialogIds));
                OnDialogUpdate?.Invoke(null, dialogEventArgs);
            }
            if (userStatusEventArgs.Data.Any())
            {
                Logger.Info("Online status changed for users: " +
                    JsonConvert.SerializeObject(userStatusEventArgs.Data.Select(i => i.UserId)));
                OnUserStatusUpdate?.Invoke(null, userStatusEventArgs);
            }
        }

        public static void Start()
        {
            Logger.Info("Long polling started");
            currentRequestInterval = LongPolling.RequestInterval;
            timer.Change(LongPolling.RequestInterval, TimeSpan.FromMilliseconds(-1));
        }

        public static void Stop()
        {
            Logger.Info("Long polling stopped");
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
                    if (LongPolling.Ts == null)
                    {
                        await GetLongPollServer();
                    }

                    var json = await SendLongRequest();

                    if (json.ContainsKey("failed"))
                    {
                        LongPolling.Ts = null;
                        OnFullReset(null, null);
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
    }
}
