/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  ObjectPoolContext
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
    /// 对象池上下文
    /// </summary>
    public static class ObjectPoolContext
    {
        #region 实例对象
        /// <summary>
        /// 当前上下文的 IObjectPool 实例
        /// </summary>
        public static IObjectPool Current
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return WebRequestObjectPool.Instance;
                }
                else
                {
                    return ThreadCallObjectPool.Instance;
                }
            }
        }
        #endregion
    }
}
