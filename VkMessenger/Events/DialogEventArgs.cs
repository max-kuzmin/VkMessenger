using System.Collections.Generic;

namespace ru.MaxKuzmin.VkMessenger.Events
{
    public class DialogEventArgs
    {
        public ISet<int> DialogIds { get; } = new HashSet<int>();
    }
}
