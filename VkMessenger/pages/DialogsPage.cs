using ru.MaxKuzmin.VkMessenger.Models;
using System.Collections.Generic;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class DialogsPage : CirclePage
    {
        private readonly CircleListView dialogsListView = new CircleListView();
        private readonly List<Dialog> dialogs = new List<Dialog>();

        public DialogsPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            SetupPage();
        }

        private void SetupPage()
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
            dialogs.AddRange(Dialog.GetDialogs());
        }
    }
}
