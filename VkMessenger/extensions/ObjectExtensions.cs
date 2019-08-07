using Newtonsoft.Json;

namespace ru.MaxKuzmin.VkMessenger.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToJson(this object obj) => JsonConvert.SerializeObject(obj);
    }
}
