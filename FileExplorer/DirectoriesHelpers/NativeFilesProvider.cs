using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using FileExplorer.CustomCollections;
using FileExplorer.ViewModels;

namespace FileExplorer.DirectoriesHelpers
{
    class NativeFilesProvider : IItemsProvider<ISystemObjectViewModel>
    {
        private readonly NativeDirectoryInfo _directoryInfo;

        public NativeFilesProvider(NativeDirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
        }

        public ObservableCollection<ISystemObjectViewModel> GetItems()
        {
            ObservableCollection<ISystemObjectViewModel> collection = new ObservableCollection<ISystemObjectViewModel>();
            DirectoryInfo fileInfo = new DirectoryInfo(_directoryInfo.Path);
            var files = fileInfo.GetFiles();
            foreach (var file in files)
            {
                try
                {
                    collection.Add(new FileViewModel(file));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return collection;
        }
    }
}
