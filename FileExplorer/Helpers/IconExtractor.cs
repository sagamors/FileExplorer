using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text;
namespace FileExplorer.Helpers
{
    public static class IconExtractor
    {
        private const WinAPI.SHGFI commonFlags = WinAPI.SHGFI.SHGFI_ICON;

        /// <summary>
        /// Options to specify the size of icons to return.
        /// </summary>
        public enum IconSize
        {
            /// <summary>
            /// Specify large icon - 32 pixels by 32 pixels.
            /// </summary>
            Large = 0,

            /// <summary>
            /// Specify small icon - 16 pixels by 16 pixels.
            /// </summary>
            Small = 1
        }

        public static Dictionary<int, ImageSource> IconsDictionary { get; } = new Dictionary<int, ImageSource>();

        public static ImageSource GetIcon(string path, int iconIndex, IconSize size = IconSize.Small)
        {
            try
            {
                ImageSource imageSource = null;
                if (IconsDictionary.TryGetValue(iconIndex, out imageSource))
                {
                    return imageSource;
                }
                Int32Rect sizeRect;
                WinAPI.SHGFI flags;
                if (IconSize.Small == size)
                {
                    flags = commonFlags | WinAPI.SHGFI.SHGFI_SMALLICON;
                    sizeRect = new Int32Rect(0, 0, 16, 16);
                }
                else
                {
                    flags = commonFlags | WinAPI.SHGFI.SHGFI_LARGEICON;
                    sizeRect = new Int32Rect(0, 0, 32, 32);
                }
                WinAPI.SHFILEINFO shfileinfo = new WinAPI.SHFILEINFO();
                WinAPI.SHGetFileInfo(path, 256, out shfileinfo, (uint) Marshal.SizeOf(shfileinfo), flags);
                if (shfileinfo.hIcon == IntPtr.Zero)
                {
                    //todo
                    Console.WriteLine("icon:"+iconIndex);
                    return null;
                    return GetIcon(path);
                }
                imageSource = Imaging.CreateBitmapSourceFromHIcon(shfileinfo.hIcon, sizeRect,
                    BitmapSizeOptions.FromEmptyOptions());
                IconsDictionary.Add(iconIndex, imageSource);
                WinAPI.DestroyIcon(shfileinfo.hIcon);
                shfileinfo.hIcon = IntPtr.Zero;

                return imageSource;
            }
            catch (Exception ex)
            {
                //todo
                Console.WriteLine("2" + ex);
                return null;
            }
        }

        public static ImageSource GetIcon(IntPtr pIDL, int iconIndex, IconSize size = IconSize.Small)
        {
            ImageSource imageSource = null;
            if (IconsDictionary.TryGetValue(iconIndex, out imageSource))
            {
                return imageSource;
            }
            Int32Rect sizeRect;
            WinAPI.SHGFI flags;
            if (IconSize.Small == size)
            {
                flags = commonFlags | WinAPI.SHGFI.SHGFI_SMALLICON;
                sizeRect = new Int32Rect(0, 0, 16, 16);
            }
            else
            {
                flags = commonFlags | WinAPI.SHGFI.SHGFI_LARGEICON;
                sizeRect = new Int32Rect(0, 0, 32, 32);
            }

            WinAPI.SHFILEINFO shfileinfo = new WinAPI.SHFILEINFO();
            WinAPI.SHGetFileInfo(pIDL, 256, out shfileinfo, (uint)Marshal.SizeOf(shfileinfo), WinAPI.SHGFI.SHGFI_SYSICONINDEX | WinAPI.SHGFI.SHGFI_PIDL| flags);
            imageSource = Imaging.CreateBitmapSourceFromHIcon(shfileinfo.hIcon, sizeRect, BitmapSizeOptions.FromEmptyOptions());
            IconsDictionary.Add(iconIndex, imageSource);
            WinAPI.DestroyIcon(shfileinfo.hIcon);
            shfileinfo.hIcon = IntPtr.Zero;
            return imageSource;
        }

        private static ImageSource GetIcon(string fileName)
        {
            Icon icon = Icon.ExtractAssociatedIcon(fileName);
            return Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        new Int32Rect(0,0,icon.Width, icon.Height),
                        BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
