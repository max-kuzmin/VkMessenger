using System;

#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class PhotoSizeDto
    {
        public Uri url { get; set; } = default!;

        public string type { get; set; } = default!;
    }
}
