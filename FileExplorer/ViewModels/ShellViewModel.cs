/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FileExplorer.CustomCollections;
using FileExplorer.Helpers;
using PropertyChanged;

namespace FileExplorer.ViewModels
{
    [ImplementPropertyChanged]
    internal class ShellViewModel : IDirectoryViewModel
    {
        private CancellationTokenSource _getFoldersCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _getChildrenCancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Gets or set the display name for this shell item.
        /// </summary>
        public string DisplayName { get; set; } = "";
        public string TypeName { get; } = "";
        public long Size { get; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                if (_isExpanded)
                {
                    if (SubDirectories.Count == 0 || SubDirectories.Count == 1 && SubDirectories[0] == null)
                        LoadSubDirectoriesAsync(_getFoldersCancellationTokenSource.Token);
                }
                else
                {
                    _getFoldersCancellationTokenSource.Cancel();
                    _getFoldersCancellationTokenSource = new CancellationTokenSource();
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            set
            {
                _isSelected = value;
                if (_isSelected)
                {
                    if (Children.Count == 0)
                    {
                        LoadChildrenAsync(_getChildrenCancellationTokenSource.Token);
                    }
                }
                else
                {
                    _getChildrenCancellationTokenSource.Cancel();
                    _getChildrenCancellationTokenSource = new CancellationTokenSource();
                }
            }
            get { return _isSelected; }
        }

        public ObservableCollectionBase<ISystemObjectViewModel> Files { get; }
        public ObservableCollectionBase<ISystemObjectViewModel> Children { private set; get; }
        public ObservableCollectionBase<IDirectoryViewModel> SubDirectories{ private set; get; }

        private ImageSource _icon;
        public ImageSource Icon
        {
            get
            {
                if (_icon == null)
                {
                    try
                    {
                        ImageSource imageSource = null;
                        if (smallIConDictionary.TryGetValue(IconIndex, out imageSource))
                        {
                            _icon = imageSource;

                        }
                        else
                        {
                            // \todo размер исправить
                            ShellAPI.SHFILEINFO shfileinfo = new ShellAPI.SHFILEINFO();
                            ShellAPI.SHGetFileInfo(_pIDL, 0, out shfileinfo, (uint)Marshal.SizeOf(shfileinfo),
                                ShellAPI.SHGFI.SHGFI_SMALLICON | ShellAPI.SHGFI.SHGFI_SYSICONINDEX |
                                ShellAPI.SHGFI.SHGFI_PIDL |
                                ShellAPI.SHGFI.SHGFI_ICON);
                            _icon = Imaging.CreateBitmapSourceFromHIcon(shfileinfo.hIcon, new Int32Rect(0, 0, 16, 16),
                                BitmapSizeOptions.FromWidthAndHeight(16, 16));
                            smallIConDictionary.Add(IconIndex, _icon);
                            ExtractIconExample.DestroyIcon(shfileinfo.hIcon);
                            shfileinfo.hIcon = IntPtr.Zero;
                        }
                    }
                    catch (Exception ex)
                    {
                        //todo исправит
                        MessageBox.Show(ex.Message, "error");
                    }
                }
                return _icon;
            }
        }

        // todo DirectoryInfo.LastWriteTime
        public DateTime? LastModificationDate
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the system image list icon index for this shell item.
        /// </summary>
        public Int32 IconIndex { get; set; } = -1;

        private string _path;
        /// <summary>
        /// Gets or sets the system path for this shell item.
        /// </summary>
        public string Path
        {
            get { return _path ?? (_path = GetPath()); }
        }

        public Task LoadSubDirectoriesAsync(CancellationToken token)
        {
            return Task.Run(() =>
            {
                // Make sure we have a folder.
                if (IsFolder == false)
                    throw new Exception("Unable to retrieve sub-folders for a non-folder.");
                var drive = Drives.FirstOrDefault(info => info.Name == Path);
                if (!HasSubFolder || (drive != null && !drive.IsReady)) return;
                try
                {
                    // Get the IEnumIDList interface pointer.
                    ShellAPI.IEnumIDList pEnum = null;

                    uint hRes = ShellFolder.EnumObjects(IntPtr.Zero, ShellAPI.SHCONTF.SHCONTF_FOLDERS, out pEnum);
                    if (hRes != 0)
                    {
                        MessageBox.Show(DisplayName);
                        Marshal.ThrowExceptionForHR((int)hRes);
                    }

                    IntPtr pIDL = IntPtr.Zero;
                    Int32 iGot = 0;
                    // Grab the first enumeration.
                    pEnum.Next(1, out pIDL, out iGot);

                    App.Current.Dispatcher.InvokeAsync(() =>
                    {
                        SubDirectories.Clear();
                    });

                    var part = new List<IDirectoryViewModel>();

                    while (!pIDL.Equals(IntPtr.Zero) && iGot == 1)
                    {
                        // Create the new ShellViewModel object.
                        var child = new ShellViewModel(DesktopShellFolder, pIDL, this);
                        part.Add(child);

                        if (part.Count > PART_SIZE)
                        {
                            var partClone = part;
                            part = new List<IDirectoryViewModel>();
                            App.Current.Dispatcher.InvokeAsync(() =>
                            {
                                SubDirectories.AddRange(partClone);
                                Children.AddRange(partClone);
                            });
                        }

                        // Free the PIDL and reset counters.
                        Marshal.FreeCoTaskMem(pIDL);
                        pIDL = IntPtr.Zero;
                        iGot = 0;
                        // Grab the next item.
                        pEnum.Next(1, out pIDL, out iGot);
                    }

                    if (part.Count > 0)
                    {
                        try
                        {
                            App.Current.Dispatcher.InvokeAsync(() =>
                            {
                                SubDirectories.AddRange(part);
                                Children.AddRange(part);
                            });
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    // Free the interface pointer.
                    if (pEnum != null)
                        Marshal.ReleaseComObject(pEnum);
                }
                catch (Exception ex)
                {
                    // \todo
                    MessageBox.Show(ex.ToString(), "Error:");
                    throw;
                }

            }, token);
        }

        public Task LoadChildrenAsync(CancellationToken token)
        {
            return Task.Run(() =>
            {
                /*               if (SubDirectories.Count == 0 || SubDirectories.Count == 1 && SubDirectories[0] == null)
                                   LoadSubDirectoriesAsync(token).Wait();
                               App.Current.Dispatcher.InvokeAsync(() =>
                               {
                                   Children = new ObservableCollectionEx<ISystemObjectViewModel>(SubDirectories);
                               });#1#

                /*                WinApiFile.WIN32_FIND_DATA wfd = new WinApiFile.WIN32_FIND_DATA();
                                    IntPtr h = WinApiFile.FindFirstFile((Path + @"*.*"), out wfd);
                                    var part = new List<FileViewModel>();
                                while (WinApiFile.FindNextFile(h, out wfd))
                                {
                                    var child = new FileViewModel(new FileInfo(Path + wfd.cFileName));
                                    part.Add(child);

                                    if (part.Count > PART_SIZE)
                                    {
                                        var partClone = part;
                                        part = new List<FileViewModel>();
                                        App.Current.Dispatcher.InvokeAsync(() =>
                                        {
                                            Children.AddRange(partClone);
                                        });
                                    }
                                }

                                WinApiFile.FindClose(h);

                                if (part.Count > 0)
                                {
                                    App.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        Children.AddRange(part);
                                    });
                                }#1#

            }, token);
        }
        #region Private Member Variables

        private static Dictionary<int, ImageSource> smallIConDictionary = new Dictionary<int, ImageSource>();

        private const ShellAPI.SHGFI vFlags =
            ShellAPI.SHGFI.SHGFI_SMALLICON |
            ShellAPI.SHGFI.SHGFI_SYSICONINDEX |
            ShellAPI.SHGFI.SHGFI_PIDL |
            ShellAPI.SHGFI.SHGFI_TYPENAME |
            ShellAPI.SHGFI.SHGFI_DISPLAYNAME;

        private const int PART_SIZE = 1000;
        // Sets a flag specifying whether or not we've got the IShellFolder interface for the Desktop.
        private static Boolean haveRootShell = false;
        private DirectoryHelper _directoryHelper;
        private ShellAPI.SHFILEINFO shInfo;

        #endregion

        public static DriveInfo[] Drives { get; } = DriveInfo.GetDrives();

        #region Public properties

        public DirectoryInfo DirectoryInfo { get; }


        private static ShellAPI.IShellFolder _desktopShellFolder = null;
        /// <summary>
        /// Gets the IShellFolder interface of the Desktop.
        /// </summary>
        public static ShellAPI.IShellFolder DesktopShellFolder
        {
            get { return _desktopShellFolder; }
        }

        private static ShellAPI.IShellFolder _rootShellFolder = null;

        /// <summary>
        /// Gets the IShellFolder interface of the root.
        /// </summary>
        public static ShellAPI.IShellFolder RootShellFolder
        {
            get { return _rootShellFolder; }
        }

        private readonly ShellAPI.IShellFolder _shellFolder;
        /// <summary>
        /// Gets the IShellFolder interface of this shell item.
        /// </summary>
        public ShellAPI.IShellFolder ShellFolder
        {
            get { return _shellFolder; }
        }

        private IntPtr _pIDL = IntPtr.Zero;
        /// <summary>
        /// Gets the fully qualified PIDL for this shell item.
        /// </summary>
        public IntPtr PIDL
        {
            get { return _pIDL; }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether this shell item is a folder.
        /// </summary>
        public bool IsFolder { get; set; } = false;

        /// <summary>
        /// Gets or sets a boolean indicating whether this shell item has any sub-folders.
        /// </summary>
        public bool HasSubFolder { get; set; } = false;



        #endregion

        #region Constructor

        /// <summary>
        /// Constructor. Creates the ShellItem object for the Desktop.
        /// </summary>
        public ShellViewModel(ShellAPI.CSIDL type = ShellAPI.CSIDL.CSIDL_DRIVES)
        {
            // new ShellItem() can only be called once.
            if (haveRootShell)
                throw new Exception("The Desktop shell item already exists so cannot be created again.");

            // Obtain the root IShellFolder interface.
            int hRes = ShellAPI.SHGetDesktopFolder(ref _desktopShellFolder);
            if (hRes != 0)
                Marshal.ThrowExceptionForHR(hRes);

            // Now get the PIDL for the Desktop shell item.
            hRes = ShellAPI.SHGetSpecialFolderLocation(IntPtr.Zero, type, ref _pIDL);

            if (hRes != 0)
                Marshal.ThrowExceptionForHR(hRes);

            if (type != ShellAPI.CSIDL.CSIDL_DESKTOP)
            {
                hRes = (int) _desktopShellFolder.BindToObject(_pIDL, IntPtr.Zero, ref ShellAPI.IID_IShellFolder,
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
            shInfo = new ShellAPI.SHFILEINFO();
            ShellAPI.SHGetFileInfo(_pIDL, 0, out shInfo, (uint) Marshal.SizeOf(shInfo), vFlags);

            Files = new ObservableCollectionEx<ISystemObjectViewModel>();
            Children = new ObservableCollectionEx<ISystemObjectViewModel>();
            SubDirectories = new ObservableCollectionEx<IDirectoryViewModel>();

            // Set the arributes to object properties.
            DisplayName = shInfo.szDisplayName;
            IconIndex = shInfo.iIcon;
            IsFolder = true;
            HasSubFolder = true;
            IsExpanded = true;
            haveRootShell = true;
        }

        /// <summary>
        /// Constructor. Create a sub-item shell item object.
        /// </summary>
        /// <param name="shDesktop">IShellFolder interface of the Desktop</param>
        /// <param name="pIDL">The fully qualified PIDL for this shell item</param>
        /// <param name="shParent">The ShellItem object for this item's parent</param>
        public ShellViewModel(ShellAPI.IShellFolder shDesktop, IntPtr pIDL, ShellViewModel shParent)
        {
            // We need the Desktop shell item to exist first.
            if (haveRootShell == false)
                throw new Exception("The root shell item must be created before creating a sub-item");

            // Create the FQ PIDL for this new item.
            _pIDL = ShellAPI.ILCombine(shParent.PIDL, pIDL);

            // Get the properties of this item.
            ShellAPI.SFGAOF uFlags = ShellAPI.SFGAOF.SFGAO_FOLDER | ShellAPI.SFGAOF.SFGAO_HASSUBFOLDER;

            // Here we get some basic attributes.
            shDesktop.GetAttributesOf(1, out _pIDL, out uFlags);
            IsFolder = Convert.ToBoolean(uFlags & ShellAPI.SFGAOF.SFGAO_FOLDER);
            // bug not working
            HasSubFolder = Convert.ToBoolean(uFlags & ShellAPI.SFGAOF.SFGAO_HASSUBFOLDER);

            // Now we want to get extended attributes such as the icon index etc.
            shInfo = new ShellAPI.SHFILEINFO();
            ShellAPI.SHGetFileInfo(_pIDL, 0, out shInfo, (uint) Marshal.SizeOf(shInfo), vFlags);
            DisplayName = shInfo.szDisplayName;
            TypeName = shInfo.szTypeName;
            IconIndex = shInfo.iIcon;

            // Create the IShellFolder interface for this item.
            if (IsFolder)
            {
               // DirectoryInfo = new DirectoryInfo(Path);
                uint hRes = shParent._shellFolder.BindToObject(pIDL, IntPtr.Zero, ref ShellAPI.IID_IShellFolder,
                    out _shellFolder);
                if (hRes != 0)
                    Marshal.ThrowExceptionForHR((int) hRes);
                _directoryHelper = new DirectoryHelper(ShellFolder,PIDL);
                // \todo реализовать нормально
                try
                {
                    HasSubFolder = _directoryHelper.HaveSubFolder();
                }
                catch (Exception ex)
                {
                   /* MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);#1#
                }
                if (HasSubFolder)
                    SubDirectories.Add(null);
            }
        }

        #endregion Constructor

        #region Destructor

        ~ShellViewModel()
        {
            // Release the IShellFolder interface of this shell item.
            if (_shellFolder != null)
                Marshal.ReleaseComObject(_shellFolder);

            // Free the PIDL too.
            if (!_pIDL.Equals(IntPtr.Zero))
                Marshal.FreeCoTaskMem(_pIDL);

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the system path for this shell item.
        /// </summary>
        /// <returns>A path string.</returns>
        public string GetPath()
        {
            StringBuilder strBuffer = new StringBuilder(256);
            ShellAPI.SHGetPathFromIDList(_pIDL,strBuffer);
            return strBuffer.ToString();
        }



        #endregion Public Methods
    }
}

*/
