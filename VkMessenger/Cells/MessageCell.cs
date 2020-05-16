using FFImageLoading.Forms;
using ru.MaxKuzmin.VkMessenger.Models;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Cells
{
    public class MessageCell : ViewCell
    {
        private readonly CachedImage photo = new CachedImage
        {
            Aspect = Aspect.AspectFit,
            HeightRequest = 40,
            WidthRequest = 40
        };

        private readonly Label text = new Label
        {
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            LineBreakMode = LineBreakMode.WordWrap,
            VerticalTextAlignment = TextAlignment.Center
        };

        private readonly StackLayout wrapperLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Padding = new Thickness(10, 0),
            VerticalOptions = LayoutOptions.FillAndExpand,
            Rotation = -180
        };

        private readonly StackLayout outerLayout = new StackLayout();

        private static readonly BindableProperty SenderIdProperty =
            BindableProperty.Create(
                nameof(Message.SenderId),
                typeof(int),
                typeof(MessageCell),
                default(int),
                propertyChanged: OnSenderIdPropertyChanged);

        private static readonly BindableProperty ReadProperty =
            BindableProperty.Create(
                nameof(Message.Read),
                typeof(bool),
                typeof(MessageCell),
                default(bool),
                propertyChanged: OnReadPropertyChanged);

        public MessageCell()
        {
            photo.SetBinding(CachedImage.SourceProperty, nameof(Message.Photo));
            text.SetBinding(Label.TextProperty, nameof(Message.Text));
            this.SetBinding(SenderIdProperty, nameof(Message.SenderId));
            this.SetBinding(ReadProperty, nameof(Message.Read));

            wrapperLayout.Children.Add(photo);
            wrapperLayout.Children.Add(text);
            outerLayout.Children.Add(wrapperLayout);
            View = outerLayout;
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
                    cell.View.BackgroundColor = CustomColors.DarkBlue;
                }
                else
                {
                    cell.wrapperLayout.RaiseChild(cell.photo);
                    cell.photo.HorizontalOptions = LayoutOptions.Start;
                    cell.text.HorizontalOptions = LayoutOptions.EndAndExpand;
                }
            }
        }

        private static void OnReadPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MessageCell cell)
            {
                if ((bool)newValue)
                {
                    cell.View.BackgroundColor = Color.Black;
                }
                else
                {
                    cell.View.BackgroundColor = CustomColors.DarkBlue;
                }
            }
        }
    }
}