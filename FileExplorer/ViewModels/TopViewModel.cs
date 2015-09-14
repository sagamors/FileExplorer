using System;
using System.Collections.Generic;
using System.Windows.Input;
using FileExplorer.Helpers;
using FileExplorer.Services;

namespace FileExplorer.ViewModels
{
    public class TopViewModel : ViewModelBase
    {
        #region private fields

        private List<string> history = new List<string>();
        private IMessageBoxService MessageBoxService = new MessageBoxService();
        private int _positionHistory = 0;

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

        public TopViewModel()
        {
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
            if (path == "") return;
            {
                if (_positionHistory != history.Count - 1 && history.Count != 0)
                {
                    Clear();
                }
                history.Add(path);
                _positionHistory = history.Count - 1;
            }
        }

        private void OnCurrentPathSet()
        {
            try
            {
                SetWithOutSave(CurrentPath);
                CurrentPathSet?.Invoke(this, new NewPathSetArgs(CurrentPath));
                AddToHistory(CurrentPath);
            }
            catch (Exception exception)
            {
                MessageBoxService.ShowError(exception.Message);
            }
        }

        private void OnCurrentPathSetOutClear()
        {
            try
            {
                CurrentPathSet?.Invoke(this, new NewPathSetArgs(CurrentPath));
            }
            catch (Exception exception)
            {
                _positionHistory--;
                Clear();
                MessageBoxService.ShowError(exception.Message);
            }
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
