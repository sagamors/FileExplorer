using System;
using System.Collections.ObjectModel;
using System.IO;
using FileExplorer.CustomCollections;
using FileExplorer.ViewModels;

namespace FileExplorer.DirectoriesHelpers
{
    class FilesProvider : IItemsProvider<ISystemObjectViewModel>
    {
        private readonly DirectoryInfo _directoryInfo;
        public FilesProvider(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
        }

        public ObservableCollection<ISystemObjectViewModel> GetItems(IProgress<int> progress)
        {
            ObservableCollection<ISystemObjectViewModel> _collection = new ObservableCollection<ISystemObjectViewModel>();
            var files = _directoryInfo.GetFiles();
            int length = files.Length;
            for (int index = 0; index < length; index++)
            {
                var file = files[index];
                _collection.Add(new FileViewModel(file));
                progress.Report(index / (length*100));
            }
            return _collection;
        }
    }
}