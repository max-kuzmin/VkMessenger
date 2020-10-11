#if DEBUG
using System.Runtime.CompilerServices;
using Tizen;
#else
using System.Reflection;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
#endif

namespace ru.MaxKuzmin.VkMessenger.Loggers
{
#if DEBUG
    public static class Logger
    {
        private const string Tag = "VK";

        public static void Info(string text, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null, [CallerLineNumber]int line = 0)
        {
            Log.Info(Tag, text, file, caller, line);
        }

        public static void Debug(string text, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null, [CallerLineNumber]int line = 0)
        {
            text = text.Replace('\n', ' ');
            Log.Debug(Tag, text, file, caller, line);
        }

        public static void Error(object e, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null, [CallerLineNumber]int line = 0)
        {
            Log.Error(Tag, e.ToString(), file, caller, line);
        }
    }
#else
    public static class Logger
    {
        private static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static void Info(string text, string? file = null, string? caller = null, int line = 0) { }

        public static void Debug(string text, string? file = null, string? caller = null, int line = 0) { }

        public static void Error(object e, string? file = null, string? caller = null, int line = 0)
        {
            _ = CrashReporterClient.SendAsync($"{e}\nAppVersion: {Version}\nUserId: {Authorization.UserId}");
        }
    }
#endif
}
