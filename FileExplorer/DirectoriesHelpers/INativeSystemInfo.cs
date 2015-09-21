using System.Windows.Media;

namespace FileExplorer.DirectoriesHelpers
{
    public interface INativeSystemInfo
    {
        string DisplayName { get; }

        string TypeName { get; }
        int IconIndex { get;}
        string Path { get; }
        ImageSource Icon { get; }
    }
}