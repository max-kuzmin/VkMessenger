using Newtonsoft.Json;
using ru.MaxKuzmin.VkMessenger.Json;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Profile
    {
        /// <summary>
        /// Positive number
        /// </summary>
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Surname { get; set; } = default!;
        [JsonConverter(typeof(ImageSourceJsonConverter))]
        public ImageSource? Photo { get; set; }
        public bool Online { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Profile profile &&
                   Id == profile.Id &&
                   Name == profile.Name &&
                   Surname == profile.Surname &&
                   EqualityComparer<ImageSource?>.Default.Equals(Photo, profile.Photo) &&
                   Online == profile.Online;
        }
    }
}
