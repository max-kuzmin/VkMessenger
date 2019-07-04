﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tizen;

namespace ru.MaxKuzmin.VkMessenger
{
    public class Logger
    {
        private const string Tag = "VK";
        private const string Ip = "192.168.0.103:5100";

        private static void Send(string level, string message)
        {
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
            Send(nameof(Info), text);
#endif
        }

        public static void Debug(string text, [CallerFilePath] string file = null, [CallerMemberName] string caller = null, [CallerLineNumber]int line = 0)
        {
#if DEBUG
            var textWithoutEndLines = text.Replace('\n', ' ');
            Log.Debug(Tag, textWithoutEndLines, file, caller, line);
            Send(nameof(Debug), textWithoutEndLines);
#endif
        }

        public static void Error(Exception e, [CallerFilePath] string file = null, [CallerMemberName] string caller = null, [CallerLineNumber]int line = 0)
        {
            Log.Error(Tag, e.ToString(), file, caller, line);
#if DEBUG
            Send(nameof(Error), e.ToString());
#endif
        }
    }
}
