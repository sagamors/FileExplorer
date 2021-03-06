﻿using System;
using System.IO;
using System.Linq;
using FileExplorer.CustomCollections;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Helpers;
using FileExplorer.Providers;

namespace FileExplorer.ViewModels
{
    public class DirectoryViewModel : DirectoryViewModelBase
    {
        #region private fields

        private NativeFileInfo _nativeDirectoryInfo;
        private DirectoryInfo _directoryInfo;

        #endregion

        #region constructor

        public DirectoryViewModel(DirectoryInfo directoryInfo, IDirectoryViewModel parent)
            : base(new NativeFileInfo(directoryInfo.FullName,true), parent)
        {
            _directoryInfo = directoryInfo;
            var directoryProvider = new SubDirectoriesProvider(_directoryInfo, this);
            Path = Environment.ExpandEnvironmentVariables(_directoryInfo.FullName);
            DisplayName = _directoryInfo.Name;
            if (Parent != null)
                VisualPath = Parent.VisualPath + "\\" + DisplayName;
            Files = new AsyncLoadCollection<ISystemObjectViewModel>(new FilesProvider(directoryInfo,this));
            SubDirectories = new AsyncLoadCollection<IDirectoryViewModel>(directoryProvider);
            Children = new UnionCollectionEx<IDirectoryViewModel, ISystemObjectViewModel, ISystemObjectViewModel>(SubDirectories, Files);
            LastModificationDate = _directoryInfo.LastWriteTime;
            try
            {
                UpdateHasItems();
            }
            catch (UnauthorizedAccessException ex)
            {
                NoAccess = true;
            }
        }

        #endregion

        #region public methods

        public override void UpdateHasItems()
        {
           var directory= _directoryInfo.EnumerateDirectories()
                .FirstOrDefault(info => ((info.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden));
            HasItems = directory!=null;
        }

        public override sealed void UpdateParameters()
        {
            NativeSystemInfo = new NativeFileInfo(Path);
            Icon =  IconExtractor.GetDirectoryIcon(Path, NativeSystemInfo.IconIndex);
            _directoryInfo = new DirectoryInfo(Path);
            LastModificationDate = _directoryInfo.LastWriteTime;
        }

        #endregion

    }
}
