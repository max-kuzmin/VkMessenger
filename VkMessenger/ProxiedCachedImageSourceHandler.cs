using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

namespace ru.MaxKuzmin.VkMessenger
{
    public class ProxiedCachedImageSourceHandler : IImageSourceHandler
    {
        public async Task<bool> LoadImageAsync(Xamarin.Forms.Platform.Tizen.Native.Image image, ImageSource imageSource, CancellationToken cancel = default)
        {
            if (imageSource is ProxiedCachedImageSource source)
            {
                try
                {
                    using (var stream = await source.GetFileStreamAsync(cancel))
                    {
                        if (stream?.CanRead == true)
                        {
                            return await image.LoadAsync(stream, cancel);
                        }
                    }
                }
                catch (TaskCanceledException) { }
                catch (Exception e)
                {
                    Tizen.Log.Error(nameof(VkMessenger), e.ToString());
                }
            }

            return false;
        }
    }
}
