using ru.MaxKuzmin.VkMessenger.Pages;
using System.Timers;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger
{
    public class App : Application
    {
        public static Timer UpdateTimer { get; } = new Timer(Setting.UpdateInterval);

        protected override void OnStart()
        {
#if DEBUG
            Tizen.Applications.Preference.Set(Setting.TokenKey, DebugSetting.Token);
#endif
            MainPage = new NavigationMainPage();
            UpdateTimer.Start();
        }

        protected override void OnSleep()
        {
            UpdateTimer.Stop();
        }

        protected override void OnResume()
        {
            UpdateTimer.Start();
        }
    }
}
