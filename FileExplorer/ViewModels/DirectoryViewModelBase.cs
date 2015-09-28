using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using FileExplorer.CustomCollections;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Exceptions;
using FileExplorer.Helpers;
using FileExplorer.Services;

namespace FileExplorer.ViewModels
{
    public abstract class DirectoryViewModelBase : ViewModelBase, IDirectoryViewModel
    {
        private bool _isOpening;

        public static event EventHandler<OpenDirectoryArgs> OpenDirectory; 

        public class OpenDirectoryArgs
        {
            public IDirectoryViewModel Directory { get; }
            public bool Cancel { get; set; }
            public OpenDirectoryArgs(IDirectoryViewModel directory)
            {
                Directory = directory;
            }
        }

        public class NoExistDirectoryArgs
        {
            public IDirectoryViewModel Directory { get; }
            public NoExistDirectoryArgs(IDirectoryViewModel directory)
            {
                Directory = directory;
            }
        }

        public static event EventHandler<NoExistDirectoryArgs> NoExistDirectory;


        #region private fields

        protected INativeSystemInfo NativeSystemInfo { set; get; }

        #endregion

        #region public properties

        public string Path { protected set;get; }
        public string VisualPath { protected set; get; }
        public string DisplayName { protected set; get; }

        public string TypeName
        {
            get
            {
                return NativeSystemInfo.TypeName;
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
        private ImageSource _icon;

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
            protected set { _icon = value; }
            get { return _icon?? NativeSystemInfo.Icon; }
        }

        public DateTime? LastModificationDate { protected set; get; }

        public bool HasItems { protected set; get; }

        public DirectoryViewModelBase(INativeSystemInfo nativeSystemInfo, IDirectoryViewModel parent)
        {
            NativeSystemInfo = nativeSystemInfo;
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
            try
            {
                if (NoAccess)
                {
                    MessageBoxService.Instance.ShowError(AccessDirectoryDeniedException.Msg);
                    return;
                }
                _isOpening = true;
                if (OnOpenDirectory(this)) return;
                LoadAll();
            }
            catch (Exception ex)
            {
                var existDirectory = PathHelper.ClearNotExistDirectories(this);
                MessageBoxService.Instance.ShowError(ex.Message);
                if (existDirectory!=null)
                {
                    if (OnOpenDirectory(existDirectory)) return;
                }
            }
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

        public FileSystemWatcher FileSystemWatcher { get; set; }

        public long Size { private set;get; }

        /// <summary>
        /// Update icon,size,last modification
        /// </summary>
        public abstract void UpdateParameters();

        #endregion

        public static bool OnOpenDirectory(IDirectoryViewModel e)
        {
            var args = new OpenDirectoryArgs(e);
            if (e.Path != "" && !Directory.Exists(e.Path))
            {
                OnNoExistDirectory(e);
                throw new DirectoryDoesExistException();
            }
            OpenDirectory?.Invoke(null, args);
            return args.Cancel;
        }


        public static void OnNoExistDirectory(IDirectoryViewModel directoryViewModel)
        {
            NoExistDirectory?.Invoke(null, new NoExistDirectoryArgs(directoryViewModel));
        }
    }
}