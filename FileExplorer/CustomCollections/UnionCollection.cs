using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace FileExplorer.CustomCollections
{
    //minimum realization 
    public class UnionCollection<TFirst,TSecond,TCommon> : ObservableCollectionBase<TCommon> where TSecond : TCommon where TFirst : TCommon
    {
        #region private fields

        private UnionEnumerator<TFirst, TSecond, TCommon> _enumerator;

        #endregion

        #region public properties

        private readonly IList<TFirst> _first;

        public IList<TFirst> First
        {
            get { return _first; }
        }

        private readonly IList<TSecond> _second;

        public IList<TSecond> Second
        {
            get { return _second; }
        }

        public override int Count
        {
            get { return _first.Count + _second.Count; }

        }

        #endregion

        #region public types

        public class UnionEnumerator<TEnum, TEnum2, TCommon> : IEnumerator<TCommon> where TEnum : TCommon
            where TEnum2 : TCommon
        {
            private int _position;
            private readonly IList<TEnum> _first;
            private readonly IList<TEnum2> _second;

            public IList<TEnum> First
            {
                get { return _first; }
            }

            public IList<TEnum2> Second
            {
                get { return _second; }
            }

            public UnionEnumerator(IList<TEnum> first, IList<TEnum2> second)
            {
                _first = first;
                _second = second;
            }

            public bool MoveNext()
            {
                _position++;
                return (_position < _first.Count + _second.Count);
            }

            public void Reset()
            {
                _position = -1;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public TCommon Current
            {
                get
                {
                    try
                    {
                        if (_position < _first.Count)
                        {
                            return _first[_position];
                        }
                        return _second[_position - _first.Count];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            public void Dispose()
            {

            }
        }

        #endregion

        #region constructors

        public UnionCollection(IObservableCollection<TFirst> first, IObservableCollection<TSecond> second)
        {
            if (first != null)
            {
                _first = first;
                first.CollectionChanged += First_CollectionChanged;
            }
            if (second != null)
            {
                second.CollectionChanged += SecondOnCollectionChanged;
                _second = second;
            }
            _enumerator = new UnionEnumerator<TFirst, TSecond, TCommon>(first, second);
        }

        #endregion

        #region public methods

        public override void AddRange(IEnumerable<TCommon> range)
        {
            throw new NotSupportedException();
        }

        public override IEnumerator<TCommon> GetEnumerator()
        {
            return _enumerator;
        }

        public override void Add(TCommon item)
        {
            _second.Add((TSecond) item);
        }

        public override bool Contains(object value)
        {
            if (value == null) return false;
            if (value.GetType() == typeof (TFirst))
                return _first.Contains((TFirst) value);
            return _second.Contains((TSecond) value);
        }

        public override void Clear()
        {
            _first.Clear();
            _second.Clear();
        }

        public override int IndexOf(object value)
        {
            return IndexOf((TCommon) value);
        }

        public override void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        public override void Remove(object value)
        {
            throw new NotSupportedException();
        }

        public override void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public override bool Contains(TCommon item)
        {
            throw new NotSupportedException();
        }

        public override void CopyTo(TCommon[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public override bool Remove(TCommon item)
        {
            throw new NotSupportedException();
        }

        public override void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public override int IndexOf(TCommon item)
        {
            if (item is TFirst)
            {
                return _first.IndexOf((TFirst) item);
            }
            return _first.Count + _second.IndexOf((TSecond) item);
        }

        public override void Insert(int index, TCommon item)
        {
            throw new NotSupportedException();
        }

        public override TCommon this[int index]
        {
            get
            {
                if (index < _first.Count)
                {
                    return _first[index];
                }
                return _second[index - _first.Count];
            }
            set { throw new NotSupportedException(); }
        }

        #endregion

        #region private methods

        private void SecondOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems,
                e.NewStartingIndex + First.Count, e.OldStartingIndex + First.Count);
            OnCollectionChanged(this, args);
        }

        private void First_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(this, e);
        }

        #endregion

    }
}
