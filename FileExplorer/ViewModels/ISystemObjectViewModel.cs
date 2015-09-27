using System;
using System.Windows.Input;
using System.Windows.Media;

namespace FileExplorer.ViewModels
{

    public interface ISystemObjectViewModel
    {
        string DisplayName { get; }
        string TypeName { get; }
        long Size { get; }
        DateTime? LastModificationDate { get; }
        string Path { get; }
        string VisualPath { get; }
        ICommand OpenCommand { get; }
        bool IsSelected { get; set; }
        ImageSource Icon { get; }
        void Open();
        void UpdateParameters();
    }
}
