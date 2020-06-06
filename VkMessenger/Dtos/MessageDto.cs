namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class MessageDto
    {
        public int date { get; set; }

        public int from_id { get; set; }

        public int id { get; set; }

        public string text { get; set; } = default!;

        public MessageDto[]? fwd_messages { get; set; }

        public AttachmentDto[]? attachments { get; set; }
    }
}
