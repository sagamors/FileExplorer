using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FileExplorer.Helpers
{
    public class FileToImageIconConverter
    {
        private string filePath;
        private ImageSource icon;
 
        public string FilePath { get { return filePath; } }

        public ImageSource Icon
        {
            get
            {
                if (icon == null && (System.IO.File.Exists(FilePath) || Directory.Exists(FilePath)))
                {
                    using (System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(FilePath))
                    {
                        if (sysicon != null)
                        {
                            icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                sysicon.Handle,
                                System.Windows.Int32Rect.Empty,
                                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                        }
                    }
                }
                return icon;
            }
        }

        public FileToImageIconConverter(string filePath)
        {
            this.filePath = filePath;
        }
    } 
}
