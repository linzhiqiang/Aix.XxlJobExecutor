using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Aix.XxlJobExecutor.Foundation
{
    /// <summary>
    /// 并发集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentHashSet<T> : IEnumerable<T>
    {
        private ConcurrentDictionary<T, bool> _keyValues = new ConcurrentDictionary<T, bool>();

        public int Count => _keyValues.Count;

        public bool TryAdd(T item)
        {
            return _keyValues.TryAdd(item, true);
        }


        public bool Contains(T item)
        {
            return _keyValues.ContainsKey(item);
        }

        public bool TryRemove(T item)
        {
            return _keyValues.TryRemove(item, out bool value);
        }

        public void Clear()
        {
            _keyValues.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            // return new ConcurrentHashSetEnum<T>(this.Dict);

            foreach (var item in _keyValues)
            {
                yield return item.Key;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ConcurrentHashSetEnum<T> : IEnumerator<T>
    {
        ConcurrentDictionary<T, bool> _keyValues;
        IEnumerator<KeyValuePair<T, bool>> _enumerator;

        public ConcurrentHashSetEnum(ConcurrentDictionary<T, bool> keyValues)
        {
            _keyValues = keyValues;
            _enumerator = _keyValues.GetEnumerator();
        }

        public T Current => _enumerator.Current.Key;

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {

        }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }
    }
}
