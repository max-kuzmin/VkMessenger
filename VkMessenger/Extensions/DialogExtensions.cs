using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ru.MaxKuzmin.VkMessenger.Extensions
{
    public static class DialogExtensions
    {
        public static async Task SetReadWithMessagesAndPublish(this Dialog dialog)
        {
            if (dialog.UnreadCount != 0)
            {
                dialog.SetReadWithMessages();

                await DialogsClient.MarkAsRead(dialog.Id);
                _ = DurableCacheManager.SaveDialog(dialog);
            }
        }
    }
}
