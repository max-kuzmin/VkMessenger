using System.Collections.Generic;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public sealed class LongPoolingUpdates
    {
        public ISet<int> UpdatedDialogIds { get; } = new HashSet<int>();
        public ISet<MessageUpdatesData> MessageUpdatesData { get; } = new HashSet<MessageUpdatesData>();
        public ISet<UserStatusUpdatesData> UserStatusUpdatesData { get; } = new HashSet<UserStatusUpdatesData>();
    }

    public struct MessageUpdatesData
    {
        public int MessageId;
        public int DialogId;
    }

    public struct UserStatusUpdatesData
    {
        public int UserId;
        public bool Status;
    }
}
