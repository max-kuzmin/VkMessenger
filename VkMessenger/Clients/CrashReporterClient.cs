using System;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Net;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    // ReSharper disable once UnusedMember.Global
    internal static class CrashReporterClient
    {
        // ReSharper disable once UnusedMember.Global
        public static async Task SendAsync(string text)
        {
            try
            {
                const string url = "https://vkmessenger.azurewebsites.net/api/crash";

                using var client = new ProxiedWebClient();
                await client.PostAsync(new Uri(url), text).ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
        }
    }
}
