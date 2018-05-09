/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  AjaxHandlerMethodReturnType
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QL.Web
{
    /// <summary>
    /// Ajax委托处理方法的返回数据类型
    /// </summary>
    public enum AjaxHandlerMethodReturnType
    {
        /// <summary>
        /// 自动处理（如果方法返回非null值则按Json数据处理，否则按空返回处理）
        /// </summary>
        Auto,
        /// <summary>
        /// 空返回(数据的输出由方法内部实现)
        /// </summary>
        Void,
        /// <summary>
        /// 返回文本数据(直接将返回结果当像文本数据输出)
        /// </summary>
        Text,
        /// <summary>
        /// 返回JSON对象(返回结果对象将被序列化为JSON格式字符串输出)
        /// </summary>
        Json
    }
}
