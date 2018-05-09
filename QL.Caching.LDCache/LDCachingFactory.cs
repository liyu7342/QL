namespace QL.Caching.LDCache
{
    using QL.Core.Caching;
    using System;
    using System.Collections.Generic;

    public class LDCachingFactory : ICachingFactory
    {
        private Dictionary<string, ICaching> _cacheInstances = new Dictionary<string, ICaching>();

        public ICaching CreateCaching(string name)
        {
            ICaching caching;
            if (!this._cacheInstances.TryGetValue(name, out caching))
            {
                lock (this._cacheInstances)
                {
                    if (!this._cacheInstances.TryGetValue(name, out caching))
                    {
                        caching = new LDCaching(name);
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
                return LDCaching.Default;
            }
        }
    }
}
