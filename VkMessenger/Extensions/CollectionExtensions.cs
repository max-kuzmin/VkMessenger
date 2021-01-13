using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ru.MaxKuzmin.VkMessenger.Extensions
{
    public static class CollectionExtensions
    {
        public static void Trim<T>(this ObservableCollection<T> collection, int batchSize)
        {
            if (collection.Count <= batchSize)
                return;

            lock (collection)
            {
                while (collection.Count > batchSize)
                {
                    collection.RemoveAt(collection.Count - 1);
                }
            }
        }

        public static void PrependRange<T>(this ObservableCollection<T> collection, IReadOnlyCollection<T> items)
        {
            if (!items.Any())
                return;

            for (int i = 0; i < items.Count; i++)
            {
                collection.Insert(i, items.ElementAt(i));
            }
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IReadOnlyCollection<T> items)
        {
            if (!items.Any())
                return;

            foreach (var item in items)
            {
                collection.Insert(collection.Count, item);
            }
        }
    }
}
