using System;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Managers;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class AuthorizationPage : ContentPage
    {
        private readonly DialogsManager dialogsManager;
        private readonly MessagesManager messagesManager;
        private readonly LongPollingManager longPollingManager;
        private readonly WebView loginWebView = new WebView();
        private InformationPopup? refreshingPopup;

        public AuthorizationPage(DialogsManager dialogsManager, MessagesManager messagesManager, LongPollingManager longPollingManager)
        {
            this.dialogsManager = dialogsManager;
            this.messagesManager = messagesManager;
            this.longPollingManager = longPollingManager;
            NavigationPage.SetHasNavigationBar(this, false);
            BackgroundColor = Color.White;
            Content = loginWebView;
            Appearing += OnAppearing;
        }

        private async void OnNavigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Result == WebNavigationResult.Failure)
            {
                new CustomPopup(
                        LocalizedStrings.AuthNoInternetError,
                        LocalizedStrings.Retry,
                        OnAppearing)
                    .Show();
                return;
            }

            var url = new Uri(((UrlWebViewSource)loginWebView.Source).Url);
            if (AuthorizationManager.AuthorizeFromUrl(url))
            {
                await AuthorizationManager.SetPhoto();
                Navigation.InsertPageBefore(new DialogsPage(dialogsManager, messagesManager), Navigation.NavigationStack[0]);
                await Navigation.PopToRootAsync();
                _ = longPollingManager.Start().ConfigureAwait(false);
                refreshingPopup?.Dismiss();
                refreshingPopup = null;
                return;
            }

            var script = AuthorizationPageScript.Script
                .Replace("{PleaseWait}", LocalizedStrings.PleaseWait);
            loginWebView.Eval(script);

            refreshingPopup?.Dismiss();
            refreshingPopup = null;
        }

        private void OnAppearing(object sender, EventArgs e)
        {
            loginWebView.Navigating += CreatePopup;
            loginWebView.Navigated += OnNavigated;
            loginWebView.Source = AuthorizationClient.GetAuthorizeUri();
        }

        private void CreatePopup(object? s = null, EventArgs? e = null)
        {
            refreshingPopup?.Dismiss();
            refreshingPopup = new InformationPopup
            {
                IsProgressRunning = true,
                Text = LocalizedStrings.Loading
            };
            refreshingPopup.Show();
        }
    }
}
