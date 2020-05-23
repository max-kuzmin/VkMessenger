using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ru.MaxKuzmin.VkMessenger.Collections
{
    public class CustomObservableCollection<T> : ObservableCollection<T>
    {
        public CustomObservableCollection()
        {
        }

        public CustomObservableCollection(IEnumerable<T> collection)
            : base(new List<T>(collection))
        {
        }

        public void InsertRange(int index, IReadOnlyCollection<T> items)
        {
            if (!items.Any())
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                base.InsertItem(index + i, items.ElementAt(i));
            }
        }

        public void AddRange(IReadOnlyCollection<T> items)
        {
            if (!items.Any())
            {
                return;
            }

            foreach (var item in items)
            {
                base.InsertItem(Count, item);
            }
        }
    }
}