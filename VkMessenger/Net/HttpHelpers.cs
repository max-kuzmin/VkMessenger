using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ru.MaxKuzmin.VkMessenger.Exceptions;
using ru.MaxKuzmin.VkMessenger.Extensions;
using ru.MaxKuzmin.VkMessenger.Loggers;

namespace ru.MaxKuzmin.VkMessenger.Net
{
    internal static class HttpHelpers
    {
        private const int Retries = 3;
        private const int IntervalMs = 500;

        public static async Task<T> RetryIfEmptyResponse<T>(Func<Task<string>> apiCall, Func<T, bool> condition,
            [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null, [CallerLineNumber] int line = 0)
        {
            string? json = null;
            for (int i = 0; i < Retries; i++)
            {
                json = await apiCall().ConfigureAwait(false);
                if (json != null)
                {
#if DEBUG
                    Logger.Debug(json, file, caller, line);
#endif
                    ExceptionHelpers.ThrowIfInvalidSession(json);

                    var dto = JsonConvert.DeserializeObject<T>(json);
                    if (dto != null && condition(dto))
                        return dto;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(IntervalMs)).ConfigureAwait(false);
            }

            Logger.Error(typeof(T).Name + " is null. Response: " + (json ?? string.Empty));
            throw new EmptyHttpResponseException();
        }
    }
}
