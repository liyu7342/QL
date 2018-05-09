/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  DateTimes
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace QL.Core.Extensions
{
    /// <summary>
    /// 与日期相关的扩展函数
    /// </summary>
    public static class DateTimes
    {
        /// <summary>
        /// 用于计算时间戳的时间值
        /// </summary>
        private static DateTime UnixTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        /// <summary>
        /// 将当前时间的值转换为时间戳
        /// </summary>
        /// <param name="time">需要转换的时间</param>
        /// <returns>从1970-01-01 0:0:0开始计算的总毫秒数</returns>
        public static long ToTimestamp(this DateTime time)
        {
            return (long)time.ToUniversalTime().Subtract(UnixTimestamp).TotalMilliseconds;
        }

        /// <summary>
        /// 获取时间戳对应的本地时间
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime FromTimestamp(this long timestamp)
        {
            return UnixTimestamp.AddMilliseconds(timestamp).ToLocalTime();
        }

        /// <summary>
        /// 获取RFC822文档定义的时间格式字符串,如: Thu, 21 Dec 2000 16:01:07 +0800
        /// </summary>
        /// <param name="time">需要转换的时间</param>
        /// <returns>RFC822格式表示的时间字符串</returns>
        public static string ToRFC822Time(this DateTime time)
        {
            string rfcTime = time.ToString("ddd, dd MMM yyyy HH':'mm':'ss", CultureInfo.InvariantCulture);
            rfcTime += time.ToString(" zzz").Replace(":", "");
            return rfcTime;
        }
    }
}
