using Newtonsoft.Json;
using ru.MaxKuzmin.VkMessenger.Json;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public sealed class AttachmentImage
    {
        [JsonConverter(typeof(ImageSourceJsonConverter))]
        public ImageSource Url { get; set; } = default!;
        public bool IsSticker { get; set; }
    }
}
