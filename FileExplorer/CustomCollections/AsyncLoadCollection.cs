using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace FileExplorer.CustomCollections
{
    //minimal realization
    public class AsyncLoadCollection<T> : ObservableCollectionBase<T> where T : class 
    {

        #region private fields

        private readonly TimeSpan _longLoadingTime = new TimeSpan(0, 0, 0, 1);
        private readonly IItemsProvider<T> _provider;
        private ObservableCollection<T> _collection = new ObservableCollection<T>();
        private SynchronizationContext _synchronizationContext;
        private Timer timer;
        private Progress<int> _progress = new Progress<int>();

        #endregion

        #region public properties

        public bool IsLoading { private set; get; }
        public bool IsLongLoading { private set; get; }
        public bool IsLoaded { get; set; }
        public bool HasItems { get; private set; }

        private int _count = 0;
        private int _progressLoading;

        public override int Count
        {
            get
            {
                return _count;
            }
        }

        public int ProgressLoading
        {
            private set
            {
                if(_progressLoading==value) return;
                _progressLoading = value;
                OnProgressChanged();
            }
            get { return _progressLoading; }
        }

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        #endregion

        #region constructor

        public AsyncLoadCollection(IItemsProvider<T> provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            _provider = provider;
            _synchronizationContext = new DispatcherSynchronizationContext(Application.Current.Dispatcher);
            timer = new Timer(_longLoadingTime.TotalMilliseconds);
            timer.Elapsed += Timer_Elapsed;

            _progress.ProgressChanged += _progress_ProgressChanged;
        }

        #endregion

        #region public methods

        public override void AddRange(IEnumerable<T> range)
        {
            throw new NotSupportedException();
        }

        public override IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        public override void Add(T item)
        {
            _collection.Add(item);
        }

        public override bool Contains(object value)
        {
            return _collection.Contains((T) value);
        }

        public override void Clear()
        {
            _collection.Clear();
        }

        public override int IndexOf(object value)
        {
            return _collection.IndexOf((T) value);
        }

        public override void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        public override void Remove(object value)
        {
            throw new NotSupportedException();
        }

        public override void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public override void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public override bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        public override void CopyTo(T[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public override bool Remove(T item)
        {
            var result = _collection.Remove(item);
            _count = _collection.Count;
            FireCollectionReset();
            return result;
        }

        public override void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public override int IndexOf(T item)
        {
            return _collection.IndexOf(item);
        }


        public Task LoadAsync()
        {
            timer.Start();
            IsLoading = true;
            if (_collection != null)
                _collection.Clear();
            _synchronizationContext.Send(SetCount, 0);
            return Task.Run(() =>
            {
                try
                {
                    _collection = _provider.GetItems(_progress);
                    _synchronizationContext.Send(LoadCompleted, _collection.Count);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            });
        }

        public override T this[int index]
        {
            get { return _collection[index]; }
            set
            {
                T originalItem = this[index];
                _collection[index] = value;
                OnCollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, originalItem,
                        index));
            }
        }

        #endregion

        #region private methods

        private void SetCount(object args)
        {
            _count = (int) args;
            HasItems = _count > 0;
            FireCollectionReset();
        }

        private void LoadCompleted(object args)
        {
            SetCount(args);
            IsLoading = false;
            IsLoaded = true;
            timer.Stop();
            IsLongLoading = false;
        }

        /// <summary>
        /// Fires the collection reset event.
        /// </summary>
        private void FireCollectionReset()
        {
            NotifyCollectionChangedEventArgs e =
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(this, e);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            IsLongLoading = true;
            timer.Stop();
        }

        private void OnProgressChanged()
        {
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(ProgressLoading,0));
        }

        private void _progress_ProgressChanged(object sender, int e)
        {
            _synchronizationContext.Send(state =>
            {
                ProgressLoading = e;

            }, 0);
        }

        #endregion

    }
}

