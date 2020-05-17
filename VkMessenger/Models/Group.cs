using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Group
    {
        public uint Id { get; set; }
        public string Name { get; set; } = default!;
        public ImageSource? Photo { get; set; }
    }
}
