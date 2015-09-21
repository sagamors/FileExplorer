﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileExplorer.ViewModels;

namespace FileExplorer.DirectoriesHelpers
{
    public class PathHelper
    {
        private readonly IDirectoryViewModel _root;

        public PathHelper(IDirectoryViewModel root)
        {
            _root = root;
        }

        public Task<IDirectoryViewModel> GetAndLoadDirectory(string path, CancellationToken token)
        {
            return Task.Run(() =>
            {
                IDirectoryViewModel child = null;
                IDirectoryViewModel findedParent = null;
                child = _root;

                //check first 
                string trimedPath = Trim(path);
                if (Trim(_root.VisualPath) == trimedPath) return _root;

                do
                {
                    findedParent = child;
                    child = findedParent.SubDirectories.FirstOrDefault(model => Contains(trimedPath, Trim(model.VisualPath)));
                    if (child == null) break;
                    if (!findedParent.SubDirectories.IsLoaded)
                    {
                        findedParent.SubDirectories.LoadAsync(token).Wait(token);
                    }
                    child.IsExpanded = true;
                } while (Trim(child.VisualPath) != trimedPath);

                if (child != null) return child;

                if (!Directory.Exists(trimedPath))
                {
                    throw new Exception("Directory does exist");
                }

                child = _root;

                while (NormalizePath(child.Path) != NormalizePath(path))
                {
                    if (!child.SubDirectories.IsLoaded)
                    {
                        child.SubDirectories.LoadAsync(token).Wait(token);
                    }
                    findedParent = child;
                    child = findedParent.SubDirectories.First(model => Contains(NormalizePath(path), NormalizePath(model.Path)));
                    child.IsExpanded = true;
                }
                return child;
            }, token);
        }

        public IDirectoryViewModel GetDirectory(string path)
        {
            IDirectoryViewModel child = null;
            IDirectoryViewModel findedParent = null;
            findedParent = _root;
            child = findedParent;
            do
            {
                findedParent = child;
                child = findedParent.SubDirectories.First(model => Contains(NormalizePath(path), NormalizePath(model.Path)));
                child.IsExpanded = true;
            } while (NormalizePath(child.Path) != NormalizePath(path));
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

        public bool Contains(string source, string path)
        {
            var splitSource = source.Split(Path.DirectorySeparatorChar);
            var splitPath = path.Split(Path.DirectorySeparatorChar);
            for (int i = 0; i < splitPath.Length; i++)
            {
                if (splitSource[i] != splitPath[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
