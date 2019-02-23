namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class AuthorizationClient
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
    }
}
