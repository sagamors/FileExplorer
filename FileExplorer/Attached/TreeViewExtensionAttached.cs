using System.Windows;
using System.Windows.Controls;
using FileExplorer.ViewModels;

namespace FileExplorer.Attached
{
    public class TreeViewExtensionAttached
    {
        public static readonly DependencyProperty SelectedProperty = DependencyProperty.RegisterAttached(
            "Selected", typeof (IDirectoryViewModel), typeof (TreeViewExtensionAttached), new PropertyMetadata(default(IDirectoryViewModel), PropertyChangedCallback ));

        private static void PropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TreeView treeView = sender as TreeView;
            if (treeView == null)
            {
                return;
            }

            treeView.SelectedItemChanged -= treeView_SelectedItemChanged;
            treeView.SelectedItemChanged += treeView_SelectedItemChanged;
        }

        public static void SetSelected(DependencyObject element, IDirectoryViewModel value)
        {
            element.SetValue(SelectedProperty, value);
        }

        public static IDirectoryViewModel GetSelected(DependencyObject element)
        {
            return (IDirectoryViewModel) element.GetValue(SelectedProperty);
        }

        public static object GetSelectedItem(DependencyObject obj)
        {
            return (object)obj.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(DependencyObject obj, object value)
        {
            obj.SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached("SelectedItem", typeof(object), typeof(TreeViewExtensionAttached), new PropertyMetadata(new object(), TreeViewSelectedItemChanged));

        private static DependencyObject obj;

        static void TreeViewSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            obj = sender;
            TreeView treeView = sender as TreeView;
            if (treeView == null)
            {
                return;
            }

            treeView.SelectedItemChanged -= treeView_SelectedItemChanged;
            treeView.SelectedItemChanged += treeView_SelectedItemChanged;
   
/*           TreeViewItem thisItem = treeView.ItemContainerGenerator.ContainerFromItem(e.NewValue) as TreeViewItem;
            if (thisItem != null)
            {
                thisItem.IsSelected = true;
                return;
            }

            for (int i = 0; i < treeView.Items.Count; i++)
                SelectItem(e.NewValue, treeView.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem);*/
        }

        static void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView treeView = sender as TreeView;
            SetSelected(treeView, (IDirectoryViewModel) e.NewValue);
        }

        private static bool SelectItem(object o, TreeViewItem parentItem)
        {
            if (parentItem == null)
                return false;

            bool isExpanded = parentItem.IsExpanded;
            if (!isExpanded)
            {
                parentItem.IsExpanded = true;
                parentItem.UpdateLayout();
            }

            TreeViewItem item = parentItem.ItemContainerGenerator.ContainerFromItem(o) as TreeViewItem;
            if (item != null)
            {
                item.IsSelected = true;
                return true;
            }

            bool wasFound = false;
            for (int i = 0; i < parentItem.Items.Count; i++)
            {
                TreeViewItem itm = parentItem.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                var found = SelectItem(o, itm);
                if (!found)
                    itm.IsExpanded = false;
                else
                    wasFound = true;
            }

            return wasFound;
        }
    }
}
