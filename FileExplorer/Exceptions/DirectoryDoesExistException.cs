using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
