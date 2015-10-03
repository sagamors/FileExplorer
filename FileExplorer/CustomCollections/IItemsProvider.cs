using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace FileExplorer.CustomCollections
{
    public interface IItemsProvider<T>
    {
        ObservableCollection<T> GetItems(IProgress<int> progress, CancellationToken token);
        event EventHandler<CountLoadedEventArgs> CountLoaded;
    }

    public abstract class ItemsProviderBase<T> : IItemsProvider<T>
    {
        public abstract ObservableCollection<T> GetItems(IProgress<int> progress, CancellationToken token);

        public event EventHandler<CountLoadedEventArgs> CountLoaded;

        protected void OnCountLoaded(int count)
        {
            CountLoaded?.Invoke(this, new CountLoadedEventArgs(count));
        }
    }

    public class CountLoadedEventArgs : EventArgs
    {
        public int Count { get; }
        public CountLoadedEventArgs(int count)
        {
            Count = count;
        }
    }
}
