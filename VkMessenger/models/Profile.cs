using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public ImageSource Photo { get; set; }
        public bool IsOnline { get; set; }
    }
}
