using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using FileExplorer.DirectoriesHelpers;
using FileExplorer.Helpers;
using FileExplorer.Services;

namespace FileExplorer.ViewModels
{
    public class TopViewModel : ViewModelBase
    {
        private readonly DirectoryWatcher _directoryWatcher;
        public PathHelper PathHelper {private get; set; }

        #region private fields

        private List<string> history = new List<string>();

        private int _positionHistory = -1;
        private Dispatcher _dispatcher;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private bool _currentPathBroken;
        private bool _fromHistory;

        #endregion

        #region public properties

        private string _currentPath;

        public string CurrentPath
        {
            set { _currentPath = value; }
            get { return _currentPath; }
        }

        public ICommand NewPathSetCommand { get; }

        public ICommand BackwardCommand { get; }
        public ICommand ForwardCommand { get; }

        private IDirectoryViewModel _selectedDirectory;
        public IDirectoryViewModel SelectedDirectory
        {
            set
            {
                _selectedDirectory = value;
                if(_selectedDirectory!=null)
                    _selectedDirectory.LoadAll();
                if (_selectedDirectory.VisualPath != CurrentPath || _selectedDirectory.Path != CurrentPath)
                {
                    CurrentPath = SelectedDirectory.VisualPath;
                }
                if (!_fromHistory)
                {
                    AddToHistory(CurrentPath);
                }

                _currentPathBroken = false;
                _fromHistory = false;
            }
            get { return _selectedDirectory; }
        }

        #endregion

        #region constructors

        public TopViewModel(PathHelper pathHelper, DirectoryWatcher directoryWatcher)
        {
            _directoryWatcher = directoryWatcher;
            _dispatcher = Dispatcher.CurrentDispatcher;
            PathHelper = pathHelper;
            NewPathSetCommand = new RelayCommand(OnCurrentPathSet);
            BackwardCommand = new RelayCommand(() => BackwardNavigation(), o => _positionHistory > 0);
            ForwardCommand = new RelayCommand(() => ForwardNavigation(), o => _positionHistory < history.Count - 1);

            DirectoryViewModelBase.OpenDirectory += DirectoryViewModelBase_OpenDirectory;
            DirectoryViewModelBase.NoExistDirectory += DirectoryViewModelBaseOnNoExistDirectory;
        }

        private void DirectoryViewModelBaseOnNoExistDirectory(object sender, DirectoryViewModelBase.NoExistDirectoryArgs e)
        {
            var directory = PathHelper.ClearNotExistDirectories(e.Directory);
            e.Directory.Parent.SubDirectories.Remove(directory);
            DirectoryWatcher.DeleteFileSystemWatcher(e.Directory);
        }

        private void DirectoryViewModelBase_OpenDirectory(object sender, DirectoryViewModelBase.OpenDirectoryArgs e)
        {
            SelectedDirectory = e.Directory;
        }

        #endregion

        #region public methods

        public void BackwardNavigation()
        {
            string path = history[_positionHistory-1];
            _positionHistory--;
            GoToPath(history[_positionHistory]);
        }

        public void ForwardNavigation()
        {
            _positionHistory++;
            GoToPath(history[_positionHistory]);
        }

        #endregion

        #region private methods

        private void ClearAfterPosition()
        {
            history.RemoveRange(_positionHistory + 1, history.Count - _positionHistory - 1);
            _positionHistory = history.Count-1;
        }

        private void AddToHistory(string path)
        {
            _dispatcher.Invoke(() =>
            {
                if (path == "") return;
                {
                    if (_positionHistory != history.Count - 1 && history.Count != 0)
                    {
                        ClearAfterPosition();
                    }
                    if (history.Count > 1 && (history[history.Count-1] == path))
                    {
                        return;
                    }
                    history.Add(path);
                    _positionHistory = history.Count - 1;
                }
            });
        }

        private void OnCurrentPathSet()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                try
                {
                    var child = PathHelper.GetAndLoadDirectory(CurrentPath, _cancellationTokenSource.Token);
                    try
                    {
                        child.Wait();
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerExceptions.FirstOrDefault(exception => exception.GetType() == typeof(OperationCanceledException)) != null) return;
                        if (!_currentPathBroken)
                        {
                            _currentPathBroken = true;
                            _positionHistory--;
                            ClearAfterPosition();
                        }
                        MessageBoxService.Instance.ShowError(ex.InnerExceptions.OfType<Exception>().First().Message);
                        return;
                    }

                    if (child.IsCanceled)
                    {
                        return;
                    }

                    if (child.IsFaulted)
                        throw child.Exception;
                    if (SelectedDirectory.VisualPath == child.Result.VisualPath ) return;
                    SelectedDirectory = child.Result;
                    AddToHistory(CurrentPath);
                }
                catch (Exception exception)
                {
                    MessageBoxService.Instance.ShowError(exception.Message);
                }

            }, _cancellationTokenSource.Token);
        }

        private void GoToPath(string path)
        {
            Task.Run(() =>
            {
                try
                {
                    var child = PathHelper.GetAndLoadDirectory(path, _cancellationTokenSource.Token);
                    try
                    {
                        child.Wait();
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerExceptions.FirstOrDefault(exception => exception.GetType() == typeof(OperationCanceledException))!=null) return;
                        if (!_currentPathBroken)
                        {
                            _currentPathBroken = true;
                            _positionHistory--;
                            ClearAfterPosition();
                        }
                        MessageBoxService.Instance.ShowError(ex.InnerExceptions.OfType<Exception>().First().Message); 
                        return;
                    }

                    if (child.IsCanceled)
                    {
                        return;
                    }

                    if (child.IsFaulted)
                        throw child.Exception;

                    if (CurrentPath == child.Result.VisualPath) return;
                    if (CurrentPath == child.Result.Path)
                    {
                        CurrentPath = child.Result.VisualPath;
                        return;
                    }
                    _fromHistory = true;
                    SelectedDirectory = child.Result;
                }
                catch (Exception exception)
                {
                    if (!_currentPathBroken)
                    {
                        _currentPathBroken = true;
                        _positionHistory--;
                        ClearAfterPosition();
                    }
                    MessageBoxService.Instance.ShowError(exception.Message);
                }

            }, _cancellationTokenSource.Token);
        }

        public class SelectedDirectoryChangedArgs : EventArgs
        {
            public IDirectoryViewModel NewDirectoryViewModel { get; }

            public SelectedDirectoryChangedArgs(IDirectoryViewModel newDirectoryViewModel)
            {
                NewDirectoryViewModel = newDirectoryViewModel;
            }
        }

        #endregion
    }
}
