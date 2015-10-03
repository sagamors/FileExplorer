using System;

namespace FileExplorer.Exceptions
{
    class DirectoryDoesExistException : Exception
    {
        public static string Msg = "Directory does exist";
        public DirectoryDoesExistException() : base(Msg)
        {
        }
    }
}
