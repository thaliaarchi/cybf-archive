using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.Utility
{
    public class StackedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private Frame _current;

        public TValue this[TKey key]
        {
            get
            {
                return _current[key];
            }

            set
            {
                _current[key] = value;
            }
        }

        public int Count
        {
            get
            {
                return _current.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return _current.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return _current.Values;
            }
        }

        public StackedDictionary()
        {
            _current = new Frame();
        }

        public void Push()
        {
            _current = new Frame(_current);
        }

        public void Pop()
        {
            if (_current.Parent == null)
                throw new InvalidOperationException("Stack empty.");

            _current = _current.Parent;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _current.Add(item);
        }

        public void Add(TKey key, TValue value)
        {
            _current.Add(key, value);
        }

        public void Clear()
        {
            while (_current.Parent != null)
                _current = _current.Parent;

            _current.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _current.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _current.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _current.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _current.GetEnumerator();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _current.Remove(item);
        }

        public bool Remove(TKey key)
        {
            return _current.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _current.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _current.GetEnumerator();
        }

        public class Frame : IDictionary<TKey, TValue>
        {
            private Frame _parent;
            private Dictionary<TKey, TValue> _table;

            public Frame Parent { get { return _parent; } }

            public TValue this[TKey key]
            {
                get
                {
                    TValue result;

                    if (_table.TryGetValue(key, out result))
                        return result;

                    if (_parent != null)
                        return _parent[key];

                    throw new KeyNotFoundException();
                }

                set
                {
                    _table[key] = value;
                }
            }

            public int Count
            {
                get
                {
                    if (_parent == null)
                        return _table.Count;
                    else
                        return _parent.Count + _table.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public ICollection<TKey> Keys
            {
                get
                {
                    if (_parent == null)
                        return _table.Keys.ToList();

                    List<TKey> result = (List<TKey>)_parent.Keys;
                    result.AddRange(_table.Keys);
                    return result;
                }
            }

            public ICollection<TValue> Values
            {
                get
                {
                    if (_parent == null)
                        return _table.Values.ToList();

                    List<TValue> result = (List<TValue>)_parent.Values;
                    result.AddRange(_table.Values);
                    return result;
                }
            }

            public Frame()
            {
                _parent = null;
                _table = new Dictionary<TKey, TValue>();
            }

            public Frame(Frame parent)
            {
                _parent = parent;
                _table = new Dictionary<TKey, TValue>();
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                ((ICollection<KeyValuePair<TKey, TValue>>)_table).Add(item);
            }

            public void Add(TKey key, TValue value)
            {
                _table.Add(key, value);
            }

            public void Clear()
            {
                _table.Clear();
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                ICollection<KeyValuePair<TKey, TValue>> collection = _table;

                if (collection.Contains(item))
                    return true;

                if (_parent != null)
                    return _parent.Contains(item);

                return false;
            }

            public bool ContainsKey(TKey key)
            {
                if (_table.ContainsKey(key))
                    return true;

                if (_parent != null)
                    return _parent.ContainsKey(key);

                return false;
            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                ICollection<KeyValuePair<TKey, TValue>> collection = _table;

                if (_parent != null)
                {
                    _parent.CopyTo(array, arrayIndex);
                    arrayIndex += _parent.Count;
                }

                collection.CopyTo(array, arrayIndex);
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                if (_parent == null)
                    return _table.GetEnumerator();

                return _parent.Concat(_table).GetEnumerator();
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                ICollection<KeyValuePair<TKey, TValue>> collection = _table;
                return collection.Remove(item);
            }

            public bool Remove(TKey key)
            {
                return _table.Remove(key);
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                if (_table.TryGetValue(key, out value))
                    return true;

                if (_parent != null)
                    return _parent.TryGetValue(key, out value);

                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

    }
}
