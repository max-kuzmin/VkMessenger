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
            ShowDialogs();
            App.UpdateTimer.Elapsed += ShowDialogs;
        }

        ~DialogsPage()
        {
            App.UpdateTimer.Elapsed -= ShowDialogs;
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
                var cellData =  new
                {
                    Text = item.GetTitle(),
                    Detail = item.LastMessage.Text,
                    ImageSource = item.GetPhoto().Source
                    
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
                cell.SetBinding(ImageCell.TextProperty, "Text");
                cell.SetBinding(ImageCell.DetailProperty, "Detail");
                cell.SetBinding(ImageCell.ImageSourceProperty, "ImageSource");
                return cell;
            });
            Content = dialogsListView;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            App.UpdateTimer.Elapsed += ShowDialogs;
        }
    }
}
