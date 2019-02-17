using System.Net;
using Tizen.Applications;

namespace ru.MaxKuzmin.VkMessenger
{
    public static class Api
    {
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


        public static string GetDialogsJson()
        {
            var getDialogsUri =
                "https://api.vk.com/method/messages.getConversations" +
                "?v=5.52" +
                "&extended=1" +
                "&access_token=" + Preference.Get<string>(Setting.TokenKey);

            using (var client = new WebClient())
            {
                return client.DownloadString(getDialogsUri);
            }
        }
    }
}
