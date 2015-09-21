using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public PathHelper PathHelper {private get; set; }

        #region private fields

        private List<string> history = new List<string>();
        private IMessageBoxService MessageBoxService = new MessageBoxService();
        private int _positionHistory = 0;
        private Dispatcher _dispatcher;
        #endregion

        #region public properties

        private string _currentPath;

        public string CurrentPath
        {
            set { _currentPath = value; }
            get { return _currentPath; }
        }

        public ICommand NewPathSetCommand { get; }
        public event EventHandler<NewPathSetArgs> CurrentPathSet;
        public ICommand BackwardCommand { get; }
        public ICommand ForwardCommand { get; }

        #endregion

        #region constructors

        public TopViewModel(PathHelper pathHelper)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            PathHelper = pathHelper;
            NewPathSetCommand = new RelayCommand(OnCurrentPathSet);
            BackwardCommand = new RelayCommand(() => BackwardNavigation(), o => _positionHistory > 0);
            ForwardCommand = new RelayCommand(() => ForwardNavigation(), o => _positionHistory < history.Count - 1);
        }

        #endregion

        #region public methods

        public void SetCurrentPath(string path)
        {
            AddToHistory(path);
            SetWithOutSave(path);
        }

        public void BackwardNavigation()
        {
            _positionHistory--;
            CurrentPath = history[_positionHistory];
            OnCurrentPathSetOutClear();
        }

        public void ForwardNavigation()
        {
            _positionHistory++;
            CurrentPath = history[_positionHistory];
            OnCurrentPathSetOutClear();
        }

        #endregion

        #region private methods

        private void SetWithOutSave(string path)
        {
            _currentPath = path;
            OnPropertyChanged("CurrentPath");
        }

        private void Clear()
        {
            history.RemoveRange(_positionHistory + 1, history.Count - _positionHistory - 1);
            _positionHistory = history.Count;
        }

        private void AddToHistory(string path)
        {
            _dispatcher.Invoke(() =>
            {
                if (path == "") return;
                {
                    if (_positionHistory != history.Count - 1 && history.Count != 0)
                    {
                        Clear();
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
                        return;
                    }

                    if (child.IsCanceled)
                    {
                        return;
                    }

                    if (child.IsFaulted)
                        throw child.Exception;

                    child.Result.IsSelected = true;
                    CurrentPathSet?.Invoke(this, new NewPathSetArgs(CurrentPath));
                    AddToHistory(CurrentPath);
                }
                catch (Exception exception)
                {
                    MessageBoxService.ShowError(exception.Message);
                }

            }, _cancellationTokenSource.Token);

            //SetWithOutSave(CurrentPath);
/*            OnCurrentPath();*/
        }

        private void OnCurrentPathSetOutClear()
        {
/*            try
            {
                OnCurrentPath();
            }
            catch (Exception exception)
            {
                _positionHistory--;
                Clear();
                MessageBoxService.ShowError(exception.Message);
            }*/
        }

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private void OnCurrentPathAsync()
        {

        }

        #endregion
    }

    public class NewPathSetArgs : EventArgs
    {
        public string Path { set; get; }

        public NewPathSetArgs(string path)
        {
            Path = path;
        }
    }
}
