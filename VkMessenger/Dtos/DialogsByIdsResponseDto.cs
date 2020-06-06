#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class DialogsByIdsResponseDto
    {
        public ConversationDto[] items { get; set; } = default!;

        public ProfileDto[]? profiles { get; set; }

        public GroupDto[]? groups { get; set; }
    }
}
