using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Helpers;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Models;
using Tizen.Multimedia;
using Tizen.System;
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

        private readonly AudioRecorder audioRecorder = new AudioRecorder(RecorderAudioCodec.Aac, RecorderFileFormat.ThreeGp)
        {
            AudioBitRate = 16000,
            TimeLimit = 300,
            AudioDevice = RecorderAudioDevice.Mic,
            AudioSampleRate = 16000,
            AudioChannels = 1
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
            VerticalOptions = LayoutOptions.EndAndExpand,
            IsEnabled = false
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

            PrivilegeChecker.PrivilegeCheck("http://tizen.org/privilege/recorder");
            PrivilegeChecker.PrivilegeCheck("http://tizen.org/privilege/mediastorage");
        }

        private void OnRecordingLimitReached(object sender, RecordingLimitReachedEventArgs e)
        {
            Toast.DisplayText(LocalizedStrings.VoiceMessageLimit);
            recordButton.ImageSource = ImageResources.RecordSymbol;
            isRecording = false;
            sendButton.IsEnabled = true;
            Logger.Error("Audio message time limit reached");
        }

        private void OnRecordButtonPressed(object sender, EventArgs e)
        {
            if (isRecording)
            {
                recordButton.ImageSource = ImageResources.RecordSymbol;
                audioRecorder.Commit();
                sendButton.IsEnabled = true;
            }
            else
            {
                recordButton.ImageSource = ImageResources.StopSymbol;
                sendButton.IsEnabled = false;
                DeleteTempFile();

                var internalStorage = StorageManager.Storages
                    .First(s => s.StorageType == StorageArea.Internal)
                    .GetAbsolutePath(DirectoryType.Others);
                voiceMessageTempPath = Path.Combine(internalStorage, Path.GetRandomFileName() + ".3gp");

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
