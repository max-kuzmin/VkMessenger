using System;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ru.MaxKuzmin.VkMessenger.Extensions
{
    public static class DialogExtensions
    {
        private static readonly TimeSpan ShowReadTimeout = TimeSpan.FromSeconds(3);

        public static async Task SetReadWithMessagesAndPublish(this Dialog dialog)
        {
            await Task.Delay(ShowReadTimeout);

            if (dialog.UnreadCount != 0 || dialog.Messages.Any(m => !m.Read))
            {
                dialog.SetReadWithMessages();

                await DialogsClient.MarkAsRead(dialog.Id);
                _ = DurableCacheManager.SaveDialog(dialog);
            }
        }
    }
}
