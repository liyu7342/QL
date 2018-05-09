/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  WebRequestObjectPool
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
    /// 基于ASP.NET Request请求上下文的对象池, 如果需要在每个Web请求结束自动回收对象池的对象数据,则需要在web.config文件里配置httpModules项.
    /// 如: "&lt;add name="WebRequestObjectPoolModule" type="QL.Core.ObjectPool.WebRequestObjectPoolModule" /&gt;"
    /// </summary>
    public class WebRequestObjectPool 
        : ObjectPoolBase
    {
        /// <summary>
        /// 实例化对象
        /// </summary>
        private WebRequestObjectPool()
        {
            if (this.HttpContext == null
                || this.HttpContext.ApplicationInstance == null)
            {
                throw new ApplicationException("当前执行环境不在WEB站点或Web应用程序下，无法实例化WebRequestObjectPool");
            }
            WebRequestObjectPool.EndRequest = this.WebRequestObjectPool_EndRequest;
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
            return this.HttpContext.Items[KEYNAME] as IObjectPool;
        }

        /// <summary>
        /// 设置对象池
        /// </summary>
        /// <param name="objectPool"></param>
        protected override void SetObjectPool(IObjectPool objectPool)
        {
            if (this.HttpContext == null) return;
            this.HttpContext.Items.Add(KEYNAME, objectPool);
        }
        /// <summary>
        /// 键值
        /// </summary>
        private string KEYNAME = "KTLIBRARY_CORE_OBJECTPOOL_WEBREQUESTOBJECTPOOL_KEYNAME";

        #region 实例对象
        static IObjectPool _Instance;
        static object _sycLock = new object();
        /// <summary>
        /// WebRequestObjectPool默认实例
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
                            _Instance = new WebRequestObjectPool();
                        }
                    }
                }
                return _Instance;
            }
        }
        #endregion


        #region 页面请求结束时调用的方法
        /// <summary>
        /// 页面请求结束时调用的方法
        /// </summary>
        internal static EventHandler EndRequest;

        /// <summary>
        /// WEB请求会话已结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebRequestObjectPool_EndRequest(object sender, EventArgs e)
        {
            IObjectPool pool = this.GetObjectPool();
            if (pool != null)
            {
                pool.Dispose();
                HttpContext.Items.Remove(KEYNAME);
            }
        }
        #endregion
    }

    /// <summary>
    /// 基于ASP.NET Request请求上下文的对象池的HttpModule，用于回收对象池数据
    /// </summary>
    public class WebRequestObjectPoolModule : IHttpModule
    {
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            context.EndRequest += (sender, e) =>
            {
                var hr = WebRequestObjectPool.EndRequest;
                if (hr != null) hr.Invoke(sender, e);
            };
        }
    }

}
