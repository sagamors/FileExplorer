using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;
using FileExplorer.CustomCollections;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.ViewModels;

namespace FileExplorer.Helpers
{
    public class DirectoryWatcher
    {
        public TopViewModel Top { get; set; }
        private readonly PathHelper _pathHelper;
        private readonly Dispatcher _dispatcher;
       // private Dictionary<string,FileSystemWatcher> _directoriesFileSystemWatchers = new Dictionary<string, FileSystemWatcher>();

        public DirectoryWatcher(TopViewModel top,IDirectoryViewModel root, PathHelper pathHelper, Dispatcher dispatcher)
        {
            Top = top;
            _pathHelper = pathHelper;
            _dispatcher = dispatcher;
            root.SubDirectories.CollectionLoaded += SubDirectories_CollectionLoaded;
        }

        #region private methods

        private void SubDirectories_CollectionLoaded(object sender, EventArgs e)
        {
            Watch((AsyncLoadCollection<IDirectoryViewModel>) sender);
        }

        private void Watch(AsyncLoadCollection<IDirectoryViewModel> subDirectories )
        {
            foreach (var directory in subDirectories)
            {
                try
                {
                    if(directory.NoAccess) continue;
                    AddDirectoryToWatch(directory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("no " + directory.Path);
                }
            }
        }

        private void AddDirectoryToWatch(IDirectoryViewModel directory)
        {
            var drive = DriveInfoEx.IsDrive(directory.Path);

            if (drive != null && !drive.IsReady) return;
            /* if (!drive.IsReady) return;*/
            FileSystemWatcher directoryFileSystemWatcher = new FileSystemWatcher();
            directoryFileSystemWatcher.Path = directory.Path;
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

            directory.FileSystemWatcher = directoryFileSystemWatcher;
            directory.SubDirectories.CollectionLoaded += SubDirectories_CollectionLoaded;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            try
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
                                index = FindFileIndex(parent, e.FullPath);
                                if (index == -1) return;
                                parent.Files[index].UpdateParameters();
                            }
                            else
                            {
                                if (child != null)
                                    child.UpdateParameters();
                            }
                            break;

                        case WatcherChangeTypes.Deleted:
                            Delete(isFile, parent, child, e.FullPath);
                            break;
                    }
                });
            }
            catch (Exception) { }
            
        }

        private int FindFileIndex(IDirectoryViewModel directoryViewModel, string path)
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
                if (!parent.Files.IsLoaded) return;
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

        private void Create(bool isFile, IDirectoryViewModel parent, string path)
        {
            int index = 0;
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
                parent.Files.Insert(index, new FileViewModel(new FileInfo(path), parent));
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
                        var newDirectory = new DirectoryViewModel(new DirectoryInfo(path), parent);
                        parent.SubDirectories.Insert(index, newDirectory);
                        AddDirectoryToWatch(newDirectory);
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
                        index = FindFileIndex(parent, path);
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

                string selectedPathDirectory = PathHelper.NormalizePath(Top.SelectedDirectory.Path);
                string deletedPath = PathHelper.NormalizePath(path);

                if (PathHelper.Contains(selectedPathDirectory, deletedPath))
                {
                    Top.SelectedDirectory = child.Parent;
                }

                child.Parent.SubDirectories.Remove(child);
                DeleteFileSystemWatcher(child);
            }
        }

        public static void DeleteFileSystemWatcher(IDirectoryViewModel directoryViewModel)
        {
            if (directoryViewModel.FileSystemWatcher != null)
            {
                directoryViewModel.FileSystemWatcher.EnableRaisingEvents = false;
                directoryViewModel.FileSystemWatcher.Dispose();
            }
            if (directoryViewModel.SubDirectories.Count != 0)
            {
                foreach (var directory in directoryViewModel.SubDirectories)
                {
                    DeleteFileSystemWatcher(directory);
                }
            }
        }

        #endregion
    }
}
