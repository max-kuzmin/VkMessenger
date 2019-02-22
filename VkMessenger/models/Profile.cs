using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public ImageSource Photo { get; set; }
        public bool IsOnline { get; set; }

        public static List<Profile> FromJsonArray(JArray profiles)
        {
            var result = new List<Profile>();

            foreach (var item in profiles)
            {
                result.Add(FromJson(item as JObject));
            }

            return result;
        }

        public static Profile FromJson(JObject profile)
        {
            return new Profile
            {
                Id = profile["id"].Value<int>(),
                Name = profile["first_name"].Value<string>(),
                Surname = profile["last_name"].Value<string>(),
                Photo = profile["photo_50"].Value<string>(),
                IsOnline = profile["online"].Value<int>() != 0
            };
        }
    }
}
