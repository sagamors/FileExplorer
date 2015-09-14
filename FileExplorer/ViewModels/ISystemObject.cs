using System;
using System.Windows.Media;
using PropertyChanged;

namespace FileExplorer.ViewModels
{

    public interface ISystemObjectViewModel
    {
        string DisplayName { get; }
        ImageSource Icon { get; }
        long Size { get; }
        DateTime? LastModificationDate { get; }
    }
}
