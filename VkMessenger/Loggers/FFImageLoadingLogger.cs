using FFImageLoading.Helpers;
using System;

namespace ru.MaxKuzmin.VkMessenger.Loggers
{
    public class FfImageLoadingLogger : IMiniLogger
    {
        public void Debug(string message)
        {
#if DEBUG
            Logger.Debug(message);
#endif
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
