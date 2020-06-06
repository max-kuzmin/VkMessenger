using Newtonsoft.Json.Linq;

#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class LongPollingUpdatesJsonDto
    {
        public int? ts { get; set; }

        public JToken[][]? updates { get; set; }

        public int? failed { get; set; }
    }
}
