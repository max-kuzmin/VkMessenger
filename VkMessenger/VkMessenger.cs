using FFImageLoading;
using FFImageLoading.Config;
using FFImageLoading.Forms.Platform;
using FFImageLoading.Work;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Net;
using System;
using System.Globalization;
using System.Threading;
using ru.MaxKuzmin.VkMessenger.Loggers;
using Tizen.Applications;
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

namespace ru.MaxKuzmin.VkMessenger
{
    public class Program : FormsApplication
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            var config = new Configuration
            {
                BitmapOptimizations = false,
                FadeAnimationEnabled = false,
                FadeAnimationForCachedImages = false,
                TransformPlaceholders = true,
                DownsampleInterpolationMode = InterpolationMode.Default,
                HttpReadBufferSize = 8192,
                DecodingMaxParallelTasks = 2,
                SchedulerMaxParallelTasks = 2,
                DiskCacheDuration = TimeSpan.FromDays(30d),
                TryToReadDiskCacheDurationFromHttpHeaders = false,
                ExecuteCallbacksOnUIThread = false,
                StreamChecksumsAsKeys = true,
                AnimateGifs = true,
                DelayInMs = 10,
                ClearMemoryCacheOnOutOfMemory = true,
                InvalidateLayout = true,

                AllowUpscale = true,
                DataResolverFactory = new ProxiedDataResolverFactory(),
                MaxMemoryCacheSize = 1024 * 1024 * 5,
#if DEBUG
                Logger = new FfImageLoadingLogger(),
                VerbosePerformanceLogging = true,
                VerboseLogging = true
#endif
            };
            config.DownloadCache = new CustomDownloadCache(config);
            ImageService.Instance.Initialize(config);

            SetCulture();

            LoadApplication(new App());
        }

        private static void Main(string[] args)
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

        private static void SetCulture()
        {
            const string RuCulture = "ru";
            if (SystemSettings.LocaleLanguage.Contains(RuCulture))
            {
                var culture = new CultureInfo(RuCulture);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                LocalizedStrings.Culture = culture;
            }
        }
    }
}
