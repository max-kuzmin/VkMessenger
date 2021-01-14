using ru.MaxKuzmin.VkMessenger.Managers;
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
            LongPollingManager.Stop();
            base.OnSleep();
        }

        protected override void OnResume()
        {
            _ = LongPollingManager.Start().ConfigureAwait(false);
            base.OnResume();
        }
    }
}
