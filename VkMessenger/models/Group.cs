using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ImageSource Photo { get; set; }
    }
}
