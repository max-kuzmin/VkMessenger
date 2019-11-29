using FFImageLoading.Helpers;
using System;

namespace ru.MaxKuzmin.VkMessenger.Loggers
{
    public class FFImageLoadingLogger : IMiniLogger
    {
        public void Debug(string message)
        {
            Logger.Debug(message);
        }

        public void Error(string errorMessage)
        {
            Logger.Error(new Exception(errorMessage));
        }

        public void Error(string errorMessage, Exception e)
        {
            Logger.Error(e);
        }
    }
}
