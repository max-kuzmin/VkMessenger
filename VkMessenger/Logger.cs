using System;
using System.Runtime.CompilerServices;
using Tizen;

namespace ru.MaxKuzmin.VkMessenger
{
    public class Logger
    {
        public static void Info(string text, [CallerMemberName] string caller = null)
        {
            Log.Info(nameof(VkMessenger), $"{caller}: {text}");
        }

        public static void Error(Exception e, [CallerMemberName] string caller = null)
        {
            Log.Error(nameof(VkMessenger), $"{caller}: {e.ToString()}");
        }
    }
}
