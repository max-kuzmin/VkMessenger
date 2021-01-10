using System;
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
            WidthRequest = 40,
            LoadingPlaceholder = ImageResources.Placeholder
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
            HorizontalOptions = LayoutOptions.Fill,
            Rotation = -180
        };

        private readonly StackLayout photoWrapperLayout = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.CenterAndExpand
        };

        private AudioLayout? audioLayout;

        private readonly StackLayout outerLayout = new StackLayout();

        private int senderId;

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

        public static readonly BindableProperty VoiceMessageSourceProperty =
            BindableProperty.Create(
                nameof(Message.VoiceMessage),
                typeof(Uri),
                typeof(MessageCell),
                default(Uri),
                propertyChanged: OnVoiceMessageSourcePropertyChanged);

        public MessageCell()
        {
            photo.SetBinding(CachedImage.SourceProperty, nameof(Message.Photo));
            text.SetBinding(Label.TextProperty, nameof(Message.Text));
            time.SetBinding(Label.TextProperty, nameof(Message.TimeFormatted));
            this.SetBinding(SenderIdProperty, nameof(Message.SenderId));
            this.SetBinding(ReadProperty, nameof(Message.Read));
            this.SetBinding(VoiceMessageSourceProperty, nameof(Message.VoiceMessage));

            photoWrapperLayout.Children.Add(photo);
            photoWrapperLayout.Children.Add(time);
            wrapperLayout.Children.Add(photoWrapperLayout);
            wrapperLayout.Children.Add(text);
            outerLayout.Children.Add(wrapperLayout);
            View = outerLayout;
            View.BackgroundColor = CustomColors.DarkBlue;
        }

        private static void OnSenderIdPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MessageCell cell && newValue is int value)
            {
                cell.senderId = value;
                if (cell.senderId != Authorization.UserId)
                {
                    cell.wrapperLayout.LowerChild(cell.photoWrapperLayout);
                    cell.text.HorizontalTextAlignment = TextAlignment.Start;
                }
                else
                {
                    cell.wrapperLayout.RaiseChild(cell.photoWrapperLayout);
                    cell.text.HorizontalTextAlignment = TextAlignment.End;
                }
            }
        }

        private static void OnReadPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MessageCell cell && newValue is bool value)
            {
                if (value)
                {
                    cell.View.BackgroundColor = Color.Black;
                }
                else
                {
                    cell.View.BackgroundColor = CustomColors.DarkBlue;
                }
            }
        }

        private static void OnVoiceMessageSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MessageCell layout && newValue is Uri uri && layout.audioLayout == null)
            {
                layout.audioLayout = new AudioLayout();
                layout.audioLayout.SetBinding(AudioLayout.SourceProperty, nameof(Message.VoiceMessage));
                layout.audioLayout.SetBinding(AudioLayout.DurationProperty, nameof(Message.VoiceMessageDuration));
                layout.wrapperLayout.Children.Add(layout.audioLayout);
                layout.wrapperLayout.Children.Remove(layout.text);

                if (layout.senderId != Authorization.UserId)
                {
                    layout.wrapperLayout.LowerChild(layout.audioLayout);
                    layout.wrapperLayout.LowerChild(layout.photoWrapperLayout);
                    layout.audioLayout.HorizontalOptions = LayoutOptions.StartAndExpand;
                }
                else
                {
                    layout.wrapperLayout.RaiseChild(layout.audioLayout);
                    layout.wrapperLayout.RaiseChild(layout.photoWrapperLayout);
                    layout.audioLayout.HorizontalOptions = LayoutOptions.EndAndExpand;
                    layout.audioLayout.SetMarginForAnimation();
                }
            }
        }
    }
}