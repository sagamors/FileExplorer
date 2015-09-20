using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using FileExplorer.CustomCollections;
using FileExplorer.ViewModels;

namespace FileExplorer.DirectoriesHelpers
{
    class SubDirectoriesProvider : IItemsProvider<IDirectoryViewModel>
    {
        private readonly DirectoryInfo _directoryInfo;
        public IDirectoryViewModel Parent { get; }
        public SubDirectoriesProvider(DirectoryInfo directoryInfo, IDirectoryViewModel parent)
        {
            _directoryInfo = directoryInfo;
            Parent = parent;
        }

        public ObservableCollection<IDirectoryViewModel> GetItems(IProgress<int> progress)
        {
            ObservableCollection<IDirectoryViewModel> _collection = new ObservableCollection<IDirectoryViewModel>();
            var directories = _directoryInfo.GetDirectories();
            int length = directories.Length;
            for (int index = 0; index < directories.Length; index++)
            {
                var info = directories[index];
                try
                {
                    _collection.Add(new DirectoryViewModel(info, Parent));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                progress.Report(index/(length*100));
            }
            return _collection;
        }
    }
}
