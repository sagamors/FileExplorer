using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Helpers;

namespace FileExplorer.ViewModels
{
    public class FileViewModel : ViewModelBase, ISystemObjectViewModel
    {
        #region private fields

        private NativeFileInfo _nativeFileInfo;

        #endregion

        #region public properties

        public DateTime? LastModificationDate
        {
            get { return FileInfo.LastWriteTime; }
        }

        public string Path { get; }
        public string VisualPath { get; }
        public ICommand OpenCommand { get; }
        public bool IsSelected { get; set; }

        public string TypeName
        {
            get
            {
                if (_nativeFileInfo == null)
                {
                    _nativeFileInfo = new NativeFileInfo(FileInfo.FullName);
                }
                return _nativeFileInfo.TypeName;
            }
        }

        public long Size
        {
            get { return FileInfo.Length; }
        }

        public FileInfo FileInfo { get; }

        public string DisplayName
        {
            get { return FileInfo.Name; }
        }

        public ImageSource Icon
        {
            get
            {
                if (_nativeFileInfo == null)
                {
                    _nativeFileInfo = new NativeFileInfo(FileInfo.FullName);
                }
                return _nativeFileInfo.Icon;
            }
        }

        #endregion

        #region constructors

        public FileViewModel(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            Path = VisualPath = fileInfo.FullName;
            OpenCommand = new RelayCommand(() => Open());
        }

        #endregion

        #region public methods

        public void Open()
        {
            Process.Start(Path);
        }

        #endregion

    }
}
