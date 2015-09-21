using System;
using System.Collections.Generic;
using System.Globalization;
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
        public TopViewModel Top { set; get; }

        public ObservableCollectionEx<IDirectoryViewModel> Items { set; get; } = new ObservableCollectionEx<IDirectoryViewModel>();

        #endregion

        #region constructors

        public MainViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            var root = new RootDirectoryViewModel();
            Items.Add(root);
            _pathHelper = new PathHelper(root);
            Top = new TopViewModel(_pathHelper);
            Top.SelectedDirectory = root;
            DirectoryViewModelBase.OpenDirectory += DirectoryViewModelBase_OpenDirectory;

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady) return;
                FileSystemWatcher directoryFileSystemWatcher = new FileSystemWatcher();
                directoryFileSystemWatcher.Path = drive.Name;
                /* Watch for changes in LastAccess and LastWrite times, and
                   the renaming of files or directories. */
                directoryFileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size;
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

        private void DirectoryViewModelBase_OpenDirectory(object sender, DirectoryViewModelBase.OpenDirectoryArgs e)
        {
            Top.SelectedDirectory = e.Directory;
        }

        #endregion

        #region private methods

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            IDirectoryViewModel parent;
            IDirectoryViewModel child = _pathHelper.GetDirectory(e.FullPath, out parent);
            bool isFile;
            int index = -1;
            isFile = Path.HasExtension(e.FullPath);

            _dispatcher.Invoke(() =>
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        Create(isFile, parent, e.FullPath);
                        break;
                    case WatcherChangeTypes.Changed:
                        if (isFile)
                        {
                            index= FindOf(parent, e.FullPath);
                            if (index == -1) return;
                            parent.Files[index] = new FileViewModel(new FileInfo(e.FullPath));
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
                        Delete(isFile, parent, child, e.FullPath);
                        break;
                }
            });
        }

        private int FindOf(IDirectoryViewModel directoryViewModel,string path)
        {
            for (int i = 0; i < directoryViewModel.Files.Count; i++)
            {
                var file = directoryViewModel.Files[i];
                if (file.Path == path)
                {
                    return i;
                }
            }
            return -1;
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            IDirectoryViewModel parent;
            IDirectoryViewModel child = _pathHelper.GetDirectory(e.FullPath, out parent);
            bool isFile;
            int index = -1;
            isFile = Path.HasExtension(e.FullPath);
            if (isFile)
            {
                if(!parent.Files.IsLoaded)  return;

            }
            else
            {
                if (!parent.SubDirectories.IsLoaded)
                {
                    parent.UpdateHasItems();
                    return;
                }
            }
            _dispatcher.Invoke(() =>
            {
                Delete(isFile, parent, child, e.OldFullPath);
                Create(isFile, parent, e.FullPath);
            });
        }

        private void Create(bool isFile,IDirectoryViewModel parent, string path)
        {
            int index  = 0;
            if (isFile)
            {
                if (parent == null) return;
                if (!parent.Files.IsLoaded) return;
                string name = Path.GetFileName(path);
                for (int i = 0; i < parent.Files.Count; i++)
                {
                    var file = parent.Files[i];
                    if (string.Compare(name, file.DisplayName, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        index = i;
                        break;
                    }
                }
                parent.Files.Insert(index, new FileViewModel(new FileInfo(path)));
            }
            else
            {
                if (parent != null)
                {
                    if (parent.SubDirectories.IsLoaded)
                    {
                        string name = Path.GetDirectoryName(path);
                        for (int i = 0; i < parent.SubDirectories.Count; i++)
                        {
                            var directory = parent.SubDirectories[i];
                            if (String.Compare(name, directory.DisplayName, StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                index = i;
                                break;
                            }
                        }
                        parent.SubDirectories.Insert(index, new DirectoryViewModel(new DirectoryInfo(path), parent));
                    }
                    else
                    {
                        parent.UpdateHasItems();
                    }
                    return;
                }
            }
        }

        private void Delete(bool isFile, IDirectoryViewModel parent, IDirectoryViewModel child, string path)
        {
            int index;
            if (isFile)
            {
                if (parent != null)
                {
                    if (parent.Files.IsLoaded)
                    {
                        index = FindOf(parent, path);
                        parent.Files.Remove(parent.Files[index]);
                    }
                }
            }
            else
            {
                if (child == null)
                {
                    if (parent != null)
                    {
                        parent.UpdateHasItems();
                        return;
                    }
                    return;
                }
                child.Parent.SubDirectories.Remove(child);
            }
        }

        #endregion
    }
}
