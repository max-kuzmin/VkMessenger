namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class LongPollingInitResponseDto
    {
        public string key { get; set; } = default!;

        public string server { get; set; } = default!;

        public int ts { get; set; }
    }
}
