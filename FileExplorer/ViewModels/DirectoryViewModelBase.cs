using System;
using System.Windows.Input;
using System.Windows.Media;
using FileExplorer.CustomCollections;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Helpers;

namespace FileExplorer.ViewModels
{
    public class DirectoryViewModelBase : ViewModelBase, IDirectoryViewModel
    {
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

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if(_isSelected)
                    LoadAll();
            }
        }

        public AsyncLoadCollection<ISystemObjectViewModel> Files { protected set; get; }
        public AsyncLoadCollection<IDirectoryViewModel> SubDirectories { protected set; get; }
        public UnionCollectionEx<IDirectoryViewModel, ISystemObjectViewModel, ISystemObjectViewModel> Children { protected set; get; }
        public IDirectoryViewModel Parent { get; }
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

        public void Open()
        {
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