#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class StickerAttachmentDto
    {
        public StickerSizeDto[] images { get; set; } = default!;
    }
}
