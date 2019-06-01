using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using Tizen;

namespace ru.MaxKuzmin.VkMessenger
{
    public class Logger
    {
        private const string LogFile = "/opt/usr/home/owner/media/Documents/VkMessenger.log";

        public static void Error(Exception e)
        {
            Log.Error(nameof(VkMessenger), e.ToString());

            try
            {
                using (var file = File.OpenWrite(LogFile))
                using (var writer = new StreamWriter(file))
                {
                    writer.WriteLine("\n\nException: " + e.ToString() + JsonConvert.SerializeObject(e));

                    if (e is WebException webEx)
                    {
                        using (var resp = new StreamReader(webEx.Response.GetResponseStream()))
                        {
                            resp.ReadToEnd();
                            writer.WriteLine(resp);
                        }
                    }
                }
            }
            catch { }
        }
    }
}
