using System;

namespace FileExplorer.Exceptions
{
    class AccessDirectoryDeniedException : Exception
    {
        public static string Msg = "Access to the directory is denied";

        public AccessDirectoryDeniedException() : base(Msg)
        {
        }
    }
}
