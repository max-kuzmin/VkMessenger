using System;

namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class UploadLinkResponseDto
    {
        public Uri upload_url { get; set; } = default!;
    }
}
