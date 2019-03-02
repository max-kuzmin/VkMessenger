namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class UserTypingEventArgs
    {
        public uint UserId { get; set; }

        public int DialogId { get; set; }
    }
}
