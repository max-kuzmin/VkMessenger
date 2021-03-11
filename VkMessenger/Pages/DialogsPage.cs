using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Helpers;
using ru.MaxKuzmin.VkMessenger.Managers;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class DialogsPage : BezelInteractionPage, IResettable
    {
        private readonly DialogsManager dialogsManager;
        private readonly MessagesManager messagesManager;
        private InformationPopup? refreshingPopup;

        private readonly CircleListView dialogsListView = new CircleListView
        {
            ItemTemplate = new DataTemplate(typeof(DialogCell)),
            BarColor = Color.Transparent,
            VerticalScrollBarVisibility = ScrollBarVisibility.Never
        };

        public DialogsPage(DialogsManager dialogsManager, MessagesManager messagesManager)
        {
            this.dialogsManager = dialogsManager;
            this.messagesManager = messagesManager;
            NavigationPage.SetHasNavigationBar(this, false);
            SetBinding(RotaryFocusObjectProperty, new Binding { Source = dialogsListView });
            dialogsListView.ItemsSource = dialogsManager.Collection;
            Content = dialogsListView;

            dialogsListView.ItemTapped += OnDialogTapped;
            Appearing += OnAppearing;
        }

        private async void OnAppearing(object s, EventArgs e)
        {
            await dialogsManager.UpdateDialogsFromCache();
            var firstDialog = dialogsManager.First();
            if (firstDialog == null)
            {
                refreshingPopup = new InformationPopup { Text = LocalizedStrings.LoadingDialogs };
                refreshingPopup?.Show();
            }
            dialogsListView.ScrollIfExist(firstDialog, ScrollToPosition.Center);
            Appearing -= OnAppearing;
        }

        /// <summary>
        /// Called when long pooling calls Reset. If update unsuccessful show error popup and retry
        /// </summary>
        private async Task InitFromApi()
        {
            await NetExceptionCatchHelpers.CatchNetException(
                dialogsManager.UpdateDialogsFromApi,
                InitFromApi,
                LocalizedStrings.DialogsNoInternetError);

            if (refreshingPopup != null)
            {
                refreshingPopup.Dismiss();
                dialogsListView.ScrollIfExist(dialogsManager.First(), ScrollToPosition.Center);
            }
        }

        /// <summary>
        /// Open messages page, mark dialog as read
        /// </summary>
        private async void OnDialogTapped(object sender, ItemTappedEventArgs e)
        {
            var dialog = (Dialog)e.Item;
            await Navigation.PushAsync(new MessagesPage(dialog.Id, messagesManager, dialogsManager));
        }

        public async Task Reset()
        {
            await InitFromApi();
        }
    }
}
