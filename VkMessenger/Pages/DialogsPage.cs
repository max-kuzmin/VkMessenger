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
    public class DialogsPage : PageWithActivityIndicator, IResettable
    {
        private readonly DialogsManager dialogsManager;
        private readonly MessagesManager messagesManager;

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
            absoluteLayout.Children.Add(dialogsListView);
            absoluteLayout.Children.Add(activityIndicator);
            Content = absoluteLayout;

            dialogsListView.ItemTapped += OnDialogTapped;
            Appearing += OnConstructor;
        }

        private async void OnConstructor(object s, EventArgs e)
        {
            await dialogsManager.UpdateDialogsFromCache();
            activityIndicator.IsVisible = true;
            dialogsListView.ScrollIfExist(dialogsManager.First(), ScrollToPosition.Center);
            Appearing -= OnConstructor;
        }

        /// <summary>
        /// Called when long pooling calls Reset. If update unsuccessful show error popup and retry
        /// </summary>
        private async Task InitFromApi()
        {
            activityIndicator.IsVisible = true;

            await NetExceptionCatchHelpers.CatchNetException(
                dialogsManager.UpdateDialogsFromApi,
                InitFromApi,
                LocalizedStrings.DialogsNoInternetError);

            activityIndicator.IsVisible = false;
            dialogsListView.ScrollIfExist(dialogsManager.First(), ScrollToPosition.Center);
        }

        /// <summary>
        /// Open messages page, mark dialog as read
        /// </summary>
        private async void OnDialogTapped(object sender, ItemTappedEventArgs e)
        {
            var dialog = (Dialog)e.Item;
            await Navigation.PushAsync(new MessagesPage(dialog.Id, messagesManager, dialogsManager));

            if (!AuthorizationManager.TutorialShown)
                new CustomPopup(
                    LocalizedStrings.MessagesPageTutorial,
                    LocalizedStrings.Ok,
                    () => AuthorizationManager.TutorialShown = true).Show();
        }

        public async Task Reset()
        {
            await InitFromApi();
        }
    }
}
