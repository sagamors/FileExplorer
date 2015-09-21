using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged;

namespace FileExplorer.CustomCollections
{
    [ImplementPropertyChanged]
    public abstract class ObservableCollectionBase<T> : IObservableCollection<T>
     {
        public virtual object SyncRoot { get; }
        public virtual bool IsSynchronized { get; }
        public virtual int Count { protected set; get; }
        public virtual bool  IsReadOnly { get; }
        public virtual bool IsFixedSize { get; }

        public abstract void AddRange(IEnumerable<T> range);

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract void Add(T item);

        public int Add(object value)
        {
            Add((T) value);
            return Count - 1;
        }

        public abstract bool Contains(object value);
        public abstract void Clear();

        public abstract int IndexOf(object value);

        public abstract void Insert(int index, object value);

        public abstract void Remove(object value);

        public abstract void RemoveAt(int index);

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T) value; }
        }

        public abstract bool Contains(T item);
        public abstract void CopyTo(T[] array, int arrayIndex);

        public abstract bool Remove(T item);
        public abstract void CopyTo(Array array, int index);
        public abstract int IndexOf(T item);
        public abstract void Insert(int index, T item);

        public abstract T this[int index] { set; get; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> 
        /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged">). 
        /// </see></summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary> 
        /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged">). 
        /// </see></summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(sender, e);
        }
    }
}
