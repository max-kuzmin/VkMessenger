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

        public async Task<Stream> GetFileStreamAsync(CancellationToken cancel)
        {
            if (!await storage.GetFileExistsAsync(fileName))
            {
                var mutex = mutexes.GetOrAdd(fileName, f => new Mutex());
                try
                {
                    using (var client = new ProxiedWebClient())
                    {
                        cancel.ThrowIfCancellationRequested();
                        var data = client.DownloadData(uri);

                        cancel.ThrowIfCancellationRequested();
                        mutex.WaitOne();
                        using (var stream = await storage.OpenFileAsync(fileName, FileMode.Create, FileAccess.Write))
                        {
                            stream.Write(data, 0, data.Length);
                        }
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }

            return await storage.OpenFileAsync(fileName, FileMode.Open, FileAccess.Read);
        }
    }
}
