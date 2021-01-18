using System;
using ru.MaxKuzmin.VkMessenger.Net;
using System.Threading;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Dtos;
using ru.MaxKuzmin.VkMessenger.Managers;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class LongPollingClient
    {
        public const int LongPoolingWaitTime = 25;

        public static async Task<LongPollingInitResponseDto> GetLongPollServer()
        {
            var url =
                "https://api.vk.com/method/messages.getLongPollServer" +
                "?v=5.124" +
                "&lp_version=3" +
                "&access_token=" + AuthorizationManager.Token;

            using var client = new ProxiedWebClient();
            var json = await HttpHelpers.RetryIfEmptyResponse<JsonDto<LongPollingInitResponseDto>>(
                () => client.GetAsync(new Uri(url)), e => e?.response != null);

            return json.response;
        }

        public static async Task<LongPollingUpdatesJsonDto> SendLongRequest(string server, string key, int ts, CancellationToken cancellationToken)
        {
            using var client = new ProxiedWebClient();

            var url = "https://" + server +
                "?act=a_check" +
                "&key=" + key +
                "&ts=" + ts +
                "&wait=" + LongPoolingWaitTime +
                "&version=3";

            var json = await HttpHelpers.RetryIfEmptyResponse<LongPollingUpdatesJsonDto>(
                () => client.GetAsync(new Uri(url), cancellationToken), e => e != null);

            return json;
        }
    }
}
