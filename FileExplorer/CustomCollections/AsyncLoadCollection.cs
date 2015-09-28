using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace FileExplorer.CustomCollections
{
    //minimal realization
    public class AsyncLoadCollection<T> : ObservableCollectionBase<T>
    {

        #region private fields

        private readonly TimeSpan _longLoadingTime = new TimeSpan(0, 0, 0, 1);
        private readonly IItemsProvider<T> _provider;
        //todo переподписаться на события
        private ObservableCollection<T> _collection = new ObservableCollection<T>();
        private SynchronizationContext _synchronizationContext;
        private Timer timer;
        private Progress<int> _progress = new Progress<int>();
        private string CountString = "Count";
        #endregion

        #region public properties

        public bool IsLoading { private set; get; }
        public bool IsLongLoading { private set; get; }
        public bool IsLoaded { get; set; }
        public bool HasItems { get; private set; }
        public int PreLoadedCount { get; private set; }
        private int _progressLoading;

        public override int Count { protected set;get; } = 0;
        public event EventHandler<ErrorEventArgs> LoadingError; 


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
        public event EventHandler<EventArgs> CollectionLoaded;

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
            provider.CountLoaded += Provider_CountLoaded;
        }

        private void Provider_CountLoaded(object sender, CountLoadedEventArgs e)
        {
           _synchronizationContext.Send(state =>
           {
               PreLoadedCount = e.Count;
               HasItems = PreLoadedCount > 0;
           },null);
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
            FireCollectionClear();
        }

        public override int IndexOf(object value)
        {
            return _collection.IndexOf((T) value);
        }

        public override void Insert(int index, object item)
        {
            Insert(index, (T) item);
        }

        public override void Remove(object value)
        {
            Remove((T) value);
        }

        public override void Insert(int index, T item)
        {
            _collection.Insert(index, item);
            Count = _collection.Count;
            FireCollectionReset();
           // OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public override void RemoveAt(int index)
        {
            Remove(_collection[index]);
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
            Count = _collection.Count;
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

        private Task _loadingTask;
        public Task LoadAsync(CancellationToken token = new CancellationToken())
        {
            if (IsLoading)
            {
                return _loadingTask;
            }
            timer.Start();
            token.Register(() =>
            {
                ResetState();
                _synchronizationContext.Send(SetCount, 0);
            }
            );
            
            IsLoading = true;
            if (_collection != null)
                _collection.Clear();
            _synchronizationContext.Send(SetCount, 0);
            return _loadingTask = Task.Run(() =>
            {
                try
                {
                    _collection = _provider.GetItems(_progress, token);
                }
                catch (Exception ex)
                {
                    OnLoadingError(ex);
                    _synchronizationContext.Send(SetCount, 0);
                    return;
                }
                _synchronizationContext.Send(LoadCompleted, _collection.Count);
            }, token);
        }

        public override T this[int index]
        {
            get { return _collection[index]; }
            set
            {
                T originalItem = this[index];
                _collection[index] = value;
                OnCollectionChanged(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new List<T>() { value}, new List<T>() { originalItem }));
            }
        }

        #endregion

        #region private methods

        private void SetCount(object args)
        {
            Count = (int) args;
            HasItems = Count > 0;
            FireCollectionReset();
        }

        private void LoadCompleted(object args)
        {
            SetCount(args);
            IsLoading = false;
            IsLoaded = true;
            timer.Stop();
            IsLongLoading = false;
            OnCollectionLoaded();
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

        /// <summary>
        /// Fires the collection reset event.
        /// </summary>
        private void FireCollectionClear()
        {
            IsLoaded = false;
            IsLoading = false;
            IsLongLoading = false;
            Count = 0;
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

        private void OnCountChanged()
        {
            OnPropertyChanged(CountString);
        }

        private void _progress_ProgressChanged(object sender, int e)
        {
            _synchronizationContext.Send(state =>
            {
                ProgressLoading = e;
            }, 0);
        }

        #endregion

        protected virtual void OnCollectionLoaded()
        {
            CollectionLoaded?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnLoadingError(Exception e)
        {
            ResetState();
            _synchronizationContext.Send(state =>
            {
                LoadingError?.Invoke(this, new ErrorEventArgs(e));
            },0);
        }

        private void ResetState()
        {
            IsLoading = false;
            IsLoaded = false;
            IsLongLoading = false;
        }
    }
}

