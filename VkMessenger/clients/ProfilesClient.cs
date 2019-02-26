﻿using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Collections.Generic;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class ProfilesClient
    {
        public static Profile FromJson(JObject profile)
        {
            return new Profile
            {
                Id = profile["id"].Value<uint>(),
                Name = profile["first_name"].Value<string>(),
                Surname = profile["last_name"].Value<string>(),
                Photo = profile["photo_50"].Value<string>(),
                IsOnline = profile["online"].Value<int>() != 0
            };
        }

        public static List<Profile> FromJsonArray(JArray profiles)
        {
            var result = new List<Profile>();

            foreach (var item in profiles)
            {
                result.Add(FromJson(item as JObject));
            }

            return result;
        }
    }
}