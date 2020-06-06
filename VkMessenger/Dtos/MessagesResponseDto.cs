namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class MessagesResponseDto
    {
        public MessageDto[] items { get; set; } = default!;

        public ProfileDto[]? profiles { get; set; }

        public GroupDto[]? groups { get; set; }

        public ConversationDto[]? conversations { get; set; }

        public int count { get; set; }
    }
}
