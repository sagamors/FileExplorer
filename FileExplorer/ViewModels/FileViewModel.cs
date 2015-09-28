using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Helpers;

namespace FileExplorer.ViewModels
{
    public class FileViewModel : ViewModelBase, IFileViewModel
    {
        #region private fields

        private NativeFileInfo _nativeFileInfo;

        #endregion

        #region public properties

        private DateTime? _lastModificationDate;
        private ImageSource _icon;
  

        public DateTime? LastModificationDate
        {
            get { return _lastModificationDate??(_lastModificationDate =FileInfo.LastWriteTime); }
            private set { _lastModificationDate = value; }
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

        private long? _size;
        public long Size
        {
            private set { _size = value; }
            get { return _size ?? (_size = FileInfo.Length).Value; }
        }

        public FileInfo FileInfo { get; }

        public string DisplayName
        {
            get { return FileInfo.Name; }
        }

        public ImageSource Icon
        {
            private set { _icon = value; }
            get { return _icon?? _nativeFileInfo.Icon; }
        }

        #endregion

        #region constructors

        public FileViewModel(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            Path = VisualPath = fileInfo.FullName;
            OpenCommand = new RelayCommand(() => Open());
            _nativeFileInfo = new NativeFileInfo(FileInfo.FullName);
        }

        #endregion

        #region public methods

        public void Open()
        {
            Process.Start(Path);
        }

        public void UpdateParameters()
        {
            _nativeFileInfo = new NativeFileInfo(FileInfo.FullName);
            Icon = _nativeFileInfo.Icon;
            Size = FileInfo.Length;
            LastModificationDate = FileInfo.LastWriteTime;
        }

        #endregion

    }
}
