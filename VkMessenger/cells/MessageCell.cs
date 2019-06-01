using FFImageLoading.Forms;
using ru.MaxKuzmin.VkMessenger.Models;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Cells
{
    public class MessageCell : ViewCell
    {
        private CachedImage photo = new CachedImage
        {
            Aspect = Aspect.AspectFit,
            HeightRequest = 40,
            WidthRequest = 40
        };
        private Label text = new Label
        {
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            LineBreakMode = LineBreakMode.WordWrap
        };
        private StackLayout wrapperLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Padding = new Thickness(10, 0),
            VerticalOptions = LayoutOptions.FillAndExpand
        };

        private static readonly BindableProperty SenderIdProperty =
            BindableProperty.Create(
                nameof(Message.SenderId),
                typeof(int),
                typeof(MessageCell),
                default(int),
                propertyChanged: OnSenderIdPropertyChanged);

        public MessageCell()
        {
            photo.SetBinding(CachedImage.SourceProperty, nameof(Message.Photo));
            text.SetBinding(Label.TextProperty, nameof(Message.Text));
            this.SetBinding(SenderIdProperty, nameof(Message.SenderId));

            wrapperLayout.Children.Add(photo);
            wrapperLayout.Children.Add(text);
            View = wrapperLayout;
        }

        private static void OnSenderIdPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MessageCell cell)
            {
                var dialogId = (int)newValue;
                if (dialogId != Authorization.UserId)
                {
                    cell.wrapperLayout.LowerChild(cell.photo);
                    cell.photo.HorizontalOptions = LayoutOptions.End;
                    cell.text.HorizontalOptions = LayoutOptions.StartAndExpand;
                }
                else
                {
                    cell.wrapperLayout.RaiseChild(cell.photo);
                    cell.photo.HorizontalOptions = LayoutOptions.Start;
                    cell.text.HorizontalOptions = LayoutOptions.EndAndExpand;
                }
            }
        }
    }
}