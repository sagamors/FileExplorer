using System;
using System.ComponentModel;
using System.Diagnostics;

namespace FileExplorer.CustomCollections
{
    public class UnionCollectionEx<TFirst, TSecond, TCommon> : UnionCollection<TFirst, TSecond, TCommon>
        where TSecond : TCommon where TFirst : TCommon
    {
        private AsyncLoadCollection<TFirst> _first;
        private AsyncLoadCollection<TSecond> _second;
        private bool _isLoaded;
        public bool IsLoading { private set; get; }

        public bool IsLoaded
        {
            private set
            {
                _isLoaded = value;
                if (_isLoaded)
                {
                    IsLoading = false;
                }
            }
            get { return _isLoaded; }
        }

        public int Progress { private set; get; }

        public UnionCollectionEx(AsyncLoadCollection<TFirst> first, AsyncLoadCollection<TSecond> second)
            : base(first, second)
        {
            first.CollectionLoaded += CollectionLoaded;
            second.CollectionLoaded += CollectionLoaded;
            _first = first;
            _second = second;
            _first.ProgressChanged += SecondOnProgressChanged;
            _first.PropertyChanged += _first_PropertyChanged;
            _second.ProgressChanged += SecondOnProgressChanged;
        }

        private void _first_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsLoading")
            {
                IsLoading = _first.IsLoading || _second.IsLoading;
            }
        }

        private void SecondOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int totalSize = _second.PreLoadedCount + _first.PreLoadedCount;
            int firstProgress = (_first.ProgressLoading * _first.PreLoadedCount) / 100;
            int secondProgress = (_second.ProgressLoading * _second.PreLoadedCount) / 100;
            Progress = (int) (((firstProgress + secondProgress) * 100.0) / totalSize);
            Debug.WriteLine(Progress);
        }

        private void CollectionLoaded(object sender, EventArgs e)
        {
            IsLoaded = _first.IsLoaded && _second.IsLoaded;
        }
    }
}
