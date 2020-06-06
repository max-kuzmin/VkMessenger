namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class DialogsResponseDto
    {
        public DialogDto[] items { get; set; } = default!;

        public ProfileDto[]? profiles { get; set; }

        public GroupDto[]? groups { get; set; }

        public int count { get; set; }

        public int unread_count { get; set; }
    }
}
