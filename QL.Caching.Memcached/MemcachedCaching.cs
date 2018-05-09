namespace QL.Caching.Memcached
{
    using Enyim.Caching;
    using Enyim.Caching.Memcached;
    using QL.Core.Caching;
    using QL.Core.Extensions;
    using QL.Core.ObjectPool;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Runtime.InteropServices;

    public class MemcachedCaching : ICaching, System.IDisposable
    {
        private static ICaching _cacheInstance = null;
        private MemcachedClient _client;
        private static object _initLock = new object();
        private bool _isDisposed;
        private System.Collections.Generic.Dictionary<string, HostFileChangeMonitor> _monitors;
        private string _sectionName;
        private bool _useObjectPool;

        public MemcachedCaching()
            : this(null, false)
        {
        }

        public MemcachedCaching(string sectionName, bool useObjectPool)
        {
            this._sectionName = sectionName;
            this._useObjectPool = useObjectPool;
            this._monitors = new System.Collections.Generic.Dictionary<string, HostFileChangeMonitor>(100, (System.Collections.Generic.IEqualityComparer<string>)System.StringComparer.InvariantCultureIgnoreCase);
        }

        public long Decrement(string key, uint amount)
        {
            return (long)this.GetClient().Decrement(key, 0L, (ulong)amount);
        }

        public void Dispose()
        {
            if (!this._isDisposed)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, HostFileChangeMonitor> pair in this._monitors)
                {
                    pair.Value.Dispose();
                }
                this._monitors.Clear();
                if (this._client != null)
                {
                    this._client.Dispose();
                    this._client = null;
                }
                this._isDisposed = true;
            }
        }

        public bool Exists(string key)
        {
            object obj2;
            return this.GetClient().TryGet(key, out obj2);
        }

        public object Get(string key)
        {
            return this.GetClient().Get(key);
        }

        public T Get<T>(string key)
        {
            return this.GetClient().Get<T>(key);
        }

        public System.Collections.Generic.IDictionary<string, object> GetAll(string[] keys)
        {
            return (System.Collections.Generic.IDictionary<string, object>)this.GetClient().Get(keys);
        }

        public System.Collections.Generic.IDictionary<string, T> GetAll<T>(string[] keys)
        {
            return (System.Collections.Generic.IDictionary<string, T>)this.GetAll(keys).ToDictionary<System.Collections.Generic.KeyValuePair<string, object>, string, T>(obj => obj.Key, obj => ((T)obj.Value));
        }

        private MemcachedClient GetClient()
        {
            Func<MemcachedClient> func = null;
            if (this._useObjectPool)
            {
                if (func == null)
                {
                    func = delegate
                    {
                        if (string.IsNullOrEmpty(this._sectionName))
                        {
                            return new MemcachedClient();
                        }
                        return new MemcachedClient(this._sectionName);
                    };
                }
                return ObjectPoolExtensions.GetOrAdd<MemcachedClient>(ObjectPoolContext.Current, "QL.Caching.Memcached.Client", func);
            }
            if (this._client == null)
            {
                if (string.IsNullOrEmpty(this._sectionName))
                {
                    this._client = new MemcachedClient();
                }
                else
                {
                    this._client = new MemcachedClient(this._sectionName);
                }
            }
            return this._client;
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

        public T GetOrAdd<T>(string key, Func<T> handler, System.DateTimeOffset absoluteExpiration)
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
            if (!this.RegisterMoniter(key, files))
            {
                return default(T);
            }
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

        public T GetOrAdd<T>(string key, Func<T> handler, System.DateTimeOffset absoluteExpiration, params string[] files)
        {
            T local;
            if (!this.RegisterMoniter(key, files))
            {
                return default(T);
            }
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

        public long Increment(string key, uint amount)
        {
            return (long)this.GetClient().Increment(key, 0L, (ulong)amount);
        }

        private bool RegisterMoniter(string key, params string[] dependentFiles)
        {
            if (this._isDisposed)
            {
                return false;
            }
            string moniterKey = key + "|" + string.Join("|", dependentFiles);
            if (!this._monitors.ContainsKey(moniterKey))
            {
                lock (this._monitors)
                {
                    if (!this._monitors.ContainsKey(moniterKey))
                    {
                        HostFileChangeMonitor moniter = new HostFileChangeMonitor(dependentFiles);
                        moniter.NotifyOnChanged(delegate(object state)
                        {
                            try
                            {
                                this.Remove(key);
                                moniter.Dispose();
                            }
                            catch
                            {
                            }
                            finally
                            {
                                this._monitors.Remove(moniterKey);
                                this.RegisterMoniter(key, dependentFiles);
                            }
                        });
                        this._monitors.Add(moniterKey, moniter);
                    }
                }
            }
            return true;
        }

        public bool Remove(string key)
        {
            return this.GetClient().Remove(key);
        }

        public void RemoveAll(System.Collections.Generic.IEnumerable<string> keys)
        {
            foreach (string str in keys)
            {
                this.GetClient().Remove(str);
            }
        }

        public void Set(string key, object data)
        {
            this.GetClient().Store(StoreMode.Set, key, data);
        }

        public void Set<T>(string key, T data)
        {
            this.GetClient().Store(StoreMode.Set, key, data);
        }

        public void Set(string key, object data, System.DateTimeOffset absoluteExpiration)
        {
            this.GetClient().Store(StoreMode.Set, key, data, (System.DateTime)absoluteExpiration.DateTime);
        }

        public void Set(string key, object data, params string[] dependentFiles)
        {
            if (this.RegisterMoniter(key, dependentFiles))
            {
                this.Set(key, data);
            }
        }

        public void Set<T>(string key, T data, System.DateTimeOffset absoluteExpiration)
        {
            this.GetClient().Store(StoreMode.Set, key, data, (System.DateTime)absoluteExpiration.DateTime);
        }

        public void Set<T>(string key, T data, params string[] dependentFiles)
        {
            if (this.RegisterMoniter(key, dependentFiles))
            {
                this.Set<T>(key, data);
            }
        }

        public void Set(string key, object data, System.DateTimeOffset absoluteExpiration, params string[] dependentFiles)
        {
            if (this.RegisterMoniter(key, dependentFiles))
            {
                this.Set(key, data, absoluteExpiration);
            }
        }

        public void Set<T>(string key, T data, System.DateTimeOffset absoluteExpiration, params string[] dependentFiles)
        {
            if (this.RegisterMoniter(key, dependentFiles))
            {
                this.Set<T>(key, data, absoluteExpiration);
            }
        }

        public bool TryGet<T>(string key, out T value)
        {
            try
            {
                object obj2;
                if (this.GetClient().TryGet(key, out obj2))
                {
                    value = (T)obj2;
                    return true;
                }
            }
            catch
            {
            }
            value = default(T);
            return false;
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
                            string sectionName = ConfigurationManager.AppSettings.Get("QL.Cache.Memcached.SectionName");
                            bool useObjectPool = Objects.As<bool>(ConfigurationManager.AppSettings.Get("QL.Cache.Memcached.UseObjectPool"), false);
                            _cacheInstance = new MemcachedCaching(sectionName, useObjectPool);
                        }
                    }
                }
                return _cacheInstance;
            }
        }
    }
}