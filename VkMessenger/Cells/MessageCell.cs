using FFImageLoading.Forms;
using ru.MaxKuzmin.VkMessenger.Layouts;
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
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.FillAndExpand
        };

        private readonly Label time = new Label
        {
            FontSize = 5,
            HorizontalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Fill,
            TextColor = Color.Gray
        };

        private readonly StackLayout wrapperLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Padding = new Thickness(10, 0),
            VerticalOptions = LayoutOptions.FillAndExpand,
            Rotation = -180
        };

        private readonly StackLayout photoWrapperLayout = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.CenterAndExpand
        };

        private readonly StackLayout outerLayout = new StackLayout();

        private readonly AudioLayout audioLayout = new AudioLayout();

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
            time.SetBinding(Label.TextProperty, nameof(Message.TimeFormatted));
            this.SetBinding(SenderIdProperty, nameof(Message.SenderId));
            this.SetBinding(ReadProperty, nameof(Message.Read));
            audioLayout.SetBinding(AudioLayout.SourceProperty, nameof(Message.VoiceMessage));
            audioLayout.SetBinding(AudioLayout.DurationProperty, nameof(Message.VoiceMessageDuration));

            photoWrapperLayout.Children.Add(photo);
            photoWrapperLayout.Children.Add(time);
            wrapperLayout.Children.Add(photoWrapperLayout);
            wrapperLayout.Children.Add(text);
            wrapperLayout.Children.Add(audioLayout);
            outerLayout.Children.Add(wrapperLayout);
            View = outerLayout;
        }

        private static void OnSenderIdPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is MessageCell cell))
            {
                return;
            }

            var senderId = (int)newValue;
            if (senderId != Authorization.UserId)
            {
                cell.wrapperLayout.LowerChild(cell.audioLayout);
                cell.wrapperLayout.LowerChild(cell.photoWrapperLayout);
                cell.text.HorizontalTextAlignment = TextAlignment.Start;
                cell.View.BackgroundColor = CustomColors.DarkBlue;
            }
            else
            {
                cell.wrapperLayout.RaiseChild(cell.audioLayout);
                cell.wrapperLayout.RaiseChild(cell.photoWrapperLayout);
                cell.text.HorizontalTextAlignment = TextAlignment.End;
            }
        }

        private static void OnReadPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is MessageCell cell))
            {
                return;
            }

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