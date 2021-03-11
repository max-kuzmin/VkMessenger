using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Exceptions;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Managers;
using ru.MaxKuzmin.VkMessenger.Pages;

namespace ru.MaxKuzmin.VkMessenger.Helpers
{
    public static class NetExceptionCatchHelpers
    {
        public static async Task CatchNetException(
            Func<Task> action,
            Func<Task> retryAction,
            string noInternetError)
        {
            try
            {
                await action();
            }
            catch (Exception e) when(e is HttpRequestException || e is WebException || e is EmptyHttpResponseException)
            {
                new CustomPopup(
                        noInternetError,
                        LocalizedStrings.Retry,
                        async () => await retryAction(),
                        true)
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
                        null,
                        true)
                    .Show();
            }
        }
    }
}
