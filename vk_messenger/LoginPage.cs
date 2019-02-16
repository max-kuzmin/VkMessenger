using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tizen.Applications;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace vk_messenger
{
    public class LoginPage : CirclePage
    {
        const string autorize = "https://oauth.vk.com/authorize?" +
            "client_id=6863412" +
            "&redirect_uri=https://oauth.vk.com/blank.html" +
            "&scope=4096" +
            "&response_type=token" +
            "&v=5.92";

        string token = null;
        WebView loginWebView = null;

        public LoginPage()
        {
            loginWebView = new WebView();
            loginWebView.Navigated += LoginCallback;
            Content = loginWebView;
            loginWebView.Source = autorize;
        }

        private async void LoginCallback(object sender, WebNavigatedEventArgs e)
        {
            await Task.Delay(1000);
            var uri = (loginWebView.Source as UrlWebViewSource).Url;
            token = string.Concat(Regex.Match(uri, @"access_token=(\d|\w)*").Value.Skip(13));
            if (token != null)
            {
                Preference.Set("token", token);
                loginWebView.Navigated -= LoginCallback;
            }
        }
    }
}
