using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class MessagesPage : CirclePage
    {
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

        private readonly StackLayout verticalLayout = new StackLayout();
        private readonly ObservableCollection<Message> messages = new ObservableCollection<Message>();

        private readonly int dialogId;

        public MessagesPage(int dialogId)
        {
            NavigationPage.SetHasNavigationBar(this, false);

            this.dialogId = dialogId;
            Setup();
            Update();
        }

        private async void Update()
        {
            try
            {
                Network.ThrowIfDisconnected();
                var newMessages = await MessagesClient.GetMessages(dialogId);

                foreach (var item in newMessages.AsEnumerable().Reverse())
                {
                    var found = messages.FirstOrDefault(d => d.Id == item.Id);

                    if (found == null)
                        messages.Insert(0, item);
                    else
                    {
                        found.Text = item.Text;
                        found.InvokePropertyChanged();
                    }
                }
            }
            catch (WebException)
            {
                Network.StartWaiting();
            }
        }

        private void Setup()
        {
            SetBinding(RotaryFocusObjectProperty, new Binding() { Source = messagesListView });
            messagesListView.ItemsSource = messages;
            messagesListView.RefreshCommand = new Command(Update);
            popupEntryView.Completed += OnSend;

            verticalLayout.Children.Add(messagesListView);
            verticalLayout.Children.Add(popupEntryView);
            Content = verticalLayout;

            //TODO: Replace with getting only one message
            LongPollingClient.OnMessageUpdate += (s, e) => { if (e.DialogId == dialogId) Update(); };
        }

        private async void OnSend(object sender, EventArgs e)
        {
            var text = popupEntryView.Text;
            try
            {
                Network.ThrowIfDisconnected();
                await MessagesClient.Send(text, dialogId);
                popupEntryView.Text = string.Empty;
            }
            catch (WebException)
            {
                popupEntryView.Text = text;
                Network.StartWaiting();
            }

            popupEntryView.Text = string.Empty;
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return true;
        }
    }
}
