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
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class MessagesPage : CirclePage, IDisposable
    {
        private readonly StackLayout verticalLayout = new StackLayout();
        private readonly Dialog dialog;
        private bool shouldScroll;
        private bool firstTapPerformed;

        private readonly SwipeGestureRecognizer swipeRecognizer = new SwipeGestureRecognizer
        {
            Direction = SwipeDirection.Right | SwipeDirection.Left
        };

        private readonly CircleListView messagesListView = new CircleListView
        {
            ItemTemplate = new DataTemplate(typeof(MessageCell)),
            HasUnevenRows = true,
            Rotation = 180,
            BarColor = Color.Transparent
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

            swipeRecognizer.Command = new Command(OpenKeyboard);
            messagesListView.GestureRecognizers.Add(swipeRecognizer);

            Appearing += UpdateAll;
        }

        /// <summary>
        /// Called on start. If update unsuccessful show error popup and retry
        /// </summary>
        private async void UpdateAll(object? s = null, EventArgs? e = null)
        {
            Appearing -= UpdateAll;

            var refreshingPopup = new InformationPopup { Text = LocalizedStrings.LoadingMessages };
            refreshingPopup.Show();

            try
            {
                await dialog.Messages.Update(dialog.Id);
                messagesListView.ScrollIfExist(dialog.Messages.FirstOrDefault(), ScrollToPosition.Center);

                messagesListView.ItemTapped += OnItemTapped;
                messagesListView.ItemAppearing += LoadMoreMessages;
                popupEntryView.Completed += OnTextCompleted;
                LongPollingClient.OnMessageUpdate += OnMessageUpdate;
            }
            catch (WebException)
            {
                new CustomPopup(
                        LocalizedStrings.MessagesNoInternetError,
                        LocalizedStrings.Retry,
                        () => UpdateAll())
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

            refreshingPopup.Dismiss();
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (!firstTapPerformed)
            {
                firstTapPerformed = true;
                _ = Task.Run(async () =>
                  {
                      await Task.Delay(TimeSpan.FromSeconds(1));
                      firstTapPerformed = false;
                  });
                return;
            }

            await dialog.SetReadWithMessagesAndPublish();

            var message = (Message)e.Item;
            if (message.FullScreenAllowed)
            {
                shouldScroll = false;
                await Navigation.PushAsync(new MessagePage(message));
            }
        }

        /// <summary>
        /// Load more messages when scroll reached the end of the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LoadMoreMessages(object sender, ItemVisibilityEventArgs e)
        {
            var message = (Message)e.Item;
            if (dialog.Messages.Count >= 20 && dialog.Messages.All(i => i.Id >= message.Id))
            {
                // Temporary disable "load more" event
                messagesListView.ItemAppearing -= LoadMoreMessages;

                await dialog.Messages.Update(dialog.Id, (uint)dialog.Messages.Count);

                // To prevent event activation
                _ = Task.Run(async () =>
                  {
                      await Task.Delay(TimeSpan.FromSeconds(0.5));
                      messagesListView.ItemAppearing += LoadMoreMessages;
                  });
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
                await dialog.Messages.Update(dialog.Id, messagesIds: items);
                _ = Task.Run(() => new Feedback().Play(FeedbackType.Vibration, "Tap"));
            }
        }

        /// <summary>
        /// Send message, mark all as read
        /// </summary>
        private async void OnTextCompleted(object? sender = null, EventArgs? args = null)
        {
            await dialog.SetReadWithMessagesAndPublish();

            var text = popupEntryView.Text;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                await MessagesClient.Send(text, dialog.Id);
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

        /// <summary>
        /// Go to previous page
        /// </summary>
        protected override bool OnBackButtonPressed()
        {
            messagesListView.ItemAppearing -= LoadMoreMessages;
            Navigation.PopAsync();
            return base.OnBackButtonPressed();
        }

        protected override void OnAppearing()
        {
            if (shouldScroll)
            {
                messagesListView.ScrollIfExist(dialog.Messages.FirstOrDefault(), ScrollToPosition.Center);
                // To prevent event activation
                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(0.5));
                    messagesListView.ItemAppearing += LoadMoreMessages;
                });
            }
            else shouldScroll = true;

            base.OnAppearing();
        }

        private async void OpenKeyboard()
        {
            popupEntryView.IsPopupOpened = true;

            await dialog.SetReadWithMessagesAndPublish();
        }

        public void Dispose()
        {
            LongPollingClient.OnMessageUpdate -= OnMessageUpdate;
        }
    }
}
