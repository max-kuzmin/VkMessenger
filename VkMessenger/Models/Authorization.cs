using System;
using Tizen.Applications;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public static class Authorization
    {
        public const uint ClientId = 6872680;

        private const string TokenKey = "Token";
        private const string UserIdKey = "UserId";
        private const string PhotoKey = "Photo";

        public static string Token
        {
            get => Preference.Contains(TokenKey) ? Preference.Get<string>(TokenKey) : null;
            set { if (value != null) Preference.Set(TokenKey, value); }
        }

        public static uint UserId
        {
            get => Preference.Contains(UserIdKey) ? (uint)Preference.Get<int>(UserIdKey) : 0u;
            set => Preference.Set(UserIdKey, (int)value);
        }

        public static ImageSource Photo
        {
            get
            {
                if (photoSource == null && Preference.Contains(UserIdKey))
                {
                    photoSource = ImageSource.FromUri(new Uri(Preference.Get<string>(PhotoKey)));
                }
                return photoSource;
            }
        }

        public static void SetPhoto(string url)
        {
            Preference.Set(PhotoKey, url);
            photoSource = ImageSource.FromUri(new Uri(url));
        }

        private static ImageSource photoSource;
    }
}
