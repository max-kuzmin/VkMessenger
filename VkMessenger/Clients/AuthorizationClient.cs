using System;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Net;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ru.MaxKuzmin.VkMessenger.Dtos;
using Xamarin.Forms;
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
                "&v=5.124";
        }

        public static async Task<bool> SetUserFromUrl(string url)
        {
            try
            {
                var token = string.Concat(Regex.Match(url, @"access_token=(\d|\w)*").Value.Skip(13));
                var userIdString = string.Concat(Regex.Match(url, @"user_id=\d*").Value.Skip(8));

                if (token.Length == TokenLength && int.TryParse(userIdString, out var userId))
                {
                    Authorization.Token = token;
                    Authorization.UserId = userId;
                    await GetPhoto();
                    return true;
                }
                
                return false;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        public static void CleanUserAndExit()
        {
            Authorization.Token = null;
            Application.Current.Quit();
        }

        private static async Task GetPhoto()
        {
            var url =
                "https://api.vk.com/method/users.get" +
                "?user_ids=" + Authorization.UserId +
                "&v=5.124" +
                "&fields=photo_50" +
                "&access_token=" + Authorization.Token;

            using var client = new ProxiedWebClient();
            var json = JsonConvert.DeserializeObject<JsonDto<UserDto[]>>(await client.DownloadStringTaskAsync(url));
            Logger.Debug(json.ToString());

            Authorization.SetPhoto(json.response.First().photo_50);
        }
    }
}
