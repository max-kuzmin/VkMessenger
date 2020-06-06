using System;

namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class PhotoSizeDto
    {
        public Uri url { get; set; } = default!;

        public string type { get; set; } = default!;
    }
}
