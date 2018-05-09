/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  IObjectPool
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QL.Core.ObjectPool
{
    /// <summary>
    /// 对象池接口
    /// </summary>
    public interface IObjectPool : IDisposable
    {
        /// <summary>
        /// 添加对象，如果已存在key，则更新旧值
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="value"></param>
        void Add(object key, object value);

        /// <summary>
        /// 判断是否存在某个对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Contains(object key);

        /// <summary>
        /// 移除某个对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Remove(object key);

        /// <summary>
        /// 返回某个对象,如果不存在则返回null
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object Get(object key);

        /// <summary>
        /// 返回对象池中已存储的对象数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 清空对象池
        /// </summary>
        void Clear();
    }
}
