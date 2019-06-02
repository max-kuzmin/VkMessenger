using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class DialogsPage : CirclePage
    {
        private Dictionary<int, MessagesPage> messagesPages = new Dictionary<int, MessagesPage>();
        private readonly ObservableCollection<Dialog> dialogs = new ObservableCollection<Dialog>();

        private readonly CircleListView dialogsListView = new CircleListView
        {
            ItemTemplate = new DataTemplate(typeof(DialogCell))
        };

        //TODO: ability to manual refresh
        public DialogsPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            Setup();
            dialogs.Update(null).ContinueWith(AfterInitialUpdate);
        }

        /// <summary>
        /// If update successfull scroll to most recent dialog, otherwise show error popup
        /// </summary>
        private void AfterInitialUpdate(Task<Exception> t)
        {
            if (t.Result != null)
            {
                new RetryInformationPopup(
                    t.Result.Message,
                    async () => await dialogs.Update(null).ContinueWith(AfterInitialUpdate))
                    .Show();
            }
            else
            {
                Scroll();
            }
        }

        /// <summary>
        /// Scroll to most recent dialog
        /// </summary>
        private void Scroll()
        {
            var firstDialog = dialogs.FirstOrDefault();
            if (firstDialog != null)
            {
                dialogsListView.ScrollTo(firstDialog, ScrollToPosition.Center, false);
            }
        }

        /// <summary>
        /// Initial setup of page
        /// </summary>
        private void Setup()
        {
            SetBinding(RotaryFocusObjectProperty, new Binding() { Source = dialogsListView });
            dialogsListView.ItemSelected += OnDialogSelected;
            dialogsListView.ItemsSource = dialogs;
            Content = dialogsListView;

            LongPollingClient.OnMessageUpdate += async (s, e) => await dialogs.Update(new[] { e.DialogId });
            LongPollingClient.OnDialogUpdate += async (s, e) => await dialogs.Update(new[] { e });

            LongPollingClient.OnUserStatusUpdate += OnUserStatusUpdate;
        }

        /// <summary>
        /// Callback of <see cref="LongPollingClient.OnUserStatusUpdate"/>. Update users statuses
        /// </summary>
        private void OnUserStatusUpdate(object sender, UserStatusEventArgs e)
        {
            foreach (var dialog in dialogs)
            {
                dialog.SetOnline(e.UserId, e.Online);
            }
        }

        /// <summary>
        /// Callback of <see cref="CircleListView.ItemTapped"/>. Open messages page
        /// </summary>
        private async void OnDialogSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var dialog = args.SelectedItem as Dialog;
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

            dialog.SetUnreadCount(0);

            try
            {
                await DialogsClient.MarkAsRead(dialog.Id);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }
}
