namespace QL.Caching.Memcached
{
    using QL.Core.Caching;
    using QL.Core.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    public class MemcachedCachingFactory : ICachingFactory
    {
        private System.Collections.Generic.Dictionary<string, ICaching> _cacheInstances = new System.Collections.Generic.Dictionary<string, ICaching>();

        public ICaching CreateCaching(string name)
        {
            ICaching caching;
            if (!this._cacheInstances.TryGetValue(name, out caching))
            {
                lock (this._cacheInstances)
                {
                    if (!this._cacheInstances.TryGetValue(name, out caching))
                    {
                        bool useObjectPool = Objects.As<bool>(ConfigurationManager.AppSettings.Get("QL.Cache.Memcached.UseObjectPool"), false);
                        caching = new MemcachedCaching(name, useObjectPool);
                        this._cacheInstances.Add(name, caching);
                    }
                }
            }
            return caching;
        }

        public ICaching Default
        {
            get
            {
                return MemcachedCaching.Default;
            }
        }
    }
}
