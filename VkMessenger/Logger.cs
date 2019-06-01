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

        public static void Error(Exception e, [CallerMemberName] string caller = null)
        {
            Log.Error(nameof(VkMessenger), e.ToString(), func: caller);
        }
    }
}
