using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Pages;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger
{
    public class App : Application
    {
        protected override void OnStart()
        {
            MainPage = new NavigationMainPage();
        }

        protected override void OnSleep()
        {
            LongPollingClient.Stop();
        }

        protected override void OnResume()
        {
            LongPollingClient.Start().Start();
        }
    }
}
