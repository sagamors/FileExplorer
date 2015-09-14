namespace FileExplorer.ViewModels.Old
{
/*    [ImplementPropertyChanged]
    public class DirectoryViewModel : ViewModelBase, IDirectoryViewModel
    {
        public DirectoryViewModel(DirectoryInfo parent)
        {
            Parent = parent;
//            Icon = new FileToImageIconConverter(parent.FullName);
        }

        // \todo может как то подругому назвать?
        public DirectoryInfo Parent { get; set; }
        // \todo может его сделать через конвертер?
//        public FileToImageIconConverter Icon { set; get; }

        public bool IsExpanded { set; get; }
        public bool IsSelected { set; get; }

        public string Path { get; }

        public string DisplayName
        {
            get { return Parent.Name; }
        }

        public ImageSource Icon { get; set; }
        public DateTime LastModificationDate { get { return Parent.LastWriteTime; } }

        public ObservableCollection<ISystemObjectViewModel> Children
        {
            get
            {
                try
                {
                    var child = new ObservableCollection<ISystemObjectViewModel>(SubDirectories);
                    // \todo переделать?
                    foreach (var file in Files)
                    {
                        child.Add(file);
                    }
                    return child;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public ObservableCollection<IDirectoryViewModel> SubDirectories
        {
            get
            {
                try
                {
                    return new ObservableCollection<IDirectoryViewModel>(Parent.GetDirectories().Select(info => new DirectoryViewModel(info)));
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public ObservableCollection<FileViewModel> Files
        {
            get
            {
                try
                {
                    return new ObservableCollection<FileViewModel>(Parent.GetFiles().Select(file => new FileViewModel(file)));
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
       
    }*/
}
