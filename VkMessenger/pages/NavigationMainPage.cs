using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class NavigationMainPage : NavigationPage
    {
        private class EmptyPage : CirclePage
        {
            protected override bool OnBackButtonPressed() => true;
        }

        public NavigationMainPage()
        {
            SetHasNavigationBar(this, false);

            LongPollingClient.OnFullReset += async (s, e) =>
            {
                await PopToRootAsync();
                await PushAsync(new DialogsPage());
            };

            PushAsync(new EmptyPage());

            if (Authorization.Token != null)
                PushAsync(new DialogsPage());
            else
                PushAsync(new AuthorizationPage());
        }
    }
}
