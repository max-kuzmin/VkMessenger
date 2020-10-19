using System;
using System.IO;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Models;
using Tizen.Multimedia;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using TizenConfig = Xamarin.Forms.PlatformConfiguration.Tizen;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class RecordVoicePage : ContentPage, IDisposable
    {
        private readonly Dialog dialog;
        private string? voiceMessageTempPath;
        private bool isRecording = false;

        private AudioRecorder audioRecorder = new AudioRecorder(RecorderAudioCodec.Aac, RecorderFileFormat.Ogg)
        {
            AudioBitRate = 128000,
            TimeLimit = 600,
            AudioDevice = RecorderAudioDevice.Mic,
            AudioSampleRate = 44100
        };

        private readonly StackLayout verticalLayout = new StackLayout
        {
            VerticalOptions = LayoutOptions.Fill
        };
        private readonly Button recordButton = new Button
        {
            ImageSource = ImageResources.RecordSymbol,
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.Center,
            WidthRequest = 75,
            HeightRequest = 75,
            Margin = new Thickness(0, 25, 0, 0)
        };
        private readonly Button sendButton = new Button
        {
            Text = LocalizedStrings.Send,
            VerticalOptions = LayoutOptions.EndAndExpand
        };

        public RecordVoicePage(Dialog dialog)
        {
            this.dialog = dialog;

            recordButton.On<TizenConfig>().SetStyle(ButtonStyle.Circle);
            sendButton.On<TizenConfig>().SetStyle(ButtonStyle.Bottom);
            verticalLayout.Children.Add(recordButton);
            verticalLayout.Children.Add(sendButton);
            Content = verticalLayout;

            audioRecorder.RecordingLimitReached += OnRecordingLimitReached;
            recordButton.Pressed += OnRecordButtonPressed;
            sendButton.Pressed += OnSendButtonPressed;
        }

        private void OnRecordingLimitReached(object sender, RecordingLimitReachedEventArgs e)
        {
            Toast.DisplayText(LocalizedStrings.VoiceMessageLimit);
            recordButton.ImageSource = ImageResources.RecordSymbol;
            isRecording = false;
            Logger.Error("Audio message time limit reached");
        }

        private void OnRecordButtonPressed(object sender, EventArgs e)
        {
            if (isRecording)
            {
                recordButton.ImageSource = ImageResources.RecordSymbol;
                audioRecorder.Commit();
            }
            else
            {
                recordButton.ImageSource = ImageResources.StopSymbol;
                DeleteTempFile();
                voiceMessageTempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
                audioRecorder.Unprepare();
                audioRecorder.Prepare();
                audioRecorder.Start(voiceMessageTempPath);
            }

            isRecording = !isRecording;
        }

        private async void OnSendButtonPressed(object sender, EventArgs e)
        {
            if (voiceMessageTempPath == null)
                return;

            await MessagesClient.Send(dialog.Id, null, voiceMessageTempPath);
            DeleteTempFile();
            await Navigation.PopAsync();
        }

        public void Dispose()
        {
            DeleteTempFile();
        }

        private void DeleteTempFile()
        {
            if (voiceMessageTempPath == null)
                return;
            _ = Task.Run(() =>
            {
                try
                {
                    File.Delete(voiceMessageTempPath);
                }
                catch
                {
                    // ignored
                }
            });
        }
    }
}
