using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace VkMessenger.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public Image Photo { get; set; }
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
                Photo = new Image { Source = profile["photo_50"].Value<string>() },
                IsOnline = profile["online"].Value<int>() != 0
            };
        }
    }
}
