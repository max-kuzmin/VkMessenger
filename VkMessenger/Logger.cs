using System;
using System.Runtime.CompilerServices;
using Tizen;

namespace ru.MaxKuzmin.VkMessenger
{
    public class Logger
    {
        public static void Info(string text, [CallerMemberName] string caller = null)
        {
            Log.Info("VK", text, func: caller);
        }

        public static void Debug(string text, [CallerMemberName] string caller = null)
        {
#if DEBUG
            Log.Debug("VK", text, func: caller);
#endif
        }

        public static void Error(Exception e, [CallerMemberName] string caller = null)
        {
            Log.Error("VK", e.ToString(), func: caller);
        }
    }
}
