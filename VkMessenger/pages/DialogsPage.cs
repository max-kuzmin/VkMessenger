using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Tizen;
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
            NavigationPage.SetHasNavigationBar(this, false);
            Setup();
            Update(null).ContinueWith(AfterInitialUpdate);
        }

        //TODO: ability to manual refresh
        /// <summary>
        /// Update dialogs from API. Can be used during setup of page or with <see cref="LongPolling"/>
        /// </summary>
        /// <param name="dialogIds">Dialog id collection or null</param>
        /// <returns>Null means update successfull</returns>
        private async Task<Exception> Update(IReadOnlyCollection<int> dialogIds)
        {
            try
            {
                var newDialogs = await DialogsClient.GetDialogs(dialogIds);

                foreach (var newDialog in newDialogs.AsEnumerable().Reverse())
                {
                    var foundDialog = dialogs.FirstOrDefault(d => d.Id == newDialog.Id);

                    if (foundDialog == null)
                        dialogs.Insert(0, newDialog);
                    else
                    {
                        UpdateDialog(newDialog, foundDialog);

                        if (dialogs.Last() != foundDialog)
                        {
                            dialogs.Remove(foundDialog);
                            dialogs.Insert(0, foundDialog);
                        }
                        else foundDialog.InvokePropertyChanged();
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Log.Error(nameof(VkMessenger), e.ToString());
                return e;
            }
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
                    async () => await Update(null).ContinueWith(AfterInitialUpdate))
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
        /// Update dialog data without recreating it
        /// </summary>
        /// <param name="newDialog">New data</param>
        /// <param name="foundDialog">Dialog to update</param>
        private static void UpdateDialog(Dialog newDialog, Dialog foundDialog)
        {
            foundDialog.LastMessage = newDialog.LastMessage;
            foundDialog.UnreadCount = newDialog.UnreadCount;

            if (newDialog.Profiles != null)
            {
                foreach (var newProfile in newDialog.Profiles)
                {
                    var foundProfile = foundDialog.Profiles.FirstOrDefault(p => p.Id == newProfile.Id);
                    if (foundProfile != null)
                        foundProfile.IsOnline = newDialog.IsOnline;
                }
            }
        }

        /// <summary>
        /// Initial setup of page
        /// </summary>
        private void Setup()
        {
            SetBinding(RotaryFocusObjectProperty, new Binding() { Source = dialogsListView });
            dialogsListView.ItemTapped += OnDialogTapped;
            dialogsListView.ItemsSource = dialogs;
            Content = dialogsListView;

            LongPollingClient.OnMessageUpdate += async (s, e) => await Update(new[] { e.DialogId });
            LongPollingClient.OnDialogUpdate += async (s, e) => await Update(new[] { e });

            LongPollingClient.OnUserStatusUpdate += OnUserStatusUpdate;
        }

        /// <summary>
        /// Callback of <see cref="LongPollingClient.OnUserStatusUpdate"/>. Update users statuses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserStatusUpdate(object sender, UserStatusEventArgs e)
        {
            foreach (var dialog in dialogs)
            {
                var profile = dialog.Profiles?.FirstOrDefault(p => p.Id == e.UserId);
                if (profile != null)
                {
                    profile.IsOnline = e.IsOnline;
                    dialog.InvokePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Callback of <see cref="CircleListView.ItemTapped"/>. Open messages page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnDialogTapped(object sender, ItemTappedEventArgs args)
        {
            var dialog = args.Item as Dialog;
            MessagesPage messagesPage;
            if (messagesPages.ContainsKey(dialog.Id))
            {
                messagesPage = messagesPages[dialog.Id];
            }
            else
            {
                messagesPage = new MessagesPage(dialog.Id);
                messagesPages.Add(dialog.Id, messagesPage);
            }

            await Navigation.PushAsync(messagesPage);

            try
            {
                await DialogsClient.MarkAsRead(dialog.Id);
            }
            catch (Exception e)
            {
                Log.Error(nameof(VkMessenger), e.ToString());
            }
        }
    }
}
