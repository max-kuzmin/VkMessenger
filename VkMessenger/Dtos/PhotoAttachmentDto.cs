#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class PhotoAttachmentDto
    {
        public PhotoSizeDto[] sizes { get; set; } = default!;
    }
}
