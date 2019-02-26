using System;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Message
    {
        public uint Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public Profile Profile { get; set; }

        public int SenderId => (int)(Profile?.Id ?? Authorization.UserId);
        public string Photo => Profile?.Photo ?? Authorization.Photo;
    }
}
