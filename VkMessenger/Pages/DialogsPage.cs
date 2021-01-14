using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Helpers;
using ru.MaxKuzmin.VkMessenger.Managers;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class DialogsPage : BezelInteractionPage, IDisposable
    {
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
            dialogsListView.ItemsSource = DialogsManager.Collection;
            Content = dialogsListView;

            dialogsListView.ItemTapped += OnDialogTapped;
            LongPollingClient.OnFullReset += OnFullReset;
            Appearing += OnAppearing;
        }

        private async void OnAppearing(object s, EventArgs e)
        {
            await DialogsManager.UpdateDialogsFromCache();
            dialogsListView.ScrollIfExist(DialogsManager.Collection.FirstOrDefault(), ScrollToPosition.Center);
            await InitFromApi();
        }

        /// <summary>
        /// Called on start. If update unsuccessful show error popup and retry
        /// </summary>
        private async Task InitFromApi()
        {
            Appearing -= OnAppearing;

            var refreshingPopup = DialogsManager.Collection.Any() ? null : new InformationPopup { Text = LocalizedStrings.LoadingDialogs };
            refreshingPopup?.Show();

            await NetExceptionCatchHelpers.CatchNetException(
                async () =>
                {
                    await DialogsManager.UpdateDialogsFromApi();
                    //Trim to batch size to prevent skipping new dialogs between cached and 20 loaded on init
                    DialogsManager.TrimDialogs();
                },
                InitFromApi,
                LocalizedStrings.DialogsNoInternetError,
                true);

            refreshingPopup?.Dismiss();
        }

        /// <summary>
        /// Open messages page, mark dialog as read
        /// </summary>
        private async void OnDialogTapped(object sender, ItemTappedEventArgs e)
        {
            var dialog = (Dialog)e.Item;
            await Navigation.PushAsync(new MessagesPage(dialog));
        }

        public void Dispose()
        {
            LongPollingClient.OnFullReset -= OnFullReset;
        }

        private async void OnFullReset(object s, EventArgs e)
        {
            await InitFromApi();
        }
    }
}
