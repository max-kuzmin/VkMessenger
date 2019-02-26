using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Collections.Generic;

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
                Photo = group["photo_50"].Value<string>()
            };
        }

        public static List<Group> FromJsonArray(JArray groups)
        {
            var result = new List<Group>();

            foreach (var item in groups)
            {
                result.Add(FromJson(item as JObject));
            }

            return result;
        }
    }
}
