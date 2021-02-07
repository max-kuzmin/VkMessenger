using System.Collections.ObjectModel;
using ru.MaxKuzmin.VkMessenger.Layouts;
using ru.MaxKuzmin.VkMessenger.Managers;
using ru.MaxKuzmin.VkMessenger.Models;
using ru.MaxKuzmin.VkMessenger.Pages;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger
{
    public class App : Application
    {
        private readonly ObservableCollection<Dialog> collection;
        private readonly LongPollingManager longPollingManager;
        private readonly DialogsManager dialogsManager;
        private readonly MessagesManager messagesManager;

        public App()
        {
            collection = new ObservableCollection<Dialog>();
            messagesManager = new MessagesManager(collection);
            dialogsManager = new DialogsManager(collection, messagesManager);
            longPollingManager = new LongPollingManager(dialogsManager, messagesManager);
            MainPage = new NavigationMainPage(dialogsManager, messagesManager, longPollingManager);
            longPollingManager.Navigation = MainPage.Navigation;
        }

        protected override void OnSleep()
        {
            _ = longPollingManager.Stop().ConfigureAwait(false);
            AudioLayout.PauseAllPlayers();
            base.OnSleep();
        }

        protected override void OnResume()
        {
            _ = longPollingManager.Start().ConfigureAwait(false);
            base.OnResume();
        }
    }
}
