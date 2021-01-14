using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Models;
using System;   
using System.Linq;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Helpers;
using ru.MaxKuzmin.VkMessenger.Layouts;
using ru.MaxKuzmin.VkMessenger.Managers;
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
            swipeRightRecognizer.Command = new Command(OnOpenRecorder);
            messagesListView.GestureRecognizers.Add(swipeLeftRecognizer);
            messagesListView.GestureRecognizers.Add(swipeRightRecognizer);

            messagesListView.ItemTapped += OnItemTapped;
            messagesListView.ItemAppearing += OnLoadMoreMessages;
            popupEntryView.Completed += OnTextCompleted;
            LongPollingClient.OnFullReset += OnFullReset;
            Appearing += OnAppearing;
        }

        private async void OnAppearing(object s, EventArgs e)
        {
            await InitFromApi();
        }

        /// <summary>
        /// Called on start. If update unsuccessful show error popup and retry
        /// </summary>
        private async Task InitFromApi()
        {
            Appearing -= OnAppearing;

            var refreshingPopup = dialog.Messages.Count > 1 ? null : new InformationPopup { Text = LocalizedStrings.LoadingMessages };
            refreshingPopup?.Show();

            await NetExceptionCatchHelpers.CatchNetException(
                async () =>
                {
                    await MessagesManager.UpdateMessagesFromApi(dialog);
                    //Trim to batch size to prevent skipping new messages between cached and 20 loaded on init
                    MessagesManager.TrimMessages(dialog);
                },
                InitFromApi,
                LocalizedStrings.MessagesNoInternetError,
                true);

            refreshingPopup?.Dismiss();
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (popupEntryView.IsPopupOpened)
                return;

            _ = DialogsManager.SetDialogAndMessagesReadAndPublish(dialog);

            var message = (Message)e.Item;
            if (message.FullScreenAllowed)
                await Navigation.PushAsync(new MessagePage(message));
        }

        /// <summary>
        /// Load more messages when scroll reached the end of the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnLoadMoreMessages(object sender, ItemVisibilityEventArgs e)
        {
            var message = (Message)e.Item;
            if (dialog.Messages.Count >= Consts.BatchSize && dialog.Messages.All(i => i.Id >= message.Id))
            {
                await MessagesManager.UpdateMessagesFromApi(dialog, dialog.Messages.Count);
                messagesListView.ScrollIfExist(message, ScrollToPosition.Center);
            }
        }

        /// <summary>
        /// Send message, mark all as read
        /// </summary>
        private async void OnTextCompleted(object? sender = null, EventArgs? args = null)
        {
            _ = DialogsManager.SetDialogAndMessagesReadAndPublish(dialog);

            var text = popupEntryView.Text;
            if (string.IsNullOrEmpty(text))
                return;

            await NetExceptionCatchHelpers.CatchNetException(
                async () =>
                {
                    await MessagesClient.Send(dialog.Id, text, null);
                    popupEntryView.Text = string.Empty;
                },
                () =>
                {
                 OnTextCompleted();
                 return Task.CompletedTask;
                },
                LocalizedStrings.SendMessageNoInternetError,
                false);
        }

        private void OpenKeyboard()
        {
            _ = DialogsManager.SetDialogAndMessagesReadAndPublish(dialog);
            popupEntryView.IsPopupOpened = true;
        }

        private async void OnOpenRecorder()
        {
            _ = DialogsManager.SetDialogAndMessagesReadAndPublish(dialog);
            await Navigation.PushAsync(new RecordVoicePage(dialog));
        }

        public void Dispose()
        {
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
            _ = DialogsManager.SetDialogAndMessagesReadAndPublish(dialog);
            return false;
        }

        private async void OnFullReset(object s, EventArgs e)
        {
            await InitFromApi();
        }
    }
}
