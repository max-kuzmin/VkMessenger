using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class DialogsPage : CirclePage
    {
        private readonly CircleListView dialogsListView = new CircleListView();
        private readonly ObservableCollection<Dialog> dialogs = new ObservableCollection<Dialog>();

        public DialogsPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            Update();
            Setup();
            LongPollingClient.OnMessageAdd += (s, e) => Update();
            LongPollingClient.OnDialogUpdate += (s, e) => Update();
        }

        private void Update()
        {
            foreach (var item in DialogsClient.GetDialogs())
            {
                var found = dialogs.FirstOrDefault(d => d.Id == item.Id);

                if (found == null)
                    dialogs.Add(item);
                else if (item.UnreadCount > 0)
                {
                    found.LastMessage = item.LastMessage;
                    found.UnreadCount = item.UnreadCount;
                    dialogs.Move(dialogs.IndexOf(found), dialogs.Count - 1);
                }
            }
        }

        private void Setup()
        {
            SetBinding(RotaryFocusObjectProperty, new Binding() { Source = dialogsListView });
            dialogsListView.ItemTemplate = new DataTemplate(typeof(DialogCell));
            dialogsListView.ItemSelected += OnDialogSelected;
            dialogsListView.ItemsSource = dialogs;
            Content = dialogsListView;
        }

        private void OnDialogSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var dialog = e.SelectedItem as Dialog;
            Navigation.PushAsync(new MessagesPage(dialog.Id));
            DialogsClient.MarkAsRead(dialog.Id);
        }
    }
}
