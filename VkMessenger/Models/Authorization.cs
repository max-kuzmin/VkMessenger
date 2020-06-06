using System;
using Tizen.Applications;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public static class Authorization
    {
        public const int ClientId = 6872680;

        private const string TokenKey = "Token";
        private const string UserIdKey = "UserId";
        private const string PhotoKey = "Photo";

        public static string? Token
        {
            get => Preference.Contains(TokenKey) ? Preference.Get<string>(TokenKey) : null;
            set
            {
                if (value != null)
                {
                    Preference.Set(TokenKey, value);
                }
                else
                {
                    Preference.Remove(TokenKey);
                }
            }
        }

        public static int UserId
        {
            get => Preference.Contains(UserIdKey) ? Preference.Get<int>(UserIdKey) : 0;
            set => Preference.Set(UserIdKey, value);
        }

        public static ImageSource? Photo
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

        private static ImageSource? photoSource;
    }
}
