using ru.MaxKuzmin.VkMessenger.Localization;
using System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class RetryInformationPopup : InformationPopup
    {
        public RetryInformationPopup(string message, Action retryAction)
        {
            void CommandToExecute()
            {
                Dismiss();
                retryAction();
            }

            Text = message;
            BottomButton = new MenuItem
            {
                Text = LocalizedStrings.Retry,
                Command = new Command(CommandToExecute)
            };
        }
    }
}
