using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ru.MaxKuzmin.VkMessenger.Exceptions;
using ru.MaxKuzmin.VkMessenger.Loggers;

namespace ru.MaxKuzmin.VkMessenger.Net
{
    internal static class HttpHelpers
    {
        private const int Retries = 3;
        private const int IntervalMs = 500;

        public static async Task<T> RetryIfEmptyResponse<T>(Func<Task<string>> apiCall, Func<T, bool> condition)
        {
            string? json = null;
            for (int i = 0; i < Retries; i++)
            {
                json = await apiCall();
                var dto = JsonConvert.DeserializeObject<T>(json);
                if (condition(dto))
                    return dto;

                await Task.Delay(TimeSpan.FromMilliseconds(IntervalMs));
            }

            Logger.Error(typeof(T).Name + " is null. Response: " + (json ?? string.Empty));
            throw new EmptyHttpResponseException();
        }
    }
}
