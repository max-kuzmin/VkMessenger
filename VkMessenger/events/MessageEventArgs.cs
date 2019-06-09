using System.Collections.Generic;

namespace ru.MaxKuzmin.VkMessenger.Events
{
    public class MessageEventArgs
    {
        public HashSet<(uint MessageId, int DialogId)> Data { get; set; } = new HashSet<(uint, int)>();
    }
}
