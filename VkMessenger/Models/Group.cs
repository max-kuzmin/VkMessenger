using Newtonsoft.Json;
using ru.MaxKuzmin.VkMessenger.Json;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Group
    {
        /// <summary>
        /// Positive number
        /// </summary>
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        [JsonConverter(typeof(ImageSourceJsonConverter))]
        public ImageSource? Photo { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Group group &&
                   Id == group.Id &&
                   Name == group.Name &&
                   EqualityComparer<ImageSource?>.Default.Equals(Photo, group.Photo);
        }
    }
}
