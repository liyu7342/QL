/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  MethodCallObjectPool
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Messaging;

namespace QL.Core.ObjectPool
{
    /// <summary>
    /// 基于线程调用的对象池
    /// </summary>
    public class ThreadCallObjectPool : ObjectPoolBase
    {
        /// <summary>
        /// 实例化对象
        /// </summary>
        private ThreadCallObjectPool()
        {

        }
        /// <summary>
        /// 返回对象池
        /// </summary>
        protected override IObjectPool GetObjectPool()
        {
            return ThreadCallObjectPool.ObjectPool;
        }

        /// <summary>
        /// 设置对象池
        /// </summary>
        /// <param name="objectPool"></param>
        protected override void SetObjectPool(IObjectPool objectPool)
        {
            ThreadCallObjectPool.ObjectPool = objectPool;
        }

        /// <summary>
        /// 
        /// </summary>
        [ThreadStatic]
        private static IObjectPool ObjectPool;

        #region 实例对象
        static IObjectPool _Instance;
        static object _sycLock = new object();
        /// <summary>
        /// MethodCallObjectPool默认实例
        /// </summary>
        public static IObjectPool Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (_sycLock)
                    {
                        if (_Instance == null)
                        {
                            _Instance = new ThreadCallObjectPool();
                        }
                    }
                }
                return _Instance;
            }
        }
        #endregion
    }
}
