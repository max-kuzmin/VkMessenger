using System;

namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class AudioMessageDto
    {
        public Uri link_mp3 { get; set; } = default!;

        public Uri link_ogg { get; set; } = default!;
    }
}
