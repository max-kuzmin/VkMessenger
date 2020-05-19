using System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class CustomPopup : InformationPopup
    {
        public CustomPopup(string message, string buttonText, Action action)
        {
            void CommandToExecute()
            {
                Dismiss();
                action();
            }

            Text = message;
            BottomButton = new MenuItem
            {
                Text = buttonText,
                Command = new Command(CommandToExecute)
            };
        }
    }
}
