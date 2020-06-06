#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class DialogsResponseDto
    {
        public DialogDto[] items { get; set; } = default!;

        public ProfileDto[]? profiles { get; set; }

        public GroupDto[]? groups { get; set; }
    }
}
