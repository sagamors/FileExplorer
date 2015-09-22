using System.Windows;
using System.Windows.Controls;
using FileExplorer.ViewModels;

namespace FileExplorer.Helpers
{
    public class TemplateSelector : DataTemplateSelector
    {
        public DataTemplate DirectoryTemplate { get; set; }
        public DataTemplate FileTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IDirectoryViewModel)
            {
                return DirectoryTemplate;
            }
            return FileTemplate;
        }
    }
}
