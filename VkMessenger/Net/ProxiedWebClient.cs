using System;
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
            string boundary = "------------------------" + DateTime.Now.Ticks.ToString("x");
            Headers.Add("Content-Type", "multipart/form-data; boundary=" + boundary);
            var fileData = Encoding.GetString(file);
            var package =
                $"--{boundary}\r\n" +
                $"Content-Disposition: form-data; name=\"file\"; filename=\"{filename}\"\r\n" +
                $"Content-Type: {contentType}\r\n\r\n" +
                $"{fileData}\r\n" +
                $"--{boundary}--\r\n";

            var reqData = Encoding.GetBytes(package);

            var response = await UploadDataTaskAsync(url, "POST", reqData);
            return Encoding.GetString(response);
        }
    }
}
