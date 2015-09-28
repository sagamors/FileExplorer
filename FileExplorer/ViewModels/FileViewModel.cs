using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Exceptions;
using FileExplorer.Helpers;
using FileExplorer.Services;

namespace FileExplorer.ViewModels
{
    public class FileViewModel : ViewModelBase, ISystemObjectViewModel
    {
        #region private fields

        private NativeFileInfo _nativeFileInfo;

        #endregion

        #region public properties

        private DateTime? _lastModificationDate;
        private ImageSource _icon;
  

        public DateTime? LastModificationDate
        {
            get { return _lastModificationDate??(_lastModificationDate =Info.LastWriteTime); }
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
                    _nativeFileInfo = new NativeFileInfo(Info.FullName);
                }
                return _nativeFileInfo.TypeName;
            }
        }

        private long? _size;
        public long Size
        {
            private set { _size = value; }
            get { return _size ?? (_size = Info.Length).Value; }
        }

        public FileInfo Info { get; }

        public string DisplayName
        {
            get { return Info.Name; }
        }

        public ImageSource Icon
        {
            private set { _icon = value; }
            get { return _icon?? _nativeFileInfo.Icon; }
        }

        #endregion

        #region constructors

        public FileViewModel(FileInfo info)
        {
            Info = info;
            Path = VisualPath = info.FullName;
            OpenCommand = new RelayCommand(() => Open());
            _nativeFileInfo = new NativeFileInfo(Info.FullName);
        }

        #endregion

        #region public methods

        public void Open()
        {
            if (!File.Exists(Path))
            {
                MessageBoxService.Instance.ShowError(FileDoesExistException.Msg);
            }
            Process.Start(Path);
        }

        public void UpdateParameters()
        {
            _nativeFileInfo = new NativeFileInfo(Info.FullName);
            Icon = _nativeFileInfo.Icon;
            Size = Info.Length;
            LastModificationDate = Info.LastWriteTime;
        }

        #endregion

    }
}
