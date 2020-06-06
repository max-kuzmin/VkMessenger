namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class LongPollingUpdatesJsonDto
    {
        public int? ts { get; set; }

        public object[][]? updates { get; set; }

        public int? failed { get; set; }
    }
}
