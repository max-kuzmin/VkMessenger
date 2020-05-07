using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Models;
using ru.MaxKuzmin.VkMessenger.pages;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class MessagesPage : CirclePage
    {
        private readonly StackLayout verticalLayout = new StackLayout();
        private readonly Dialog dialog;
        private bool shouldScroll;

        private readonly CircleListView messagesListView = new CircleListView
        {
            HorizontalOptions = LayoutOptions.StartAndExpand,
            ItemTemplate = new DataTemplate(typeof(MessageCell)),
            HasUnevenRows = true
        };

        private readonly PopupEntry popupEntryView = new PopupEntry
        {
            VerticalOptions = LayoutOptions.End,
            Placeholder = "Type here...",
            HorizontalTextAlignment = TextAlignment.Center,
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            TextColor = Color.White,
            PlaceholderColor = Color.Gray,
            HeightRequest = 50
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

            Appearing += UpdateAll;
        }

        /// <summary>
        /// Called on start. If update unsuccessful show error popup and retry
        /// </summary>
        private async void UpdateAll(object s, EventArgs e)
        {
            Appearing -= UpdateAll;

            var refreshingPopup = new InformationPopup { Text = "Loading messages..." };
            refreshingPopup.Show();

            if (await dialog.Messages.Update(dialog.Id))
            {
                messagesListView.ScrollIfExist(dialog.Messages.LastOrDefault(), ScrollToPosition.Center);

                messagesListView.ItemTapped += OnItemTapped;
                messagesListView.ItemAppearing += LoadMoreMessages;
                popupEntryView.Completed += OnTextCompleted;
                LongPollingClient.OnMessageUpdate += OnMessageUpdate;
            }
            else
            {
                new RetryInformationPopup(
                    "Can't load messages. No internet connection",
                    () => UpdateAll(null, null))
                    .Show();
            }

            refreshingPopup.Dismiss();
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
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
                // Temporary "load more" event
                messagesListView.ItemAppearing -= LoadMoreMessages;

                await dialog.Messages.Update(dialog.Id, (uint)dialog.Messages.Count);
                messagesListView.ScrollIfExist(message, ScrollToPosition.MakeVisible);

                // To prevent event activation
                Task.Run(async () =>
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
            var items = args.Data.Where(e => e.DialogId == dialog.Id).Select(e => e.MessageId).ToArray();

            if (items.Any())
            {
                await dialog.Messages.Update(dialog.Id, messagesIds: items);
                new Feedback().Play(FeedbackType.Vibration, "Tap");
            }
        }

        /// <summary>
        /// Send message, mark all as read
        /// </summary>
        private async void OnTextCompleted(object sender, EventArgs args)
        {
            await dialog.SetReadWithMessagesAndPublish();

            var text = popupEntryView.Text;
            if (!string.IsNullOrEmpty(text))
            {
                if (await MessagesClient.Send(text, dialog.Id))
                {
                    popupEntryView.Text = string.Empty;
                }
                else
                {
                    popupEntryView.Text = text;
                    new RetryInformationPopup(
                        "Message wasn't send",
                        () => OnTextCompleted(null, null))
                        .Show();
                }
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

        protected override async void OnAppearing()
        {
            if (shouldScroll)
            {
                messagesListView.ScrollIfExist(dialog.Messages.LastOrDefault(), ScrollToPosition.Center);
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
    }
}
