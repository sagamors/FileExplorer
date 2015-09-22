using System.IO;
using System.Linq;

namespace FileExplorer.Helpers
{
    public static class DriveInfoEx
    {
        public static DriveInfo IsDrive(string path)
        {
            return DriveInfo.GetDrives().FirstOrDefault(info => info.Name == path);
        }
    }
}
