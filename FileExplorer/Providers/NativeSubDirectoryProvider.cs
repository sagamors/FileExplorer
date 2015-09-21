using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        public override ObservableCollection<IDirectoryViewModel> GetItems(IProgress<int> progress)
        {
            ObservableCollection<IDirectoryViewModel> _collection = new ObservableCollection<IDirectoryViewModel>();
            var directories2 = _systemInfo.GetDirectories();
            int length = directories2.Count;
            OnCountLoaded(length);
            double delta = 100.0 / length;
            for (int index = 0; index < length; index++)
            {
                var info = directories2[index];
                try
                {
                    _collection.Add(new RootDirectoryViewModel(info, Parent));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                progress.Report((int)(index * delta));
            }
            return _collection;
        }
    }
}