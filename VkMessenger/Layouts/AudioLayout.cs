using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Loggers;
using Tizen.Multimedia;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using Button = Xamarin.Forms.Button;
using Label = Xamarin.Forms.Label;
using MediaPlayer = Xamarin.Forms.Platform.Tizen.Native.MediaPlayer;
using TextAlignment = Xamarin.Forms.TextAlignment;
using TizenConfig = Xamarin.Forms.PlatformConfiguration.Tizen;

namespace ru.MaxKuzmin.VkMessenger.Layouts
{
    public sealed class AudioLayout : StackLayout, IDisposable
    {
        private const int seekDelta = 5000;
        private const int timerInterval = 1;

        private Timer? timer;
        private MediaPlayer? player;
        private static event EventHandler? OnPauseAllPlayers;
        private bool isLoading;

        private readonly Button playButton = new Button
        {
            ImageSource = ImageResources.PlaySymbol,
            WidthRequest = 48
        };

        private readonly Button scrollBackButton = new Button
        {
            IsEnabled = false,
            ImageSource = ImageResources.BackDisabledSymbol,
            WidthRequest = 48
        };

        private readonly Button scrollForwardButton = new Button
        {
            IsEnabled = false,
            ImageSource = ImageResources.ForwardDisabledSymbol,
            WidthRequest = 48
        };

        private readonly Label durationLabel = new Label
        {
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            TextColor = Color.Gray,
            WidthRequest = 60
        };

        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(
                nameof(Source),
                typeof(Uri),
                typeof(AudioLayout),
                default(Uri),
                propertyChanged: OnSourcePropertyChanged);

        public static readonly BindableProperty DurationProperty =
            BindableProperty.Create(
                nameof(Duration),
                typeof(int?),
                typeof(AudioLayout),
                default(int?),
                propertyChanged: OnDurationPropertyChanged);

        public Uri? Source { get; private set; }
        public int? Duration { get; private set; }

        public AudioLayout()
        {
            Orientation = StackOrientation.Horizontal;
            playButton.Released += OnPlayButtonClicked;
            scrollBackButton.On<TizenConfig>().SetStyle(ButtonStyle.Circle);
            playButton.On<TizenConfig>().SetStyle(ButtonStyle.Circle);
            scrollForwardButton.On<TizenConfig>().SetStyle(ButtonStyle.Circle);

            Children.Add(scrollBackButton);
            Children.Add(playButton);
            Children.Add(scrollForwardButton);
            Children.Add(durationLabel);
        }

        public void Dispose()
        {
            OnPauseAllPlayers -= PauseThisPlayer;
            player?.Dispose();
            timer?.Dispose();
        }

        private async void OnPlayButtonClicked(object s, EventArgs e)
        {
            if (isLoading)
                return;
            if (player == null)
                await InitAndPlay();
            else if (player.State != PlaybackState.Playing)
                await Start();
            else
                Pause();
        }

        private static void OnSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AudioLayout layout && newValue is Uri uri)
            {
                layout.Source = uri;
            }
        }

        private static void OnDurationPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AudioLayout layout && newValue is int duration)
            {
                layout.Duration = duration;
                layout.durationLabel.Text = duration + LocalizedStrings.Sec;
            }
        }

        private async Task InitAndPlay()
        {
            if (Source == null)
                return;

            if (!AudioManager.GetConnectedDevices().Any(e => 
                e.Type == AudioDeviceType.BuiltinSpeaker || e.Type == AudioDeviceType.BluetoothMedia))
                Toast.DisplayText(LocalizedStrings.CantPlayAudioMessageNoHeadphones);

            isLoading = true;
            playButton.ImageSource = ImageResources.LoadingSymbol;
            playButton.IsEnabled = false;
            try
            {
                player = await InitPlayer();

                scrollBackButton.Released += OnScrollBackButtonClicked;
                scrollForwardButton.Released += OnScrollForwardButtonClicked;
                player.PlaybackCompleted += OnPlaybackCompleted;
                scrollBackButton.IsEnabled = true;
                scrollForwardButton.IsEnabled = true;
                scrollBackButton.ImageSource = ImageResources.BackSymbol;
                scrollForwardButton.ImageSource = ImageResources.ForwardSymbol;

                await Start();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                playButton.ImageSource = ImageResources.PlaySymbol;
                playButton.IsEnabled = true;
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task<MediaPlayer> InitPlayer()
        {
            var tempFileName = await DocumentsClient.DownloadDocumentToTempFile(Source!);

            OnPauseAllPlayers += PauseThisPlayer;

            timer = new Timer(
                obj => Device.InvokeOnMainThreadAsync(UpdateDurationLabel),
                null,
                TimeSpan.FromMilliseconds(-1),
                TimeSpan.FromSeconds(timerInterval));

            return new MediaPlayer
            {
                Source = new FileMediaSource
                {
                    File = tempFileName
                }
            };
        }

        private void UpdateDurationLabel()
        {
            if (player == null)
                return;

            durationLabel.Text = (player.Duration - player.Position) / 1000 + LocalizedStrings.Sec;
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

        private void Pause()
        {
            if (player == null)
                return;

            playButton.ImageSource = ImageResources.PlaySymbol;
            player.Pause();
        }

        private async Task Start()
        {
            if (player == null)
                return;

            OnPauseAllPlayers?.Invoke(this, null);
            playButton.IsEnabled = true;
            playButton.ImageSource = ImageResources.PauseSymbol;

            await player.Start();
            timer?.Change(TimeSpan.Zero, TimeSpan.FromSeconds(timerInterval));
        }

        private void OnPlaybackCompleted(object s, EventArgs e)
        {
            if (player == null)
                return;

            Pause();
            player.Seek(0);
            playButton.ImageSource = ImageResources.PlaySymbol;
            timer?.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromSeconds(timerInterval));
            durationLabel.Text = Duration + LocalizedStrings.Sec;
        }

        private void PauseThisPlayer(object s, EventArgs e)
        {
            if (s != this)
                Pause();
        }

        /// <summary>
        /// Prevents player activation on swipe and other actions
        /// </summary>
        public static void PauseAllPlayers()
        {
            OnPauseAllPlayers?.Invoke(null, null);
        }

        public void SetMarginForAnimation()
        {
            scrollBackButton.Margin = new Thickness(30, 0, 0, 0);
        }
    }
}
