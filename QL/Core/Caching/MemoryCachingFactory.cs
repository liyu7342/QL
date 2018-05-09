namespace QL.Core.Caching
{
    using System;
    using System.Collections.Generic;

    public class MemoryCachingFactory : ICachingFactory
    {
        private Dictionary<string, ICaching> _cacheInstances = new Dictionary<string, ICaching>();
        public static readonly ICachingFactory Instance = new MemoryCachingFactory();

        public ICaching CreateCaching(string name)
        {
            ICaching caching;
            if (!this._cacheInstances.TryGetValue(name, out caching))
            {
                lock (this._cacheInstances)
                {
                    if (!this._cacheInstances.TryGetValue(name, out caching))
                    {
                        caching = new MemoryCaching(name);
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
                return MemoryCaching.Default;
            }
        }
    }
}
