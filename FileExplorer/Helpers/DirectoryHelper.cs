using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.Helpers
{
    class DirectoryHelper
    {
        public static DriveInfo[] Drives { get; } = DriveInfo.GetDrives();

        private IntPtr _pIDL = IntPtr.Zero;
        /// <summary>
        /// Gets the fully qualified PIDL for this shell item.
        /// </summary>
        public IntPtr PIDL
        {
            get { return _pIDL; }
        }

        public ShellAPI.IShellFolder ShellFolder { private set; get; }

        public DirectoryHelper(ShellAPI.IShellFolder shellFolder, IntPtr pidl)
        {
            ShellFolder = shellFolder;
            _pIDL = pidl;
        }

        /// <summary>
        /// Gets the system path for this shell item.
        /// </summary>
        /// <returns>A path string.</returns>
        public string GetPath()
        {
            StringBuilder strBuffer = new StringBuilder(256);
            ShellAPI.SHGetPathFromIDList(_pIDL, strBuffer);
            return strBuffer.ToString();
        }

        public bool HaveSubFolder()
        {
            var drive = Drives.FirstOrDefault(info => info.Name == GetPath());
            if (drive != null && !drive.IsReady) return false;
            // Get the IEnumIDList interface pointer.
            ShellAPI.IEnumIDList pEnum = null;
            try
            {
                uint hRes = ShellFolder.EnumObjects(IntPtr.Zero, ShellAPI.SHCONTF.SHCONTF_FOLDERS, out pEnum);
                if (hRes != 0)
                {
                    Marshal.ThrowExceptionForHR((int) hRes);
                }
            }
                        
            catch (Exception ex)
            {
                // \todo

            }
            IntPtr pIDL = IntPtr.Zero;
            Int32 iGot = 0;
            // Grab the first enumeration.
            pEnum.Next(1, out pIDL, out iGot);
            return !pIDL.Equals(IntPtr.Zero) && iGot == 1;
        }
    }
}
