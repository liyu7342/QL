/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  DateTimeRange
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QL.Database
{
    /// <summary>
    /// 日期/时间的值范围
    /// </summary>
    public class DateTimeRange
    {
        /// <summary>
        /// 实例化一个默认不包含时间部分的日期范围
        /// </summary>
        public DateTimeRange()
        {
            this.ContainTime = false;
            this.MinTimeAllowed = false;
            this.MaxTimeAllowed = false;
        }
        /// <summary>
        /// 根据是否包含时间条件实例化
        /// </summary>
        /// <param name="containTime">是否包含时间部分</param>
        public DateTimeRange(bool containTime)
            : this()
        {
            this.ContainTime = containTime;
        }
        /// <summary>
        /// 根据起始值与结束值实例化
        /// </summary>
        /// <param name="start">起始值</param>
        /// <param name="end">结束值</param>
        public DateTimeRange(DateTime start, DateTime end)
            : this(false, start, end)
        { }
        /// <summary>
        /// 根据起始值与结束值实例化
        /// </summary>
        /// <param name="containTime">是否包含时间部分</param>
        /// <param name="start">起始值</param>
        /// <param name="end">结束值</param>
        public DateTimeRange(bool containTime, DateTime start, DateTime end)
            : this(containTime)
        {
            this.Start = start;
            this.End = end;
        }
        /// <summary>
        /// 是否包含时间
        /// </summary>
        public bool ContainTime { get; private set; }

        /// <summary>
        /// 是否允许起始或结束值为DateTime.MinValue(默认不允许)。 
        /// 如果不允许则起始或结束值设置为DateTime.MinValue的话等于未设置值
        /// </summary>
        public bool MinTimeAllowed { get; set; }

        /// <summary>
        /// 是否允许起始或结束值为DateTime.MaxValue(默认不允许)。 
        /// 如果不允许则起始或结束值设置为DateTime.MaxValue的话等于未设置值
        /// </summary>
        public bool MaxTimeAllowed { get; set; }

        /// <summary>
        /// 起始值
        /// </summary>
        private DateTime? _Start;
        /// <summary>
        /// 结束值
        /// </summary>
        private DateTime? _End;

        /// <summary>
        /// 起始值
        /// </summary>
        public DateTime? Start
        {
            get
            {
                return _Start;
            }
            set
            {
                if (value.HasValue)
                {
                    if (!this.MaxTimeAllowed && value.Value == DateTime.MaxValue)
                    {
                        _Start = null;
                    }
                    else if (!this.MinTimeAllowed && value.Value == DateTime.MinValue)
                    {
                        _Start = null;
                    }
                    else
                    {
                        //如果不包含有时间，则表示时间是一天开始的那时刻，即凌晨0:0:0.0
                        _Start = this.ContainTime ? value : value.Value.Date;
                    }
                }
                else
                {
                    _Start = value;
                }
            }
        }

        /// <summary>
        /// 结束值
        /// </summary>
        public DateTime? End
        {
            get
            {
                return _End;
            }
            set
            {
                if (value.HasValue)
                {
                    if (!this.MaxTimeAllowed && value.Value == DateTime.MaxValue)
                    {
                        _End = null;
                    }
                    else if (!this.MinTimeAllowed && value.Value == DateTime.MinValue)
                    {
                        _End = null;
                    }
                    else
                    {
                        //如果不包含有时间，则表示时间是一天结束的那时刻，即晚上23:59:59.999
                        _End = this.ContainTime ? value : value.Value.Date.AddDays(1).AddMilliseconds(-1);
                    }
                }
                else
                {
                    _End = value;
                }
            }
        }

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
