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

        public ObservableCollection<IDirectoryViewModel> GetItems(IProgress<int> progress)
        {
            ObservableCollection<IDirectoryViewModel> _collection = new ObservableCollection<IDirectoryViewModel>();
            var directories2 = _directoryInfo.GetDirectories();
            int length = directories2.Count;
            for (int index = 0; index < length; index++)
            {
              
                var info = directories2[index];
                try
                {
                    _collection.Add(new RootDirectoryViewModel(info, Parent));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                progress.Report(index / (length * 100));
            }
            return _collection;
        }
    }
}