using System;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Net;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Dtos;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class AuthorizationClient
    {
        private const int TokenLength = 85;
        private const int MessagesAccessFlag = 4096;
        private const int OfflineAccessFlag = 65536;
        private const int DocsFlag = 131072;
        private const int AppClientId = 6872680;

        public static string GetAuthorizeUri()
        {
            return
                "https://oauth.vk.com/authorize" +
                "?client_id=" + AppClientId +
                "&scope=" + (MessagesAccessFlag + OfflineAccessFlag + DocsFlag) +
                "&response_type=token" +
                "&v=5.124";
        }

        public static (string Token, int UserId)? SetUserFromUrl(string url)
        {
            var token = string.Concat(Regex.Match(url, @"access_token=(\d|\w)*").Value.Skip(13));
            var userIdString = string.Concat(Regex.Match(url, @"user_id=\d*").Value.Skip(8));

            if (token.Length == TokenLength && int.TryParse(userIdString, out var userId))
                return (token, userId);

            return null;
        }

        public static async Task<Uri> GetPhoto(string token, int userId)
        {
            try
            {
                var url =
                    "https://api.vk.com/method/users.get" +
                    "?user_ids=" + userId +
                    "&v=5.124" +
                    "&fields=photo_50" +
                    "&access_token=" + token;

                using var client = new ProxiedWebClient();
                var json = await HttpHelpers.RetryIfEmptyResponse<JsonDto<UserDto[]>>(
                    () => client.DownloadStringTaskAsync(url), e => e?.response != null);

                return json.response.First().photo_50;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }
    }
}
