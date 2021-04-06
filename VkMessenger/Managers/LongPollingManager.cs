using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Dtos;
using ru.MaxKuzmin.VkMessenger.Exceptions;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Localization;
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
        private string? Key;
        private string? Server;
        private int? Ts;
        private readonly TimeSpan LongPoolingRequestInterval = TimeSpan.FromSeconds(2);
        private readonly TimeSpan LongPoolingStartInterval = TimeSpan.FromSeconds(1);
        private readonly TimeSpan LongPoolingStopInterval = TimeSpan.FromSeconds(10);
        private const string CanceledException = "canceled";

        private CancellationTokenSource? startingTokenSource, stoppingTokenSource, startedTokenSource;
        private Status status = Status.Stopped;
        private bool isInitialized;
        private bool isWarningPopupOpened;

        enum Status
        {
            Starting,
            Stopping,
            Started,
            Stopped
        }

        public INavigation? Navigation { get; set; }

        public LongPollingManager(DialogsManager dialogsManager, MessagesManager messagesManager)
        {
            this.dialogsManager = dialogsManager;
            this.messagesManager = messagesManager;
        }

        public async Task Start()
        {
            if (AuthorizationManager.Token == null)
                return;

            switch (status)
            {
                case Status.Starting:
                    startingTokenSource?.Cancel();
                    break;
                case Status.Started:
                    return;
                case Status.Stopped:
                    break;
                case Status.Stopping:
                    stoppingTokenSource?.Cancel();
                    return;
            }

            try
            {
                Logger.Info("Long polling start requested");

                startingTokenSource = new CancellationTokenSource();
                var token = startingTokenSource.Token;
                status = Status.Starting;
                await Task.Delay(LongPoolingStartInterval, token);
            }
            catch
            {
                return;
            }

            try
            {
                Logger.Info("Long polling started");

                startedTokenSource?.Cancel();
                startedTokenSource = new CancellationTokenSource();
                var token = startedTokenSource.Token;
                status = Status.Started;
                await MainLoop(token);
            }
            catch
            {
                // ignored
            }

            Logger.Info("Long polling stopped");
        }

        public async Task Stop()
        {
            switch (status)
            {
                case Status.Starting:
                    startingTokenSource?.Cancel();
                    return;
                case Status.Stopped:
                    return;
                case Status.Started:
                    break;
                case Status.Stopping:
                    stoppingTokenSource?.Cancel();
                    break;
            }

            try
            {
                Logger.Info("Long polling stop requested");

                stoppingTokenSource = new CancellationTokenSource();
                var token = stoppingTokenSource.Token;
                status = Status.Stopping;
                await Task.Delay(LongPoolingStopInterval, token);
            }
            catch
            {
                return;
            }

            startedTokenSource?.Cancel();
            status = Status.Stopped;
        }

        /// <summary>
        /// <see cref="LongPollingManager"/> main loop
        /// </summary>
        private async Task MainLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (AuthorizationManager.Token != null)
                {
                    try
                    {
                        if (isInitialized)
                        {
                            var json = await LongPollingClient.SendLongRequest(Server!, Key!, Ts!.Value, cancellationToken);
                            if (json.failed != null)
                            {
                                isInitialized = false;
                                isWarningPopupOpened = false;
                            }
                            else
                                await ParseLongPollingJson(json);
                        }
                        else
                        {
                            // It's important to get params before reset pages to stay synced with it's content
                            await GetServer(cancellationToken);
                            await Reset();
                            isInitialized = true;
                        }
                    }
                    // Show warning popup if GetServer failed (not initialized), if back button on popup pressed,
                    // it will be dismissed until next initialization
                    catch (Exception e) when(e is HttpRequestException || e is WebException || e is EmptyHttpResponseException)
                    {
                        if (!isInitialized && !isWarningPopupOpened)
                        {
                            isWarningPopupOpened = true;
                            new CustomPopup(
                                    LocalizedStrings.DialogsNoInternetError,
                                    LocalizedStrings.Retry,
                                    () => isWarningPopupOpened = false,
                                    true)
                                .Show();
                        }
                    }
                    catch (Exception e)
                    {
                        if (!e.Message.Contains(CanceledException))
                            Logger.Error(e);
                    }
                }

                await Task.Delay(LongPoolingRequestInterval, cancellationToken);
            }
        }

        private async Task GetServer(CancellationToken cancellationToken)
        {
            var response = await LongPollingClient.GetLongPollServer(cancellationToken);
            Key = response.key;
            Server = response.server;
            Ts = response.ts;
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
                                var deleted = (update[2].Value<int>() & 131072) != 0;
                                var updateData = new MessageUpdatesData
                                {
                                    MessageId = update[1].Value<int>(),
                                    DialogId = update[3].Value<int>()
                                };
                                if (deleted)
                                    parsedUpdates.MessageDeletionsData.Add(updateData);
                                else
                                    parsedUpdates.MessageUpdatesData.Add(updateData);
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
                        {
                            parsedUpdates.UpdatedDialogIds.Add(update[1].Value<int>());
                            break;
                        }
                    case 52:
                        {
                            parsedUpdates.UpdatedDialogIds.Add(update[2].Value<int>());
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
                Logger.Info($"Long pooling dialogs update {parsedUpdates.UpdatedDialogIds.ToJson()}");

                await HandleDialogUpdates(parsedUpdates.UpdatedDialogIds);
            }

            if (parsedUpdates.MessageUpdatesData.Any())
            {
                Logger.Info($"Long pooling messages update {parsedUpdates.MessageUpdatesData.Select(e => e.MessageId).ToJson()}");

                await HandleMessageUpdates(parsedUpdates.MessageUpdatesData);
            }

            if (parsedUpdates.MessageDeletionsData.Any())
            {
                Logger.Info($"Long pooling messages deletion {parsedUpdates.MessageDeletionsData.Select(e => e.MessageId).ToJson()}");

                HandleMessageDeletions(parsedUpdates.MessageDeletionsData);
            }

            if (parsedUpdates.UserStatusUpdatesData.Any())
            {
                Logger.Info($"Long pooling online status changed for users {parsedUpdates.UserStatusUpdatesData.Select(e => e.UserId).ToJson()}");

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

            dialogsManager.ReorderDialogs(updates.Select(e => e.DialogId).Distinct().ToArray());

            new Feedback().Play(FeedbackType.Vibration, "Tap");
        }

        private void HandleMessageDeletions(ISet<MessageUpdatesData> deletions)
        {
            var groups = deletions.GroupBy(e => e.DialogId).ToArray();
            foreach (var group in groups)
            {
                var messageIds = group.Select(e => e.MessageId).Distinct().ToArray();
                messagesManager.DeleteMessagesFromCollectionOnly(group.Key, messageIds);
            }

            dialogsManager.ReorderDialogs(deletions.Select(e => e.DialogId).Distinct().ToArray());

            new Feedback().Play(FeedbackType.Vibration, "Tap");
        }

        private void HandleUserStatusUpdates(ISet<UserStatusUpdatesData> updates)
        {
            dialogsManager.SetDialogsOnline(updates.Select(e => (e.UserId, e.Status)).ToArray());
        }

        private async Task HandleDialogUpdates(ISet<int> dialogIds)
        {
            await dialogsManager.UpdateDialogsFromApiByIds(dialogIds.ToArray());
            new Feedback().Play(FeedbackType.Vibration, "Tap");
        }

        private async Task Reset()
        {
            Logger.Info("Long pooling reset");

            if (Navigation == null)
                return;

            dialogsManager.SetAllDialogsInitRequired();

            foreach (var page in Navigation.NavigationStack.OfType<IResettable>())
            {
                await page.Reset();
            }
        }
    }
}
