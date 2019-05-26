namespace ru.MaxKuzmin.VkMessenger.Events
{
    public class UserTypingEventArgs
    {
        public uint UserId { get; set; }

        public int DialogId { get; set; }
    }
}
