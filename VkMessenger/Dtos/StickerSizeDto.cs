using System;

#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class StickerSizeDto
    {
        public Uri url { get; set; } = default!;

        public int width { get; set; }

        public int height { get; set; }
    }
}
