using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using FileExplorer.CustomCollections;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.ViewModels;

namespace FileExplorer.Providers
{
    class NativeSubDirectoryProvider : ItemsProviderBase<IDirectoryViewModel>
    {
        private readonly NativeDirectoryInfo _systemInfo;
        public IDirectoryViewModel Parent { get; }

        public NativeSubDirectoryProvider(NativeDirectoryInfo systemInfo, IDirectoryViewModel parent)
        {
            _systemInfo = systemInfo;
            Parent = parent;
        }

        public override ObservableCollection<IDirectoryViewModel> GetItems(IProgress<int> progress, CancellationToken token)
        {
            ObservableCollection<IDirectoryViewModel> _collection = new ObservableCollection<IDirectoryViewModel>();
            var directories2 = _systemInfo.GetDirectories();
            int length = directories2.Count;
            OnCountLoaded(length);
            double delta = 100.0 / length;
            for (int index = 0; index < length; index++)
            {
                token.ThrowIfCancellationRequested();
                var info = directories2[index];
        
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var newDirectory = new RootDirectoryViewModel(info, Parent);
                    _collection.Add(newDirectory);
                });
    
                progress.Report((int)((index + 1) * delta));
            }
            return _collection;
        }
    }
}