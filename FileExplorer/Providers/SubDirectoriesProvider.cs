using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using FileExplorer.CustomCollections;
using FileExplorer.ViewModels;

namespace FileExplorer.Providers
{
    class SubDirectoriesProvider : ItemsProviderBase<IDirectoryViewModel>
    {
        private readonly DirectoryInfo _directoryInfo;
        public IDirectoryViewModel Parent { get; }
        public SubDirectoriesProvider(DirectoryInfo directoryInfo, IDirectoryViewModel parent)
        {
            _directoryInfo = directoryInfo;
            Parent = parent;
        }

        public override ObservableCollection<IDirectoryViewModel> GetItems(IProgress<int> progress, CancellationToken token)
        {
            ObservableCollection<IDirectoryViewModel> _collection = new ObservableCollection<IDirectoryViewModel>();
            var directories = _directoryInfo.GetDirectories();
            int length = directories.Length;
            OnCountLoaded(length);
            double delta = 100.0/length;
            for (int index = 0; index < directories.Length; index++)
            {
                token.ThrowIfCancellationRequested();
                var info = directories[index];
                try
                {
                    _collection.Add(new DirectoryViewModel(info, Parent));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                progress.Report((int)((index + 1) * delta));

            }
            return _collection;
        }
    }
}
