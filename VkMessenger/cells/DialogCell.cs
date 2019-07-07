using FFImageLoading.Forms;
using ru.MaxKuzmin.VkMessenger.Models;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Cells
{
    public class DialogCell : ViewCell
    {
        private CachedImage photo = new CachedImage
        {
            HorizontalOptions = LayoutOptions.Start,
            Aspect = Aspect.AspectFit,
            HeightRequest = 75,
            WidthRequest = 75,
            LoadingPlaceholder = ImageSource.FromFile(
                        Tizen.Applications.Application.Current.DirectoryInfo.SharedResource + "/placeholder.png")

        };
        private Label text = new Label
        {
            VerticalOptions = LayoutOptions.Fill,
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            TextColor = Color.Gray,
            LineBreakMode = LineBreakMode.TailTruncation,
            HeightRequest = 40
        };
        private Label title = new Label
        {
            VerticalOptions = LayoutOptions.Fill,
            FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
            FontAttributes = FontAttributes.Bold,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        private Label unreadCount = new Label
        {
            VerticalOptions = LayoutOptions.Fill,
            FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
            FontAttributes = FontAttributes.Bold
        };
        private Label onlineIndicator = new Label
        {
            VerticalOptions = LayoutOptions.Fill,
            FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
            TextColor = Color.LightGreen,
            FontAttributes = FontAttributes.Bold
        };
        private StackLayout titleLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal
        };
        private StackLayout titleAndtextLayout = new StackLayout
        {
            Orientation = StackOrientation.Vertical
        };
        private StackLayout wrapperLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Padding = new Thickness(10, 5)
        };

        private static readonly BindableProperty UnreadCountProperty =
            BindableProperty.Create(
                nameof(Dialog.UnreadCount),
                typeof(uint),
                typeof(DialogCell),
                default(uint),
                propertyChanged: OnUnreadCountPropertyChanged);

        private static readonly BindableProperty OnlineProperty =
            BindableProperty.Create(
                nameof(Dialog.Online),
                typeof(bool),
                typeof(DialogCell),
                default(bool),
                propertyChanged: OnOnlinePropertyChanged);

        private static readonly BindableProperty TextProperty =
            BindableProperty.Create(
                nameof(Dialog.Text),
                typeof(uint),
                typeof(DialogCell),
                default(uint),
                propertyChanged: OnTextPropertyChanged);

        public DialogCell()
        {
            photo.SetBinding(CachedImage.SourceProperty, nameof(Dialog.Photo));
            title.SetBinding(Label.TextProperty, nameof(Dialog.Title));
            text.SetBinding(Label.TextProperty, nameof(Dialog.Text));
            this.SetBinding(UnreadCountProperty, nameof(Dialog.UnreadCount));
            this.SetBinding(OnlineProperty, nameof(Dialog.Online));
            this.SetBinding(TextProperty, nameof(Dialog.Text));

            titleLayout.Children.Add(title);
            titleLayout.Children.Add(unreadCount);
            titleLayout.Children.Add(onlineIndicator);
            titleAndtextLayout.Children.Add(titleLayout);
            titleAndtextLayout.Children.Add(text);
            wrapperLayout.Children.Add(photo);
            wrapperLayout.Children.Add(titleAndtextLayout);
            View = wrapperLayout;
        }

        private static void OnUnreadCountPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DialogCell cell)
            {
                var unreadCount = (uint)newValue;
                if (unreadCount > 0)
                {
                    cell.unreadCount.Text = $"({unreadCount})";
                    cell.View.BackgroundColor = Color.FromHex("00354A");
                }
                else
                {
                    cell.unreadCount.Text = string.Empty;
                    cell.View.BackgroundColor = Color.Black;
                }
            }
        }

        private static void OnOnlinePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DialogCell cell)
            {
                cell.onlineIndicator.Text = (bool)newValue ? "•" : string.Empty;
            }
        }

        private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DialogCell cell)
            {
                cell.text.Text = newValue.ToString();
            }
        }
    }
}
