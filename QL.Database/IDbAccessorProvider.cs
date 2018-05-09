/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  IDbAccessorProvider
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QL.Database
{
    /// <summary>
    /// 数据访问者驱动
    /// </summary>
    public interface IDbAccessorProvider : IDisposable
    {
        /// <summary>
        /// 返回数据库帮助对象实例
        /// </summary>
        DbHelper DbHelper { get; }
    }
}
