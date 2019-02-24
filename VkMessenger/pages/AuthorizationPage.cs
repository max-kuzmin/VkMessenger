using ru.MaxKuzmin.VkMessenger.Clients;
using System.Threading.Tasks;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class AuthorizationPage : CirclePage
    {
        WebView loginWebView = new WebView();

        public AuthorizationPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            Content = loginWebView;
            loginWebView.Source = AuthorizationClient.GetAutorizeUri();
            loginWebView.Navigated += LoginCallback;
        }

        private async void LoginCallback(object sender, WebNavigatedEventArgs e)
        {
            await Task.Delay(1000);
            var url = (loginWebView.Source as UrlWebViewSource).Url;
            if (AuthorizationClient.SetUserFromUrl(url))
            {
                loginWebView.Navigated -= LoginCallback;
                await Navigation.PushAsync(new DialogsPage());
                Navigation.RemovePage(this);
            }
        }
    }
}
