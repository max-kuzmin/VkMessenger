using ru.MaxKuzmin.VkMessenger.Net;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tizen;

namespace ru.MaxKuzmin.VkMessenger.Loggers
{
    public class Logger
    {
        private const string Tag = "VK";
        private const string Ip = "192.168.0.103:5100";
        private static bool UseLogServer = false;

        private static void SendToLogServer(string level, string message)
        {
            if (!UseLogServer) return;

            Task.Run(() =>
            {
                try
                {
                    using (var client = new ProxiedWebClient())
                        client.DownloadString($"http://{Ip}/log?level={level}&message={message}");
                }
                catch { }
            });
        }

        public static void Info(string text, [CallerFilePath] string file = null, [CallerMemberName] string caller = null, [CallerLineNumber]int line = 0)
        {
            Log.Info(Tag, text, file, caller, line);
#if DEBUG
            SendToLogServer(nameof(Info), text);
#endif
        }

        public static void Debug(string text, [CallerFilePath] string file = null, [CallerMemberName] string caller = null, [CallerLineNumber]int line = 0)
        {
#if DEBUG
            var textWithoutLineEndings = text.Replace('\n', ' ');
            Log.Debug(Tag, textWithoutLineEndings, file, caller, line);
            SendToLogServer(nameof(Debug), textWithoutLineEndings);
#endif
        }

        public static void Error(Exception e, [CallerFilePath] string file = null, [CallerMemberName] string caller = null, [CallerLineNumber]int line = 0)
        {
            Log.Error(Tag, e.ToString(), file, caller, line);
#if DEBUG
            SendToLogServer(nameof(Error), e.ToString());
#endif
        }
    }
}
