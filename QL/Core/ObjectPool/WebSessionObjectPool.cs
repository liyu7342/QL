/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  WebSessionObjectPool
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace QL.Core.ObjectPool
{
    /// <summary>
    /// 基于ASP.NET Session会话期的对象池,如果需要自动回收对象池的对象数据,则需要在global.asax文件里的session_end事件里调用Dispose方法回收.
    /// </summary>
    public class WebSessionObjectPool : ObjectPoolBase
    {
        /// <summary>
        /// 实例化对象
        /// </summary>
        private WebSessionObjectPool()
        {
            if (this.HttpContext == null)
            {
                throw new ApplicationException("当前执行环境不在WEB站点或Web应用程序下，无法实例化WebSessionObjectPool");
            }
        }

        /// <summary>
        /// 当前的HTTP请求会话
        /// </summary>
        private HttpContext HttpContext
        {
            get
            {
                return HttpContext.Current;
            }
        }

        /// <summary>
        /// 返回对象池
        /// </summary>
        protected override IObjectPool GetObjectPool()
        {
            if (this.HttpContext == null) return null;
            return this.HttpContext.Session[KEYNAME] as IObjectPool;
        }

        /// <summary>
        /// 设置对象池
        /// </summary>
        /// <param name="objectPool"></param>
        protected override void SetObjectPool(IObjectPool objectPool)
        {
            if (this.HttpContext == null) return;
            this.HttpContext.Session[KEYNAME] = objectPool;
        }
        /// <summary>
        /// 键值
        /// </summary>
        private string KEYNAME = "KTLIBRARY_CORE_OBJECTPOOL_WEBSESSIONOBJECTPOOL_KEYNAME";

        #region 实例对象
        static IObjectPool _Instance;
        static object _sycLock = new object();
        /// <summary>
        /// WebSessionObjectPool默认实例
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
                            _Instance = new WebSessionObjectPool();
                        }
                    }
                }
                return _Instance;
            }
        }
        #endregion
    }
}
