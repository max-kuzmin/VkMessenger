using Tizen.Applications;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class NavigationMainPage : NavigationPage
    {
        public NavigationMainPage()
        {
            SetHasNavigationBar(this, false);

            if (Preference.Contains(Setting.TokenKey))
            {
                PushAsync(new DialogsPage());
            }
            else
            {
                PushAsync(new LoginPage());
            }
        }
    }
}
