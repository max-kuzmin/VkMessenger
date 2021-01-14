using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Dtos;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Models;
using ru.MaxKuzmin.VkMessenger.Pages;
using Tizen.System;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Managers
{
    public class LongPollingManager
    {
        private readonly DialogsManager dialogsManager;
        private readonly MessagesManager messagesManager;
        private readonly INavigation navigation;
        private string? Key;
        private string? Server;
        private int? Ts;
        public readonly TimeSpan LongPoolingRequestInterval = TimeSpan.FromSeconds(2);

        public CancellationTokenSource? cancellationTokenSource;

        public LongPollingManager(DialogsManager dialogsManager, MessagesManager messagesManager, INavigation navigation)
        {
            this.dialogsManager = dialogsManager;
            this.messagesManager = messagesManager;
            this.navigation = navigation;
        }


        public async Task Start()
        {
            cancellationTokenSource?.Cancel();
#if DEBUG
            Logger.Info("Long polling started");
#endif
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            while (!token.IsCancellationRequested)
            {
                await MainLoop(token);
            }
#if DEBUG
            Logger.Info("Long polling stopped");
#endif
        }

        public void Stop()
        {
#if DEBUG
            Logger.Info("Long polling stop requested");
#endif
            cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// <see cref="LongPollingManager"/> main loop
        /// </summary>
        private async Task MainLoop(CancellationToken cancellationToken)
        {
            if (AuthorizationManager.Token != null)
            {
                try
                {
                    if (Ts == null || Server == null || Key == null)
                    {
                        var response = await LongPollingClient.GetLongPollServer();
                        Key = response.key;
                        Server = response.server;
                        Ts ??= response.ts;
                    }

                    var json = await LongPollingClient.SendLongRequest(Server, Key, Ts.Value, cancellationToken);

                    if (json.failed != null)
                    {
                        await Reset();
                    }
                    else
                    {
                        await ParseLongPollingJson(json);
                    }
                }
                catch (Exception e) when (e.Message.Contains("The request was canceled")) { }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

                try
                {
                    await Task.Delay(LongPoolingRequestInterval, cancellationToken);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private async Task ParseLongPollingJson(LongPollingUpdatesJsonDto json)
        {
            Ts = json.ts;

            var parsedUpdates = new LongPoolingUpdates();

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
                            {
                                parsedUpdates.MessageUpdatesData.Add(new MessageUpdatesData
                                {
                                    MessageId = update[1].Value<int>(),
                                    DialogId = update[3].Value<int>()
                                });
                                parsedUpdates.UpdatedDialogIds.Add(update[3].Value<int>());
                            }

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
                            parsedUpdates.UpdatedDialogIds.Add(update[1].Value<int>());
                            break;
                        }
                    case 8:
                        {
                            parsedUpdates.UserStatusUpdatesData.Add(new UserStatusUpdatesData
                            {
                                UserId = Math.Abs(update[1].Value<int>()),
                                Status = true
                            }); // User ids send as negative values
                            break;
                        }
                    case 9:
                        {
                            parsedUpdates.UserStatusUpdatesData.Add(new UserStatusUpdatesData
                            {
                                UserId = Math.Abs(update[1].Value<int>()),
                                Status = false
                            }); // User ids send as negative values
                            break;
                        }
                }
            }

            if (parsedUpdates.UpdatedDialogIds.Any())
            {
#if DEBUG
                Logger.Info("Dialogs update: " + parsedUpdates.UpdatedDialogIds.ToJson());
#endif
                var updatedMessagesIds = await HandleDialogUpdates(parsedUpdates.UpdatedDialogIds);

                //Exclude updated messages from further updates
                parsedUpdates.MessageUpdatesData = parsedUpdates.MessageUpdatesData
                    .Where(e => !updatedMessagesIds.Contains(e.MessageId)).ToHashSet();
            }

            if (parsedUpdates.MessageUpdatesData.Any())
            {
#if DEBUG
                Logger.Info("Messages update: " + parsedUpdates.MessageUpdatesData.Select(i => i.MessageId).ToJson());
#endif
                await HandleMessageUpdates(parsedUpdates.MessageUpdatesData);
            }

            if (parsedUpdates.UserStatusUpdatesData.Any())
            {
#if DEBUG
                Logger.Info("Online status changed for users: " + parsedUpdates.UserStatusUpdatesData.Select(i => i.UserId).ToJson());
#endif
                HandleUserStatusUpdates(parsedUpdates.UserStatusUpdatesData);
            }
        }

        private async Task HandleMessageUpdates(ISet<MessageUpdatesData> updates)
        {
            var groups = updates.GroupBy(e => e.DialogId).ToArray();
            foreach (var group in groups)
            {
                var messageIds = group.Select(e => e.MessageId).Distinct().ToArray();
                await messagesManager.UpdateMessagesFromApiByIds(group.Key, messageIds);
            }

            new Feedback().Play(FeedbackType.Vibration, "Tap");
        }

        private void HandleUserStatusUpdates(ISet<UserStatusUpdatesData> updates)
        {
            dialogsManager.SetDialogsOnline(updates.Select(e => (e.UserId, e.Status)).ToArray());
        }

        private async Task<int[]> HandleDialogUpdates(ISet<int> dialogIds)
        {
            var updatedMessagesIds = await dialogsManager.UpdateDialogsFromApiByIds(dialogIds.ToArray());
            new Feedback().Play(FeedbackType.Vibration, "Tap");
            return updatedMessagesIds;
        }

        private async Task Reset()
        {
            Ts = null;

            foreach (var page in navigation.NavigationStack.OfType<IResettable>())
            {
                await page.Reset();
            }
        }
    }
}
