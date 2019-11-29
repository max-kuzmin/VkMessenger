using System.Collections.Generic;

namespace ru.MaxKuzmin.VkMessenger.Events
{
    public class UserStatusEventArgs
    {
        public ISet<(uint UserId, bool Status)> Data { get; } = new HashSet<(uint, bool)>();
    }
}
