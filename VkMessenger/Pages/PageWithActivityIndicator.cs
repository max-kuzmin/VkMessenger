using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class PageWithActivityIndicator: BezelInteractionPage
    {
        protected readonly ActivityIndicator activityIndicator = new ActivityIndicator
        {
            IsRunning = true,
            Scale = 0.5,
            IsVisible = false
        };

        protected readonly AbsoluteLayout absoluteLayout = new AbsoluteLayout();

        public PageWithActivityIndicator()
        {
            AbsoluteLayout.SetLayoutBounds(activityIndicator, new Rectangle(0.5, 0, 75, 75));
            AbsoluteLayout.SetLayoutFlags(activityIndicator, AbsoluteLayoutFlags.XProportional);
        }
    }
}
