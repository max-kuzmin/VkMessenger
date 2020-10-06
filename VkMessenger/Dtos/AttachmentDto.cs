#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class AttachmentDto
    {
        public string type { get; set; } = default!;

        public PhotoAttachmentDto? photo { get; set; }

        public LinkDto? link { get; set; }

        public StickerAttachmentDto? sticker { get; set; }

        public AudioMessageDto? audio_message { get; set; }
    }
}
