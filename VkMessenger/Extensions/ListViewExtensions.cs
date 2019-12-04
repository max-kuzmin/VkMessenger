using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Extensions
{
    public static class ListViewExtensions
    {
        public static void ScrollIfExist<T>(this ListView listView, T itemToScroll, ScrollToPosition type)
        {
            if (itemToScroll != null)
            {
                listView.ScrollTo(itemToScroll, type, false);
            }
        }
    }
}
