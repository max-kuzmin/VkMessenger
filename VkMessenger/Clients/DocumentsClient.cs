using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Dtos;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Managers;
using ru.MaxKuzmin.VkMessenger.Net;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class DocumentsClient
    {
        public static async Task<long> UploadAudioFile(string filePath)
        {
            try
            {
                var uploadLink = await GetUploadLink("audio_message");

                var fileDescriptor = await UploadFileUsingLink(uploadLink, filePath);

                var id = await SaveUploadedFileAsDoc(fileDescriptor);
                return id;

            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        private static async Task<Uri> GetUploadLink(string type)
        {
            var url =
                "https://api.vk.com/method/docs.getUploadServer" +
                "?v=5.124" +
                "&type=" + type +
                "&access_token=" + AuthorizationManager.Token;

            using var client = new ProxiedWebClient();
            var uploadLinkResponse = await HttpHelpers.RetryIfEmptyResponse<JsonDto<UploadLinkResponseDto>>(
                () => client.GetAsync(new Uri(url)), e => e?.response != null);
            return uploadLinkResponse.response.upload_url;
        }

        private static async Task<string> UploadFileUsingLink(Uri link, string filePath)
        {
            var fileData = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
            var fileName = Path.GetFileName(filePath);
            var fileExt = Path.GetExtension(filePath);
            var contentType = fileExt switch
            {
                ".3gp" => "video/3gpp",
                _ => throw new NotSupportedException("Content type is not supported")
            };

            using var client = new ProxiedWebClient();
            var deserialized = await HttpHelpers.RetryIfEmptyResponse<UploadFileResponseDto>(
                () => client.UploadFileAsync(fileData, fileName, contentType, link), e => e?.file != null);
            return deserialized.file;
        }

        private static async Task<long> SaveUploadedFileAsDoc(string fileDesc)
        {
            var url =
                    "https://api.vk.com/method/docs.save" +
                    "?v=5.124" +
                    "&file=" + fileDesc +
                    "&access_token=" + AuthorizationManager.Token;

            using var client = new ProxiedWebClient();
            var saveDocResponse = await HttpHelpers.RetryIfEmptyResponse<JsonDto<SaveDocResponseDto>>(
                () => client.GetAsync(new Uri(url)), e => e?.response != null);
            return saveDocResponse.response.audio_message!.id;
        }

        public static async Task<string> DownloadDocumentToTempFile(Uri source)
        {
            try
            {
                var fileName = source.Segments.Last();
                var tempFileName = Path.Combine(Path.GetTempPath(), fileName);
                var client = new ProxiedWebClient();
                if (!File.Exists(tempFileName))
                {
                    await client.DownloadFileAsync(source, tempFileName).ConfigureAwait(false);
                }

                return tempFileName;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }
    }
}
