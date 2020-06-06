using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ru.MaxKuzmin.VkMessenger.Loggers;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class GroupsClient
    {
        private static Group FromJson(JObject group)
        {
            return new Group
            {
                Id = group["id"]!.Value<int>(),
                Name = group["name"]!.Value<string>(),
                Photo = ImageSource.FromUri(new Uri(group["photo_50"]!.Value<string>()))
            };
        }

        public static IReadOnlyCollection<Group> FromJsonArray(JArray groups)
        {
            try
            {
                return groups == null
                    ? Array.Empty<Group>()
                    : groups.Select(item => FromJson((JObject) item)).ToArray();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }
    }
}
