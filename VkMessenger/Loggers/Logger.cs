﻿using System.Runtime.CompilerServices;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Managers;
using Tizen;

namespace ru.MaxKuzmin.VkMessenger.Loggers
{
    public static class Logger
    {
        private const string Tag = "VK";
        private const string Version = "1.5.0";

        public static void Info(string text, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null, [CallerLineNumber] int line = 0)
        {
            Log.Info(Tag, text, file, caller, line);
        }

        public static void Debug(string text, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null, [CallerLineNumber] int line = 0)
        {
            text = text.Replace('\n', ' ');
            Log.Debug(Tag, text, file, caller, line);
        }

        public static void Error(object e, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null, [CallerLineNumber] int line = 0)
        {
            Log.Error(Tag, e.ToString(), file, caller, line);
            _ = CrashReporterClient.SendAsync($"{e}\nAppVersion: {Version}\nUserId: {AuthorizationManager.UserId}");
        }

        public static void ErrorAndAwait(object e, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null, [CallerLineNumber] int line = 0)
        {
            Log.Error(Tag, e.ToString(), file, caller, line);
            CrashReporterClient.Send($"{e}\nAppVersion: {Version}\nUserId: {AuthorizationManager.UserId}");
        }
    }
}
