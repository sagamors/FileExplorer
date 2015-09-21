using System;
using System.Collections.ObjectModel;
using System.IO;
using FileExplorer.CustomCollections;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.ViewModels;

namespace FileExplorer.Providers
{
    class NativeFilesProvider : ItemsProviderBase<ISystemObjectViewModel>
    {
        private readonly NativeDirectoryInfo _directoryInfo;

        public NativeFilesProvider(NativeDirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
        }

        public override ObservableCollection<ISystemObjectViewModel> GetItems(IProgress<int> progress)
        {
            ObservableCollection<ISystemObjectViewModel> collection = new ObservableCollection<ISystemObjectViewModel>();
            DirectoryInfo fileInfo = new DirectoryInfo(_directoryInfo.Path);
            var files = fileInfo.GetFiles();
            OnCountLoaded(files.Length);
            int length = files.Length;
            for (int index = 0; index < length; index++)
            {
                var file = files[index];
                try
                {
                    collection.Add(new FileViewModel(file));
                    progress.Report((int)((index * 1.0 / length) * 100));
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
