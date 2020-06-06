#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class PeerDto
    {
        public int id { get; set; }

        public string type { get; set; } = default!;
    }
}
