namespace QL.Caching.LDCache
{
    using QL.Core;
    using QL.Core.Caching;
    using QL.Core.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Runtime.InteropServices;

    public class LDCaching : ICaching, IDisposable
    {
        private static ICaching _cacheInstance = null;
        private static ICachingFactory _cachingFactory = null;
        private static object _dcacheLock = new object();
        private ICaching _dCaching;
        private static object _initLock = new object();
        private bool _isDisposed;
        private int _localCacheTimeout;
        private MemoryCaching _memoryCaching;
        private Dictionary<string, HostFileChangeMonitor> _monitors;
        private string _name;
        private static int LDCacheTimeout = ConfigurationManager.AppSettings["QL.Cache.LDCache.Timeout"].As<int>(2);

        public LDCaching()
            : this(null, LDCacheTimeout, GetDCachingFactory())
        {
        }

        public LDCaching(string name)
            : this(name, LDCacheTimeout, GetDCachingFactory())
        {
        }

        public LDCaching(string name, int localCacheTimeout, ICachingFactory dCachingFactory)
        {
            this._localCacheTimeout = 2;
            this._name = name;
            this._localCacheTimeout = localCacheTimeout;
            this._memoryCaching = string.IsNullOrEmpty(name) ? new MemoryCaching() : new MemoryCaching(name);
            this._dCaching = string.IsNullOrEmpty(name) ? dCachingFactory.Default : dCachingFactory.CreateCaching(name);
            this._monitors = new Dictionary<string, HostFileChangeMonitor>(100, StringComparer.InvariantCultureIgnoreCase);
        }

        public long Decrement(string key, uint amount)
        {
            return this._dCaching.Decrement(key, amount);
        }

        public void Dispose()
        {
            if (!this._isDisposed)
            {
                foreach (KeyValuePair<string, HostFileChangeMonitor> pair in this._monitors)
                {
                    pair.Value.Dispose();
                }
                this._monitors.Clear();
                this._dCaching.Dispose();
                this._memoryCaching.Dispose();
                this._isDisposed = true;
            }
        }

        public bool Exists(string key)
        {
            if (!this._memoryCaching.Exists(key))
            {
                return this._dCaching.Exists(key);
            }
            return true;
        }

        public object Get(string key)
        {
            if (this._memoryCaching.Exists(key))
            {
                return this._memoryCaching.Get(key);
            }
            object data = this._dCaching.Get(key);
            if (data != null)
            {
                this.SetLocal(key, data);
            }
            return data;
        }

        public T Get<T>(string key)
        {
            if (this._memoryCaching.Exists(key))
            {
                return this._memoryCaching.Get<T>(key);
            }
            T objA = this._dCaching.Get<T>(key);
            if (!object.Equals(objA, default(T)))
            {
                this.SetLocal(key, objA);
            }
            return objA;
        }

        public IDictionary<string, object> GetAll(string[] keys)
        {
            if (!this._memoryCaching.Exists(keys.First<string>()))
            {
                return this._dCaching.GetAll(keys);
            }
            return this._memoryCaching.GetAll(keys);
        }

        public IDictionary<string, T> GetAll<T>(string[] keys)
        {
            if (!this._memoryCaching.Exists(keys.First<string>()))
            {
                return this._dCaching.GetAll<T>(keys);
            }
            return this._memoryCaching.GetAll<T>(keys);
        }

        private static ICachingFactory GetDCachingFactory()
        {
            if (_cachingFactory == null)
            {
                lock (_dcacheLock)
                {
                    if (_cachingFactory == null)
                    {
                        string instance = ConfigurationManager.AppSettings["QL.Cache.LDCache.DCFactory"];
                        _cachingFactory = Utility.CreateInstance<ICachingFactory>(instance) ?? MemoryCachingFactory.Instance;
                    }
                }
            }
            return _cachingFactory;
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

        public T GetOrAdd<T>(string key, Func<T> handler, DateTimeOffset absoluteExpiration, params string[] files)
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
            return this._dCaching.Increment(key, amount);
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
            this._memoryCaching.Remove(key);
            return this._dCaching.Remove(key);
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            this._memoryCaching.RemoveAll(keys);
            this._dCaching.RemoveAll(keys);
        }

        public void Set(string key, object data)
        {
            this.SetLocal(key, data);
            this._dCaching.Set(key, data);
        }

        public void Set<T>(string key, T data)
        {
            this.SetLocal(key, data);
            this._dCaching.Set<T>(key, data);
        }

        public void Set(string key, object data, DateTimeOffset absoluteExpiration)
        {
            this.SetLocal(key, data);
            this._dCaching.Set(key, data, absoluteExpiration);
        }

        public void Set(string key, object data, params string[] dependentFiles)
        {
            if (this.RegisterMoniter(key, dependentFiles))
            {
                this.Set(key, data);
            }
        }

        public void Set<T>(string key, T data, DateTimeOffset absoluteExpiration)
        {
            this.SetLocal(key, data);
            this._dCaching.Set<T>(key, data, absoluteExpiration);
        }

        public void Set<T>(string key, T data, params string[] dependentFiles)
        {
            if (this.RegisterMoniter(key, dependentFiles))
            {
                this.Set<T>(key, data);
            }
        }

        public void Set(string key, object data, DateTimeOffset absoluteExpiration, params string[] dependentFiles)
        {
            if (this.RegisterMoniter(key, dependentFiles))
            {
                this.Set(key, data, absoluteExpiration);
            }
        }

        public void Set<T>(string key, T data, DateTimeOffset absoluteExpiration, params string[] dependentFiles)
        {
            if (this.RegisterMoniter(key, dependentFiles))
            {
                this.Set<T>(key, data, absoluteExpiration);
            }
        }

        private void SetLocal(string key, object data)
        {
            this._memoryCaching.Set(key, data, DateTime.Now.AddSeconds((double)this._localCacheTimeout));
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (this._memoryCaching.Exists(key))
            {
                return this._memoryCaching.TryGet<T>(key, out value);
            }
            bool flag = this._dCaching.TryGet<T>(key, out value);
            if (flag)
            {
                this.SetLocal(key, (T)value);
            }
            return flag;
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
                            string name = ConfigurationManager.AppSettings["QL.Cache.LDCache.Name"];
                            _cacheInstance = new LDCaching(name);
                        }
                    }
                }
                return _cacheInstance;
            }
        }
    }
}