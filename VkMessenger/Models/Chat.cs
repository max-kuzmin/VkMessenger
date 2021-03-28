using Newtonsoft.Json;
using ru.MaxKuzmin.VkMessenger.Json;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Chat
    {
        /// <summary>
        /// Positive number
        /// </summary>
        public int Id { get; set; }
        [JsonConverter(typeof(ImageSourceJsonConverter))]
        public ImageSource? Photo { get; set; }
        public string Title { get; set; } = default!;

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Chat chat &&
                   Id == chat.Id &&
                   EqualityComparer<ImageSource?>.Default.Equals(Photo, chat.Photo) &&
                   Title == chat.Title;
        }
    }
}
