using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Tizen.Network.Connection;

namespace ru.MaxKuzmin.VkMessenger.Net
{
    public sealed class ProxiedWebClient: IDisposable
    {
        private readonly HttpClient httpClient;

        public ProxiedWebClient()
        {
            string proxyAddress = ConnectionManager.GetProxy(AddressFamily.IPv4);
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(proxyAddress)
            };
            httpClient = new HttpClient(handler, true);
        }

        public async Task<string> UploadFileAsync(byte[] file, string filename, string contentType, Uri url, CancellationToken cancellationToken = default)
        {
            string boundary = DateTime.Now.Ticks.ToString("x");
            var fileContent = new ByteArrayContent(file)
            {
                Headers = {ContentType = new MediaTypeHeaderValue(contentType)}
            };
            var content = new MultipartFormDataContent(boundary);
            content.Add(fileContent, "file", filename);
            var response = await httpClient.PostAsync(url, content, cancellationToken);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetAsync(Uri url, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.GetAsync(url, cancellationToken);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task DownloadFileAsync(Uri url, string path, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.GetAsync(url, cancellationToken);
            var stream = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(path, stream, cancellationToken);
        }

        public async Task<string> PostAsync(Uri url, string text, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsync(url, new StringContent(text), cancellationToken);
            return await response.Content.ReadAsStringAsync();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}
