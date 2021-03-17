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
    public class MessagesPage : PageWithActivityIndicator, IResettable
    {
        private readonly int dialogId;
        private readonly MessagesManager messagesManager;
        private readonly DialogsManager dialogsManager;
        private bool newMessageInputShown;
        private readonly TimeSpan newMessageInputShownTimeout = TimeSpan.FromSeconds(2);

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

        public MessagesPage(int dialogId, MessagesManager messagesManager, DialogsManager dialogsManager)
        {
            this.dialogId = dialogId;
            this.messagesManager = messagesManager;
            this.dialogsManager = dialogsManager;

            NavigationPage.SetHasNavigationBar(this, false);
            SetBinding(RotaryFocusObjectProperty, new Binding { Source = messagesListView });
            messagesListView.ItemsSource = messagesManager.GetMessages(dialogId);
            absoluteLayout.Children.Add(messagesListView);
            absoluteLayout.Children.Add(popupEntryView);
            absoluteLayout.Children.Add(activityIndicator);
            Content = absoluteLayout;

            swipeLeftRecognizer.Command = new Command(OpenKeyboard);
            swipeRightRecognizer.Command = new Command(OnOpenRecorder);
            messagesListView.GestureRecognizers.Add(swipeLeftRecognizer);
            messagesListView.GestureRecognizers.Add(swipeRightRecognizer);

            messagesListView.ItemTapped += OnItemTapped;
            messagesListView.ItemLongPressed += OnItemLongPressed;
            messagesListView.ItemAppearing += OnLoadMoreMessages;
            popupEntryView.Completed += OnTextCompleted;

            if (dialogsManager.GetIsInitRequired(this.dialogId))
                Appearing += OnConstructor;
        }

        private async void OnConstructor(object s, EventArgs e)
        {
            Appearing -= OnConstructor;
            await InitFromApi();
        }

        /// <summary>
        /// Called on start. If update unsuccessful show error popup and retry
        /// </summary>
        private async Task InitFromApi()
        {
            activityIndicator.IsVisible = true;

            await NetExceptionCatchHelpers.CatchNetException(
                async () =>
                {
                    await messagesManager.UpdateMessagesFromApi(dialogId);
                    dialogsManager.SetIsInitRequiredToFalse(dialogId);
                },
                InitFromApi,
                LocalizedStrings.MessagesNoInternetError);

            activityIndicator.IsVisible = false;
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (newMessageInputShown)
                return;

            await dialogsManager.SetDialogAndMessagesReadAndPublish(dialogId);

            var message = (Message)e.Item;
            if (message.FullScreenAllowed)
                await Navigation.PushAsync(new MessagePage(message));
        }

        private async void OnItemLongPressed(object sender, ItemLongPressedEventArgs e)
        {
            if (newMessageInputShown)
                return;

            var message = (Message)e.Item;
            
            // Possible to delete current user messages that is not older than 1d
            if (dialogId == AuthorizationManager.UserId
                || message.Profile?.Id != AuthorizationManager.UserId
                || message.Date < DateTime.Now.AddDays(-1)
                || message.Deleted)
                return;

            activityIndicator.IsVisible = true;

            await NetExceptionCatchHelpers.CatchNetException(
                () => messagesManager.DeleteMessage(dialogId, message.Id),
                () =>
                {
                    OnItemLongPressed(sender, e);
                    return Task.CompletedTask;
                },
                LocalizedStrings.DeleteMessageNoInternetError);

            activityIndicator.IsVisible = false;
        }

        /// <summary>
        /// Load more messages when scroll reached the end of the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnLoadMoreMessages(object sender, ItemVisibilityEventArgs e)
        {
            if (dialogsManager.GetIsInitRequired(dialogId))
                return;

            var message = (Message)e.Item;
            var messagesCount = messagesManager.GetMessagesCount(dialogId);
            if (messagesCount >= Consts.BatchSize && messagesManager.IsMessageOlderThanAll(dialogId, message.Id))
            {
                await messagesManager.UpdateMessagesFromApi(dialogId, messagesCount);
                messagesListView.ScrollIfExist(message, ScrollToPosition.Center);
            }
        }

        /// <summary>
        /// Send message, mark all as read
        /// </summary>
        private async void OnTextCompleted(object? sender = null, EventArgs? args = null)
        {
            await dialogsManager.SetDialogAndMessagesReadAndPublish(dialogId);

            var text = popupEntryView.Text;
            if (string.IsNullOrEmpty(text))
                return;

            activityIndicator.IsVisible = true;

            await NetExceptionCatchHelpers.CatchNetException(
                async () =>
                {
                    await MessagesClient.Send(dialogId, text, null);
                    popupEntryView.Text = string.Empty;
                },
                () =>
                {
                 OnTextCompleted();
                 return Task.CompletedTask;
                },
                LocalizedStrings.SendMessageNoInternetError);

            activityIndicator.IsVisible = false;
        }

        private async void OpenKeyboard()
        {
            if (!dialogsManager.CanWrite(dialogId))
                return;

            newMessageInputShown = true;
            await dialogsManager.SetDialogAndMessagesReadAndPublish(dialogId);
            popupEntryView.IsPopupOpened = true;
            await Task.Delay(newMessageInputShownTimeout);
            newMessageInputShown = false;
        }

        private async void OnOpenRecorder()
        {
            if (!dialogsManager.CanWrite(dialogId))
                return;

            newMessageInputShown = true;
            await dialogsManager.SetDialogAndMessagesReadAndPublish(dialogId);
            await Navigation.PushAsync(new RecordVoicePage(dialogId));
            await Task.Delay(newMessageInputShownTimeout);
            newMessageInputShown = false;
        }

        protected override void OnDisappearing()
        {
            AudioLayout.PauseAllPlayers();
            base.OnDisappearing();
        }

        public async Task Reset()
        {
            await InitFromApi();
        }
    }
}
