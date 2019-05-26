using System;
using Tizen.Wearable.CircularUI.Forms;
using Tizen.Wearable.CircularUI.Forms.Renderer;
using Xamarin.Forms.Platform.Tizen;

namespace ru.MaxKuzmin.VkMessenger
{
    class Program : FormsApplication
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            LoadApplication(new App());
        }

        static void Main(string[] args)
        {
            var app = new Program();
            Forms.Init(app);
            FormsCircularUI.Init();
            try
            {
                app.Run(args);
            }
            catch (Exception e)
            {
                Log.Error(nameof(VkMessenger), e.ToString());
                new InformationPopup
                {
                    Title = "Unhandled exception",
                    Text = e.Message,
                }.Show();
            }
        }
    }
}
