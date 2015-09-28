using System.Collections.Specialized;
using System.IO;
using System.Linq;
using FileExplorer.CustomCollections;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Providers;
using PropertyChanged;

namespace FileExplorer.ViewModels
{
    [ImplementPropertyChanged]
    internal class RootDirectoryViewModel : DirectoryViewModelBase
    {
        public static DriveInfo[] Drives { get; } = DriveInfo.GetDrives();
        #region constructors

        public RootDirectoryViewModel() : base(new NativeDirectoryInfo(),null)
        {
            var nativeSubDirectoryProvider = new NativeSubDirectoryProvider((NativeDirectoryInfo)NativeSystemInfo, this);
            var nativeFilesProvider = new NativeFilesProvider(null,this);
            DisplayName = NativeSystemInfo.DisplayName;
            Path = NativeSystemInfo.Path;
            HasItems = true;
            VisualPath = DisplayName;
            SubDirectories = new AsyncLoadCollection<IDirectoryViewModel>(nativeSubDirectoryProvider);
            Files = new AsyncLoadCollection<ISystemObjectViewModel>(nativeFilesProvider);
            IsExpanded = true;
            IsSelected = true;
            Children = new UnionCollectionEx<IDirectoryViewModel, ISystemObjectViewModel, ISystemObjectViewModel>(SubDirectories, Files);
        }

        public RootDirectoryViewModel(NativeDirectoryInfo nativeDirectoryInfo, IDirectoryViewModel parent) : base(nativeDirectoryInfo, parent)
        {
            NativeSystemInfo = nativeDirectoryInfo;
            var directoryUnfo = new DirectoryInfo(nativeDirectoryInfo.Path);
            var subDirectoryProvider = new SubDirectoriesProvider(directoryUnfo, this);
            var filesProvider = new FilesProvider(directoryUnfo,this);
            DisplayName = nativeDirectoryInfo.DisplayName;
            Path = nativeDirectoryInfo.Path;
            VisualPath = Parent.VisualPath + "\\" + DisplayName;
            //is drive?
            DriveInfo driveInfo =  Drives.FirstOrDefault(info => PathHelper.NormalizePath(info.Name) == PathHelper.NormalizePath(Path));
            HasItems = driveInfo?.IsReady ?? directoryUnfo.EnumerateDirectories().Any();
            SubDirectories = new AsyncLoadCollection<IDirectoryViewModel>(subDirectoryProvider);
            SubDirectories.CollectionChanged += _subDirectories_CollectionChanged;
            Files = new AsyncLoadCollection<ISystemObjectViewModel>(filesProvider);
            Children = new UnionCollectionEx<IDirectoryViewModel, ISystemObjectViewModel, ISystemObjectViewModel>(SubDirectories, Files);
        }

        #endregion

        #region private methods

        private void _subDirectories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasItems = SubDirectories.Count != 0;
        }

        public override sealed void UpdateParameters()
        {
            var native = ((NativeDirectoryInfo) NativeSystemInfo);
            NativeSystemInfo = new NativeDirectoryInfo(native.PIDL, native.Parent);
            Icon = NativeSystemInfo.Icon;
        }

        #endregion

    }
}
