using Newtonsoft.Json;
using ru.MaxKuzmin.VkMessenger.Json;
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
    }
}
