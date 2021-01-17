using ru.MaxKuzmin.VkMessenger.Managers;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class NavigationMainPage : NavigationPage
    {
        public NavigationMainPage(DialogsManager dialogsManager, MessagesManager messagesManager, LongPollingManager longPollingManager)
        {
            SetHasNavigationBar(this, false);

            if (AuthorizationManager.Token != null)
                PushAsync(new DialogsPage(dialogsManager, messagesManager));
            else
                PushAsync(new AuthorizationPage(dialogsManager, messagesManager, longPollingManager));
        }
    }
}
