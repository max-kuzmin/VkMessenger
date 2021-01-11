using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Exceptions;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Models;
using ru.MaxKuzmin.VkMessenger.pages;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Layouts;
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class MessagesPage : BezelInteractionPage, IDisposable
    {
        private readonly StackLayout verticalLayout = new StackLayout();
        private readonly Dialog dialog;

        private readonly SwipeGestureRecognizer swipeLeftRecognizer = new SwipeGestureRecognizer
        {
            Direction = SwipeDirection.Left
        };
        private readonly SwipeGestureRecognizer swipeRightRecognizer = new SwipeGestureRecognizer
        {
            Direction = SwipeDirection.Right
        };

        private readonly CircleListView messagesListView = new CircleListView
        {
            ItemTemplate = new DataTemplate(typeof(MessageCell)),
            HasUnevenRows = true,
            Rotation = 180,
            BarColor = Color.Transparent,
            VerticalScrollBarVisibility = ScrollBarVisibility.Never
        };

        private readonly PopupEntry popupEntryView = new PopupEntry
        {
            IsVisible = false
        };

        public MessagesPage(Dialog dialog)
        {
            this.dialog = dialog;

            NavigationPage.SetHasNavigationBar(this, false);
            SetBinding(RotaryFocusObjectProperty, new Binding { Source = messagesListView });
            messagesListView.ItemsSource = this.dialog.Messages;
            verticalLayout.Children.Add(messagesListView);
            verticalLayout.Children.Add(popupEntryView);
            Content = verticalLayout;

            swipeLeftRecognizer.Command = new Command(OpenKeyboard);
            swipeRightRecognizer.Command = new Command(OpenRecorder);
            messagesListView.GestureRecognizers.Add(swipeLeftRecognizer);
            messagesListView.GestureRecognizers.Add(swipeRightRecognizer);

            Appearing += InitFromApi;
        }

        /// <summary>
        /// Called on start. If update unsuccessful show error popup and retry
        /// </summary>
        private async void InitFromApi(object? s = null, EventArgs? e = null)
        {
            Appearing -= InitFromApi;

            var refreshingPopup = dialog.Messages.Count > 1 ? null : new InformationPopup { Text = LocalizedStrings.LoadingMessages };
            refreshingPopup?.Show();

            try
            {
                await dialog.Messages.Update(dialog.Id, dialog.UnreadCount);
                //Trim to batch size to prevent skipping new messages between cached and 20 loaded on init
                dialog.Messages.Trim(Consts.BatchSize);
                //messagesListView.ScrollIfExist(dialog.Messages.FirstOrDefault(), ScrollToPosition.Center);

                messagesListView.ItemTapped += OnItemTapped;
                messagesListView.ItemAppearing += LoadMoreMessages;
                popupEntryView.Completed += OnTextCompleted;
                LongPollingClient.OnMessageUpdate += OnMessageUpdate;
                LongPollingClient.OnFullReset += OnFullReset;
            }
            catch (WebException)
            {
                new CustomPopup(
                        LocalizedStrings.MessagesNoInternetError,
                        LocalizedStrings.Retry,
                        () => InitFromApi())
                    .Show();
            }
            catch (InvalidSessionException)
            {
                new CustomPopup(
                        LocalizedStrings.InvalidSessionError,
                        LocalizedStrings.Ok,
                        AuthorizationClient.CleanUserAndExit)
                    .Show();
            }
            catch (Exception ex)
            {
                new CustomPopup(
                        ex.ToString(),
                        LocalizedStrings.Ok)
                    .Show();
            }

            refreshingPopup?.Dismiss();
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (popupEntryView.IsPopupOpened)
                return;

            _ = dialog.SetReadWithMessagesAndPublish();

            var message = (Message)e.Item;
            if (message.FullScreenAllowed)
                await Navigation.PushAsync(new MessagePage(message));
        }

        /// <summary>
        /// Load more messages when scroll reached the end of the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LoadMoreMessages(object sender, ItemVisibilityEventArgs e)
        {
            var message = (Message)e.Item;
            if (dialog.Messages.Count >= Consts.BatchSize && dialog.Messages.All(i => i.Id >= message.Id))
            {
                await dialog.Messages.Update(dialog.Id, dialog.Messages.Count);
                messagesListView.ScrollIfExist(message, ScrollToPosition.Center);
            }
        }

        /// <summary>
        /// Update messages collection
        /// </summary>
        private async void OnMessageUpdate(object sender, MessageEventArgs args)
        {
            var items = args.Data
                .Where(e => e.DialogId == dialog.Id)
                .Select(e => e.MessageId)
                .Reverse()
                .ToArray();

            if (items.Any())
            {
                await dialog.Messages.UpdateByIds(items, dialog.Id, dialog.UnreadCount);
                // ReSharper disable once AssignmentIsFullyDiscarded
                _ = Task.Run(() => new Feedback().Play(FeedbackType.Vibration, "Tap"));
            }
        }

        /// <summary>
        /// Send message, mark all as read
        /// </summary>
        private async void OnTextCompleted(object? sender = null, EventArgs? args = null)
        {
            _ = dialog.SetReadWithMessagesAndPublish();

            var text = popupEntryView.Text;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                await MessagesClient.Send(dialog.Id, text, null);
                popupEntryView.Text = string.Empty;
            }
            catch (WebException)
            {
                new CustomPopup(
                        LocalizedStrings.SendMessageNoInternetError,
                        LocalizedStrings.Retry,
                        () => OnTextCompleted())
                    .Show();
            }
            catch (InvalidSessionException)
            {
                new CustomPopup(
                        LocalizedStrings.InvalidSessionError,
                        LocalizedStrings.Ok,
                        AuthorizationClient.CleanUserAndExit)
                    .Show();
            }
            catch (Exception ex)
            {
                new CustomPopup(
                        ex.ToString(),
                        LocalizedStrings.Ok,
                        Application.Current.Quit)
                    .Show();
            }
        }

        private void OpenKeyboard()
        {
            _ = dialog.SetReadWithMessagesAndPublish();
            popupEntryView.IsPopupOpened = true;
        }

        private async void OpenRecorder()
        {
            _ = dialog.SetReadWithMessagesAndPublish();
            await Navigation.PushAsync(new RecordVoicePage(dialog));
        }

        public void Dispose()
        {
            messagesListView.ItemTapped -= OnItemTapped;
            messagesListView.ItemAppearing -= LoadMoreMessages;
            popupEntryView.Completed -= OnTextCompleted;
            LongPollingClient.OnMessageUpdate -= OnMessageUpdate;
            LongPollingClient.OnFullReset -= OnFullReset;
        }

        protected override void OnDisappearing()
        {
            AudioLayout.PauseAllPlayers();
            base.OnDisappearing();
        }

        /// <inheritdoc />
        protected override bool OnBackButtonPressed()
        {
            _ = dialog.SetReadWithMessagesAndPublish();
            return false;
        }

        private void OnFullReset(object s, EventArgs e)
        {
            Dispose();
            if (Navigation.NavigationStack.Contains(this))
                InitFromApi();
        }
    }
}
