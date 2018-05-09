/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  DictionaryObjectPool
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace QL.Core.ObjectPool
{
    /// <summary>
    /// 采用字典来存储对象的对象池
    /// </summary>
    public class DictionaryObjectPool : IObjectPool
    {
        /// <summary>
        /// 实例化(不区分大小写)
        /// </summary>
        public DictionaryObjectPool()
            : this(10, StringComparer.InvariantCultureIgnoreCase)
        {
        }
        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="comparer"></param>
        public DictionaryObjectPool(int capacity, IEqualityComparer comparer)
        {
            this.Pool = Hashtable.Synchronized(new Hashtable(capacity, comparer));
        }
        /// <summary>
        /// 
        /// </summary>
        private Hashtable Pool;

        #region IObjectPool 成员
        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="value"></param>
        public void Add(object key, object value)
        {
            if (this.Pool.ContainsKey(key))
            {
                this.Pool[key] = value;
            }
            else
            {
                this.Pool.Add(key, value);
            }
        }
        /// <summary>
        /// 判断是否存在某个对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(object key)
        {
            return this.Pool.ContainsKey(key);
        }
        /// <summary>
        /// 移除某个对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(object key)
        {
            bool r = this.Contains(key);
            if (r) this.Pool.Remove(key);
            return r;
        }
        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            this.Pool.Clear();
        }
        /// <summary>
        /// 返回某个对象,如果不存在则返回null
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(object key)
        {
            if (this.Contains(key)) return this.Pool[key];
            return null;
        }
        /// <summary>
        /// 返回对象池中已存储的对象数量
        /// </summary>
        public int Count
        {
            get
            {
                return this.Pool.Count;
            }
        }
        #endregion

        #region IDisposable 成员
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this.Pool.Count == 0) return;

            object[] values = new object[this.Pool.Count];
            this.Pool.Values.CopyTo(values, 0);
            this.Pool.Clear();

            foreach (var p in values)
            {
                if (p is IDisposable)
                {
                    ((IDisposable)p).Dispose();
                }
            }
        }
        #endregion

        #region 实例对象
        static IObjectPool _Instance;
        static object _sycLock = new object();
        /// <summary>
        /// DictionaryObjectPool 实例
        /// </summary>
        public static IObjectPool Default
        {
            get
            {
                if (_Instance == null)
                {
                    lock (_sycLock)
                    {
                        if (_Instance == null)
                        {
                            _Instance = new DictionaryObjectPool();
                        }
                    }
                }
                return _Instance;
            }
        }
        #endregion
    }
}
