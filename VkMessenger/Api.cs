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
            var url =
                "https://api.vk.com/method/messages.getConversations" +
                "?v=5.92" +
                "&extended=1" +
                "&access_token=" + Preference.Get<string>(Setting.TokenKey);

            using (var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        public static string GetMessagesJson(int peerId)
        {
            var url =
                "https://api.vk.com/method/messages.getHistory" +
                "?v=5.92" +
                "&peer_id=" + peerId +
                "&access_token=" + Preference.Get<string>(Setting.TokenKey);

            using (var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        public static void MarkAsRead(int peerId)
        {
            var url =
                "https://api.vk.com/method/messages.markAsRead" +
                "?v=5.92" +
                "&peer_id=" + peerId +
                "&access_token=" + Preference.Get<string>(Setting.TokenKey);

            using (var client = new WebClient())
            {
                client.DownloadString(url);
            }
        }
    }
}
