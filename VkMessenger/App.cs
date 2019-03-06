using ru.MaxKuzmin.VkMessenger.Pages;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger
{
    public class App : Application
    {
        protected override void OnStart()
        {
#if DEBUG
            DebugSetting.Set();
#endif

            MainPage = new NavigationMainPage();
            Network.Start();
        }

        protected override void OnSleep()
        {
            Network.Stop();
        }

        protected override void OnResume()
        {
            Network.Start();
        }
    }
}
