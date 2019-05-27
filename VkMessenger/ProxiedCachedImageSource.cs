using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ru.MaxKuzmin.VkMessenger
{
    public class ProxiedCachedImageSource : ImageSource
    {
        private const string CacheName = "imagecache";
        private static readonly IIsolatedStorageFile storage;
        private static readonly ConcurrentDictionary<string, Mutex> mutexes;

        private readonly Uri uri;
        private readonly string fileName;

        public ProxiedCachedImageSource(Uri uri)
        {
            this.uri = uri;
            fileName = Path.Combine(CacheName, Device.PlatformServices.GetMD5Hash(uri.AbsoluteUri));
        }

        static ProxiedCachedImageSource()
        {
            mutexes = new ConcurrentDictionary<string, Mutex>();
            storage = Device.PlatformServices.GetUserStoreForApplication();

            if (!storage.GetDirectoryExistsAsync(CacheName).Result)
            {
                storage.CreateDirectoryAsync(CacheName).Wait();
            }
        }

        public async Task<Stream> GetFileStreamAsync()
        {
            if (!await storage.GetFileExistsAsync(fileName))
            {
                try
                {
                    using (var client = new ProxiedWebClient())
                    {
                        var data = client.DownloadData(uri);

                        var mutex = mutexes.GetOrAdd(fileName, f => new Mutex());
                        mutex.WaitOne();

                        using (var stream = await storage.OpenFileAsync(fileName, FileMode.Create, FileAccess.Write))
                        {
                            await stream.WriteAsync(data, 0, data.Length);
                        }

                        mutex.ReleaseMutex();
                    }
                }
                catch (Exception e)
                {
                    Tizen.Log.Error(nameof(VkMessenger), e.ToString());
                    return null;
                }
            }

            return await storage.OpenFileAsync(fileName, FileMode.Open, FileAccess.Read);
        }
    }
}
