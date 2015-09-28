using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media;
using FileExplorer.Helpers;

namespace FileExplorer.DirectoriesHelpers
{
    public class NativeFileInfo : INativeSystemInfo
    {
        #region private properties

        private WinAPI.SHFILEINFO shInfo;
        private const WinAPI.SHGFI vFlags =
            WinAPI.SHGFI.SHGFI_SMALLICON |
            WinAPI.SHGFI.SHGFI_SYSICONINDEX |
            WinAPI.SHGFI.SHGFI_TYPENAME |
            WinAPI.SHGFI.SHGFI_DISPLAYNAME |
            WinAPI.SHGFI.SHGFI_USEFILEATTRIBUTES;

        private const  WinAPI.SHGFI imageFlags = WinAPI.SHGFI.SHGFI_ICON | WinAPI.SHGFI.SHGFI_SMALLICON;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or set the display name for this shell item.
        /// </summary>
        public string DisplayName
        {
            get { return shInfo.szDisplayName; }
        }

        public string TypeName
        {
            get { return shInfo.szTypeName; }
        }

        public int IconIndex { get { return shInfo.iIcon; } }
        public string Path { private set; get; }

        private ImageSource _icon;
        public ImageSource Icon
        {
            get
            {
                if (_icon == null)
                {
                    _icon = IconExtractor.GetIcon(Path, IconIndex);
                }
                return _icon;
            }
        }

        #endregion Public properties

        #region constructors

        public NativeFileInfo(string path)
        {
            Path = path;
            shInfo = new WinAPI.SHFILEINFO();
            WinAPI.SHGetFileInfo(path, WinAPI.FILE_ATTRIBUTE_NORMAL, out shInfo, (uint) Marshal.SizeOf(shInfo), vFlags);
        }

        #endregion

    }
}
