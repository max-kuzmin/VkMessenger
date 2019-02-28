using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class SendMessagePage : CirclePage
    {
        private readonly PopupEntry popupEntryView = new PopupEntry();
        private readonly int dialogId;

        public SendMessagePage(int dialogId)
        {
            NavigationPage.SetHasNavigationBar(this, false);

            this.dialogId = dialogId;
            Setup();
        }

        private void Setup()
        {
            popupEntryView.MaxLength = Message.MaxLength;
            popupEntryView.Completed += OnSend;
            popupEntryView.Placeholder = "Enter message";
            Content = popupEntryView;
        }

        private void OnSend(object sender, EventArgs e)
        {
            MessagesClient.Send(popupEntryView.Text, dialogId);
            Navigation.PopAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return true;
        }

        protected override void OnAppearing()
        {
            popupEntryView.Focus();
            base.OnAppearing();
        }
    }
}
