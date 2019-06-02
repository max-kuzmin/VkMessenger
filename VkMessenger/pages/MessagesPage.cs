using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
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

        //TODO: ability to manual refresh
        public MessagesPage(Dialog dialog)
        {
            NavigationPage.SetHasNavigationBar(this, false);

            this.dialog = dialog;
            Setup();
            dialog.Messages.Update(dialog.Id, null).ContinueWith(AfterInitialUpdate);
        }

        /// <summary>
        /// If update successfull scroll to most recent message, otherwise show error popup
        /// </summary>
        private void AfterInitialUpdate(Task<Exception> t)
        {
            if (t.Result != null)
            {
                new RetryInformationPopup(
                    t.Result.Message,
                    async () => await dialog.Messages.Update(dialog.Id, null).ContinueWith(AfterInitialUpdate))
                    .Show();
            }
            else
            {
                Scroll();
            }
        }

        /// <summary>
        /// Scroll to most recent message
        /// </summary>
        private void Scroll()
        {
            var lastMessage = dialog.Messages.LastOrDefault();
            if (lastMessage != null)
            {
                messagesListView.ScrollTo(lastMessage, ScrollToPosition.Center, false);
            }
        }

        /// <summary>
        /// Scroll to most recent message
        /// </summary>
        protected override void OnAppearing()
        {
            Scroll();
            base.OnAppearing();
        }

        /// <summary>
        /// Initial setup of page
        /// </summary>
        private void Setup()
        {
            SetBinding(RotaryFocusObjectProperty, new Binding() { Source = messagesListView });
            messagesListView.ItemsSource = dialog.Messages;
            popupEntryView.Completed += OnSend;

            verticalLayout.Children.Add(messagesListView);
            verticalLayout.Children.Add(popupEntryView);
            Content = verticalLayout;
            LongPollingClient.OnMessageUpdate += OnMessageUpdate;
        }

        /// <summary>
        /// Update messages collection
        /// </summary>
        private async void OnMessageUpdate(object sender, MessageEventArgs args)
        {
            if (args.DialogId == dialog.Id)
            {
                if (await dialog.Messages.Update(dialog.Id, new[] { args.MessageId }) == null)
                {
                    Scroll();
                }
            }
        }

        /// <summary>
        /// Send message
        /// </summary>
        private async void OnSend(object sender, EventArgs args)
        {
            dialog.MarkReadWithMessages();

            try
            {
                await DialogsClient.MarkAsRead(dialog.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

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
            catch (Exception e)
            {
                popupEntryView.Text = text;
                Logger.Error(e);
                new RetryInformationPopup(e.Message, () => OnSend(null, null));
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
