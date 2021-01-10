using System;

#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class ProfileDto
    {
        /// <summary>
        /// Positive number
        /// </summary>
        public int id { get; set; }

        public string first_name { get; set; } = default!;

        public string last_name { get; set; } = default!;

        public Uri photo_50 { get; set; } = default!;

        public int online { get; set; }
    }
}
