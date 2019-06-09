using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.exceptions;
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

        private static bool isStarted = false;
        private static readonly Mutex mutex = new Mutex();

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
                LongPolling.Ts = LongPolling.Ts ?? json["response"]["ts"].Value<uint>();
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

                if (json == null)
                {
                    throw new LongPollingServerException();
                }

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

                if (messageEventArgs.Data.Any()) OnMessageUpdate?.Invoke(null, messageEventArgs);
                if (dialogEventArgs.DialogIds.Any()) OnDialogUpdate?.Invoke(null, dialogEventArgs);
                if (userStatusEventArgs.Data.Any()) OnUserStatusUpdate?.Invoke(null, userStatusEventArgs);
            }
        }

        /// <summary>
        /// <see cref="LongPolling"/> main loop
        /// </summary>
        public async static void Start()
        {
            if (isStarted || Authorization.Token == null)
                return;
            else isStarted = true;

            mutex.WaitOne();
            Logger.Info("Long polling started");

            while (isStarted)
            {
                try
                {
                    if (LongPolling.Key == null)
                        await GetLongPollServer();
                    else
                        await SendLongRequest();
                }
                catch (LongPollingServerException e)
                {
                    LongPolling.Key = null;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

                await Task.Delay(LongPolling.RequestInterval);
            }

            mutex.ReleaseMutex();
            Logger.Info("Long polling stopped");
        }

        public static void Stop()
        {
            Logger.Info("Requested long polling stop");
            isStarted = false;
        }
    }
}
