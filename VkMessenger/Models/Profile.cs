using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Profile
    {
        public uint Id { get; set; }
        public string Name { get; set; } = default!;
        public string Surname { get; set; } = default!;
        public ImageSource? Photo { get; set; }
        public bool Online { get; set; }
    }
}
