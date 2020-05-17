using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Collections;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class DialogsPage : CirclePage
    {
        private readonly Dictionary<int, MessagesPage> messagesPages = new Dictionary<int, MessagesPage>();
        private readonly CustomObservableCollection<Dialog> dialogs = new CustomObservableCollection<Dialog>();

        private readonly CircleListView dialogsListView = new CircleListView
        {
            ItemTemplate = new DataTemplate(typeof(DialogCell))
        };

        public DialogsPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            SetBinding(RotaryFocusObjectProperty, new Binding { Source = dialogsListView });
            dialogsListView.ItemsSource = dialogs;
            Content = dialogsListView;

            Appearing += UpdateAll;
        }

        /// <summary>
        /// Called on start. If update unsuccessful show error popup and retry
        /// </summary>
        private async void UpdateAll(object? sender = null, EventArgs? args = null)
        {
            Appearing -= UpdateAll;

            var refreshingPopup = new InformationPopup { Text = LocalizedStrings.LoadingDialogs };
            refreshingPopup.Show();


            if (await dialogs.Update())
            {
                dialogsListView.ScrollIfExist(dialogs.FirstOrDefault(), ScrollToPosition.Center);

                dialogsListView.ItemTapped += OnDialogTapped;
                LongPollingClient.OnMessageUpdate += async (s, e) => await dialogs.Update(e.Data.Select(i => i.DialogId).ToArray());
                LongPollingClient.OnDialogUpdate += async (s, e) => await dialogs.Update(e.DialogIds.ToArray());
                LongPollingClient.OnUserStatusUpdate += (s, e) => dialogs.SetOnline(e.Data);
            }
            else
            {
                new RetryInformationPopup(
                    LocalizedStrings.DialogsNoInternetError,
                    () => UpdateAll())
                    .Show();
            }

            refreshingPopup.Dismiss();
        }

        /// <summary>
        /// Open messages page, mark dialog as read
        /// </summary>
        private async void OnDialogTapped(object sender, ItemTappedEventArgs e)
        {
            var dialog = (Dialog)e.Item;
            MessagesPage messagesPage;
            if (messagesPages.ContainsKey(dialog.Id))
            {
                messagesPage = messagesPages[dialog.Id];
            }
            else
            {
                messagesPage = new MessagesPage(dialog);
                messagesPages.Add(dialog.Id, messagesPage);
            }

            await Navigation.PushAsync(messagesPage);

            await dialog.SetReadWithMessagesAndPublish();
        }

        protected override bool OnBackButtonPressed() => true;
    }
}
