using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class MessagesPage : CirclePage
    {
        private readonly CircleListView messagesListView = new CircleListView();
        private readonly Dialog dialog;
        private readonly ObservableCollection<Message> messages = new ObservableCollection<Message>();

        public MessagesPage(Dialog dialog)
        {
            NavigationPage.SetHasNavigationBar(this, false);

            this.dialog = dialog;
            Setup();
        }

        private void Update()
        {
            foreach (var item in MessagesClient.GetMessages(dialog.PeerId))
            {
                var found = messages.FirstOrDefault(d => d.Id == item.Id);

                if (found == null)
                    messages.Insert(0, item);
            }
        }

        private void Setup()
        {
            SetBinding(RotaryFocusObjectProperty, new Binding() { Source = messagesListView });
            messagesListView.ItemTemplate = new DataTemplate(typeof(MessageCell));
            Content = messagesListView;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Update();
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return true;
        }
    }
}
