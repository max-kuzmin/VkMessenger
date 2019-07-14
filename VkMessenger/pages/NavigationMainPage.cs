using System;
using System.Threading.Tasks;
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
                var newPage = new DialogsPage();
                await Navigation.PushAsync(newPage);
                await Task.Delay(TimeSpan.FromSeconds(1)); //to prevent app closing

                foreach (var page in Navigation.NavigationStack)
                {
                    if (page != newPage && page != this)
                        Navigation.RemovePage(page);
                }
            };
        }
    }
}
