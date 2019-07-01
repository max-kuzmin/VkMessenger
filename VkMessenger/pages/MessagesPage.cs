using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Models;
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

        private readonly CircleListView messagesListView = new CircleListView
        {
            HorizontalOptions = LayoutOptions.StartAndExpand,
            ItemTemplate = new DataTemplate(typeof(MessageCell)),
            HasUnevenRows = true
        };

        private readonly PopupEntry popupEntryView = new PopupEntry
        {
            VerticalOptions = LayoutOptions.End,
            MaxLength = Message.MaxLength,
            Placeholder = "Type here...",
            HorizontalTextAlignment = TextAlignment.Center,
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            TextColor = Color.White,
            PlaceholderColor = Color.Gray
        };

        public MessagesPage(Dialog dialog)
        {
            this.dialog = dialog;
            Setup();
            dialog.Messages.Update(dialog.Id, 0, null).ContinueWith(AfterInitialUpdate);
        }

        /// <summary>
        /// If update successfull scroll to most recent message, otherwise show error popup
        /// </summary>
        private void AfterInitialUpdate(Task<bool> t)
        {
            if (!t.Result)
            {
                new RetryInformationPopup(
                    "Can't load messages",
                    async () => await dialog.Messages.Update(dialog.Id, 0, null).ContinueWith(AfterInitialUpdate))
                    .Show();
            }
            else
            {
                messagesListView.ItemAppearing += LoadMoreMessages;
            }
        }

        /// <summary>
        /// Initial setup of page
        /// </summary>
        private void Setup()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            SetBinding(RotaryFocusObjectProperty, new Binding() { Source = messagesListView });
            messagesListView.ItemsSource = dialog.Messages;
            popupEntryView.Completed += OnTextCompleted;

            verticalLayout.Children.Add(messagesListView);
            verticalLayout.Children.Add(popupEntryView);
            Content = verticalLayout;
            LongPollingClient.OnMessageUpdate += OnMessageUpdate;
            LongPollingClient.OnFullRefresh += RefreshAllAndScroll;
        }

        /// <summary>
        /// Called when long polling token outdated
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private async void RefreshAllAndScroll(object s, EventArgs e)
        {
            await dialog.Messages.Update(dialog.Id, 0, null);
            Scroll();
        }

        /// <summary>
        /// Scroll to most recent message
        /// </summary>
        private void Scroll()
        {
            var firstMessage = dialog.Messages.FirstOrDefault();
            if (firstMessage != null)
            {
                messagesListView.ScrollTo(firstMessage, ScrollToPosition.Center, false);
            }
        }

        /// <summary>
        /// Load more messages when scroll reached the end of the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LoadMoreMessages(object sender, ItemVisibilityEventArgs e)
        {
            if (dialog.Messages.All(i => i.Id >= (e.Item as Message).Id))
            {
                messagesListView.ItemAppearing -= LoadMoreMessages;
                await dialog.Messages.Update(dialog.Id, (uint)dialog.Messages.Count, null);
                messagesListView.ItemAppearing += LoadMoreMessages;
            }
        }

        /// <summary>
        /// Update messages collection
        /// </summary>
        private async void OnMessageUpdate(object sender, MessageEventArgs args)
        {
            var items = args.Data.Where(e => e.DialogId == dialog.Id);

            if (items.Any())
            {
                await dialog.Messages.Update(0, 0, items.Select(e => e.MessageId).ToArray());
            }

            Scroll();
            new Feedback().Play(FeedbackType.Vibration, "Tap");
        }

        /// <summary>
        /// Send message, mark all as read
        /// </summary>
        private async void OnTextCompleted(object sender, EventArgs args)
        {
            dialog.SetReadWithMessages();

            await DialogsClient.MarkAsRead(dialog.Id);

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
            Navigation.PopAsync();
            return base.OnBackButtonPressed();
        }
    }
}
