using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Helpers;

namespace FileExplorer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region private fields

        private PathHelper _pathHelper;
        private Dispatcher _dispatcher;
        private DirectoryWatcher _directoryWatcher;

        #endregion

        #region public properties

        public string Title { get; } = "FileExplorer";
        public TopViewModel Top { set; get; }

        public ObservableCollectionEx<IDirectoryViewModel> Items { set; get; } = new ObservableCollectionEx<IDirectoryViewModel>();

        #endregion

        #region constructors

        public MainViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            var root = new RootDirectoryViewModel();
            Items.Add(root);
            _pathHelper = new PathHelper(root);
            Top = new TopViewModel(_pathHelper);
            Top.SelectedDirectory = root;

            DirectoryViewModelBase.OpenDirectory += DirectoryViewModelBase_OpenDirectory;
            _directoryWatcher = new DirectoryWatcher(Top,root,_pathHelper,Dispatcher.CurrentDispatcher);
        }

        private void DirectoryViewModelBase_OpenDirectory(object sender, DirectoryViewModelBase.OpenDirectoryArgs e)
        {
            Top.SelectedDirectory = e.Directory;
        }

        #endregion



        // Define the event handlers.

    }
}
