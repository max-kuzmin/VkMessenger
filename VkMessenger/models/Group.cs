using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ImageSource Photo { get; set; }

        public static List<Group> FromJsonArray(JArray groups)
        {
            var result = new List<Group>();

            foreach (var item in groups)
            {
                result.Add(FromJson(item as JObject));
            }

            return result;
        }

        public static Group FromJson(JObject group)
        {
            return new Group
            {
                Id = group["id"].Value<int>(),
                Name = group["name"].Value<string>(),
                Photo = group["photo_50"].Value<string>()
            };
        }
    }
}
