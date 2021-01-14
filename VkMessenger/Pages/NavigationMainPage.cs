using ru.MaxKuzmin.VkMessenger.Managers;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class NavigationMainPage : NavigationPage
    {
        public NavigationMainPage()
        {
            SetHasNavigationBar(this, false);

            if (AuthorizationManager.Token != null)
                PushAsync(new DialogsPage());
            else
                PushAsync(new AuthorizationPage());
        }
    }
}
