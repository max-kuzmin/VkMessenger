using System;
using System.Net;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Exceptions;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Managers;
using ru.MaxKuzmin.VkMessenger.Pages;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Helpers
{
    public static class NetExceptionCatchHelpers
    {
        public static async Task CatchNetException(
            Func<Task> action,
            Func<Task> retryAction,
            string noInternetError,
            bool quitOnUnknown)
        {
            try
            {
                await action();
            }
            catch (WebException)
            {
                new CustomPopup(
                        noInternetError,
                        LocalizedStrings.Retry,
                        async () => await retryAction())
                    .Show();
            }
            catch (InvalidSessionException)
            {
                new CustomPopup(
                        LocalizedStrings.InvalidSessionError,
                        LocalizedStrings.Ok,
                        AuthorizationManager.CleanUserAndExit)
                    .Show();
            }
            catch (Exception ex)
            {
                new CustomPopup(
                        ex.ToString(),
                        LocalizedStrings.Ok,
                        quitOnUnknown ? Application.Current.Quit : (Action?)null)
                    .Show();
            }
        }
    }
}
