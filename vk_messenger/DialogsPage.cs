using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Tizen.Applications;
using Tizen.Wearable.CircularUI.Forms;
using vk_messenger.models;
using Xamarin.Forms;

namespace vk_messenger
{
    public class DialogsPage : CirclePage
    {
        private readonly string getDialogs = "https://api.vk.com/method/messages.getConversations?" +
            "v=5.52" +
            "&extended=1" +
            "&access_token=";

        public DialogsPage()
        {
            Content = new CircleStackLayout();
            Content.VerticalOptions = LayoutOptions.Center;
            getDialogs += Preference.Get<string>("token");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ShowDialogs();
        }

        private void ShowDialogs()
        {
            using (var client = new WebClient())
            {
                var json = JObject.Parse(client.DownloadString(getDialogs));
                var dialogs = Dialog.FromJsonArray(json["response"]["items"] as JArray);

                var lines = (Content as CircleStackLayout).Children;
                lines.Clear();

                foreach (var item in dialogs)
                {
                    lines.Add(new Label
                    {
                        Text = item.Name
                    });
                }
            }
        }
    }
}
