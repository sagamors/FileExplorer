using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media;
using FileExplorer.Helpers;

namespace FileExplorer.DirectoriesHelpers
{
    internal class NativeDirectoryInfo : INativeSystemInfo
    {
        #region private fields

        private static object _locker = new object();
        private static DriveInfo[] Drives { get; } = DriveInfo.GetDrives();
        private IntPtr pIDL;
        private Int32 iGot;
        private WinAPI.IEnumIDList pEnum = null;
        private bool? _hasSubFolder;
        private static Boolean haveRootShell = false;
        private WinAPI.SHFILEINFO shInfo;

        private const WinAPI.SHGFI vFlags =
            WinAPI.SHGFI.SHGFI_SMALLICON |
            WinAPI.SHGFI.SHGFI_SYSICONINDEX |
            WinAPI.SHGFI.SHGFI_PIDL |
            WinAPI.SHGFI.SHGFI_TYPENAME |
            WinAPI.SHGFI.SHGFI_DISPLAYNAME;

        #endregion

        #region Public static properties

        private static WinAPI.IShellFolder _desktopShellFolder = null;

        /// <summary>
        /// Gets the IShellFolder interface of the Desktop.
        /// </summary>
        public static WinAPI.IShellFolder DesktopShellFolder
        {
            get { return _desktopShellFolder; }
        }

        private static WinAPI.IShellFolder _rootShellFolder = null;

        /// <summary>
        /// Gets the IShellFolder interface of the root.
        /// </summary>
        public static WinAPI.IShellFolder RootShellFolder
        {
            get { return _rootShellFolder; }
        }

        #endregion Public properties

        #region Public properties

        /// <summary>
        /// Gets or set the display name for this shell item.
        /// </summary>
        public string DisplayName { get; set; } = "";

        public string TypeName { get; } = "";
        public bool IsFolder { get; set; }

        public bool HasSubFolder
        {
            get
            {
                if (_hasSubFolder.HasValue)
                    return _hasSubFolder.Value;
                GetFirts();
                return !pIDL.Equals(IntPtr.Zero) && iGot == 1;
            }
            private set { _hasSubFolder = value; }
        }

        public int IconIndex { get; set; }

        private string _path;

        public string Path
        {
            get
            {
                if (_path == null)
                {
                    StringBuilder strBuffer = new StringBuilder(256);
                    WinAPI.SHGetPathFromIDList(_pIDL, strBuffer);
                    _path = strBuffer.ToString();
                }
                return _path;
            }
        }

        private ImageSource _icon;

        public ImageSource Icon
        {
            get
            {
                if (_icon == null)
                {
                    _icon = IconExtractor.GetIcon(_pIDL, IconIndex);
                }
                return _icon;
            }
        }

        private IntPtr _pIDL = IntPtr.Zero;
        /// <summary>
        /// Gets the fully qualified PIDL for this shell item.
        /// </summary>
        public IntPtr PIDL
        {
            get { return _pIDL; }
        }

        private readonly WinAPI.IShellFolder _shellFolder;

        /// <summary>
        /// Gets the IShellFolder interface of this shell item.
        /// </summary>
        public WinAPI.IShellFolder ShellFolder
        {
            get { return _shellFolder; }
        }

        #endregion Public properties

        #region constructors

        public NativeDirectoryInfo(WinAPI.CSIDL type = WinAPI.CSIDL.CSIDL_DRIVES)
        {
            // new ShellItem() can only be called once.
            if (haveRootShell)
                throw new Exception("The Desktop shell item already exists so cannot be created again.");

            // Obtain the root IShellFolder interface.
            int hRes = WinAPI.SHGetDesktopFolder(ref _desktopShellFolder);
            if (hRes != 0)
                Marshal.ThrowExceptionForHR(hRes);

            // Now get the PIDL for the Desktop shell item.
            hRes = WinAPI.SHGetSpecialFolderLocation(IntPtr.Zero, type, ref _pIDL);

            if (hRes != 0)
                Marshal.ThrowExceptionForHR(hRes);

            if (type != WinAPI.CSIDL.CSIDL_DESKTOP)
            {
                hRes = (int) _desktopShellFolder.BindToObject(_pIDL, IntPtr.Zero, ref WinAPI.IID_IShellFolder,
                    out _shellFolder);
                _rootShellFolder = ShellFolder;
            }
            else
            {
                _rootShellFolder = DesktopShellFolder;
                _shellFolder = DesktopShellFolder;
            }

            if (hRes != 0)
                Marshal.ThrowExceptionForHR(hRes);

            // Now retrieve some attributes for the root shell item.
            shInfo = new WinAPI.SHFILEINFO();
            WinAPI.SHGetFileInfo(_pIDL, 0, out shInfo, (uint) Marshal.SizeOf(shInfo), vFlags);

            // Set the arributes to object properties.
            DisplayName = shInfo.szDisplayName;
            IconIndex = shInfo.iIcon;
            IsFolder = true;
            haveRootShell = true;
        }

        public NativeDirectoryInfo(IntPtr pIDL, NativeDirectoryInfo shParent)
        {
            // We need the Desktop shell item to exist first.
            if (haveRootShell == false)
                throw new Exception("The root shell item must be created before creating a sub-item");

            // Create the FQ PIDL for this new item.
            _pIDL = WinAPI.ILCombine(shParent.PIDL, pIDL);

            // Get the properties of this item.
            WinAPI.SFGAOF uFlags = WinAPI.SFGAOF.SFGAO_FOLDER | WinAPI.SFGAOF.SFGAO_HASSUBFOLDER;

            // Here we get some basic attributes.
            DesktopShellFolder.GetAttributesOf(1, out _pIDL, out uFlags);
            IsFolder = Convert.ToBoolean(uFlags & WinAPI.SFGAOF.SFGAO_FOLDER);
            // bug not working
            HasSubFolder = Convert.ToBoolean(uFlags & WinAPI.SFGAOF.SFGAO_HASSUBFOLDER);
            // Now we want to get extended attributes such as the icon index etc.
            shInfo = new WinAPI.SHFILEINFO();
            WinAPI.SHGetFileInfo(_pIDL, 0, out shInfo, (uint) Marshal.SizeOf(shInfo), vFlags);
            DisplayName = shInfo.szDisplayName;
            TypeName = shInfo.szTypeName;
            IconIndex = shInfo.iIcon;

            // Create the IShellFolder interface for this item.
            if (IsFolder)
            {
                uint hRes = shParent._shellFolder.BindToObject(pIDL, IntPtr.Zero, ref WinAPI.IID_IShellFolder,
                    out _shellFolder);
                if (hRes != 0)
                    Marshal.ThrowExceptionForHR((int) hRes);
            }
        }

        #endregion

        #region public methods

        public bool IsReady()
        {
            var drive = Drives.FirstOrDefault(info => info.Name == Path);
            return ((drive != null && !drive.IsReady));
        }

        public List<NativeDirectoryInfo> GetDirectories()
        {
            lock (_locker)
            {
                var directories = new List<NativeDirectoryInfo>();

                // Make sure we have a folder.
                if (IsFolder == false)
                    throw new Exception("Unable to retrieve sub-folders for a non-folder.");
                if (!HasSubFolder || IsReady()) return directories;
                // Get the IEnumIDList interface pointer.

                while (!pIDL.Equals(IntPtr.Zero) && iGot == 1)
                {
                    // Create the new ShellViewModel object.
                    var child = new NativeDirectoryInfo(pIDL, this);
                    directories.Add(child);
                    // Free the PIDL and reset counters.
                    Marshal.FreeCoTaskMem(pIDL);
                    pIDL = IntPtr.Zero;
                    iGot = 0;
                    // Grab the next item.
                    pEnum.Next(1, out pIDL, out iGot);
                }
                // Free the interface pointer.
                if (pEnum != null)
                    Marshal.ReleaseComObject(pEnum);
                return directories;
            }
        }

        #endregion

        #region private methods

        private void GetFirts()
        {
            try
            {
                uint hRes = ShellFolder.EnumObjects(IntPtr.Zero, WinAPI.SHCONTF.SHCONTF_FOLDERS, out pEnum);

                if (hRes != 0)
                {
                    Marshal.ThrowExceptionForHR((int) hRes);
                }

                pIDL = IntPtr.Zero;
                iGot = 0;
                // Grab the first enumeration.
                pEnum.Next(1, out pIDL, out iGot);
            }
            catch (Exception ex)
            {
                _hasSubFolder = false;
            }
        }

        #endregion

    }
}

