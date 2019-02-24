using ru.MaxKuzmin.VkMessenger.Models;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Cells
{
    public class MessageCell : ViewCell
    {
        private Image photo = new Image
        {
            Aspect = Aspect.AspectFit,
            HeightRequest = 40,
            WidthRequest = 40
        };
        private Label text = new Label
        {
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand,
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
            photo.SetBinding(Image.SourceProperty, nameof(Message.Photo));
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
                if ((int)newValue != Authorization.UserId)
                {
                    cell.wrapperLayout.LowerChild(cell.photo);
                    cell.photo.HorizontalOptions = LayoutOptions.End;
                }
                else
                {
                    cell.wrapperLayout.RaiseChild(cell.photo);
                    cell.photo.HorizontalOptions = LayoutOptions.Start;
                }
            }
        }
    }
}