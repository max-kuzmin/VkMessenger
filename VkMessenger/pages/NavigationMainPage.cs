using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class NavigationMainPage : NavigationPage
    {
        public NavigationMainPage()
        {
            SetHasNavigationBar(this, false);

            if (Authorization.Token != null)
                Navigation.PushAsync(new DialogsPage());
            else
                Navigation.PushAsync(new AuthorizationPage());

            LongPollingClient.OnFullReset += async (s, e) =>
            {
                await Navigation.PushAsync(new DialogsPage());
            };
        }
    }
}
