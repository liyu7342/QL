namespace QL.Core.Caching
{
    using QL.Core.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Runtime.InteropServices;

    public class MemoryCaching : ICaching, IDisposable
    {
        private MemoryCache _cache;
        private static ICaching _cacheInstance = null;
        private static object _initLock = new object();

        public MemoryCaching()
        {
            this._cache = MemoryCache.Default;
        }

        public MemoryCaching(string name)
        {
            this._cache = new MemoryCache(name, null);
        }

        public long Decrement(string key, uint amount)
        {
            long data = this.Exists(key) ? this.Get(key).As<long>() : 0L;
            data -= amount;
            this.Set<long>(key, data);
            return data;
        }

        public void Dispose()
        {
            this._cache.Dispose();
        }

        public bool Exists(string key)
        {
            return this._cache.Contains(key, null);
        }

        public object Get(string key)
        {
            return this._cache.Get(key, null);
        }

        public T Get<T>(string key)
        {
            return (T)this._cache.Get(key, null);
        }

        public IDictionary<string, object> GetAll(string[] keys)
        {
            return this._cache.GetValues(null, keys);
        }

        public IDictionary<string, T> GetAll<T>(string[] keys)
        {
            return (IDictionary<string, T>)this._cache.GetValues(null, keys).ToDictionary<KeyValuePair<string, object>, string, T>(obj => obj.Key, obj => ((T)obj.Value));
        }

        public T GetOrAdd<T>(string key, Func<T> handler)
        {
            T local;
            if (!this.TryGet<T>(key, out local))
            {
                local = handler();
                if (local != null)
                {
                    this.Set<T>(key, local);
                }
            }
            return local;
        }

        public T GetOrAdd<T>(string key, Func<T> handler, DateTimeOffset absoluteExpiration)
        {
            T local;
            if (!this.TryGet<T>(key, out local))
            {
                local = handler();
                if (local != null)
                {
                    this.Set<T>(key, local, absoluteExpiration);
                }
            }
            return local;
        }

        public T GetOrAdd<T>(string key, Func<T> handler, params string[] files)
        {
            T local;
            if (!this.TryGet<T>(key, out local))
            {
                local = handler();
                if (local != null)
                {
                    this.Set<T>(key, local, files);
                }
            }
            return local;
        }

        public T GetOrAdd<T>(string key, Func<T> handler, DateTimeOffset absoluteExpiration, params string[] files)
        {
            T local;
            if (!this.TryGet<T>(key, out local))
            {
                local = handler();
                if (local != null)
                {
                    this.Set<T>(key, local, absoluteExpiration, files);
                }
            }
            return local;
        }

        public long Increment(string key, uint amount)
        {
            long data = this.Exists(key) ? this.Get(key).As<long>() : 0L;
            data += amount;
            this.Set<long>(key, data);
            return data;
        }

        public bool Remove(string key)
        {
            return (this._cache.Remove(key, null) != null);
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            foreach (string str in keys)
            {
                this._cache.Remove(str, null);
            }
        }

        public void Set(string key, object data)
        {
            this._cache.Set(key, data, ObjectCache.InfiniteAbsoluteExpiration, null);
        }

        public void Set<T>(string key, T data)
        {
            this._cache.Set(key, data, ObjectCache.InfiniteAbsoluteExpiration, null);
        }

        public void Set(string key, object data, DateTimeOffset absoluteExpiration)
        {
            this._cache.Set(key, data, absoluteExpiration, null);
        }

        public void Set(string key, object data, params string[] dependentFiles)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                ChangeMonitors = { new HostFileChangeMonitor(dependentFiles) }
            };
            this._cache.Set(key, data, policy, null);
        }

        public void Set<T>(string key, T data, DateTimeOffset absoluteExpiration)
        {
            this._cache.Set(key, data, absoluteExpiration, null);
        }

        public void Set<T>(string key, T data, params string[] dependentFiles)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                ChangeMonitors = { new HostFileChangeMonitor(dependentFiles) }
            };
            this._cache.Set(key, data, policy, null);
        }

        public void Set(string key, object data, DateTimeOffset absoluteExpiration, params string[] dependentFiles)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = absoluteExpiration
            };
            policy.ChangeMonitors.Add(new HostFileChangeMonitor(dependentFiles));
            this._cache.Set(key, data, policy, null);
        }

        public void Set<T>(string key, T data, DateTimeOffset absoluteExpiration, params string[] dependentFiles)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = absoluteExpiration
            };
            policy.ChangeMonitors.Add(new HostFileChangeMonitor(dependentFiles));
            this._cache.Set(key, data, policy, null);
        }

        public bool TryGet<T>(string key, out T value)
        {
            object obj2 = this.Get(key);
            if (obj2 == null)
            {
                value = default(T);
                return false;
            }
            value = (T)obj2;
            return true;
        }

        public static ICaching Default
        {
            get
            {
                if (_cacheInstance == null)
                {
                    lock (_initLock)
                    {
                        if (_cacheInstance == null)
                        {
                            string str = ConfigurationManager.AppSettings["QL.Cache.Memory.Name"];
                            if (string.IsNullOrEmpty(str))
                            {
                                _cacheInstance = new MemoryCaching();
                            }
                            else
                            {
                                _cacheInstance = new MemoryCaching(str);
                            }
                        }
                    }
                }
                return _cacheInstance;
            }
        }
    }
}