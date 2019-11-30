using FFImageLoading.Forms;
using ru.MaxKuzmin.VkMessenger.Models;
using Tizen.Applications;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.pages
{
    public class MessagePage : CirclePage
    {
        private readonly CircleScrollView scrollView = new CircleScrollView();
        private readonly StackLayout wrapperLayout = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.FillAndExpand
        };

        private Label CreateLabel(string text) => new Label
        {
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            LineBreakMode = LineBreakMode.WordWrap,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(30, 70, 30, 0),
            Text = text
        };

        private Label CreateUri(string text) => new Label
        {
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            LineBreakMode = LineBreakMode.WordWrap,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(30, 10, 30, 0),
            TextColor = CustomColors.BrightBlue,
            TextDecorations = TextDecorations.Underline,
            Text = text
        };

        private CachedImage CreateImage(ImageSource source) => new CachedImage
        {
            Margin = new Thickness(0, 10, 0, 0),
            LoadingPlaceholder = ImageSource.FromFile(
                Tizen.Applications.Application.Current.DirectoryInfo.SharedResource + "/Placeholder.png"),
            Source = source
        };

        public MessagePage(Message message)
        {
            NavigationPage.SetHasNavigationBar(this, false);

            wrapperLayout.Children.Add(CreateLabel(message.FullText));

            foreach (var item in message.AttachmentMessages)
            {
                var text = $"{item.Profile.Name}: \"{item.Text}\"";
                wrapperLayout.Children.Add(CreateLabel(text));
            }

            foreach (var item in message.AttachmentUris)
            {
                var uri = CreateUri(item.ToString());
                wrapperLayout.Children.Add(uri);

                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) =>
                    AppControl.SendLaunchRequest(new AppControl
                    {
                        Operation = AppControlOperations.View,
                        Uri = item.ToString()
                    });
                uri.GestureRecognizers.Add(tapGestureRecognizer);
            }

            foreach (var item in message.AttachmentImages)
            {
                var image = CreateImage(item);
                wrapperLayout.Children.Add(image);
            }

            var emptyLabel = new Label { Margin = new Thickness(0, 0, 0, 70) };
            wrapperLayout.Children.Add(emptyLabel);

            scrollView.Content = wrapperLayout;
            Content = scrollView;
            SetBinding(RotaryFocusObjectProperty, new Binding { Source = scrollView });
        }
    }
}
