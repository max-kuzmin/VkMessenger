﻿namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class UserStatusEventArgs
    {
        public uint UserId { get; set; }

        public bool IsOnline { get; set; }
    }
}
