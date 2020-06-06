using System.Collections.Generic;

namespace ru.MaxKuzmin.VkMessenger.Events
{
    public class MessageEventArgs
    {
        public ISet<(int MessageId, int DialogId)> Data { get; } = new HashSet<(int, int)>();
    }
}
