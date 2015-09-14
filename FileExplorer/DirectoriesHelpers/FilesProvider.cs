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

        public ObservableCollection<ISystemObjectViewModel> GetItems()
        {
            ObservableCollection<ISystemObjectViewModel> _collection = new ObservableCollection<ISystemObjectViewModel>();
            var files = _directoryInfo.GetFiles();
            foreach (var file in files)
            {
                _collection.Add(new FileViewModel(file));
            }
            return _collection;
        }
    }
}