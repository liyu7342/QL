/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  WebApplicationObjectPool
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
    /// 基于ASP.NET 应用程序的对象池
    /// </summary>
    public class WebApplicationObjectPool : ObjectPoolBase
    {
        /// <summary>
        /// 实例化对象
        /// </summary>
        private WebApplicationObjectPool()
        {
            if (this.HttpContext == null)
            {
                throw new ApplicationException("当前执行环境不在WEB站点或Web应用程序下，无法实例化WebApplicationObjectPool");
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
        /// 返回DictionaryObjectPool对象
        /// </summary>
        protected override IObjectPool GetObjectPool()
        {
            if (this.HttpContext == null) return null;
            var application = this.HttpContext.Application;
            application.Lock();
            var item = application[KEYNAME] as IObjectPool;
            application.UnLock();
            return item;
        }

        /// <summary>
        /// 设置对象池
        /// </summary>
        /// <param name="objectPool"></param>
        protected override void SetObjectPool(IObjectPool objectPool)
        {
            if (this.HttpContext == null) return;
            var application = this.HttpContext.Application;
            application.Lock();
            application[KEYNAME] = objectPool;
            application.UnLock();
        }
        /// <summary>
        /// 键值
        /// </summary>
        private string KEYNAME = "KTLIBRARY_CORE_OBJECTPOOL_WEBAPPLICATIONOBJECTPOOL_KEYNAME";

        #region 实例对象
        static IObjectPool _Instance;
        static object _sycLock = new object();
        /// <summary>
        /// WebApplicationObjectPool默认实例
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
                            _Instance = new WebApplicationObjectPool();
                        }
                    }
                }
                return _Instance;
            }
        }
        #endregion
    }
}
