using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Tizen.Network.Connection;

namespace ru.MaxKuzmin.VkMessenger.Net
{
    public sealed class ProxiedWebClient : WebClient
    {
        public ProxiedWebClient()
        {
            string proxyAddress = ConnectionManager.GetProxy(AddressFamily.IPv4);
            if (!string.IsNullOrEmpty(proxyAddress))
            {
                Proxy = new WebProxy(proxyAddress, true);
            }
        }

        public async Task<string> UploadMultipartAsync(byte[] file, string filename, string contentType, Uri url)
        {
            string boundary = "----" + DateTime.Now.Ticks.ToString("x");
            Headers.Add("Content-Type", "multipart/form-data; boundary=" + boundary);
            var package1 =
                $"--{boundary}\r\n" +
                $"Content-Disposition: form-data; name=\"file\"; filename=\"{filename}\"\r\n" +
                $"Content-Type: {contentType}\r\n\r\n";
            var package2 = $"\r\n--{boundary}--\r\n";

            var reqData = new List<byte>();
            reqData.AddRange(Encoding.GetBytes(package1));
            reqData.AddRange(file);
            reqData.AddRange(Encoding.GetBytes(package2));

            var response = await UploadDataTaskAsync(url, "POST", reqData.ToArray());
            return Encoding.GetString(response);
        }
    }
}
