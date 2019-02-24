using ru.MaxKuzmin.VkMessenger.Models;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Cells
{
    public class DialogCell : ViewCell
    {
        private Image photo = new Image
        {
            HorizontalOptions = LayoutOptions.Start,
            Aspect = Aspect.AspectFit,
            HeightRequest = 75,
            WidthRequest = 75

        };
        private Label name = new Label
        {
            VerticalOptions = LayoutOptions.Start,
            FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
            FontAttributes = FontAttributes.Bold
        };
        private Label unreadCount = new Label
        {
            VerticalOptions = LayoutOptions.FillAndExpand,
            FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
            FontAttributes = FontAttributes.Bold
        };
        private Label text = new Label
        {
            VerticalOptions = LayoutOptions.FillAndExpand,
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            TextColor = Color.Gray,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        private StackLayout nameLayout = new StackLayout
        {
            HorizontalOptions = LayoutOptions.Start,
            Orientation = StackOrientation.Horizontal
        };
        private StackLayout nameAndtextLayout = new StackLayout
        {
            HorizontalOptions = LayoutOptions.FillAndExpand,
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

        public DialogCell()
        {
            photo.SetBinding(Image.SourceProperty, nameof(Dialog.Photo));
            name.SetBinding(Label.TextProperty, nameof(Dialog.Name));
            text.SetBinding(Label.TextProperty, nameof(Dialog.Text));
            this.SetBinding(UnreadCountProperty, nameof(Dialog.UnreadCount));

            nameLayout.Children.Add(name);
            nameLayout.Children.Add(unreadCount);
            nameAndtextLayout.Children.Add(nameLayout);
            nameAndtextLayout.Children.Add(text);
            wrapperLayout.Children.Add(photo);
            wrapperLayout.Children.Add(nameAndtextLayout);
            View = wrapperLayout;
        }

        private static void OnUnreadCountPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DialogCell cell)
            {
                var unreadCount = (uint)newValue;
                if (unreadCount > 0)
                {
                    cell.unreadCount.Text = $"({(uint)newValue})";
                    cell.View.BackgroundColor = Color.FromHex("00354A");
                }
                else
                {
                    cell.unreadCount.Text = string.Empty;
                    cell.View.BackgroundColor = Color.Black;
                }
            }
        }
    }
}
