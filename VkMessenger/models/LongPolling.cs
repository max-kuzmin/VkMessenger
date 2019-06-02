using System;
using Tizen.Applications;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public static class LongPolling
    {
        public static string Key { get; set; }
        public static string Server { get; set; }
        public static uint Ts { get; set; }
        public const uint WaitTime = 25;
        public static TimeSpan DelayAfterError { get; } = TimeSpan.FromSeconds(25);
    }
}
