using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Collections.Generic;
using System.Timers;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class DialogsPage : CirclePage
    {
        private readonly CircleListView dialogsListView = new CircleListView();

        public DialogsPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            SetupPage();
        }

        private void ShowDialogs(object sender = null, ElapsedEventArgs e = null)
        {
            var json = JObject.Parse(Api.GetDialogsJson());
            var profiles = Profile.FromJsonArray(json["response"]["profiles"] as JArray);
            var groups = Group.FromJsonArray(json["response"]["groups"] as JArray);
            var dialogs = Dialog.FromJsonArray(json["response"]["items"] as JArray, profiles, groups);

            var cellsData = new List<object>();
            foreach (var item in dialogs)
            {
                var cellData = new CellData
                {
                    Text = item.GetTitle(),
                    Detail = item.LastMessage.Text,
                    ImageSource = item.GetPhoto().Source,
                    Dialog = item,
                    TextColor = item.UnreadCount > 0 ? Color.Yellow : Color.White

                };
                cellsData.Add(cellData);
            }
            dialogsListView.ItemsSource = cellsData;
        }

        private void SetupPage()
        {
            SetBinding(CirclePage.RotaryFocusObjectProperty, new Binding() { Source = dialogsListView });
            dialogsListView.ItemTemplate = new DataTemplate(() =>
            {
                var cell = new ImageCell();
                cell.SetBinding(ImageCell.TextProperty, nameof(CellData.Text));
                cell.SetBinding(ImageCell.DetailProperty, nameof(CellData.Detail));
                cell.SetBinding(ImageCell.ImageSourceProperty, nameof(CellData.ImageSource));
                cell.SetBinding(ImageCell.TextColorProperty, nameof(CellData.TextColor));
                return cell;
            });
            dialogsListView.ItemSelected += OnDialogSelected;
            Content = dialogsListView;
        }

        private void OnDialogSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var dialog = (e.SelectedItem as CellData).Dialog;
            Navigation.PushAsync(new MessagesPage(dialog));
            Api.MarkAsRead(dialog.GetPeerId());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ShowDialogs();
        }

        public class CellData
        {
            public string Text { get; set; }
            public string Detail { get; set; }
            public ImageSource ImageSource { get; set; }
            public Dialog Dialog { get; set; }
            public Color TextColor { get; set; }
        }
    }
}
