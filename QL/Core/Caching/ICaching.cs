namespace QL.Core.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public interface ICaching : IDisposable
    {
        long Decrement(string key, uint amount);
        bool Exists(string key);
        object Get(string key);
        T Get<T>(string key);
        IDictionary<string, object> GetAll(string[] keys);
        IDictionary<string, T> GetAll<T>(string[] keys);
        T GetOrAdd<T>(string key, Func<T> handler);
        T GetOrAdd<T>(string key, Func<T> handler, DateTimeOffset absoluteExpiration);
        T GetOrAdd<T>(string key, Func<T> handler, params string[] files);
        T GetOrAdd<T>(string key, Func<T> handler, DateTimeOffset absoluteExpiration, params string[] files);
        long Increment(string key, uint amount);
        bool Remove(string key);
        void RemoveAll(IEnumerable<string> keys);
        void Set(string key, object data);
        void Set<T>(string key, T data);
        void Set(string key, object data, DateTimeOffset absoluteExpiration);
        void Set(string key, object data, params string[] dependentFiles);
        void Set<T>(string key, T data, DateTimeOffset absoluteExpiration);
        void Set<T>(string key, T data, params string[] dependentFiles);
        void Set(string key, object data, DateTimeOffset absoluteExpiration, params string[] dependentFiles);
        void Set<T>(string key, T data, DateTimeOffset absoluteExpiration, params string[] dependentFiles);
        bool TryGet<T>(string key, out T value);
    }
}
