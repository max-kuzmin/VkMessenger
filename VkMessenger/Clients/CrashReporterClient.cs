using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Net;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    internal static class CrashReporterClient
    {
        public static async Task SendAsync(string text)
        {
            try
            {
                const string url = "https://vkmessenger.azurewebsites.net/api/crash";

                using var client = new ProxiedWebClient();
                await client.UploadStringTaskAsync(url, text);
            }
            catch { }
        }
    }
}
