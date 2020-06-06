namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class AttachmentDto
    {
        public string type { get; set; } = default!;

        public PhotoAttachmentDto? photo { get; set; }

        public LinkDto? link { get; set; }
    }
}
