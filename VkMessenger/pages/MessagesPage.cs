using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class MessagesPage : CirclePage
    {
        private readonly CircleListView messagesListView = new CircleListView();
        private readonly Dialog dialog;
        private readonly List<Message> messages = new List<Message>();

        public MessagesPage(Dialog dialog)
        {
            NavigationPage.SetHasNavigationBar(this, false);

            this.dialog = dialog;
            SetupPage();
        }

        private void SetupPage()
        {
            SetBinding(CirclePage.RotaryFocusObjectProperty, new Binding() { Source = messagesListView });
            messagesListView.ItemTemplate = new DataTemplate(() =>
            {
                var cell = new EntryCell();
                cell.SetBinding(EntryCell.TextProperty, nameof(Message.Text));
                return cell;
            });
            Content = messagesListView;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            messages.AddRange(Message.GetMessages(dialog.PeerId));
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return true;
        }
    }
}
