using FFImageLoading.Helpers;
using System;
using Tizen;

namespace ru.MaxKuzmin.VkMessenger
{
    public class CustomLogger : IMiniLogger
    {
        public void Debug(string message)
        {
            Log.Debug(nameof(FFImageLoading), message);
        }

        public void Error(string errorMessage)
        {
            Log.Error(nameof(FFImageLoading), errorMessage);
        }

        public void Error(string errorMessage, Exception ex)
        {
            Log.Error(nameof(FFImageLoading), errorMessage);
        }
    }
}
