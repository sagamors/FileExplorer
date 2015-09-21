using System.Windows.Input;
using System.Windows.Media;
using FileExplorer.CustomCollections;
using Microsoft.Win32;

namespace FileExplorer.ViewModels
{
    public interface IDirectoryViewModel : ISystemObjectViewModel
    {
        bool HasItems { get; }
        bool IsExpanded { set; get; }
        AsyncLoadCollection<ISystemObjectViewModel> Files { get; }
        AsyncLoadCollection<IDirectoryViewModel> SubDirectories { get; }
        UnionCollectionEx<IDirectoryViewModel, ISystemObjectViewModel, ISystemObjectViewModel> Children { get; }
        IDirectoryViewModel Parent { get; }
    }
}
