using System.Threading.Tasks;
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
            base.OnStart();
        }

        protected override void OnSleep()
        {
            Task.Run(LongPollingClient.Stop);
            base.OnSleep();
        }

        protected override void OnResume()
        {
            Task.Run(LongPollingClient.Start);
            base.OnResume();
        }
    }
}
