using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tizen;
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
            Update(null);
        }

        private async void Update(IReadOnlyCollection<uint> messagesIds)
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

                Scroll();
            }
            catch (Exception e)
            {
                Log.Error(nameof(VkMessenger), e.ToString());
                Toast.DisplayText(e.Message);
            }
        }

        private void Scroll()
        {
            var lastMessage = messages.LastOrDefault();
            if (lastMessage != null)
            {
                messagesListView.ScrollTo(lastMessage, ScrollToPosition.Center, false);
            }
        }

        protected override void OnAppearing()
        {
            Scroll();
            base.OnAppearing();
        }

        private void Setup()
        {
            SetBinding(RotaryFocusObjectProperty, new Binding() { Source = messagesListView });
            messagesListView.ItemsSource = messages;
            popupEntryView.Completed += OnSend;

            verticalLayout.Children.Add(messagesListView);
            verticalLayout.Children.Add(popupEntryView);
            Content = verticalLayout;

            LongPollingClient.OnMessageUpdate += (s, e) =>
            {
                if (e.DialogId == dialogId)
                {
                    Update(new[] { e.MessageId });
                }
            };
        }

        private async void OnSend(object sender, EventArgs args)
        {
            var text = popupEntryView.Text;
            try
            {
                await MessagesClient.Send(text, dialogId);
                popupEntryView.Text = string.Empty;
            }
            catch (Exception e)
            {
                popupEntryView.Text = text;
                Log.Error(nameof(VkMessenger), e.ToString());
                Toast.DisplayText(e.Message);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return base.OnBackButtonPressed();
        }
    }
}
