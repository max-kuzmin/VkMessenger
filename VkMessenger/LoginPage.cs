using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tizen.Applications;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace VkMessenger
{
    public class LoginPage : CirclePage
    {
        WebView loginWebView = new WebView();

        public LoginPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            Content = loginWebView;
            loginWebView.Source = Api.GetAutorizeUri();
            loginWebView.Navigated += LoginCallback;
        }

        private async void LoginCallback(object sender, WebNavigatedEventArgs e)
        {
            await Task.Delay(1000);
            var uri = (loginWebView.Source as UrlWebViewSource).Url;
            var token = string.Concat(Regex.Match(uri, @"access_token=(\d|\w)*").Value.Skip(13));
            if (token != string.Empty)
            {
                Preference.Set(Setting.TokenKey, token);
                loginWebView.Navigated -= LoginCallback;

                await Navigation.PushAsync(new DialogsPage());
                Navigation.RemovePage(this);
            }
        }
    }
}
