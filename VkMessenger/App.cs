using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ru.MaxKuzmin.VkMessenger
{
    public class App : Application
    {
        static App()
        {
            Registrar.Registered.Register(typeof(ProxiedCachedImageSource), typeof(ProxiedCachedImageSourceHandler));
        }

        protected override void OnStart()
        {
#if DEBUG
            DebugSetting.Set();
#endif

            MainPage = new NavigationMainPage();
        }

        protected override void OnSleep()
        {
            LongPollingClient.Stop();
        }

        protected override void OnResume()
        {
            LongPollingClient.Start();
        }
    }
}
