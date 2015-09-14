using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using FileExplorer.CustomCollections;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Helpers;
using PropertyChanged;

namespace FileExplorer.ViewModels
{
    [ImplementPropertyChanged]
    internal class RootDirectoryViewModel : ViewModelBase, IDirectoryViewModel
    {
        #region private fields

        private NativeDirectoryInfo _nativeDirectoryInfo;

        #endregion

        #region public proerties

        public string Path { get; }
        public string VisualPath { get; }
        public string TypeName { get; }

        public bool HasItems { private set; get; }
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

        public string DisplayName { get; }

        private ImageSource _icon;

        public ImageSource Icon
        {
            get { return _icon ?? (_icon = _nativeDirectoryInfo.Icon); }
        }

        public long Size { get; }
        public DateTime? LastModificationDate { get; }

        #endregion

        #region constructors

        public RootDirectoryViewModel()
        {
            _nativeDirectoryInfo = new NativeDirectoryInfo();
            var nativeSubDirectoryProvider = new NativeSubDirectoryProvider(_nativeDirectoryInfo, this);
            var nativeFilesProvider = new NativeFilesProvider(null);
            TypeName = _nativeDirectoryInfo.TypeName;
            DisplayName = _nativeDirectoryInfo.DisplayName;
            Path = _nativeDirectoryInfo.Path;
            Size = -1;
            HasItems = true;
            VisualPath = DisplayName;
            _subDirectories = new AsyncLoadCollection<IDirectoryViewModel>(nativeSubDirectoryProvider);
            _files = new AsyncLoadCollection<ISystemObjectViewModel>(nativeFilesProvider);
            IsExpanded = true;
            IsSelected = true;
            Children = new UnionCollection<IDirectoryViewModel, ISystemObjectViewModel, ISystemObjectViewModel>(
                SubDirectories, Files);
            OpenCommand = new RelayCommand(() => Open());
        }

        public RootDirectoryViewModel(NativeDirectoryInfo nativeDirectoryInfo, IDirectoryViewModel parent)
        {
            _nativeDirectoryInfo = nativeDirectoryInfo;
            Parent = parent;
            var directoryUnfo = new DirectoryInfo(nativeDirectoryInfo.Path);
            var subDirectoryProvider = new SubDirectoriesProvider(directoryUnfo, this);
            var filesProvider = new FilesProvider(directoryUnfo);
            TypeName = nativeDirectoryInfo.TypeName;
            DisplayName = nativeDirectoryInfo.DisplayName;
            Path = nativeDirectoryInfo.Path;
            VisualPath = Parent.VisualPath + "\\" + DisplayName;
            Size = -1;
            HasItems = directoryUnfo.EnumerateDirectories().Any();
            _subDirectories = new AsyncLoadCollection<IDirectoryViewModel>(subDirectoryProvider);
            _subDirectories.CollectionChanged += _subDirectories_CollectionChanged;
            _files = new AsyncLoadCollection<ISystemObjectViewModel>(filesProvider);
            Children =
                new UnionCollection<IDirectoryViewModel, ISystemObjectViewModel, ISystemObjectViewModel>(
                    SubDirectories, Files);
            OpenCommand = new RelayCommand(() => Open());
        }

        #endregion

        #region private methods

        private void _subDirectories_CollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            HasItems = _subDirectories.Count != 0;
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
