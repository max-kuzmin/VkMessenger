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
                return;

            lock (this)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    base.InsertItem(index + i, items.ElementAt(i));
                }
            }
        }

        public void AddRange(IReadOnlyCollection<T> items)
        {
            if (!items.Any())
                return;

            lock (this)
            {
                foreach (var item in items)
                {
                    base.InsertItem(Count, item);
                }
            }
        }

        public new void Insert(int index, T item)
        {
            lock (this)
            {
                base.Insert(index, item);
            }
        }

        public new bool Remove(T item)
        {
            lock (this)
            {
                return base.Remove(item);
            }
        }

        public void Trim(int batchSize)
        {
            if (Count <= batchSize)
                return;

            lock (this)
            {
                while (Count > batchSize)
                {
                    RemoveAt(Count - 1);
                }
            }
        }
    }
}