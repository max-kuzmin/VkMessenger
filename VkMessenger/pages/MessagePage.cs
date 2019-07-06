using ru.MaxKuzmin.VkMessenger.Models;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.pages
{
    public class MessagePage : CirclePage
    {
        private readonly CircleScrollView scrollView = new CircleScrollView();
        private readonly Image image = new Image
        {
            Margin = new Thickness(0, 10, 0, 70)
        };
        private StackLayout wrapperLayout = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.FillAndExpand
        };
        private readonly Label text = new Label
        {
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            LineBreakMode = LineBreakMode.WordWrap,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(30, 70, 30, 10)
        };

        public MessagePage(Message message)
        {
            NavigationPage.SetHasNavigationBar(this, false);

            image.Source = message.BigAttachmentImage;
            text.Text = message.Text;
            wrapperLayout.Children.Add(text);
            wrapperLayout.Children.Add(image);
            scrollView.Content = wrapperLayout;
            Content = scrollView;
            SetBinding(RotaryFocusObjectProperty, new Binding { Source = scrollView });
        }
    }
}
