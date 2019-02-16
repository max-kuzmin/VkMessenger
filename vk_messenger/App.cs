using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Tizen.Wearable.CircularUI.Forms;
using System.Web;
using Tizen.Applications;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;

namespace vk_messenger
{
    public class App : Xamarin.Forms.Application
    {
        protected override void OnStart()
        {
            if (Preference.Contains("token"))
            {
                MainPage = new DialogsPage();
            }
            else
            {
                MainPage = new LoginPage();
            }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
