using ru.MaxKuzmin.VkMessenger.Clients;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class NavigationMainPage : NavigationPage
    {
        public NavigationMainPage()
        {
            SetHasNavigationBar(this, false);

            if (AuthorizationClient.Token != null)
                PushAsync(new DialogsPage());
            else
                PushAsync(new AuthorizationPage());
        }
    }
}
