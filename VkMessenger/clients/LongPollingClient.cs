using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class LongPollingClient
    {
        public static event EventHandler<MessageEventArgs> OnMessageAdd;

        public static event EventHandler<MessageEventArgs> OnMessageUpdate;

        public static event EventHandler<uint> OnDialogUpdate;

        public static event EventHandler<UserStatusEventArgs> OnUserStatusUpdate;

        private static void GetLongPollServer()
        {
            var url =
                "https://api.vk.com/method/messages.getLongPollServer" +
                "?v=5.92" +
                "&lp_version=3" +
                "&access_token=" + Models.Authorization.Token;

            using (var client = new WebClient())
            {
                var json = JObject.Parse(client.DownloadString(url));

                LongPolling.Key = json["response"]["key"].Value<string>();
                LongPolling.Server = json["response"]["server"].Value<string>();
                LongPolling.Ts = json["response"]["ts"].Value<uint>();
            }
        }

        private static void SendLongRequest()
        {
            var url = "https://" + LongPolling.Server +
                "?act=a_check" +
                "&key=" + LongPolling.Key +
                "&ts=" + LongPolling.Ts +
                "&wait=" + LongPolling.WaitTime +
                "&version=3";

            using (var client = new WebClient())
            {
                var json = JObject.Parse(client.DownloadString(url));

                LongPolling.Ts = json["ts"].Value<uint>();

                foreach (JArray update in json["updates"] as JArray)
                {
                    switch (update[0].Value<uint>())
                    {
                        case 4:
                            OnMessageAdd?.Invoke(null,
                                new MessageEventArgs { MessageId = update[1].Value<uint>(), DialogId = update[3].Value<uint>() });
                            break;
                        case 1:
                        case 2:
                        case 3:
                        case 5:
                            OnMessageUpdate?.Invoke(null,
                                new MessageEventArgs { MessageId = update[1].Value<uint>(), DialogId = update[3].Value<uint>() });
                            break;
                        case 6:
                        case 7:
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                            OnDialogUpdate?.Invoke(null, update[1].Value<uint>());
                            break;
                        case 8:
                            OnUserStatusUpdate?.Invoke(null,
                                new UserStatusEventArgs { UserId = update[1].Value<uint>(), Online = true });
                            break;
                        case 9:
                            OnUserStatusUpdate?.Invoke(null,
                                new UserStatusEventArgs { UserId = update[1].Value<uint>(), Online = false });
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public static void Start()
        {
            if (LongPolling.Started) return;
            LongPolling.Started = true;

            Task.Run(() =>
            {
                while (LongPolling.Started)
                {
                    if (Models.Authorization.Token != null)
                    {
                        if (LongPolling.Key == null)
                        {
                            GetLongPollServer();
                            Task.Delay(10000);
                        }
                        else
                        {
                            SendLongRequest();
                        }
                    }
                }
            });
        }

        public static void Stop()
        {
            LongPolling.Started = false;
        }
    }
}
