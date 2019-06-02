using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Threading.Tasks;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class LongPollingClient
    {
        public static event EventHandler<MessageEventArgs> OnMessageUpdate;

        public static event EventHandler<int> OnDialogUpdate;

        public static event EventHandler<UserStatusEventArgs> OnUserStatusUpdate;

        public static event EventHandler<UserTypingEventArgs> OnUserTyping;

        private static bool isStarted = false;

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

                LongPolling.Key = json["response"]["key"].Value<string>();
                LongPolling.Server = json["response"]["server"].Value<string>();
                LongPolling.Ts = json["response"]["ts"].Value<uint>();
            }
        }

        private async static Task SendLongRequest()
        {
            var url = "https://" + LongPolling.Server +
                "?act=a_check" +
                "&key=" + LongPolling.Key +
                "&ts=" + LongPolling.Ts +
                "&wait=" + LongPolling.WaitTime +
                "&version=3";

            using (var client = new ProxiedWebClient())
            {
                var json = JObject.Parse(await client.DownloadStringTaskAsync(url));

                LongPolling.Ts = json["ts"].Value<uint>();

                foreach (JArray update in json["updates"] as JArray)
                {
                    switch (update[0].Value<uint>())
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            OnMessageUpdate?.Invoke(null,
                                new MessageEventArgs { MessageId = update[1].Value<uint>(), DialogId = update[3].Value<int>() });
                            break;
                        case 6:
                        case 7:
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                        case 51:
                        case 52:
                            OnDialogUpdate?.Invoke(null, update[1].Value<int>());
                            break;
                        case 8:
                            OnUserStatusUpdate?.Invoke(null,
                                new UserStatusEventArgs { UserId = (uint)update[1].Value<int>(), IsOnline = true });
                            break;
                        case 9:
                            OnUserStatusUpdate?.Invoke(null,
                                new UserStatusEventArgs { UserId = (uint)update[1].Value<int>(), IsOnline = false });
                            break;
                        case 61:
                            OnUserTyping?.Invoke(null,
                                new UserTypingEventArgs { UserId = (uint)update[1].Value<int>(), DialogId = update[1].Value<int>() });
                            break;
                        case 62:
                            OnUserTyping?.Invoke(null,
                                new UserTypingEventArgs { UserId = (uint)update[1].Value<int>(), DialogId = update[2].Value<int>() });
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// <see cref="LongPolling"/> main loop
        /// </summary>
        public async static void Start()
        {
            if ((isStarted && Authorization.Token != null) || !LongPolling.Enabled)
                return;

            Logger.Info("Long polling started");
            isStarted = true;

            while (isStarted)
            {
                try
                {
                    if (LongPolling.Key == null)
                        await GetLongPollServer();
                    else
                        await SendLongRequest();
                    continue;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

                await Task.Delay(LongPolling.DelayAfterError);
            }
        }

        public static void Stop()
        {
            Logger.Info("Long polling stopped");

            isStarted = false;
        }
    }
}
