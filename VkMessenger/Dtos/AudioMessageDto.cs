using System;

namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class AudioMessageDto
    {
        public Uri link_mp3 { get; set; } = default!;

        public int duration { get; set; }

        public long id { get; set; }
    }
}
