using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class AuthorizationClient
    {
        public static string GetAutorizeUri()
        {
            return
                "https://oauth.vk.com/authorize" +
                "?client_id=" + Models.Authorization.ClientId +
                "&redirect_uri=https://oauth.vk.com/blank.html" +
                "&scope=" + (4096 + 65536) +
                "&response_type=token" +
                "&v=5.92";
        }

        public static bool SetUserFromUrl(string url)
        {
            var token = string.Concat(Regex.Match(url, @"access_token=(\d|\w)*").Value.Skip(13));
            var userIdString = string.Concat(Regex.Match(url, @"user_id=\d*").Value.Skip(8));
            if (token.Length == 85 && uint.TryParse(userIdString, out var userId))
            {
                Models.Authorization.Token = token;
                Models.Authorization.UserId = userId;
                GetPhoto().Start();
                return true;
            }
            else return false;
        }

        private async static Task GetPhoto()
        {
            var url =
                "https://api.vk.com/method/users.get" +
                "?user_ids=" + Models.Authorization.UserId +
                "&v=5.92" +
                "&fields=photo_50" +
                "&access_token=" + Models.Authorization.Token;

            using (var client = new WebClient())
            {
                var json = JObject.Parse(await client.DownloadStringTaskAsync(url));
                Models.Authorization.SetPhoto(json["response"][0]["photo_50"].Value<string>());
            }
        }
    }
}
