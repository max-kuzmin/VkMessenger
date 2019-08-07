using System.Collections.Generic;

namespace ru.MaxKuzmin.VkMessenger.Events
{
    public class MessageEventArgs
    {
        public HashSet<(uint MessageId, int DialogId)> Data { get; } = new HashSet<(uint, int)>();
    }
}
