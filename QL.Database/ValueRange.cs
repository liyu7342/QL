/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  ValueRange
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QL.Database
{
    /// <summary>
    /// 值区间范围
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueRange<T>
        where T : struct
    {
        /// <summary>
        /// 实例化默认实例
        /// </summary>
        public ValueRange() { }
        /// <summary>
        /// 根据起始值与结束值实例化
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public ValueRange(T start, T end)
        {
            this.Start = start;
            this.End = end;
        }
    
        /// <summary>
        /// 起始值
        /// </summary>
        public T? Start { get; set; }
        /// <summary>
        /// 结束值
        /// </summary>
        public T? End { get; set; }

        /// <summary>
        /// 区间是否有值
        /// </summary>
        public bool HasValue
        {
            get
            {
                return Start.HasValue || End.HasValue;
            }
        }
    }
}
