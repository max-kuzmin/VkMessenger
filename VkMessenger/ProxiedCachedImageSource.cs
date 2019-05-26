using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ru.MaxKuzmin.VkMessenger
{
    public class ProxiedCachedImageSource : ImageSource
    {
        private const string CacheName = "ImageCache";
        private static readonly IIsolatedStorageFile storage = Device.PlatformServices.GetUserStoreForApplication();
        private readonly Uri uri;

        public ProxiedCachedImageSource(Uri uri)
        {
            this.uri = uri;
        }

        static ProxiedCachedImageSource()
        {
            if (!storage.GetDirectoryExistsAsync(CacheName).Result)
                storage.CreateDirectoryAsync(CacheName).Wait();
        }

        public async Task<string> GetFileAsync()
        {
            var fileName = Path.Combine(CacheName, Device.PlatformServices.GetMD5Hash(uri.AbsoluteUri));

            if (!await storage.GetFileExistsAsync(fileName))
            {
                try
                {
                    using (var client = new ProxiedWebClient())
                    {
                        client.DownloadFile(uri, fileName);
                    }
                }
                catch
                {
                    return null;
                }
            }

            return fileName;
        }
    }
}
