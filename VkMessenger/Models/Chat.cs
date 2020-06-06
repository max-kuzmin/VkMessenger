using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Chat
    {
        /// <summary>
        /// Positive number
        /// </summary>
        public int Id { get; set; }
        public ImageSource? Photo { get; set; }
        public string Title { get; set; } = default!;
    }
}
