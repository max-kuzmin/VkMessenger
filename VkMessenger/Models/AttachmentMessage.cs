namespace ru.MaxKuzmin.VkMessenger.Models
{
    public sealed class AttachmentMessage
    {
        public Profile? Profile { get; set; }
        public string Text { get; set; } = default!;
    }
}
