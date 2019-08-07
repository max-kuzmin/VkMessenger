using System.Collections.Generic;

namespace ru.MaxKuzmin.VkMessenger.Events
{
    public class DialogEventArgs
    {
        public HashSet<int> DialogIds { get; } = new HashSet<int>();
    }
}
