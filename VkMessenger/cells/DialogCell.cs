using ru.MaxKuzmin.VkMessenger.Models;
using Xamarin.Forms;
namespace ru.MaxKuzmin.VkMessenger.Cells
{
    public class DialogCell: ImageCell
    {
        public DialogCell()
        {
            this.SetBinding(TextProperty, nameof(Dialog.Name));
            this.SetBinding(DetailProperty, nameof(Dialog.Text));
            this.SetBinding(ImageSourceProperty, nameof(Dialog.Photo));
            this.SetBinding(TextColorProperty, nameof(Dialog.TextColor));
        }
    }
}
