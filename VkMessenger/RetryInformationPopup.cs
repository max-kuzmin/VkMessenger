﻿using System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger
{
    public class RetryInformationPopup : InformationPopup
    {
        public RetryInformationPopup(string message, Action retryAction)
        {
            Text = message;
            BottomButton = new MenuItem
            {
                Text = "Retry",
                Command = new Command(retryAction)
            };
        }
    }
}