#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class JsonDto<T>
    {
        public T response { get; set; } = default!;
    }
}
