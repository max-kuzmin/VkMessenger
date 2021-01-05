using System;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Json
{
    public sealed class ImageSourceJsonConverter : JsonConverter<ImageSource>
    {
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, ImageSource? value, JsonSerializer serializer)
        {
            if (value != null && value is UriImageSource uriValue)
                writer.WriteValue(uriValue.Uri.ToString());
            else
                writer.WriteUndefined();
        }

        /// <inheritdoc />
        public override ImageSource ReadJson(JsonReader reader, Type objectType, ImageSource? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.Value is string value && !string.IsNullOrWhiteSpace(value))
                return ImageSource.FromUri(new Uri(value));
            else
                return null!;
        }
    }
}
