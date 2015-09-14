using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using FileExplorer.CustomCollections;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Helpers;

namespace FileExplorer.ViewModels
{
    public class DirectoryViewModel : ViewModelBase, IDirectoryViewModel
    {
        #region private fields

        private NativeFileInfo _nativeDirectoryInfo;
        private DirectoryInfo _directoryInfo;

        #endregion

        #region public properties

        public string Path { get; }
        public string VisualPath { get; }
        public string DisplayName { get; }

        public string TypeName
        {
            get
            {
                if (_nativeDirectoryInfo == null)
                {
                    _nativeDirectoryInfo = new NativeFileInfo(Path);
                }
                return _nativeDirectoryInfo.TypeName;
            }
        }

        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                if (!_subDirectories.IsLoaded && !_subDirectories.IsLoading && _isExpanded)
                    _subDirectories.LoadAsync();
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if (!_subDirectories.IsLoaded && !_subDirectories.IsLoading && _isSelected)
                    _subDirectories.LoadAsync();
                if (!_files.IsLoaded && !_files.IsLoading && _isSelected)
                {
                    _files.LoadAsync();
                }
            }
        }

        private readonly AsyncLoadCollection<ISystemObjectViewModel> _files;

        public AsyncLoadCollection<ISystemObjectViewModel> Files
        {
            get { return _files; }
        }

        public ObservableCollectionBase<ISystemObjectViewModel> Children { get; }

        private readonly AsyncLoadCollection<IDirectoryViewModel> _subDirectories;

        public AsyncLoadCollection<IDirectoryViewModel> SubDirectories
        {
            get { return _subDirectories; }
        }

        public IDirectoryViewModel Parent { get; }
        public ICommand OpenCommand { get; }

        public ImageSource Icon
        {
            get
            {
                if (_nativeDirectoryInfo == null)
                {
                    _nativeDirectoryInfo = new NativeFileInfo(Path);
                }
                return _nativeDirectoryInfo.Icon;
            }
        }

        public long Size { get; }
        public DateTime? LastModificationDate { get; }

        private bool _hasItems;

        public bool HasItems
        {
            get { return _hasItems; }
        }

        #endregion

        #region constructor

        public DirectoryViewModel(DirectoryInfo directoryInfo, IDirectoryViewModel parent)
        {
            Parent = parent;
            _directoryInfo = directoryInfo;
            var directoryProvider = new SubDirectoriesProvider(_directoryInfo, this);
            Path = _directoryInfo.FullName;
            LastModificationDate = directoryInfo.LastWriteTime;
            DisplayName = _directoryInfo.Name;
            if (Parent != null)
                VisualPath = Parent.VisualPath + "\\" + DisplayName;
            _hasItems = directoryInfo.EnumerateDirectories().Any();
            Size = -1;
            _files = new AsyncLoadCollection<ISystemObjectViewModel>(new FilesProvider(directoryInfo));
            _subDirectories = new AsyncLoadCollection<IDirectoryViewModel>(directoryProvider);
            Children =
                new UnionCollection<IDirectoryViewModel, ISystemObjectViewModel, ISystemObjectViewModel>(
                    SubDirectories, Files);
            OpenCommand = new RelayCommand(() => Open());
        }

        #endregion

        #region public methods

        public void Open()
        {
            Parent.IsExpanded = true;
            IsSelected = true;
        }

        #endregion

    }
}
