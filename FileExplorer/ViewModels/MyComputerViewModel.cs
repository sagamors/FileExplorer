using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FileExplorer.ViewModels
{
 /*   public class MyComputerViewModel : IDirectoryViewModel
    {
        public string Path { get; }
        public string DisplayName { get; }
        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }
        public ImageSource Icon { get; set; }
        public DateTime LastModificationDate { get; }
        public ObservableCollection<ISystemObjectViewModel> Children { get; }

        // todo обновление
        public ObservableCollection<IDirectoryViewModel> SubDirectories
        {
            get {
                ObservableCollection <IDirectoryViewModel> res = new ObservableCollection<IDirectoryViewModel>();
                foreach (string logicalDrive in Directory.GetLogicalDrives())
                {
                    res.Add(new DirectoryViewModel(new DirectoryInfo(logicalDrive)));
                };
                return res;
            }
        }

        public  MyComputerViewModel()
        {
            DisplayName = "Sda";
        }
    }*/
}
