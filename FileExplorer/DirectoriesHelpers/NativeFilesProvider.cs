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

        public ObservableCollection<ISystemObjectViewModel> GetItems(IProgress<int> progress)
        {
            ObservableCollection<ISystemObjectViewModel> collection = new ObservableCollection<ISystemObjectViewModel>();
            DirectoryInfo fileInfo = new DirectoryInfo(_directoryInfo.Path);
            var files = fileInfo.GetFiles();
            int length = files.Length;
            for (int index = 0; index < length; index++)
            {
                var file = files[index];
                try
                {
                    collection.Add(new FileViewModel(file));
                    progress.Report(index / (length * 100));
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
