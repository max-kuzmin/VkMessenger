namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class DialogDto
    {
        public ConversationDto conversation { get; set; } = default!;

        public MessageDto? last_message { get; set; }
    }
}
