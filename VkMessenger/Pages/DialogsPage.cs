using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Exceptions;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class DialogsPage : BezelInteractionPage, IDisposable
    {
        private readonly Dictionary<int, MessagesPage> messagesPages = new Dictionary<int, MessagesPage>();
        private readonly ObservableCollection<Dialog> dialogs = new ObservableCollection<Dialog>();

        private readonly CircleListView dialogsListView = new CircleListView
        {
            ItemTemplate = new DataTemplate(typeof(DialogCell)),
            BarColor = Color.Transparent,
            VerticalScrollBarVisibility = ScrollBarVisibility.Never
        };

        public DialogsPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            SetBinding(RotaryFocusObjectProperty, new Binding { Source = dialogsListView });
            dialogsListView.ItemsSource = dialogs;
            Content = dialogsListView;

            Appearing += OnAppearing;
        }

        private async void OnAppearing(object s, EventArgs e)
        {
            await dialogs.GetFromCache();
            dialogsListView.ScrollIfExist(dialogs.FirstOrDefault(), ScrollToPosition.Center);
            await InitFromApi();
        }

        /// <summary>
        /// Called on start. If update unsuccessful show error popup and retry
        /// </summary>
        private async Task InitFromApi()
        {
            Appearing -= OnAppearing;

            var refreshingPopup = dialogs.Any() ? null : new InformationPopup { Text = LocalizedStrings.LoadingDialogs };
            refreshingPopup?.Show();

            try
            {
                await dialogs.Update();
                //Trim to batch size to prevent skipping new dialogs between cached and 20 loaded on init
                dialogs.Trim(Consts.BatchSize);

                dialogsListView.ItemTapped += OnDialogTapped;
                LongPollingClient.OnMessageUpdate += OnMessageUpdate;
                LongPollingClient.OnDialogUpdate += OnDialogUpdate;
                LongPollingClient.OnUserStatusUpdate += OnUserStatusUpdate;
                LongPollingClient.OnFullReset += OnFullReset;
            }
            catch (WebException)
            {
                new CustomPopup(
                    LocalizedStrings.DialogsNoInternetError,
                    LocalizedStrings.Retry,
                    async () => await InitFromApi())
                    .Show();
            }
            catch (InvalidSessionException)
            {
                new CustomPopup(
                        LocalizedStrings.InvalidSessionError,
                        LocalizedStrings.Ok,
                        AuthorizationClient.CleanUserAndExit)
                    .Show();
            }
            catch (Exception ex)
            {
                new CustomPopup(
                    ex.ToString(),
                    LocalizedStrings.Ok,
                    Application.Current.Quit)
                    .Show();
            }

            refreshingPopup?.Dismiss();
        }

        private void OnUserStatusUpdate(object s, UserStatusEventArgs e)
        {
            dialogs.SetOnline(e.Data);
        }

        private async void OnDialogUpdate(object s, DialogEventArgs e)
        {
            await dialogs.UpdateByIds(e.DialogIds.ToArray());
            new Feedback().Play(FeedbackType.Vibration, "Tap");
        }

        private async void OnMessageUpdate(object s, MessageEventArgs e)
        {
            await dialogs.UpdateByIds(e.Data.Select(i => i.DialogId).ToArray());
            new Feedback().Play(FeedbackType.Vibration, "Tap");
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
        }

        public void Dispose()
        {
            dialogsListView.ItemTapped -= OnDialogTapped;
            LongPollingClient.OnMessageUpdate -= OnMessageUpdate;
            LongPollingClient.OnDialogUpdate -= OnDialogUpdate;
            LongPollingClient.OnUserStatusUpdate -= OnUserStatusUpdate;
            LongPollingClient.OnFullReset -= OnFullReset;
        }

        private async void OnFullReset(object s, EventArgs e)
        {
            Dispose();
            await InitFromApi();
        }
    }
}
