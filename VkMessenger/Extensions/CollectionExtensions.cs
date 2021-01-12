using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
#if DEBUG
using ru.MaxKuzmin.VkMessenger.Loggers;
#endif

namespace ru.MaxKuzmin.VkMessenger.Extensions
{
    public static class CollectionExtensions
    {
        public static void Trim<T>(this ObservableCollection<T> collection, int batchSize)
        {
            if (collection.Count <= batchSize)
                return;

#if DEBUG
            Logger.Debug("Try to lock " + typeof(T).Name + "s " + collection.GetHashCode());
#endif
            lock (collection)
            {
#if DEBUG
                Logger.Debug("Locked " + typeof(T).Name + "s " + collection.GetHashCode());
#endif
                while (collection.Count > batchSize)
                {
                    collection.RemoveAt(collection.Count - 1);
                }
            }
#if DEBUG
            Logger.Debug("Unlocked " + typeof(T).Name + "s " + collection.GetHashCode());
#endif
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
