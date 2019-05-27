using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

namespace ru.MaxKuzmin.VkMessenger
{
    public class ProxiedCachedImageSourceHandler : IImageSourceHandler
    {
        public async Task<bool> LoadImageAsync(Xamarin.Forms.Platform.Tizen.Native.Image image, ImageSource imageSource, CancellationToken cancelationToken = default)
        {
            if (imageSource is ProxiedCachedImageSource source)
            {
                var stream = await source.GetFileStreamAsync();
                if (stream != null)
                {
                    return await image.LoadAsync(stream, cancelationToken);
                }
            }

            return false;
        }
    }
}
