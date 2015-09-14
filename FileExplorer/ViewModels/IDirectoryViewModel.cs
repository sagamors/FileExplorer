using System.Windows.Input;
using FileExplorer.CustomCollections;

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
    }
}
