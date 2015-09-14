using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Helpers;

namespace FileExplorer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region private fields

        private PathHelper _pathHelper;
        private Dispatcher _dispatcher;
        private List<FileSystemWatcher> _directoriesFileSystemWatchers = new List<FileSystemWatcher>();

        #endregion

        #region public properties

        public string Title { get; } = "FileExplorer";
        public TopViewModel Top { set; get; } = new TopViewModel();

        public ObservableCollectionEx<IDirectoryViewModel> Items { set; get; } =
            new ObservableCollectionEx<IDirectoryViewModel>();

        private IDirectoryViewModel _selectedDirectory;

        public IDirectoryViewModel SelectedDirectory
        {
            set
            {
                _selectedDirectory = value;
                OnSelectedDirectoryChanged(value);
            }
            get { return _selectedDirectory; }
        }

        public event EventHandler<SelectedDirectoryChangedArgs> SelectedDirectoryChanged;

        #endregion

        #region public types

        public class SelectedDirectoryChangedArgs : EventArgs
        {
            public IDirectoryViewModel NewDirectoryViewModel { get; }

            public SelectedDirectoryChangedArgs(IDirectoryViewModel newDirectoryViewModel)
            {
                NewDirectoryViewModel = newDirectoryViewModel;
            }
        }

        #endregion

        #region constructors

        public MainViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            SelectedDirectoryChanged += MainViewModel_SelectedDirectoryChanged;
            var root = new RootDirectoryViewModel();
            Items.Add(root);
            SelectedDirectory = root;
            _pathHelper = new PathHelper(SelectedDirectory);
            Top.CurrentPathSet += Top_CurrentPathSet;
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady) return;
                FileSystemWatcher directoryFileSystemWatcher = new FileSystemWatcher();
                directoryFileSystemWatcher.Path = drive.Name;
                /* Watch for changes in LastAccess and LastWrite times, and
                   the renaming of files or directories. */
                directoryFileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                                                          NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Add event handlers.
                directoryFileSystemWatcher.Changed += OnChanged;
                directoryFileSystemWatcher.Created += OnChanged;
                directoryFileSystemWatcher.Deleted += OnChanged;
                directoryFileSystemWatcher.Renamed += OnRenamed;
                // Begin watching.
                directoryFileSystemWatcher.EnableRaisingEvents = true;
                _directoriesFileSystemWatchers.Add(directoryFileSystemWatcher);
            }
        }

        #endregion

        #region private methods

        private void MainViewModel_SelectedDirectoryChanged(object sender, SelectedDirectoryChangedArgs e)
        {
            string path = e.NewDirectoryViewModel.VisualPath;
            if (Top.CurrentPath == path) return;
            Top.SetCurrentPath(path);
        }

        private void OnSelectedDirectoryChanged(IDirectoryViewModel directoryViewModel)
        {
            SelectedDirectoryChanged?.Invoke(this, new SelectedDirectoryChangedArgs(directoryViewModel));
        }

        private void Top_CurrentPathSet(object sender, NewPathSetArgs e)
        {
            var child = _pathHelper.GetAndLoadDirectory(e.Path);
            child.IsSelected = true;
        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            IDirectoryViewModel child;
            _dispatcher.Invoke(() =>
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:

                        break;
                    case WatcherChangeTypes.Changed:
                        bool isFile = File.Exists(e.FullPath);
                        child = _pathHelper.GetDirectory(e.FullPath);
                        int index = -1;
                        if (isFile)
                        {
                            for (int i = 0; i < child.Files.Count; i++)
                            {
                                var file = child.Files[i];
                                if (file.Path == e.FullPath)
                                {
                                    index = i;
                                    break;
                                }
                            }
                            if (index == -1) return;
                            child.Files[index] = new FileViewModel(new FileInfo(e.FullPath));
                        }
                        else
                        {
                            index = child.Parent.SubDirectories.IndexOf(child);
                            bool isSelected = child.IsSelected;
                            child.IsSelected = false;
                            child.Parent.SubDirectories[index] = new DirectoryViewModel(new DirectoryInfo(e.FullPath),
                                child.Parent) {IsExpanded = child.IsExpanded, IsSelected = isSelected};
                        }
                        break;

                    case WatcherChangeTypes.Deleted:
                        child = _pathHelper.GetDirectory(e.FullPath);
                        child.Parent.SubDirectories.Remove(child);
                        break;
                }
            });
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }

        #endregion

    }
}
