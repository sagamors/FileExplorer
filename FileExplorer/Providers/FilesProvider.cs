using System;
using System.Collections.ObjectModel;
using System.IO;
using FileExplorer.CustomCollections;
using FileExplorer.ViewModels;

namespace FileExplorer.Providers
{
    class FilesProvider : ItemsProviderBase<ISystemObjectViewModel>
    {

        private readonly DirectoryInfo _directoryInfo;
        public FilesProvider(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
        }

        public override ObservableCollection<ISystemObjectViewModel> GetItems(IProgress<int> progress)
        {
            ObservableCollection<ISystemObjectViewModel> _collection = new ObservableCollection<ISystemObjectViewModel>();
            var files = _directoryInfo.GetFiles();
            OnCountLoaded(files.Length);
            int length = files.Length;
            double delta = 100.0/length;
            for (int index = 0; index < length; index++)
            {
                var file = files[index];
                _collection.Add(new FileViewModel(file));
                progress.Report((int)(index* delta));
            }
            return _collection;
        }
    }
}