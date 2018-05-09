/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  Strings
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;
using System.Web;
using System.Globalization;

namespace QL.Core.Extensions
{
    /// <summary>
    /// 与字符串相关的扩展方法
    /// </summary>
    public static class Strings
    {
        #region 加密签名函数块
        /// <summary>
        /// 采用UTF-8编码求此字符串的MD5值
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns>32位长度的MD5哈希值（小写字母）</returns>
        public static string MD5(this string text)
        {
            return MD5(text, Encoding.UTF8);
        }
        /// <summary>
        /// 根据对应的字符编码求此字符串的MD5值
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="encoding">字符编码</param>
        /// <returns>32位长度的MD5哈希值（小写字母）</returns>
        public static string MD5(this string text, Encoding encoding)
        {
            if (text == null) throw new ArgumentNullException("text");

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            byte[] output = md5.ComputeHash(encoding.GetBytes(text));

            md5.Clear();

            StringBuilder code = new StringBuilder();
            for (int i = 0; i < output.Length; i++)
            {
                code.Append(output[i].ToString("x2"));
            }
            return code.ToString();
        }
        #endregion

        #region 数据判断函数块
        /// <summary>
        /// 如果字符串值为null或空字符串，则返回<paramref name="replacement"/>替换值
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="replacement">替换值</param>
        /// <returns></returns>
        public static string IfEmpty(this string text, string replacement)
        {
            return string.IsNullOrEmpty(text) ? replacement : text;
        }
        /// <summary>
        /// 如果字符串值为null或空字符串，则调用<paramref name="func"/>函数并返回<paramref name="func"/>函数返回值
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static string IfEmpty(this string text, Func<string> func)
        {
            return string.IsNullOrEmpty(text) ? func.Invoke() : text;
        }
        /// <summary>
        /// 判断此字符串是否由数字(1~9数字)组成
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns></returns>
        public static bool IsInteger(this string text)
        {
            return IsMatch(text, @"^\d+$");
        }
        /// <summary>
        /// 判断此字符串是否是日期时间格式
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns></returns>
        public static bool IsDateTime(this string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            DateTime dt;
            return DateTime.TryParse(text, out dt);
        }
        /// <summary>
        /// 判断此字符串是否是正确的Email格式
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns></returns>
        public static bool IsValidEmail(this string text)
        {
            return IsMatch(text, @"^\w+([\-_+\.]\w+)*@\w+([\-_\.]\w+)*\.\w+([\-_\.]\w+)*$");
        }
        /// <summary>
        /// 判断此字符串是否符合某种格式
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="pattern">用于匹配的正则表达式</param>
        /// <returns></returns>
        public static bool IsMatch(this string text, string pattern)
        {
            return IsMatch(text, pattern, RegexOptions.None);
        }
        /// <summary>
        /// 判断此字符串是否符合某种格式
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="pattern">用于匹配的正则表达式</param>
        /// <param name="options">表达式选项</param>
        /// <returns></returns>
        public static bool IsMatch(this string text, string pattern, RegexOptions options)
        {
            if (text == null) return false;
            return Regex.IsMatch(text, pattern, options);
        }
        /// <summary>
        /// 判断字符串是否包含某个分隔项，如“A,B,C”字符串包含有“C”项但没有“D”项
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="item">判断是否包含的分隔项</param>
        /// <param name="separator">分隔符，如“,”</param>
        /// <returns>如果<paramref name="text"/>包含有<paramref name="item"/>则返回true，否则false</returns>
        /// <example>
        /// <code>
        /// string text = "a,b,c,d";
        /// Console.Write(text.IsContain("b",","));   //true
        /// Console.Write(text.IsContain("e",","));   //false
        /// Console.Write(text.IsContain("A",","));   //false
        /// </code>
        /// </example>
        public static bool IsContain(this string text, string item, string separator)
        {
            return IsContain(text, item, separator, false);
        }
        /// <summary>
        /// 判断字符串是否包含某个分隔项，如“A,B,C”字符串包含有“C”项但没有“D”项
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="item">判断是否包含的分隔项</param>
        /// <param name="separator">分隔符，如“,”</param>
        /// <param name="ignoreCase">是否不区分大小写</param>
        /// <returns>如果<paramref name="text"/>包含有<paramref name="item"/>则返回true，否则false</returns>
        /// <example>
        /// <code>
        /// string text = "a,b,c,d";
        /// Console.Write(text.IsContain("b",",", true));   //true
        /// Console.Write(text.IsContain("e",",", true));   //false
        /// Console.Write(text.IsContain("A",",", true));   //true
        /// Console.Write(text.IsContain("A",",", false));  //false
        /// </code>
        /// </example>
        public static bool IsContain(this string text, string item, string separator, bool ignoreCase)
        {
            if (text == null && item == null) return true;
            if (text == string.Empty && item == string.Empty) return true;
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(item)) return false;

            StringComparison sc = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (string.IsNullOrEmpty(separator)) return text.Equals(item, sc);

            int s = 0;
            int p;
            do
            {
                p = text.IndexOf(separator, s, sc);
                if (p != -1)
                {
                    if ((p - s) == item.Length)
                    {
                        if (item.Equals(text.Substring(s, item.Length), sc)) return true;
                    }
                    s = p + 1;
                }
                else if((text.Length - s) == item.Length)
                {
                    if (item.Equals(text.Substring(s), sc)) return true;
                }

            } while (p != -1 && s < text.Length);
            return false;
        }
        #endregion

        #region 字符串循环处理
        /// <summary>
        /// 对所有分隔项进行循环处理
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="separator">分隔项之间的分隔符，如果为null或空则表示对每个字符处理</param>
        /// <param name="action">对分隔项进行操作的动作函数。第一个参数表示第几项分隔项(从1开始数起)，第二个参数表示分隔项的值</param>
        public static void Each(this string text, string separator, Action<int, string> action)
        {
            text.Each(separator, false, -1, action);
        }
        /// <summary>
        /// 对所有分隔项进行循环处理
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="separator">分隔项之间的分隔符，如果为null或空则表示对每个字符处理</param>
        /// <param name="ignoreCase">对分隔符的查找是否不区分大小写处理</param>
        /// <param name="action">对分隔项进行操作的动作函数。第一个参数表示第几项分隔项(从1开始数起)，第二个参数表示分隔项的值</param>
        public static void Each(this string text, string separator, bool ignoreCase, Action<int, string> action)
        {
            text.Each(separator, ignoreCase, -1, action);
        }
        /// <summary>
        /// 对所有分隔项进行循环处理
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="separator">分隔项之间的分隔符，如果为null或空则表示对每个字符处理</param>
        /// <param name="count">指定的分隔项的最大数量，如果值小于0则表示不限制</param>
        /// <param name="action">对分隔项进行操作的动作函数。第一个参数表示第几项分隔项(从1开始数起)，第二个参数表示分隔项的值</param>
        public static void Each(this string text, string separator, int count, Action<int, string> action)
        {
            text.Each(separator, false, count, action);
        }
        /// <summary>
        /// 对所有分隔项进行循环处理
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="separator">分隔项之间的分隔符，如果为null或空则表示对每个字符处理</param>
        /// <param name="ignoreCase">对分隔符的查找是否不区分大小写处理</param>
        /// <param name="count">指定的分隔项的最大数量，如果值小于0则表示不限制</param>
        /// <param name="action">对分隔项进行操作的动作函数。第一个参数表示第几项分隔项(从1开始数起)，第二个参数表示分隔项的值</param>
        public static void Each(this string text, string separator, bool ignoreCase, int count, Action<int, string> action)
        {
            if (action == null || string.IsNullOrEmpty(text)) return;
            if (count == 0) return;

            bool es = string.IsNullOrEmpty(separator);

            StringComparison sc = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            int s = 0;
            int p;
            int index = 0;
            do
            {
                if (count > 0)
                {
                    count--;
                }
                if (count == 0)
                {
                    break;
                }
                if (es)
                {
                    p = s + 1;
                }
                else
                {
                    p = text.IndexOf(separator, s, sc);
                }
                if (p != -1)
                {
                    action(++index, text.Substring(s, p - s));
                    if (es)
                    {
                        s = p;
                    }
                    else
                    {
                        s = p + 1;
                    }
                }
            } while (p != -1 && s < text.Length);
            if (s < text.Length)
            {
                action(++index, s == 0 ? text : text.Substring(s));
            }
        }
        #endregion

        #region 字符替换函数块

        /// <summary>
        /// 替换字符
        /// </summary>
        /// <param name="text">要替换的内容</param>
        /// <param name="find">要查找的内容</param>
        /// <param name="replacement">替换查找的内容字符</param>
        /// <param name="ignoreCase">是否不区分大小写</param>
        /// <returns>替换后的数据</returns>
        /// <exception cref="System.ArgumentNullException">replacement为null时抛出</exception>
        public static string Replace(this string text, string find, string replacement, bool ignoreCase)
        {
            if (text == null || find == null) return text;
            if (text.Length < 1 || find.Length < 1) return text;
            if (replacement == null) throw new ArgumentNullException("replacement");

            return Regex.Replace(text, Regex.Escape(find), replacement, (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None));
        }
        /// <summary>
        /// 替换数据,前缀与后缀是不区分大小写. 
        /// 如: key="name", value="小王", prefix="{", suffix="}". 如果text="用户名:{name}"则替换后的数据为"用户名:小王"
        /// </summary>
        /// <param name="text">要替换的内容</param>
        /// <param name="replacement">替换查找的数据.key值类型需要是字符串, key=需要替换的数据, value=用于替换的数据</param>
        /// <param name="prefix">前缀,如"{"</param>
        /// <param name="suffix">后缀,如"}"</param>
        /// <returns>替换后的数据</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //UBB转换示例代码
        /// string text = "[b]粗体字[/b][i]斜体字[/i]";
        /// Dictionary<string, string> items = new Dictionary<string, string>();
        /// items.Add("b", "<strong>");
        /// items.Add("/b", "</strong>");
        /// items.Add("i", "<i>");
        /// items.Add("i", "</i>");
        /// string content = text.Replace(items, "[", "]");  //content = "<strong>粗体字</strong><i>斜体字</i>
        /// ]]>
        /// </code>
        /// </example>
        public static string Replace(this string text, IDictionary replacement, string prefix, string suffix)
        {
            return Replace(text, replacement, prefix, suffix, true, true);
        }
        /// <summary>
        /// 替换数据, 前缀与后缀是不区分大小写
        /// 如: key="name", value="小王", prefix="{", suffix="}". 则如果text="用户名:{name}"则替换后的数据为"用户名:小王"
        /// </summary>
        /// <param name="text">要替换的内容</param>
        /// <param name="replacement">替换查找的数据.key值类型需要是字符串, key=需要替换的数据, value=用于替换的数据</param>
        /// <param name="prefix">前缀,如"{"</param>
        /// <param name="suffix">后缀,如"}"</param>
        /// <param name="removeEmpty">是否需要清除空项.即是不存在的项</param>
        /// <returns>替换后的数据</returns>
        public static string Replace(this string text, IDictionary replacement, string prefix, string suffix, bool removeEmpty)
        {
            return Replace(text, replacement, prefix, suffix, true, removeEmpty);
        }
        /// <summary>
        /// 替换数据. 如: key="name", value="小王", prefix="{", suffix="}". 如果text="用户名:{name}"则替换后的数据为"用户名:小王"
        /// </summary>
        /// <param name="text">要替换的内容</param>
        /// <param name="replacement">替换查找的数据.key值类型需要是字符串, key=需要替换的数据, value=用于替换的数据</param>
        /// <param name="prefix">前缀,如"{"</param>
        /// <param name="suffix">后缀,如"}"</param>
        /// <param name="ignoreCase">前缀与后缀的查询是否忽略大小写</param>
        /// <param name="removeEmpty">是否需要清除空项.即是不存在的项</param>
        /// <remarks>如果前缀或后缀有一项为空则忽略前后缀，只根据replacement里的key值替换</remarks>
        /// <returns>替换后的数据</returns>
        public static string Replace(this string text, IDictionary replacement, string prefix, string suffix, bool ignoreCase, bool removeEmpty)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(suffix))
            {
                foreach (var p in replacement.Keys)
                {
                    string key = p.ToString("");
                    string value = replacement[p].ToString("");
                    if (!string.IsNullOrEmpty(key))
                        text = Replace(text, key, value, true);
                }
            }
            else
            {
                text = Replace(text, key =>
                {
                    if (replacement.Contains(key))
                    {
                        return replacement[key].ToString("");
                    }
                    else
                    {
                        return removeEmpty ? "" : null;
                    }
                }, prefix, suffix, ignoreCase);
            }
            return text;
        }

        /// <summary>
        /// 替换数据. 前缀与后缀是不区分大小写
        /// </summary>
        /// <param name="text">要替换的内容</param>
        /// <param name="replacement">返回最终替换数据的委托. 如果委托返回null则表示不替换,否则替换数据</param>
        /// <param name="prefix">前缀,如"{"</param>
        /// <param name="suffix">后缀,如"}"</param>
        /// <exception cref="System.ArgumentException">prefix或suffix为空</exception>
        /// <returns>替换后的数据</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //UBB转换示例
        /// string text = "[b]粗体[/b][i]斜体[/i][red]红色[/red]";
        /// text = text.Replace(key=>{
        ///     switch(key.ToLower()){
        ///         case "b":
        ///             return "<strong>";
        ///         case "/b":
        ///             return "</strong>";
        ///         case "i":
        ///             return "<i>";
        ///         case "/i":
        ///             return "</i>";
        ///         case "red":
        ///             return "<font color=\"red\">";
        ///         case "/red":
        ///             return "</font>";
        ///         default:
        ///             return null;
        ///     }
        /// },"[","]");
        /// Console.WriteLine(text);
        /// ]]>
        /// </code>
        /// </example>
        public static string Replace(this string text, Func<string, string> replacement, string prefix, string suffix)
        {
            return Replace(text, replacement, prefix, suffix, true);
        }

        /// <summary>
        /// 替换数据
        /// </summary>
        /// <param name="text">要替换的内容</param>
        /// <param name="replacement">返回最终替换数据的委托. 如果委托返回null则表示不替换,否则替换数据</param>
        /// <param name="prefix">前缀,如"{"</param>
        /// <param name="suffix">后缀,如"}"</param>
        /// <param name="ignoreCase">前缀与后缀的查询是否忽略大小写</param>
        /// <exception cref="System.ArgumentException">prefix或suffix为空</exception>
        /// <returns>替换后的数据</returns>
        public static string Replace(this string text, Func<string, string> replacement, string prefix, string suffix, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(prefix)) throw new ArgumentException("prefix");
            if (string.IsNullOrEmpty(suffix)) throw new ArgumentException("suffix");

            StringComparison comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
            int p, e, s, offset = 0;
            StringBuilder buffer = new StringBuilder(text.Length);
            do
            {
                p = text.IndexOf(prefix, offset, comparison);
                if (p == -1) break;
                s = p + prefix.Length;
                e = text.IndexOf(suffix, s, comparison);
                if (e == -1) break;

                string key = text.Substring(s, e - s);
                bool ok = false;
                if (key.IndexOfAny(new char[] { '\r', '\n', '\t' }) == -1)  //不允许key值里有回车,换行,TAB字符存在
                {
                    string value = replacement(key);
                    if (value != null)
                    {
                        ok = true;
                        buffer.Append(text.Substring(offset, p - offset));    //拷入前缀字符前的数据
                        buffer.Append(value);                                    //拷入替换后的数据
                    }
                }
                if (!ok)
                {
                    //拷入原字符
                    buffer.Append(text.Substring(offset, (e + suffix.Length) - offset));
                }
                offset = e + suffix.Length;  //偏移
            } while (true);
            if (offset < text.Length) buffer.Append(text.Substring(offset));
            return buffer.ToString();
        }
        #endregion

        #region 数据转换
        /// <summary>
        /// 将字符串转换为JavaScript里的字符串,对于非英文字母、数字的字符将转换为“\uxxxx”格式的转义字符
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToJavaScriptString(this string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            StringBuilder buffer = null;
            int startIndex = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')))
                {
                    //非字母或数字，则直接转换为\uxxxx格式字符
                    if (buffer == null)
                    {
                        buffer = new StringBuilder(text.Length + 6);
                    }
                    if (i > startIndex)
                    {
                        buffer.Append(text, startIndex, i - startIndex);
                    }
                    startIndex = i + 1;

                    buffer.Append(@"\u");
                    buffer.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                }
                else if (buffer != null)
                {
                    buffer.Append(c);
                    startIndex = i + 1;
                }
            }
            if (buffer == null) return text;
            return buffer.ToString();
        }

        /// <summary>
        /// 将字符串转换为某个类型值，如果转换失败则返回对应类型的默认值
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="error">是否转换有错误</param>
        /// <returns></returns>
        public static object ConvertTo(this string text, Type targetType, out bool error)
        {
            object result = null;
            error = false;

            TypeCode tc = Type.GetTypeCode(targetType);
            if (targetType.IsEnum)
            {
                //枚举
                try
                {
                    result = Enum.Parse(targetType, text, true);
                }
                catch
                {
                    result = Enum.GetValues(targetType).GetValue(0);
                    error = true;
                }
            }
            else if (tc == TypeCode.String)
            {
                return text;
            }
            else if (tc == TypeCode.DBNull)
            {
                return DBNull.Value;
            }
            else if (tc == TypeCode.Empty)
            {
                return null;
            }
            else if (tc == TypeCode.Char)
            {
                return text[0];
            }
            else if (tc == TypeCode.Boolean)
            {
                //布尔值。则处理两种特殊的值：yes/no; 1/0
                if ("yes".Equals(text, StringComparison.OrdinalIgnoreCase)
                    || "1".Equals(text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else if ("no".Equals(text, StringComparison.OrdinalIgnoreCase)
                   || "0".Equals(text, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                else
                {
                    bool b;
                    error = true;
                    result = false;
                    if (bool.TryParse(text, out b))
                    {
                        error = false;
                        result = b;
                    }
                }
            }
            else if (tc == TypeCode.SByte)
            {
                sbyte sb;
                error = true;
                result = (sbyte)0;
                if (sbyte.TryParse(text, out sb))
                {
                    error = false;
                    result = sb;
                }
            }
            else if (tc == TypeCode.Byte)
            {
                byte by;
                error = true;
                result = (byte)0;
                if (byte.TryParse(text, out by))
                {
                    error = false;
                    result = by;
                }
            }
            else if (tc == TypeCode.Int16)
            {
                short i16;
                error = true;
                result = (short)0;
                if (short.TryParse(text, out i16))
                {
                    error = false;
                    result = i16;
                }
            }
            else if (tc == TypeCode.UInt16)
            {
                ushort ui16;
                error = true;
                result = (ushort)0;
                if (ushort.TryParse(text, out ui16))
                {
                    error = false;
                    result = ui16;
                }
            }
            else if (tc == TypeCode.Int32)
            {
                int i32;
                error = true;
                result = 0;
                if (int.TryParse(text, out i32))
                {
                    error = false;
                    result = i32;
                }
            }
            else if (tc == TypeCode.UInt32)
            {
                uint ui32;
                error = true;
                result = (uint)0;
                if (uint.TryParse(text, out ui32))
                {
                    error = false;
                    result = ui32;
                }
            }
            else if (tc == TypeCode.Int64)
            {
                long i64;
                error = true;
                result = (long)0;
                if (long.TryParse(text, out i64))
                {
                    error = false;
                    result = i64;
                }
            }
            else if (tc == TypeCode.UInt64)
            {
                ulong ui64;
                error = true;
                result = (ulong)0;
                if (ulong.TryParse(text, out ui64))
                {
                    error = false;
                    result = ui64;
                }
            }
            else if (tc == TypeCode.Single)
            {
                Single s;
                error = true;
                result = (Single)0;
                if (Single.TryParse(text, out s))
                {
                    error = false;
                    result = s;
                }
            }
            else if (tc == TypeCode.Double)
            {
                Double d;
                error = true;
                result = (Double)0;
                if (Double.TryParse(text, out d))
                {
                    error = false;
                    result = d;
                }
            }
            else if (tc == TypeCode.Decimal)
            {
                Decimal d1;
                error = true;
                result = (Decimal)0;
                if (Decimal.TryParse(text, out d1))
                {
                    error = false;
                    result = d1;
                }
            }
            else if (tc == TypeCode.DateTime)
            {
                DateTime dt;
                error = true;
                result = DateTime.MinValue;
                if (DateTime.TryParse(text, out dt))
                {
                    error = false;
                    result = dt;
                }
            }
            else
            {
                try
                {
                    result = Convert.ChangeType(text, targetType, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    error = true;
                    result = null;
                }
            }
            return result;
        }
        #endregion

        #region 数据读取
        /// <summary>
        /// 获取固定长度的字符串
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="maxlen">要获取的最大长度，如果字符串<paramref name="text"/>的长度大于此值，则将截取</param>
        /// <returns></returns>
        public static string Substr(this string text, int maxlen)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return (text.Length > maxlen ? text.Substring(0, maxlen) : text);
        }
        /// <summary>
        /// 获取固定位长度的字符串。
        /// 注：此方法计算的是字符位长度，比如中文字符在GB码中一个字符占用2个字符位。
        /// 例子：
        /// "中国人".Substr(5,Encoding.GetEncoding("gb2312")); 输出：“中国”两个字符，因为“中国”共4个字符位，如果再加上“人”则为6个字符位，大于所要求的“5”个字符位，所以只输出前2个字符“中国”；
        /// "中国China".Substr(5,Encoding.GetEncoding("gb2312")); 输出：“中国C"三个字符。“中国”4个字符位长度+“C”1个字符位长度
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="maxlen">要获取的最大字符位长度，如果字符串<paramref name="text"/>的长度长于此值，则将截取。
        /// 注：长度是计算字符所占用的位长度，比如对于双字节字符（如中文）则一个字符为2个字符位</param>
        /// <param name="charset">字符编码。</param>
        /// <returns></returns>
        public static string Substr(this string text, int maxlen, Encoding charset)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            int count = 0;
            int byteLength = 0;
            foreach (char c in text)
            {
                byteLength += charset.GetBytes(c.ToString()).Length;
                if (byteLength < maxlen)
                {
                    count++;
                }
                else if (byteLength == maxlen)
                {
                    count++;
                    break;
                }
                else
                {
                    break;
                }
            }
            if (count == text.Length) return text;
            return text.Substring(0, count);
        }
        /// <summary>
        /// 将字符串按行读取，并返回所有行
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns></returns>
        public static string[] ReadAllLines(this string text)
        {
            return ReadAllLines(text, false);
        }
        /// <summary>
        /// 将字符串按行读取，并返回所有行
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="removeEmptyLine">是否不读取空行</param>
        /// <returns></returns>
        public static string[] ReadAllLines(this string text, bool removeEmptyLine)
        {
            List<string> lines = new List<string>(10);
            if (!string.IsNullOrEmpty(text))
            {
                using (StringReader reader = new StringReader(text))
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (removeEmptyLine && line.Length == 0) continue;
                        lines.Add(line);
                    }
                }
            }
            return lines.ToArray();
        }
        
        /// <summary>
        /// 将字符串按行读取并返回所有行，读取时将每行的前后空白字符去除。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns></returns>
        public static string[] TrimReadAllLines(this string text)
        {
            return TrimReadAllLines(text, false);
        }
        /// <summary>
        /// 将字符串按行读取并返回所有行，读取时将每行的前后空白字符去除。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="removeEmptyLine">是否不读取空行</param>
        /// <returns></returns>
        public static string[] TrimReadAllLines(this string text, bool removeEmptyLine)
        {
            List<string> lines = new List<string>(10);
            if (!string.IsNullOrEmpty(text))
            {
                using (StringReader reader = new StringReader(text))
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (removeEmptyLine && line.Length == 0) continue;
                        lines.Add(line);
                    }
                }
            }
            return lines.ToArray();
        }
        /// <summary>
        /// 从字符串查找某段文本数据。如果有"text"捕获项并成功获取，则返回此捕获项值，如果未设置"text"捕获项则获取第1个捕获项，如果没有捕获项则返回匹配项
        /// </summary>
        /// <param name="text">字符串数据</param>
        /// <param name="pattern">用于查找的正则表达式，其中可以设置一个"text"捕获项，如:(&lt;text&gt;.+)</param>
        /// <param name="ignoreCase">是否不区分大小写</param>
        /// <returns></returns>
        public static string FindText(this string text, string pattern, bool ignoreCase)
        {
            return FindText(text, pattern, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }
        /// <summary>
        /// 查找某段文本数据。如果有"text"捕获项并成功获取，则返回此捕获项值，如果未设置"text"捕获项则获取第1个捕获项，如果没有捕获项则返回匹配项
        /// </summary>
        /// <param name="text">字符串数据</param>
        /// <param name="pattern">用于查找的正则表达式，其中可以设置一个"text"捕获项，如:(&lt;text&gt;.+)</param>
        /// <param name="options">选项</param>
        /// <returns></returns>
        public static string FindText(this string text, string pattern, RegexOptions options)
        {
            var m = Regex.Match(text, pattern, options);
            if (m.Success)
            {
                if (m.Groups["text"].Success) return m.Groups["text"].Value;
                if (m.Groups.Count > 1) return m.Groups[1].Value;
                return m.Value;
            }
            return string.Empty;
        }
        #endregion
    }
}
