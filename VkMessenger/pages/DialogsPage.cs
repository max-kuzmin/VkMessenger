using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Models;
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

        public DialogsPage()
        {
            Setup();
            RefreshAll().ContinueWith(AfterInitialUpdate);
        }

        /// <summary>
        /// If update unsuccessfull show error popup and retry
        /// </summary>
        private void AfterInitialUpdate(Task<bool> t)
        {
            if (!t.Result)
            {
                new RetryInformationPopup(
                    "Can't load dialogs",
                    async () => await RefreshAll().ContinueWith(AfterInitialUpdate))
                    .Show();
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
                dialogsListView.ScrollTo(firstDialog, ScrollToPosition.Center, false); //TODO
            }
        }

        /// <summary>
        /// Initial setup of page
        /// </summary>
        private void Setup()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            SetBinding(RotaryFocusObjectProperty, new Binding() { Source = dialogsListView });
            dialogsListView.ItemTapped += OnDialogTapped;
            dialogsListView.ItemsSource = dialogs;
            Content = dialogsListView;

            LongPollingClient.OnMessageUpdate += async (s, e) => await dialogs.Update(e.Data.Select(i => i.DialogId).ToArray());
            LongPollingClient.OnDialogUpdate += async (s, e) => await dialogs.Update(e.DialogIds);
            LongPollingClient.OnUserStatusUpdate += OnUserStatusUpdate;
            LongPollingClient.OnFullRefresh += async (s, e) => await RefreshAll();
        }

        /// <summary>
        /// Called on start or when long polling token outdated
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private async Task<bool> RefreshAll()
        {
            var refreshingPopup = new InformationPopup() { Text = "Refreshing..." };
            refreshingPopup.Show();
            var result = await dialogs.Update(null);
            Scroll();
            refreshingPopup.Dismiss();
            return result;
        }

        /// <summary>
        /// Update user status in every dialog
        /// </summary>
        private void OnUserStatusUpdate(object sender, UserStatusEventArgs e)
        {
            foreach (var dialog in dialogs)
            {
                foreach (var (UserId, Status) in e.Data)
                {
                    dialog.SetOnline(UserId, Status);
                }
            }
        }

        /// <summary>
        /// Open messages page, mark dialog as read
        /// </summary>
        private async void OnDialogTapped(object sender, ItemTappedEventArgs e)
        {
            var dialog = e.Item as Dialog;
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

            dialog.SetReadWithMessages();

            await DialogsClient.MarkAsRead(dialog.Id);
        }
    }
}
