using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Helpers;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Loggers;
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
        private readonly int dialogId;
        private string? voiceMessageTempPath;
        private bool isRecording;

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

        public RecordVoicePage(int dialogId)
        {
            this.dialogId = dialogId;

            recordButton.On<TizenConfig>().SetStyle(ButtonStyle.Circle);
            sendButton.On<TizenConfig>().SetStyle(ButtonStyle.Bottom);
            verticalLayout.Children.Add(recordButton);
            verticalLayout.Children.Add(sendButton);
            Content = verticalLayout;

            audioRecorder.RecordingLimitReached += OnRecordingLimitReached;
            recordButton.Clicked += OnRecordButtonPressed;
            sendButton.Clicked += OnSendButtonPressed;

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
                audioRecorder.Commit();
                audioRecorder.Unprepare();
                recordButton.ImageSource = ImageResources.RecordSymbol;
                sendButton.IsEnabled = true;
            }
            else
            {
                DeleteTempFile();

                var internalStorage = StorageManager.Storages
                    .First(s => s.StorageType == StorageArea.Internal)
                    .GetAbsolutePath(DirectoryType.Others);
                voiceMessageTempPath = Path.Combine(internalStorage, Path.GetRandomFileName() + ".3gp");

                audioRecorder.Prepare();
                audioRecorder.Start(voiceMessageTempPath);
                recordButton.ImageSource = ImageResources.StopSymbol;
                sendButton.IsEnabled = false;
            }

            isRecording = !isRecording;
        }

        private async void OnSendButtonPressed(object sender, EventArgs e)
        {
            if (voiceMessageTempPath == null)
                return;

            await NetExceptionCatchHelpers.CatchNetException(
                async () =>
                {
                    sendButton.IsEnabled = false;
                    await MessagesClient.Send(dialogId, null, voiceMessageTempPath);
                    DeleteTempFile();
                    await Navigation.PopAsync();
                },
                () =>
                {
                    OnSendButtonPressed(sender, e);
                    return Task.CompletedTask;
                },
                LocalizedStrings.SendMessageNoInternetError,
                false);

            sendButton.IsEnabled = true;
        }

        public void Dispose()
        {
            audioRecorder.Dispose();
            DeleteTempFile();
        }

        private void DeleteTempFile()
        {
            if (voiceMessageTempPath == null)
                return;

            try
            {
                File.Delete(voiceMessageTempPath);
            }
            catch
            {
                // ignored
            }
        }
    }
}
