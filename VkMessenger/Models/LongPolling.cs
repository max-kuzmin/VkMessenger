using System;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public static class LongPolling
    {
        public static string? Key { get; set; }
        public static string? Server { get; set; }
        public static int? Ts { get; set; }
        public const int WaitTime = 25;
        public static readonly TimeSpan RequestInterval = TimeSpan.FromSeconds(2);
    }
}
