using System;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Clients;
using Tizen.Applications;
using Xamarin.Forms;
using Application = Xamarin.Forms.Application;

namespace ru.MaxKuzmin.VkMessenger.Managers
{
    public static class AuthorizationManager
    {
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
            private set
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
            private set
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

        public static async Task<bool> AuthorizeFromUrl(string url)
        {
            var result = AuthorizationClient.SetUserFromUrl(url);
            if (result.HasValue)
            {
                Token = result.Value.Token;
                UserId = result.Value.UserId;

                var photoUrl = await AuthorizationClient.GetPhoto(Token, UserId);
                SetPhoto(photoUrl);
            }

            return result.HasValue;
        }

        public static void CleanUserAndExit()
        {
            Token = null;
            Application.Current.Quit();
        }

        private static void SetPhoto(Uri url)
        {
            Preference.Set(PhotoKey, url.ToString());
            photoSource = ImageSource.FromUri(url);
        }
    }
}
