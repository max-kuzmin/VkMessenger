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
        private readonly CircleListView dialogsListView = new CircleListView
        {
            ItemTemplate = new DataTemplate(typeof(DialogCell))
        };
        private readonly ObservableCollection<Dialog> dialogs = new ObservableCollection<Dialog>();

        public DialogsPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            Update();
            Setup();
        }

        private void Update()
        {
            lock (dialogs)
            {
                foreach (var item in DialogsClient.GetDialogs().AsEnumerable().Reverse())
                {
                    var found = dialogs.FirstOrDefault(d => d.Id == item.Id);

                    if (found == null)
                        dialogs.Insert(0, item);
                    else if (found.LastMessage.Text != item.LastMessage.Text)
                    {
                        found.LastMessage = item.LastMessage;
                        found.UnreadCount = item.UnreadCount;

                        if (dialogs.Last() != found)
                        {
                            dialogs.Remove(found);
                            dialogs.Insert(0, found);
                        }
                        else found.InvokePropertyChanged();
                    }
                }
            }
        }

        private void Setup()
        {
            SetBinding(RotaryFocusObjectProperty, new Binding() { Source = dialogsListView });
            dialogsListView.ItemSelected += OnDialogSelected;
            dialogsListView.ItemsSource = dialogs;
            Content = dialogsListView;

            LongPollingClient.OnMessageAdd += (s, e) => Update();
            LongPollingClient.OnDialogUpdate += (s, e) => Update();
        }

        private void OnDialogSelected(object sender, SelectedItemChangedEventArgs e)
        {   
            var dialog = e.SelectedItem as Dialog;
            Navigation.PushAsync(new MessagesPage(dialog.Id));
            DialogsClient.MarkAsRead(dialog.Id);
        }
    }
}
