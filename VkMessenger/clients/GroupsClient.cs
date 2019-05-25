using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class GroupsClient
    {
        public static Group FromJson(JObject group)
        {
            return new Group
            {
                Id = group["id"].Value<uint>(),
                Name = group["name"].Value<string>(),
                Photo = new UriImageSource
                {
                    Uri = new Uri(group["photo_50"].Value<string>()),
                    CachingEnabled = true,
                    CacheValidity = TimeSpan.FromDays(1)
                }
            };
        }

        public static IReadOnlyCollection<Group> FromJsonArray(JArray groups)
        {
            var result = new List<Group>();

            if (groups != null)
            {
                foreach (var item in groups)
                {
                    result.Add(FromJson(item as JObject));
                }
            }

            return result;
        }
    }
}
