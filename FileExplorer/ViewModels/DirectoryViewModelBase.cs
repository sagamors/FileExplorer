using System;
using System.Windows.Input;
using System.Windows.Media;
using FileExplorer.CustomCollections;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Helpers;
using FileExplorer.Services;

namespace FileExplorer.ViewModels
{
    public class DirectoryViewModelBase : ViewModelBase, IDirectoryViewModel
    {
        private bool _isOpening;

        public static event EventHandler<OpenDirectoryArgs> OpenDirectory; 

        public class OpenDirectoryArgs
        {
            public IDirectoryViewModel Directory { get; }
            public OpenDirectoryArgs(IDirectoryViewModel directory)
            {
                Directory = directory;
            }
        }

        #region private fields

        protected INativeSystemInfo _nativeSystemInfo;

        #endregion

        #region public properties

        public string Path { protected set;get; }
        public string VisualPath { protected set; get; }
        public string DisplayName { protected set; get; }

        public string TypeName
        {
            get
            {
                return _nativeSystemInfo.TypeName;
            }
        }

        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                if (!SubDirectories.IsLoaded && !SubDirectories.IsLoading && _isExpanded)
                    SubDirectories.LoadAsync();
            }
        }

        private bool _isSelected;
        private AsyncLoadCollection<IDirectoryViewModel> _subDirectories;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if(_isSelected)
                    Open();
            }
        }

        public AsyncLoadCollection<ISystemObjectViewModel> Files { protected set; get; }

        public AsyncLoadCollection<IDirectoryViewModel> SubDirectories
        {
            protected set
            {
                if(_subDirectories!=null)
                _subDirectories.LoadingError -= SubDirectories_LoadingError;
                _subDirectories = value;
                if (_subDirectories != null)
                    _subDirectories.LoadingError += SubDirectories_LoadingError;
            }
            get { return _subDirectories; }
        }

        public UnionCollectionEx<IDirectoryViewModel, ISystemObjectViewModel, ISystemObjectViewModel> Children { protected set; get; }
        public IDirectoryViewModel Parent { get; }
        public bool NoAccess { protected set;get; }
        public ICommand OpenCommand { get; }

        public ImageSource Icon
        {
            get
            {
                return _nativeSystemInfo.Icon;
            }
        }

        public long Size { protected set; get; }
        public DateTime? LastModificationDate { protected set; get; }

        public bool HasItems { protected set; get; }

        public DirectoryViewModelBase(INativeSystemInfo nativeSystemInfo, IDirectoryViewModel parent)
        {
            _nativeSystemInfo = nativeSystemInfo;
            Parent = parent;
            OpenCommand = new RelayCommand(Open);
            Size = -1;

        }

        private void SubDirectories_LoadingError(object sender, System.IO.ErrorEventArgs e)
        {
            if (_isOpening)
            {
                MessageBoxService.Instance.ShowError(e.GetException().Message);
                Parent.Open();
            }
            _isOpening = false;
        }

        public void Open()
        {
            if (NoAccess)
            {
                MessageBoxService.Instance.ShowError("Access to the directory is denied");
                return;
            }
            _isOpening = true;
            LoadAll();
            OnOpenDirectory(this);
        }

        public void LoadAll()
        {
            if (!SubDirectories.IsLoaded && !SubDirectories.IsLoading)
                SubDirectories.LoadAsync();
            if (!Files.IsLoaded && !Files.IsLoading)
            {
                Files.LoadAsync();
            }
        }

        public virtual void UpdateHasItems()
        {
            if(SubDirectories.IsLoaded)
                HasItems = SubDirectories.HasItems;
        }

        #endregion

        private static void OnOpenDirectory(IDirectoryViewModel e)
        {
            OpenDirectory?.Invoke(null, new OpenDirectoryArgs(e));
        }
    }
}