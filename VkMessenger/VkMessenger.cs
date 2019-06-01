using FFImageLoading;
using FFImageLoading.Config;
using FFImageLoading.Forms.Platform;
using System;
using Tizen.Applications;
using Tizen.Wearable.CircularUI.Forms.Renderer;
using Xamarin.Forms.Platform.Tizen;

namespace ru.MaxKuzmin.VkMessenger
{
    class Program : FormsApplication
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            var config = new Configuration()
            {
                ExecuteCallbacksOnUIThread = true,
                DataResolverFactory = new ProxiedDataResolverFactory(),
#if DEBUG
                VerboseLogging = true,
                VerboseLoadingCancelledLogging = true,
                VerboseMemoryCacheLogging = true,
                Logger = new CustomLogger()
#endif
            };
            ImageService.Instance.Initialize(config);

#if DEBUG
            DebugSetting.Set();
#endif

            LoadApplication(new App());
        }

        static void Main(string[] args)
        {
            var app = new Program();
            CachedImageRenderer.Init(app);
            Forms.Init(app);
            FormsCircularUI.Init();
            try
            {
                app.Run(args);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        protected override void OnLowMemory(LowMemoryEventArgs e)
        {
            base.OnLowMemory(e);
            ImageService.Instance.InvalidateMemoryCache();
        }
    }
}
