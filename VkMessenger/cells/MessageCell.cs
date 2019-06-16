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
            LineBreakMode = LineBreakMode.WordWrap,
            VerticalTextAlignment = TextAlignment.Center
        };
        private CachedImage attachmentImage = new CachedImage();
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
            attachmentImage.SetBinding(CachedImage.SourceProperty, nameof(Message.AttachmentImage));
            this.SetBinding(SenderIdProperty, nameof(Message.SenderId));
            this.SetBinding(ReadProperty, nameof(Message.Read));

            wrapperLayout.Children.Add(photo);
            wrapperLayout.Children.Add(text);
            wrapperLayout.Children.Add(attachmentImage);
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
                    cell.text.HorizontalOptions = LayoutOptions.Start;
                    cell.attachmentImage.HorizontalOptions = LayoutOptions.StartAndExpand;
                    cell.View.BackgroundColor = Color.FromHex("00354A");
                }
                else
                {
                    cell.wrapperLayout.RaiseChild(cell.photo);
                    cell.photo.HorizontalOptions = LayoutOptions.Start;
                    cell.text.HorizontalOptions = LayoutOptions.End;
                    cell.attachmentImage.HorizontalOptions = LayoutOptions.EndAndExpand;
                }
            }
        }

        private static void OnReadPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MessageCell cell)
            {
                if ((bool)newValue)
                {
                    //Is always set as read
                    cell.View.BackgroundColor = Color.Black;
                }
                else
                {
                    cell.View.BackgroundColor = Color.FromHex("00354A");
                }
            }
        }
    }
}