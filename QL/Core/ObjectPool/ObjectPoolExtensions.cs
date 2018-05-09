using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QL.Core.ObjectPool
{
    /// <summary>
    /// 对象池的扩展方法
    /// </summary>
    public static class ObjectPoolExtensions
    {
        /// <summary>
        /// 返回某个对象,如果不存在则返回参数value
        /// </summary>
        /// <param name="pool">对象池</param>
        /// <param name="key">键值</param>
        /// <param name="value">如果对象池不存在对应键的数据，则返回此值</param>
        /// <returns></returns>
        public static object GetOrAdd(this IObjectPool pool, object key, object value)
        {
            if (!pool.Contains(key))
            {
                return value;
            }
            else
            {
                return pool.Get(key);
            }
        }
        /// <summary>
        /// 返回某个对象,如果不存在则调用函数委托获取值,并将值存入对象池后返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="key">键值</param>
        /// <param name="func">如果对象池不存在对应键的数据，则从此函数委托获取新值并存入对象池后返回</param>
        /// <returns></returns>
        public static T GetOrAdd<T>(this IObjectPool pool, object key, Func<T> func)
        {
            if (!pool.Contains(key))
            {
                T value = func.Invoke();
                pool.Add(key, value);
                return value;
            }
            else
            {
                return (T)pool.Get(key);
            }
        }
    }
}
