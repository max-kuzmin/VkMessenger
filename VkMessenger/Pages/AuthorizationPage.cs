using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Localization;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class AuthorizationPage : CirclePage
    {
        private readonly WebView loginWebView = new WebView();

        public AuthorizationPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            BackgroundColor = Color.White;
            Content = loginWebView;
            loginWebView.Navigated += LoginCallback;
        }

        private async void LoginCallback(object sender, WebNavigatedEventArgs e)
        {
            if (e.Result == WebNavigationResult.Failure)
            {
                new CustomPopup(
                        LocalizedStrings.AuthNoInternetError,
                        LocalizedStrings.Retry,
                        () => loginWebView.Source = AuthorizationClient.GetAuthorizeUri())
                    .Show();
                return;
            }

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
                        document.body.innerText = 'Loading...';
                        document.body.style.textAlign = 'center';
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

                window.addEventListener('load', (e) => hideAll());
                hideAll();
             ");

            var url = ((UrlWebViewSource)loginWebView.Source).Url;
            if (await AuthorizationClient.SetUserFromUrl(url))
            {
                loginWebView.Navigated -= LoginCallback;
                Navigation.InsertPageBefore(new DialogsPage(), Navigation.NavigationStack[0]);
                await Navigation.PopToRootAsync();
            }
        }

        protected override void OnAppearing()
        {
            loginWebView.Source = AuthorizationClient.GetAuthorizeUri();
            base.OnAppearing();
        }
    }
}
