using Tizen.Applications;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public static class Authorization
    {
        private const string TokenKey = "Token";
        private const string UserIdKey = "UserId";
        private const string PhotoKey = "Photo";

        public static string Token
        {
            get => Preference.Contains(TokenKey) ? Preference.Get<string>(TokenKey) : null;
            set { if (value != null) Preference.Set(TokenKey, value); }
        }

        public static int UserId
        {
            get => Preference.Contains(UserIdKey) ? Preference.Get<int>(UserIdKey) : 0;
            set => Preference.Set(UserIdKey, value);
        }

        public static string Photo
        {
            get => Preference.Contains(UserIdKey) ? Preference.Get<string>(PhotoKey) : null;
            set => Preference.Set(PhotoKey, value);
        }
    }
}
