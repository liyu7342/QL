/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  ObjectPoolBase
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QL.Core.ObjectPool
{
    /// <summary>
    /// 对象池的基类
    /// </summary>
    public abstract class ObjectPoolBase : IObjectPool
    {
        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <returns></returns>
        protected virtual IObjectPool CreateObjectPool()
        {
            return new DictionaryObjectPool();
        }

        /// <summary>
        /// 获取对象池
        /// </summary>
        protected abstract IObjectPool GetObjectPool();

        /// <summary>
        /// 设置对象池
        /// </summary>
        /// <param name="objectPool"></param>
        protected abstract void SetObjectPool(IObjectPool objectPool);

        #region IObjectPool 成员
        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="value"></param>
        public virtual void Add(object key, object value)
        {
            IObjectPool pool = this.GetObjectPool();
            if (pool == null)
            {
                lock (this)
                {
                    pool = this.GetObjectPool();
                    if (pool == null)
                    {
                        pool = this.CreateObjectPool();
                        this.SetObjectPool(pool);
                    }
                }
            }
            pool.Add(key, value);
        }
        /// <summary>
        /// 判断是否存在某个对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(object key)
        {
            IObjectPool pool = this.GetObjectPool();
            return pool == null ? false : pool.Contains(key);
        }
        /// <summary>
        /// 移除某个对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(object key)
        {
            IObjectPool pool = this.GetObjectPool();
            if (pool != null)
            {
                return pool.Remove(key);
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 返回某个对象,如果不存在则返回null
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(object key)
        {
            IObjectPool pool = this.GetObjectPool();
            return pool == null ? null : pool.Get(key);
        }

        /// <summary>
        /// 返回对象池中已存储的对象数量
        /// </summary>
        public int Count
        {
            get
            {
                IObjectPool pool = this.GetObjectPool();
                return pool == null ? 0 : pool.Count;
            }
        }
        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            IObjectPool pool = this.GetObjectPool();
            if (pool != null) pool.Clear();
        }
        #endregion

        #region IDisposable 成员
        /// <summary>
        /// 释放内存资源
        /// </summary>
        public virtual void Dispose()
        {
            IObjectPool pool = this.GetObjectPool();
            if (pool != null) pool.Dispose();
        }
        #endregion
    }
}
