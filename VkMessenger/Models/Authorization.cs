using System;
using Tizen.Applications;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public static class Authorization
    {
        public const int ClientId = 6872680;

        private const string TokenKey = "Token2";
        private const string UserIdKey = "UserId";
        private const string PhotoKey = "Photo";

        private static string? token;
        private static int userId;
        private static ImageSource? photoSource;

        public static string? Token
        {
            get
            {
                if (token == null)
                    token = Preference.Contains(TokenKey) ? Preference.Get<string>(TokenKey) : null;
                return token;
            }
            set
            {
                if (value != null)
                {
                    Preference.Set(TokenKey, value);
                    token = value;
                }
                else
                {
                    Preference.Remove(TokenKey);
                }
            }
        }

        public static int UserId
        {
            get
            {
                if (userId == 0)
                    userId = Preference.Contains(UserIdKey) ? Preference.Get<int>(UserIdKey) : 0;
                return userId;
            }
            set
            {
                Preference.Set(UserIdKey, value);
                userId = value;
            }
        }

        public static ImageSource? Photo
        {
            get
            {
                if (photoSource == null && userId != 0)
                    photoSource = ImageSource.FromUri(new Uri(Preference.Get<string>(PhotoKey)));
                return photoSource;
            }
        }

        public static void SetPhoto(Uri url)
        {
            Preference.Set(PhotoKey, url.ToString());
            photoSource = ImageSource.FromUri(url);
        }
    }
}
