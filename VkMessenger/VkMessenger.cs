using FFImageLoading;
using FFImageLoading.Config;
using FFImageLoading.Forms.Platform;
using FFImageLoading.Work;
using System;
using Tizen.Applications;
using Tizen.Wearable.CircularUI.Forms.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

namespace ru.MaxKuzmin.VkMessenger
{
    class Program : FormsApplication
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Logger.Error(e.ExceptionObject as Exception);

            var config = new Configuration
            {
                AllowUpscale = false,
                DataResolverFactory = new ProxiedDataResolverFactory(),
                DecodingMaxParallelTasks = 1,
                DownsampleInterpolationMode = InterpolationMode.None,
                ExecuteCallbacksOnUIThread = true,
                SchedulerMaxParallelTasks = 2,
                MaxMemoryCacheSize = 1024 * 1024 * 8,
#if DEBUG
                Logger = new FFImageLoadingLogger(),
                VerboseLogging = true
#endif
            };
            ImageService.Instance.Initialize(config);

            LoadApplication(new App());
        }

        static void Main(string[] args)
        {
            var app = new Program();
            CachedImageRenderer.Init(app);
            Forms.Init(app);
            FormsCircularUI.Init();
            app.Run(args);
        }

        protected override void OnLowMemory(LowMemoryEventArgs e)
        {
            base.OnLowMemory(e);
            ImageService.Instance.InvalidateMemoryCache();
        }
    }
}
