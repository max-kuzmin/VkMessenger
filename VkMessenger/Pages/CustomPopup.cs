using System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class CustomPopup : InformationPopup
    {
        public CustomPopup(string message, string buttonText, Action? action = null, bool dismissOnBackButton = false)
        {
            void CommandToExecute()
            {
                Dismiss();
                action?.Invoke();
            }
            
            Text = message;
            BottomButton = new MenuItem
            {
                Text = buttonText,
                Command = new Command(CommandToExecute)
            };

            if (dismissOnBackButton)
                BackButtonPressed += (s, e) => Dismiss();
        }
    }
}
