using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileExplorer.Exceptions;
using FileExplorer.Helpers;
using FileExplorer.ViewModels;

namespace FileExplorer.DirectoriesHelpers
{
    public class PathHelper
    {
        public IDirectoryViewModel Root { private get; set; }

        public PathHelper(IDirectoryViewModel root)
        {
            Root = root;
        }

        public Task<IDirectoryViewModel> GetAndLoadDirectory(string path, CancellationToken token)
        {
            return Task.Run(() =>
            {
                IDirectoryViewModel child = null;
                IDirectoryViewModel findedParent = null;
                child = Root;

                //check first 
                string trimedPath = Trim(path);
                bool isVisualPath = Contains(trimedPath, Trim(Root.VisualPath));
                while (Trim(child.VisualPath) != trimedPath)
                {
                    if (!child.SubDirectories.IsLoaded)
                    {
                        child.SubDirectories.LoadAsync(token).Wait(token);
                    }
                    findedParent = child;
                    child = findedParent.SubDirectories.FirstOrDefault(model => Contains(trimedPath, Trim(model.VisualPath)));
                    if (child == null) break;
        /*            child.IsExpanded = true;*/
                }

                if (child != null) return child;

                if (isVisualPath || !Directory.Exists(trimedPath))
                {
                    throw new DirectoryDoesExistException();
                }

                child = Root;

                while (NormalizePath(child.Path) != NormalizePath(path))
                {
                    if (!child.SubDirectories.IsLoaded)
                    {
                        child.SubDirectories.LoadAsync(token).Wait(token);
                    }
                    findedParent = child;
                    child = findedParent.SubDirectories.First(model => Contains(NormalizePath(path), NormalizePath(model.Path)));
                }
                return child;
            }, token);
        }

        public IDirectoryViewModel GetDirectory(string path, out IDirectoryViewModel parent)
        {
            IDirectoryViewModel child = null;
            IDirectoryViewModel findedParent = null;
            findedParent = Root;
            child = findedParent;
            parent = null;
            do
            {
                findedParent = child;
                child = findedParent.SubDirectories.FirstOrDefault(model => Contains(NormalizePath(path), NormalizePath(model.Path)));
                if (child == null)
                {
                    if (NormalizePath(findedParent.Path) == NormalizePath(Directory.GetParent(path).FullName))
                    {
                        parent = findedParent;
                    }
                    return null;
                }
            } while (NormalizePath(child.Path) != NormalizePath(path));
            if (child != null)
            {
                parent = child.Parent;
            }
            return child;
        }

        public static string NormalizePath(string path)
        {
            if (path == "") return "";
            return Trim(Path.GetFullPath(new Uri(path).LocalPath));
        }

        private static string Trim(string path)
        {
            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .ToUpperInvariant();
        }

        public static bool Contains(string source, string path)
        {
            var splitSource = source.Split(Path.DirectorySeparatorChar);
            var splitPath = path.Split(Path.DirectorySeparatorChar);
            if (splitSource.Length < splitPath.Length) return false;
            for (int i = 0; i < splitPath.Length; i++)
            {
                if (splitSource[i] != splitPath[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static IList<string> GetTopDirectories(string path)
        {
            var splitPath = NormalizePath(path).Split(Path.DirectorySeparatorChar);
            List<string> topList = new List<string>();
            topList.Add(splitPath[0] + Path.DirectorySeparatorChar);
            for (int i = 1; i < splitPath.Length; i++)
            {
                topList.Add(topList.Last() + Path.DirectorySeparatorChar + splitPath[i]);
            }
            return topList;
        }


        public static IDirectoryViewModel ClearNotExistDirectories(IDirectoryViewModel directory)
        {
            IDirectoryViewModel noExistDirectory = null;
            IDirectoryViewModel currentDirectory = directory;
            while (currentDirectory.Parent!=null)
            {
 
                if (currentDirectory.Path == "" || Directory.Exists(currentDirectory.Path))
                {
                    if (noExistDirectory != null)
                    {
                        currentDirectory.SubDirectories.Remove(noExistDirectory);
                    }
                    return currentDirectory;
                }
                DirectoryWatcher.DeleteFileSystemWatcher(currentDirectory);
                currentDirectory.SubDirectories.Clear();
                currentDirectory.Files.Clear();
                noExistDirectory = currentDirectory;
                currentDirectory = currentDirectory.Parent;
            }
            // only this pc
            if (currentDirectory.Path == "")
            {
                if (noExistDirectory != null)
                {
                    currentDirectory.SubDirectories.Remove(noExistDirectory);
                }
                return currentDirectory;
            }
        
            return null;
        }
    }
}
