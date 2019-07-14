using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class AuthorizationClient
    {
        public static string GetAuthorizeUri()
        {
            return
                "https://oauth.vk.com/authorize" +
                "?client_id=" + Models.Authorization.ClientId +
                "&redirect_uri=https://oauth.vk.com/blank.html" +
                "&scope=" + (4096 + 65536) +
                "&response_type=token" +
                "&v=5.92";
        }

        public static async Task<bool> SetUserFromUrl(string url)
        {
            var token = string.Concat(Regex.Match(url, @"access_token=(\d|\w)*").Value.Skip(13));
            var userIdString = string.Concat(Regex.Match(url, @"user_id=\d*").Value.Skip(8));
            if (token.Length == 85 && uint.TryParse(userIdString, out var userId))
            {
                Models.Authorization.Token = token;
                Models.Authorization.UserId = userId;
                await GetPhoto();
                return true;
            }
            else return false;
        }

        private static async Task GetPhoto()
        {
            var url =
                "https://api.vk.com/method/users.get" +
                "?user_ids=" + Models.Authorization.UserId +
                "&v=5.92" +
                "&fields=photo_50" +
                "&access_token=" + Models.Authorization.Token;

            using (var client = new ProxiedWebClient())
            {
                var json = JObject.Parse(await client.DownloadStringTaskAsync(url));
                Logger.Debug(json.ToString());

                Models.Authorization.SetPhoto(json["response"][0]["photo_50"].Value<string>());
            }
        }
    }
}
