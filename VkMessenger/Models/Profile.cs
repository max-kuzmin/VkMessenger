using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Profile
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public ImageSource Photo { get; set; }
        public bool Online { get; set; }
    }
}
