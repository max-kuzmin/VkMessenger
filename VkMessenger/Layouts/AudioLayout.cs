using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Net;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen.Native;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using Button = Xamarin.Forms.Button;

namespace ru.MaxKuzmin.VkMessenger.Layouts
{
    public sealed class AudioLayout: StackLayout, IDisposable
    {
        private const string PlaySymbol = "▶️";
        private const string PauseSymbol = "⏸";
        private const string BackSymbol = "⏪";
        private const string ForwardSymbol = "⏩️";
        private const string LoadingSymbol = "🔄";

        private static event EventHandler? OnStopAllPlayers;

        private readonly Button playButton = new Button
        {
            Text = PlaySymbol,
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Button)),
            BackgroundColor = Color.Transparent
        };
        private readonly Button scrollBackButton = new Button    
        {
            Text = BackSymbol,
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Button)),
            IsEnabled = false,
            BackgroundColor = Color.Transparent
        };
        private readonly Button scrollForwardButton = new Button
        {
            Text = ForwardSymbol,
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Button)),
            IsEnabled = false,
            BackgroundColor = Color.Transparent
        };

        private MediaPlayer? player;

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
            playButton.Clicked += OnPlayButtonClicked;

            Children.Add(scrollBackButton);
            Children.Add(playButton);
            Children.Add(scrollForwardButton);
        }

        public void Dispose()
        {
            OnStopAllPlayers -= StopThisPlayer;
            player?.Dispose();
        }

        private async void OnPlayButtonClicked(object s, EventArgs e)
        {
            if (playButton.Text == LoadingSymbol)
            {
                return;
            }
            if (player == null)
            {
                await InitAndPlay();
            }
            else if (player.State != PlaybackState.Playing)
            {
                await Start();
            }
            else
            {
                playButton.Text = PlaySymbol;
                player.Pause();
            }
        }

        private async Task Start()
        {
            if (player == null)
                return;

            playButton.IsEnabled = true;
            playButton.Text = PauseSymbol;
            await player.Start();
            OnStopAllPlayers?.Invoke(this, null);
        }

        private static void OnSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AudioLayout layout && newValue is Uri uri)
            {
                layout.Source = uri;
                layout.IsVisible = true;
            }
        }

        private async Task InitAndPlay()
        {
            if (Source == null)
                return;

            playButton.Text = LoadingSymbol;
            playButton.IsEnabled = false;
            try
            {
                player = await InitPlayer();

                scrollBackButton.Clicked += OnScrollBackButtonClicked;
                scrollForwardButton.Clicked += OnScrollForwardButtonClicked;
                player.PlaybackCompleted += OnPlaybackCompleted;
                scrollBackButton.IsEnabled = true;
                scrollForwardButton.IsEnabled = true;

                await Start();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                playButton.Text = PlaySymbol;
                playButton.IsEnabled = true;
            }
        }

        private async Task<MediaPlayer> InitPlayer()
        {
            var fileName = Source!.Segments.Last();
            var tempFileName = Path.Combine(Path.GetTempPath(), fileName);

            if (!File.Exists(tempFileName))
            {
                var client = new ProxiedWebClient();
                await client.DownloadFileTaskAsync(Source!, tempFileName);
            }

            OnStopAllPlayers += StopThisPlayer;
            
            return new MediaPlayer
            {
                Source = new FileMediaSource
                {
                    File = tempFileName
                }
            };
        }

        private void OnScrollBackButtonClicked(object s, EventArgs e)
        {
            if (player == null)
                return;
            
            var pos = Math.Max(player.Position - seekDelta, 0);
            player.Seek(pos);
        }

        private void OnScrollForwardButtonClicked(object s, EventArgs e)
        {
            if (player == null)
                return;

            var pos = Math.Min(player.Position + seekDelta, player.Duration);
            player.Seek(pos);
        }

        private void OnPlaybackCompleted(object s, EventArgs e)
        {
            if (player == null)
                return;

            player.Stop();
            player.Seek(0);
            playButton.Text = PlaySymbol;
        }

        private void StopThisPlayer(object s, EventArgs e)
        {
            if (s != this)
                OnPlaybackCompleted(s, e);
        }
    }
}
