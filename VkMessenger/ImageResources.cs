using System.IO;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger
{
    public static class ImageResources
    {
        private static readonly string SharedResource = Tizen.Applications.Application.Current.DirectoryInfo.SharedResource;
        public static ImageSource? Placeholder = ImageSource.FromFile(Path.Combine(SharedResource, "Placeholder.png"));
        public static ImageSource? PlaySymbol = ImageSource.FromFile(Path.Combine(SharedResource, "PlaySymbol.png"));
        public static ImageSource? PauseSymbol = ImageSource.FromFile(Path.Combine(SharedResource, "PauseSymbol.png"));
        public static ImageSource? BackSymbol = ImageSource.FromFile(Path.Combine(SharedResource, "BackSymbol.png"));
        public static ImageSource? ForwardSymbol = ImageSource.FromFile(Path.Combine(SharedResource, "ForwardSymbol.png"));
        public static ImageSource? BackDisabledSymbol = ImageSource.FromFile(Path.Combine(SharedResource, "BackDisabledSymbol.png"));
        public static ImageSource? ForwardDisabledSymbol = ImageSource.FromFile(Path.Combine(SharedResource, "ForwardDisabledSymbol.png"));
        public static ImageSource? LoadingSymbol = ImageSource.FromFile(Path.Combine(SharedResource, "LoadingSymbol.png"));
        public static ImageSource? StopSymbol = ImageSource.FromFile(Path.Combine(SharedResource, "StopSymbol.png"));
        public static ImageSource? RecordSymbol = ImageSource.FromFile(Path.Combine(SharedResource, "RecordSymbol.png"));
    }
}
