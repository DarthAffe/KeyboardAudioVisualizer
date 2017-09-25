using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using RGB.NET.Core;

// Taken from https://codereview.stackexchange.com/questions/116562/custom-implementation-of-observabledictionary
namespace KeyboardAudioVisualizer.Helper
{
    public class ObservableDictionary<TKey, TValue> : AbstractBindable, IDictionary<TKey, TValue>, INotifyCollectionChanged
    {
        #region Constants

        private const string INDEXER_NAME = "Item[]";

        #endregion

        #region Properties & Fields

        private readonly IList<TValue> _values;
        private readonly IDictionary<TKey, int> _indexMap;

        private readonly SimpleMonitor _monitor = new SimpleMonitor();

        #endregion

        #region Constructor

        public ObservableDictionary()
        {
            _values = new List<TValue>();
            _indexMap = new Dictionary<TKey, int>();
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _values = new List<TValue>();
            _indexMap = new Dictionary<TKey, int>();

            int idx = 0;
            foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
            {
                _indexMap.Add(kvp.Key, idx);
                _values.Add(kvp.Value);

                idx++;
            }
        }

        public ObservableDictionary(int capacity)
        {
            _values = new List<TValue>(capacity);
            _indexMap = new Dictionary<TKey, int>(capacity);
        }

        #endregion

        #region Virtual Add/Remove/Change Control Methods

        protected virtual void AddItem(TKey key, TValue value)
        {
            CheckReentrancy();

            int index = _values.Count;
            _indexMap.Add(key, index);
            _values.Add(value);

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(Keys));
            OnPropertyChanged(nameof(Values));
            OnPropertyChanged(INDEXER_NAME);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, key, value, index);
        }

        protected virtual bool RemoveItem(TKey key)
        {
            CheckReentrancy();

            int index = _indexMap[key];
            TValue value = _values[index];

            if (_indexMap.Remove(key))
            {
                _values.RemoveAt(index);

                List<TKey> keys = _indexMap.Keys.ToList();

                foreach (TKey existingKey in keys)
                {
                    if (_indexMap[existingKey] > index)
                        _indexMap[existingKey]--;
                }

                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(Keys));
                OnPropertyChanged(nameof(Values));
                OnPropertyChanged(INDEXER_NAME);
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, key, value, index);

                return true;
            }

            return false;
        }

        protected virtual bool RemoveItem(KeyValuePair<TKey, TValue> item)
        {
            CheckReentrancy();

            if (_indexMap.ContainsKey(item.Key) && _values[_indexMap[item.Key]].Equals(item.Value))
            {
                int index = _indexMap[item.Key];
                TValue value = _values[index];

                _indexMap.Remove(item.Key);
                _values.RemoveAt(index);

                List<TKey> keys = _indexMap.Keys.ToList();

                foreach (TKey existingKey in keys)
                {
                    if (_indexMap[existingKey] > index)
                        _indexMap[existingKey]--;
                }

                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(Keys));
                OnPropertyChanged(nameof(Values));
                OnPropertyChanged(INDEXER_NAME);
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, item.Key, item.Value, index);

                return true;
            }

            return false;
        }

        protected virtual void RemoveAllItems()
        {

            CheckReentrancy();
            _values.Clear();
            _indexMap.Clear();

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(Keys));
            OnPropertyChanged(nameof(Values));
            OnPropertyChanged(INDEXER_NAME);
            OnCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        protected virtual void ChangeItem(TKey key, TValue newValue)
        {

            CheckReentrancy();

            if (!_indexMap.ContainsKey(key))
                AddItem(key, newValue);
            else
            {
                int index = _indexMap[key];
                TValue oldValue = _values[index];
                _values[index] = newValue;

                OnPropertyChanged(nameof(Values));
                OnPropertyChanged(INDEXER_NAME);
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, key, oldValue, newValue, index);
            }
        }

        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return (IDisposable)_monitor;
        }

        protected void CheckReentrancy()
        {
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            if (_monitor.Busy && CollectionChanged != null && CollectionChanged.GetInvocationList().Length > 1)
                throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed");
        }


        #endregion

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value) => AddItem(key, value);

        public bool ContainsKey(TKey key) => _indexMap.ContainsKey(key);

        public bool Remove(TKey key) => RemoveItem(key);

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_indexMap.TryGetValue(key, out int index))
            {
                value = _values[index];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }


        public ICollection<TKey> Keys => _indexMap.Keys;

        public ICollection<TValue> Values => _values;

        public TValue this[TKey key]
        {
            get
            {
                int index = _indexMap[key];
                return _values[index];
            }
            set => ChangeItem(key, value);
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Clear() => RemoveAllItems();

        public int Count => _indexMap.Count;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => _indexMap.ContainsKey(item.Key) && _values[_indexMap[item.Key]].Equals(item.Value);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<TKey, int> kvp in _indexMap)
            {
                array[arrayIndex] = new KeyValuePair<TKey, TValue>(kvp.Key, _values[kvp.Value]);
                arrayIndex++;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => RemoveItem(item);

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (KeyValuePair<TKey, int> kvp in _indexMap)
            {
                KeyValuePair<TKey, TValue> pair = new KeyValuePair<TKey, TValue>(kvp.Key, _values[kvp.Value]);
                yield return pair;
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;

            using (BlockReentrancy())
                handler?.Invoke(this, e);
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action) => OnCollectionChanged(new NotifyCollectionChangedEventArgs(action));

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, TKey key, TValue value, int index) => OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, new KeyValuePair<TKey, TValue>(key, value), index));

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, TKey key, TValue oldValue, TValue newValue, int index)
        {
            KeyValuePair<TKey, TValue> newPair = new KeyValuePair<TKey, TValue>(key, newValue);
            KeyValuePair<TKey, TValue> oldPair = new KeyValuePair<TKey, TValue>(key, oldValue);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newPair, oldPair, index));
        }

        #endregion

        private class SimpleMonitor : IDisposable
        {
            private int _busyCount;

            public bool Busy => _busyCount > 0;

            public void Enter() => _busyCount = _busyCount + 1;

            public void Dispose() => _busyCount = _busyCount - 1;
        }
    }
}
