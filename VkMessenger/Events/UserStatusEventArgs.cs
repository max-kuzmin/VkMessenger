using System.Collections.Generic;

namespace ru.MaxKuzmin.VkMessenger.Events
{
    public class UserStatusEventArgs
    {
        public ISet<(int UserId, bool Status)> Data { get; } = new HashSet<(int, bool)>();
    }
}
