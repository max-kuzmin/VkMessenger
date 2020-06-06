namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class ChatSettingsDto
    {
        public string title { get; set; } = default!;

        public PhotoDto? photo { get; set; }

        public int[]? active_ids { get; set; }
    }
}
