namespace QL.Core
{
    using QL.Core.Caching;
    using System;
    using System.Configuration;

    public static class Cache
    {
        private static ICachingFactory _cachingFactory = null;
        private static object _initLock = new object();

        public static void Add(string key, object data)
        {
            Current.Set(key, data);
        }

        public static void Add<T>(string key, T data)
        {
            Current.Set<T>(key, data);
        }

        public static void Add(string key, object data, DateTimeOffset absoluteExpiration)
        {
            Current.Set(key, data, absoluteExpiration);
        }

        public static void Add(string key, object data, params string[] files)
        {
            Current.Set(key, data, files);
        }

        public static void Add<T>(string key, T data, DateTimeOffset absoluteExpiration)
        {
            Current.Set<T>(key, data, absoluteExpiration);
        }

        public static void Add<T>(string key, T data, params string[] files)
        {
            Current.Set<T>(key, data, files);
        }

        public static void Add(string key, object data, DateTimeOffset absoluteExpiration, params string[] files)
        {
            Current.Set(key, data, absoluteExpiration, files);
        }

        public static void Add<T>(string key, T data, DateTimeOffset absoluteExpiration, params string[] files)
        {
            Current.Set<T>(key, data, absoluteExpiration, files);
        }

        public static ICaching CreateCaching(string name)
        {
            return GetCachingFactory().CreateCaching(name);
        }

        public static bool Exists(string key)
        {
            return Current.Exists(key);
        }

        public static object Get(string key)
        {
            return Current.Get(key);
        }

        public static T Get<T>(string key)
        {
            return Current.Get<T>(key);
        }

        private static ICachingFactory GetCachingFactory()
        {
            if (_cachingFactory == null)
            {
                lock (_initLock)
                {
                    if (_cachingFactory == null)
                    {
                        string instance = ConfigurationManager.AppSettings["QL.Cache.Factory"];
                        _cachingFactory = Utility.CreateInstance<ICachingFactory>(instance) ?? MemoryCachingFactory.Instance;
                    }
                }
            }
            return _cachingFactory;
        }

        public static T GetOrAdd<T>(string key, Func<T> handler)
        {
            return Current.GetOrAdd<T>(key, handler);
        }

        public static T GetOrAdd<T>(string key, Func<T> handler, DateTimeOffset absoluteExpiration)
        {
            return Current.GetOrAdd<T>(key, handler, absoluteExpiration);
        }

        public static T GetOrAdd<T>(string key, Func<T> handler, params string[] files)
        {
            return Current.GetOrAdd<T>(key, handler, files);
        }

        public static T GetOrAdd<T>(string key, Func<T> handler, DateTimeOffset absoluteExpiration, params string[] files)
        {
            return Current.GetOrAdd<T>(key, handler, absoluteExpiration, files);
        }

        public static void Remove(string key)
        {
            Current.Remove(key);
        }

        public static ICaching Current
        {
            get
            {
                return GetCachingFactory().Default;
            }
        }
    }
}