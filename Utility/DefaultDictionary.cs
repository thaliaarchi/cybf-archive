using System;
using System.Collections;
using System.Collections.Generic;

namespace CyBF.Utility
{
    public class DefaultDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private IDictionary<TKey, TValue> _innerDictionary;
        private TValue _defaultValue;

        public TValue this[TKey key]
        {
            get
            {
                TValue value;

                if (_innerDictionary.TryGetValue(key, out value))
                    return value;
                else
                    return _defaultValue;
            }

            set
            {
                if (value.Equals(_defaultValue))
                    _innerDictionary.Remove(key);
                else
                    _innerDictionary[key] = value;
            }
        }

        public int Count
        {
            get
            {
                return _innerDictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _innerDictionary.IsReadOnly;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return _innerDictionary.Keys;
            }
        }
        
        public ICollection<TValue> Values
        {
            get
            {
                return _innerDictionary.Values;
            }
        }

        public TValue DefaultValue
        {
            get
            {
                return _defaultValue;
            }
        }

        public DefaultDictionary()
        {
            _innerDictionary = new Dictionary<TKey, TValue>();
            _defaultValue = default(TValue);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (!item.Value.Equals(_defaultValue))
                _innerDictionary.Add(item);
        }

        public void Add(TKey key, TValue value)
        {
            if (!value.Equals(_defaultValue))
                _innerDictionary.Add(key, value);
        }

        public void Clear()
        {
            _innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _innerDictionary.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _innerDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _innerDictionary.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _innerDictionary.Remove(item);
        }

        public bool Remove(TKey key)
        {
            return _innerDictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = this[key];
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }
    }
}
