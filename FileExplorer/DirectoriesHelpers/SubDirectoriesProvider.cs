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

        public ObservableCollection<IDirectoryViewModel> GetItems()
        {
            ObservableCollection<IDirectoryViewModel> _collection = new ObservableCollection<IDirectoryViewModel>();
            var directories = _directoryInfo.GetDirectories();

            foreach (var info in directories)
            {
                try
                {
                    _collection.Add(new DirectoryViewModel(info, Parent));
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
