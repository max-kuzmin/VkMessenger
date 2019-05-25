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

        public static UriImageSource Photo
        {
            get
            {
                if (photoSource == null && Preference.Contains(UserIdKey))
                {
                    photoSource = new UriImageSource
                    {
                        Uri = new Uri(Preference.Get<string>(PhotoKey)),
                        CachingEnabled = true,
                        CacheValidity = TimeSpan.FromDays(1)
                    };
                }
                return photoSource;
            }
        }

        public static void SetPhoto(string url)
        {
            Preference.Set(PhotoKey, url);
            photoSource = new UriImageSource
            {
                Uri = new Uri(url),
                CachingEnabled = true,
                CacheValidity = TimeSpan.FromDays(1)
            };
        }

        private static UriImageSource photoSource = null;
    }
}
