using System.Linq;
using System.Text.RegularExpressions;
using Tizen.Applications;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class AuthorizationClient
    {
        private const string TokenKey = "Token";

#if DEBUG
        static AuthorizationClient()
        {
            Token = DebugSetting.Token;
        }
#endif

        public static string GetAutorizeUri()
        {
            return
                "https://oauth.vk.com/authorize" +
                "?client_id=" + Setting.ClientId +
                "&redirect_uri=https://oauth.vk.com/blank.html" +
                "&scope=4096" +
                "&response_type=token" +
                "&v=5.92";
        }

        public static string Token
        {
            get => Preference.Contains(TokenKey) ? Preference.Get<string>(TokenKey) : null;
            set => Preference.Set(TokenKey, value);
        }

        public static bool SetToken(string urlWithToken)
        {
            var token = string.Concat(Regex.Match(urlWithToken, @"access_token=(\d|\w)*").Value.Skip(13));
            if (token.Length == 85)
            {
                Preference.Set(TokenKey, token);
                return true;
            }
            else return false;
        }
    }
}
