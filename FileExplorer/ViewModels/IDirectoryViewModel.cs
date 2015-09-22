using FileExplorer.CustomCollections;


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
        bool NoAccess { get; }
        void LoadAll();
        void UpdateHasItems();
    }
}
