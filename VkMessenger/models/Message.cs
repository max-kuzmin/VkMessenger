using System;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Sender { get; set; }
        public DateTime Date { get; set; }
    }
}
