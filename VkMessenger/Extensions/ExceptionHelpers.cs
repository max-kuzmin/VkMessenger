using ru.MaxKuzmin.VkMessenger.Exceptions;

namespace ru.MaxKuzmin.VkMessenger.Extensions
{
    public static class ExceptionHelpers
    {
        public static void ThrowIfInvalidSession(string? response)
        {
            if (response?.Contains("invalid session") == true)
            {
                throw new InvalidSessionException();
            }
        }
    }
}
