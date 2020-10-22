using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ru.MaxKuzmin.VkMessenger.Dtos;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Models;
using ru.MaxKuzmin.VkMessenger.Net;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class DocumentsClient
    {
        public static async Task<long> UploadAudioFile(string filePath)
        {
            try
            {
                var uploadLinkResponse = await HttpHelpers.RetryIfEmptyResponse<JsonDto<UploadLinkResponseDto>>(
                    () => GetUploadLinkJson("audio_message"), e => e?.response != null);
                var uploadLink = uploadLinkResponse.response.upload_url;

                var fileDescriptor = await UploadFileUsingLink(uploadLink, filePath);

                var saveDocResponse = await HttpHelpers.RetryIfEmptyResponse<JsonDto<SaveDocResponseDto>>(
                    () => SaveUploadedFileAsDoc(fileDescriptor), e => e?.response != null);
                var id = saveDocResponse.response.audio_message!.id;
                return id;

            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        public static async Task<string> GetUploadLinkJson(string type)
        {
            try
            {
                var url =
                    "https://api.vk.com/method/docs.getUploadServer" +
                    "?v=5.124" +
                    "&type=" + type +
                    "&access_token=" + Authorization.Token;

                using var client = new ProxiedWebClient();
                return await client.DownloadStringTaskAsync(url);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        public static async Task<string> UploadFileUsingLink(Uri link, string filePath)
        {
            try
            {
                var fileData = await File.ReadAllBytesAsync(filePath);
                var fileName = Path.GetFileName(filePath);
                var fileExt = Path.GetExtension(filePath);
                var contentType = fileExt switch
                {
                    ".3gp" => "video/3gpp",
                    _ => throw new NotSupportedException("Content type is not supported")
                };

                using var client = new ProxiedWebClient();
                var json = await client.UploadMultipartAsync(fileData, fileName, contentType, link);
                var deserialized = JsonConvert.DeserializeObject<UploadFileResponseDto>(json);
                return deserialized.file;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        public static async Task<string> SaveUploadedFileAsDoc(string fileDesc)
        {
            try
            {
                var url =
                    "https://api.vk.com/method/docs.save" +
                    "?v=5.124" +
                    "&file=" + fileDesc +
                    "&access_token=" + Authorization.Token;

                using var client = new ProxiedWebClient();
                return await client.DownloadStringTaskAsync(url);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }
    }
}
