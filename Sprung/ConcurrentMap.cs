using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprung
{
    public class ConcurrentMap<K, V>
    {
        readonly Dictionary<K, V> _map = new Dictionary<K, V>();

        public ICollection<K> Keys()
        {
            lock (_map)
            {
                return new List<K>(_map.Keys);
            }
        }

        public ICollection<V> Values()
        {
            lock (_map)
            {
                return new List<V>(_map.Values);
            }
        }

        public bool TryGetValue(K key, out V value)
        {
            lock (_map)
            {
                return _map.TryGetValue(key, out value);
            }
        }

        public bool TryAdd(K key, V value)
        {
            lock (_map)
            {
                if (!_map.ContainsKey(key))
                {
                    _map.Add(key, value);
                    return true;
                }
                return false;
            }
        }

        public bool TryRemove(K key)
        {
            lock (_map)
            {
                return _map.Remove(key);
            }
        }
    }
}
