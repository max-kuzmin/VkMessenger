using System;

namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class GroupDto
    {
        public int id { get; set; }

        public string name { get; set; } = default!;

        public Uri photo_50 { get; set; } = default!;
    }
}
