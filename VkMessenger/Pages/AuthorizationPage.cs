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
        private readonly WebView loginWebView = new WebView();
        private InformationPopup? refreshingPopup;

        public AuthorizationPage(DialogsManager dialogsManager, MessagesManager messagesManager)
        {
            this.dialogsManager = dialogsManager;
            this.messagesManager = messagesManager;
            NavigationPage.SetHasNavigationBar(this, false);
            BackgroundColor = Color.White;
            Content = loginWebView;
            loginWebView.Navigated += OnNavigated;
        }

        private async void OnNavigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Result == WebNavigationResult.Failure && refreshingPopup != null)
            {
                new CustomPopup(
                        LocalizedStrings.AuthNoInternetError,
                        LocalizedStrings.Retry,
                        OnAppearing)
                    .Show();
                return;
            }

            refreshingPopup?.Dismiss();
            refreshingPopup = null;

            loginWebView.Eval(@"
                function hide(elem) {
                    var elems = document.getElementsByClassName(elem);
                    if (elems.length > 0) elems[0].style.display = 'none';
                }

                function white(elem) {
                    var elems = document.getElementsByClassName(elem);
                    if (elems.length > 0) elems[0].style.backgroundColor = 'white';
                }

                function hideAll() {
                    if (document.getElementsByClassName('button').length === 0) {
                        document.body.innerText = '" + LocalizedStrings.PleaseWait + @"';
                        document.body.style.textAlign = 'center';
                        document.body.style.paddingTop = '150px';
                    }

                    hide('mh_btn_label');
                    hide('near_btn');
                    hide('fi_header fi_header_light');
                    hide('button wide_button gray_button');

                    document.body.style.marginLeft = '50px';
                    document.body.style.marginRight = '50px';
                    document.body.style.backgroundColor = 'white';
                    white('basis__content mcont');
                    white('vk__page');
                }

                window.addEventListener('load', e => hideAll());
                hideAll();
             ");

            var url = ((UrlWebViewSource)loginWebView.Source).Url;

            if (await AuthorizationManager.AuthorizeFromUrl(url))
            {
                loginWebView.Navigated -= OnNavigated;
                Navigation.InsertPageBefore(new DialogsPage(dialogsManager, messagesManager), Navigation.NavigationStack[0]);
                await Navigation.PopToRootAsync();
            }
        }

        protected override void OnAppearing()
        {
            refreshingPopup?.Dismiss();
            refreshingPopup = new InformationPopup
            {
                Text = LocalizedStrings.LoadingAuthPage
            };
            refreshingPopup.Show();
            loginWebView.Source = AuthorizationClient.GetAuthorizeUri();
            base.OnAppearing();
        }
    }
}
