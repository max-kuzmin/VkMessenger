using System;
using ru.MaxKuzmin.VkMessenger.Clients;
using System.Threading.Tasks;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class AuthorizationPage : CirclePage
    {
        readonly WebView loginWebView = new WebView();

        public AuthorizationPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            Content = loginWebView;
            loginWebView.Navigated += LoginCallback;
        }

        private async void LoginCallback(object sender, WebNavigatedEventArgs e)
        {
            await Task.Delay(TimeSpan.FromSeconds(2)); //wait page loading
            var url = (loginWebView.Source as UrlWebViewSource).Url;
            if (await AuthorizationClient.SetUserFromUrl(url))
            {
                loginWebView.Navigated -= LoginCallback;
                await Navigation.PushAsync(new DialogsPage());
            }
        }

        protected override void OnAppearing()
        {
            loginWebView.Source = AuthorizationClient.GetAuthorizeUri();
            base.OnAppearing();
        }
    }
}
