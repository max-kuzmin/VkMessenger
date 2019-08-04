using System;
using ru.MaxKuzmin.VkMessenger.Clients;
using System.Threading.Tasks;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class AuthorizationPage : CirclePage
    {
        readonly WebView loginWebView = new WebView
        {
            Margin = new Thickness(50, 0, 50, 0)
        };

        public AuthorizationPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            Content = loginWebView;
            loginWebView.Navigated += LoginCallback;
        }

        private async void LoginCallback(object sender, WebNavigatedEventArgs e)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.5)); // Wait page loading

            loginWebView.Eval("$('.FloatBtn').style.display = 'none'");

            var url = (loginWebView.Source as UrlWebViewSource).Url;
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
