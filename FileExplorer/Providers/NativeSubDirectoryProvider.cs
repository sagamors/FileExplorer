using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using FileExplorer.CustomCollections;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Helpers;
using FileExplorer.ViewModels;

namespace FileExplorer.Providers
{
    internal class NativeSubDirectoryProvider : ItemsProviderBase<IDirectoryViewModel>
    {
        private readonly NativeDirectoryInfo _systemInfo;
        public IDirectoryViewModel Parent { get; }

        public NativeSubDirectoryProvider(NativeDirectoryInfo systemInfo, IDirectoryViewModel parent)
        {
            _systemInfo = systemInfo;
            Parent = parent;
        }

        public override ObservableCollection<IDirectoryViewModel> GetItems(IProgress<int> progress,CancellationToken token)
        {
            ObservableCollection<IDirectoryViewModel> _collection = new ObservableCollection<IDirectoryViewModel>();
            var directories = _systemInfo.GetDirectories();
            int length = directories.Count;
            OnCountLoaded(length);
            double delta = 100.0 / length;
            for (int index = 0; index < length; index++)
            {
              
                token.ThrowIfCancellationRequested();
                var info = directories[index];
                var directory= new RootDirectoryViewModel(info, Parent);
                DriveInfo drive = DriveInfoEx.IsDrive(directory.Path);
                if (drive != null && drive.IsReady)
                {
                    _collection.Add(directory);
                }
                else
                {
                    if (drive == null)
                    {
                        _collection.Add(directory);
                    }
                }
                
                progress.Report((int) ((index + 1) * delta));
            }
            return _collection;
        }
    }
}