using Tizen.Applications;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public static class LongPolling
    {
        public static string Key { get; set; }
        public static string Server { get; set; }
        public static uint Ts { get; set; }
        public const uint WaitTime = 25;

        private const string EnabledKey = "LongPollingEnabled";

        public static bool Enabled
        {
            get => Preference.Contains(EnabledKey) ? Preference.Get<bool>(EnabledKey) : true;
            set => Preference.Set(EnabledKey, value);
        }
    }
}
