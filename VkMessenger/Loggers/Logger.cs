using System.Runtime.CompilerServices;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
#if DEBUG
using Tizen;
#endif

namespace ru.MaxKuzmin.VkMessenger.Loggers
{
    public static class Logger
    {
        private const string Tag = "VK";
        private const string Version = "1.5.0";

        public static void Info(string text, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null, [CallerLineNumber] int line = 0)
        {
#if DEBUG
            Log.Info(Tag, text, file, caller, line);
#endif
        }

        public static void Debug(string text, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null, [CallerLineNumber] int line = 0)
        {
#if DEBUG
            text = text.Replace('\n', ' ');
            Log.Debug(Tag, text, file, caller, line);
#endif
        }

        public static void Error(object e, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null, [CallerLineNumber] int line = 0)
        {
#if DEBUG
            Log.Error(Tag, e.ToString(), file, caller, line);
#endif
            _ = CrashReporterClient.SendAsync($"{e}\nAppVersion: {Version}\nUserId: {Authorization.UserId}");
        }

        public static void ErrorAndAwait(object e, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null, [CallerLineNumber] int line = 0)
        {
#if DEBUG
            Log.Error(Tag, e.ToString(), file, caller, line);
#endif
            CrashReporterClient.Send($"{e}\nAppVersion: {Version}\nUserId: {Authorization.UserId}");
        }
    }
}
