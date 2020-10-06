using System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;

namespace ru.MaxKuzmin.VkMessenger.Layouts
{
    public sealed class AudioLayout: StackLayout
    {
        private const string PlaySymbol = "▶️";
        private const string PauseSymbol = "⏸";
        private const string BackSymbol = "⏪";
        private const string ForwardSymbol = "⏩️";
        private const string LoadingSymbol = "🔄";
        public readonly Button playButton = new Button
        {
            Text = PlaySymbol,
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Button))
        };
        public readonly Button scrollBackButton = new Button
        {
            Text = BackSymbol,
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Button))
        };
        public readonly Button scrollForwardButton = new Button
        {
            Text = ForwardSymbol,
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Button))
        };

        public readonly MediaPlayer player = new MediaPlayer();
        private const int seekDelta = 5000;

        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(
                nameof(Source),
                typeof(Uri),
                typeof(AudioLayout),
                default(Uri),
                propertyChanged: OnSourcePropertyChanged);

        public Uri? Source { get; set; }

        public AudioLayout()
        {
            Orientation = StackOrientation.Horizontal;
            IsVisible = false;
            player.BufferingStarted += OnBufferingStarted;
            player.BufferingCompleted += OnBufferingCompleted;
            player.PlaybackStarted += OnPlaybackStarted;
            player.PlaybackPaused += OnPlaybackPaused;
            playButton.Clicked += OnPlayButtonClicked;

            scrollBackButton.Clicked += (s, e) => player.Seek(player.Position - seekDelta);
            scrollForwardButton.Clicked += (s, e) => player.Seek(player.Position + seekDelta);

            Children.Add(scrollBackButton);
            Children.Add(playButton);
            Children.Add(scrollForwardButton);
        }

        private void OnPlaybackPaused(object sender, EventArgs e)
        {
            playButton.Text = PlaySymbol;
        }

        private void OnPlaybackStarted(object sender, EventArgs e)
        {
            playButton.Text = PauseSymbol;
        }

        private void OnBufferingStarted(object sender, EventArgs e)
        {
            playButton.Text = LoadingSymbol;
        }

        private void OnBufferingCompleted(object sender, EventArgs e)
        {
            playButton.Text = PlaySymbol;
        }

        private void OnPlayButtonClicked(object s, EventArgs e)
        {
            if (player.State != PlaybackState.Playing)
                player.Start();
            else
                player.Pause();
        }

        private static void OnSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AudioLayout layout && newValue is Uri uri)
            {
                layout.Source = uri;
                layout.player.Source = new UriMediaSource
                {
                    Uri = uri
                };
                layout.IsVisible = true;
            }
        }
    }
}
