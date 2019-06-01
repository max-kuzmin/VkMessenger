using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class MessagesPage : CirclePage
    {
        private readonly StackLayout verticalLayout = new StackLayout();
        private readonly ObservableCollection<Message> messages = new ObservableCollection<Message>();
        private readonly int dialogId;

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

        public MessagesPage(int dialogId)
        {
            NavigationPage.SetHasNavigationBar(this, false);

            this.dialogId = dialogId;
            Setup();
            Update(null).ContinueWith(AfterInitialUpdate);
        }

        //TODO: ability to manual refresh
        /// <summary>
        /// Update messages from API. Can be used during setup of page or with <see cref="LongPolling"/>
        /// </summary>
        /// <param name="messagesIds">Message id collection or null</param>
        /// <returns>Null means update successfull</returns>
        private async Task<Exception> Update(IReadOnlyCollection<uint> messagesIds)
        {
            try
            {
                var newMessages = await MessagesClient.GetMessages(dialogId, messagesIds);

                foreach (var item in newMessages.AsEnumerable().Reverse())
                {
                    var foundMessage = messages.FirstOrDefault(d => d.Id == item.Id);

                    if (foundMessage == null)
                        messages.Add(item);
                    else
                    {
                        foundMessage.Text = item.Text;
                        foundMessage.InvokePropertyChanged();
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return e;
            }
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
                    async () => await Update(null).ContinueWith(AfterInitialUpdate))
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
            var lastMessage = messages.LastOrDefault();
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
            messagesListView.ItemsSource = messages;
            popupEntryView.Completed += OnSend;

            verticalLayout.Children.Add(messagesListView);
            verticalLayout.Children.Add(popupEntryView);
            Content = verticalLayout;
            LongPollingClient.OnMessageUpdate += OnMessageUpdate;
        }

        /// <summary>
        /// <see cref="LongPollingClient.OnMessageUpdate"/> callback. Update or add message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnMessageUpdate(object sender, MessageEventArgs args)
        {
            if (args.DialogId == dialogId)
            {
                await Update(new[] { args.MessageId }).ContinueWith(t =>
                {
                    if (t.Result == null)
                    {
                        Scroll();
                    }
                });
            }
        }

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnSend(object sender, EventArgs args)
        {
            var text = popupEntryView.Text;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                await MessagesClient.Send(text, dialogId);
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
        /// <returns></returns>
        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return base.OnBackButtonPressed();
        }
    }
}
