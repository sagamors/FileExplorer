using System.IO;
using System.Linq;
using FileExplorer.CustomCollections;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Helpers;
using FileExplorer.Providers;

namespace FileExplorer.ViewModels
{
    public class DirectoryViewModel : DirectoryViewModelBase
    {
        #region private fields

        private NativeFileInfo _nativeDirectoryInfo;
        private DirectoryInfo _directoryInfo;

        #endregion

        #region constructor

        public DirectoryViewModel(DirectoryInfo directoryInfo, IDirectoryViewModel parent) : base(new NativeFileInfo(directoryInfo.FullName), parent)
        { 
            Parent = parent;
            _directoryInfo = directoryInfo;
            var directoryProvider = new SubDirectoriesProvider(_directoryInfo, this);
            Path = _directoryInfo.FullName;
            LastModificationDate = directoryInfo.LastWriteTime;
            DisplayName = _directoryInfo.Name;
            if (Parent != null)
                VisualPath = Parent.VisualPath + "\\" + DisplayName;
            HasItems = directoryInfo.EnumerateDirectories().Any();
            Size = -1;
            Files = new AsyncLoadCollection<ISystemObjectViewModel>(new FilesProvider(directoryInfo));
            SubDirectories = new AsyncLoadCollection<IDirectoryViewModel>(directoryProvider);
            Children =new UnionCollection<IDirectoryViewModel, ISystemObjectViewModel, ISystemObjectViewModel>(
                    SubDirectories, Files);
            OpenCommand = new RelayCommand(() => Open());
        }

        #endregion

        #region public methods

        public override void Open()
        {
            Parent.IsExpanded = true;
            IsSelected = true;
        }

        #endregion

    }
}
