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
        ObservableCollectionBase<ISystemObjectViewModel> Children { get; }
        AsyncLoadCollection<IDirectoryViewModel> SubDirectories { get; }
        IDirectoryViewModel Parent { get; }
        int ChildrenProgressLoading { get; }
        bool ChildrenLoaded { set; }
    }
}
