using System;
using System.Collections.ObjectModel;
using FileExplorer.DirectoriesHelpers;

namespace FileExplorer.CustomCollections
{
    public interface IItemsProvider<T>
    {
        ObservableCollection<T> GetItems(IProgress<int> progress);
        event EventHandler<CountLoadedEventArgs> CountLoaded;
    }

    public abstract class ItemsProviderBase<T> : IItemsProvider<T>
    {
        public abstract ObservableCollection<T> GetItems(IProgress<int> progress);

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
