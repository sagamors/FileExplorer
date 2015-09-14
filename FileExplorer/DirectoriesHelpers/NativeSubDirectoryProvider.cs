using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using FileExplorer.CustomCollections;
using FileExplorer.ViewModels;

namespace FileExplorer.DirectoriesHelpers
{
    class NativeSubDirectoryProvider : IItemsProvider<IDirectoryViewModel>
    {
        private readonly NativeDirectoryInfo _directoryInfo;
        public IDirectoryViewModel Parent { get; }

        public NativeSubDirectoryProvider(NativeDirectoryInfo directoryInfo, IDirectoryViewModel parent)
        {
            _directoryInfo = directoryInfo;
            Parent = parent;
        }

        public ObservableCollection<IDirectoryViewModel> GetItems()
        {
            ObservableCollection<IDirectoryViewModel> _collection = new ObservableCollection<IDirectoryViewModel>();
            var directories2 = _directoryInfo.GetDirectories();
            foreach (var info in directories2)
            {
                try
                {
                    _collection.Add(new RootDirectoryViewModel(info, Parent));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            return _collection;
        }
    }
}