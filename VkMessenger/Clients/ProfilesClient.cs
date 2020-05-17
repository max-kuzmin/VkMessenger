using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class ProfilesClient
    {
        private static Profile FromJson(JObject profile)
        {
            return new Profile
            {
                Id = profile["id"].Value<uint>(),
                Name = profile["first_name"].Value<string>(),
                Surname = profile["last_name"].Value<string>(),
                Photo = ImageSource.FromUri(new Uri(profile["photo_50"].Value<string>())),
                Online = profile["online"].Value<uint>() != 0
            };
        }

        public static IReadOnlyCollection<Profile> FromJsonArray(JArray profiles)
        {
            return profiles == null
                ? Array.Empty<Profile>()
                : profiles.Select(item => FromJson(item as JObject)).ToArray();
        }
    }
}
