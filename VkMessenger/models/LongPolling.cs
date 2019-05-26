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
        public static TimeSpan DelayBetweenRequests { get; } = TimeSpan.FromSeconds(10);

        private const string EnabledKey = "LongPollingEnabled";

        //TODO: ability to switch it from GUI
        public static bool Enabled
        {
            get => Preference.Contains(EnabledKey) ? Preference.Get<bool>(EnabledKey) : true;
            set => Preference.Set(EnabledKey, value);
        }
    }
}
