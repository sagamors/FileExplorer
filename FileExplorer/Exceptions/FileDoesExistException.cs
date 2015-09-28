using System;

namespace FileExplorer.Exceptions
{
    class FileDoesExistException : Exception
    {
        public static string Msg = "File does exist";
        public FileDoesExistException() : base(Msg)
        {
        }
    }
}