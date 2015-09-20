using System;
using System.Collections.ObjectModel;

namespace FileExplorer.CustomCollections
{
    public interface IItemsProvider<T>
    {
        ObservableCollection<T> GetItems(IProgress<int> progress);
    }
}
