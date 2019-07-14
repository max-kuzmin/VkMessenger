using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ru.MaxKuzmin.VkMessenger
{
    public class CustomObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public CustomObservableCollection()
        {
        }

        public CustomObservableCollection(IEnumerable<T> collection) : base(new List<T>(collection))
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void ClearItems()
        {
            base.ClearItems();
            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionReset();
        }

        protected override void RemoveItem(int index)
        {
            T removedItem = this[index];

            base.RemoveItem(index);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove, removedItem, index));
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item, index));
        }

        public void InsertRange(int index, IList<T> items)
        {
            foreach (var item in items)
            {
                base.InsertItem(index, item);
            }

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            //It's important to use event args without index
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, items));
        }

        public void AddRange(IList<T> items)
        {
            var startingIndex = Count;
            foreach (var item in items)
            {
                base.InsertItem(Count, item);
            }

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            //It's important to use event args with index
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, items.Cast<object>().ToList(), startingIndex));
        }

        protected override void SetItem(int index, T item)
        {
            T originalItem = this[index];
            base.SetItem(index, item);

            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace, originalItem, item, index));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) =>
            PropertyChanged?.Invoke(this, e);

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) =>
            CollectionChanged?.Invoke(this, e);

        private void OnCountPropertyChanged() => OnPropertyChanged(EventArgsCache.CountPropertyChanged);

        private void OnIndexerPropertyChanged() => OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);

        private void OnCollectionReset() => OnCollectionChanged(EventArgsCache.ResetCollectionChanged);

        private static class EventArgsCache
        {
            public static readonly PropertyChangedEventArgs CountPropertyChanged =
                new PropertyChangedEventArgs("Count");
            public static readonly PropertyChangedEventArgs IndexerPropertyChanged =
                new PropertyChangedEventArgs("Item[]");
            public static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged =
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
        }
    }
}