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

        public MessagesPage(Dialog dialog)
        {
            NavigationPage.SetHasNavigationBar(this, false);

            this.dialog = dialog;
            SetupPage();
        }

        private void ShowMessages(object sender = null, ElapsedEventArgs e = null)
        {
            var json = JObject.Parse(Api.GetMessagesJson(dialog.GetPeerId()));
            var messages = Message.FromJsonArray(json["response"]["items"] as JArray);

            var cellsData = new List<string>();
            foreach (var item in messages)
            {
                var user = dialog.Profiles.FirstOrDefault(s => s.Id == item.Sender);
                if (user != null)
                    cellsData.Add($"{user.Name}: {item.Text}");
                else
                    cellsData.Add(item.Text);
            }
            messagesListView.ItemsSource = cellsData;
        }

        private void SetupPage()
        {
            SetBinding(CirclePage.RotaryFocusObjectProperty, new Binding() { Source = messagesListView });
            Content = messagesListView;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ShowMessages();
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return true;
        }
    }
}
