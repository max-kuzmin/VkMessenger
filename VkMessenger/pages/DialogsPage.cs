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
            Setup();
        }

        private void Update()
        {
            foreach (var item in Dialog.GetDialogs())
            {
                var found = dialogs.FirstOrDefault(d => d.Id == item.Id);

                if (found != null && item.UnreadCount > 0)
                {
                    found.LastMessage = item.LastMessage;
                    found.UnreadCount = item.UnreadCount;
                    dialogs.Move(dialogs.IndexOf(found), 0);
                }
                else dialogs.Insert(0, item);
            }
        }

        private void Setup()
        {
            SetBinding(CirclePage.RotaryFocusObjectProperty, new Binding() { Source = dialogsListView });
            dialogsListView.ItemTemplate = new DataTemplate(() =>
            {
                var cell = new ImageCell();
                cell.SetBinding(ImageCell.TextProperty, nameof(Dialog.Title));
                cell.SetBinding(ImageCell.DetailProperty, nameof(Dialog.Text));
                cell.SetBinding(ImageCell.ImageSourceProperty, nameof(Dialog.Photo));
                cell.SetBinding(ImageCell.TextColorProperty, nameof(Dialog.TextColor));
                return cell;
            });
            dialogsListView.ItemSelected += OnDialogSelected;
            dialogsListView.ItemsSource = dialogs;
            Content = dialogsListView;
        }

        private void OnDialogSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var dialog = e.SelectedItem as Dialog;
            Navigation.PushAsync(new MessagesPage(dialog));
            Api.MarkAsRead(dialog.PeerId);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Update();
        }
    }
}
