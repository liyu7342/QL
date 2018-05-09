/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  AjaxHandlerMethodAttribute
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QL.Web
{
    /// <summary>
    /// Ajax委托处理方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class AjaxHandlerMethodAttribute : Attribute
    {
        /// <summary>
        /// 构造AjaxHandlerMethodAttribute实例
        /// </summary>
        public AjaxHandlerMethodAttribute() : this(null)
        {}
        /// <summary>
        /// 根据方法名称实例化
        /// </summary>
        /// <param name="name"></param>
        public AjaxHandlerMethodAttribute(string name)
        {
            this.Name = name;
            this.IsTerminative = true;
            this.AllowBrowserCache = false;
            this.ReturnType = AjaxHandlerMethodReturnType.Auto;
        }

        /// <summary>
        /// 委托名称, 如果为null或空值则对应方法名。
        /// 此参数对应于客户端提交的ajaxhandler参数值
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// 设置输出内容的类型.如text/xml; text/plain; text/html; application/x-javascript等等.
        /// 如果为空则根据ReturnType自动处理, ReturnType = Text 输出 text/plain, ReturnType = JSON 输出 application/x-javascript
        /// </summary>
        public string ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// 是否允许浏览器缓存数据.默认为false(不允许缓存数据)
        /// </summary>
        public bool AllowBrowserCache
        {
            get;
            set;
        }

        /// <summary>
        /// 是否终止页面执行,默认是终止
        /// </summary>
        public bool IsTerminative
        {
            get;
            set;
        }

        /// <summary>
        /// 方法返回类型.默认是自动(Auto)
        /// </summary>
        public AjaxHandlerMethodReturnType ReturnType { get; set; }

    }
}
