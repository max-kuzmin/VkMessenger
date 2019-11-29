using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Net;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Authorization = ru.MaxKuzmin.VkMessenger.Models.Authorization;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class AuthorizationClient
    {
        private const int TokenLength = 85;
        private const int MessagesAccessFlag = 4096;
        private const int OfflineAccessFlag = 65536;

        public static string GetAuthorizeUri()
        {
            return
                "https://oauth.vk.com/authorize" +
                "?client_id=" + Authorization.ClientId +
                "&scope=" + (MessagesAccessFlag + OfflineAccessFlag) +
                "&response_type=token" +
                "&v=5.92";
        }

        public static async Task<bool> SetUserFromUrl(string url)
        {
            var token = string.Concat(Regex.Match(url, @"access_token=(\d|\w)*").Value.Skip(13));
            var userIdString = string.Concat(Regex.Match(url, @"user_id=\d*").Value.Skip(8));

            if (token.Length == TokenLength && uint.TryParse(userIdString, out var userId))
            {
                Authorization.Token = token;
                Authorization.UserId = userId;
                await GetPhoto();
                return true;
            }
            else return false;
        }

        private static async Task GetPhoto()
        {
            var url =
                "https://api.vk.com/method/users.get" +
                "?user_ids=" + Authorization.UserId +
                "&v=5.92" +
                "&fields=photo_50" +
                "&access_token=" + Authorization.Token;

            using (var client = new ProxiedWebClient())
            {
                var json = JObject.Parse(await client.DownloadStringTaskAsync(url));
                Logger.Debug(json.ToString());

                Authorization.SetPhoto(json["response"][0]["photo_50"].Value<string>());
            }
        }
    }
}
