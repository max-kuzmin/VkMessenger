﻿namespace ru.MaxKuzmin.VkMessenger.Events
{
    public class UserStatusEventArgs
    {
        public uint UserId { get; set; }

        public bool IsOnline { get; set; }
    }
}