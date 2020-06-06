using System;

#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class UserDto
    {
        public int id { get; set; }

        public Uri photo_50 { get; set; } = default!;
    }
}
