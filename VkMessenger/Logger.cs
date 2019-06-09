using System;
using System.Runtime.CompilerServices;
using Tizen;

namespace ru.MaxKuzmin.VkMessenger
{
    public class Logger
    {
        public static void Info(string text, [CallerMemberName] string caller = null)
        {
            Log.Info(nameof(VkMessenger), text, func: caller);
        }

        public static void Debug(string text, [CallerMemberName] string caller = null)
        {
            Log.Debug(nameof(VkMessenger), text, func: caller);
        }

        public static void Error(Exception e, bool onlyMessage = false, [CallerMemberName] string caller = null)
        {
            Log.Error(nameof(VkMessenger), onlyMessage ? e.Message : e.ToString(), func: caller);
        }
    }
}
