using ru.MaxKuzmin.VkMessenger.Models;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Cells
{
    public class MessageCell : EntryCell
    {
        public MessageCell()
        {
            this.SetBinding(TextProperty, nameof(Message.Text));
        }
    }
}
