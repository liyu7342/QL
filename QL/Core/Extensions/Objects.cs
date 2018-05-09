/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  Objects
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 
using System.Collections;
using System.ComponentModel;
using System.Reflection;
namespace QL.Core.Extensions
{
    /// <summary>
    /// 与Object相关的扩展函数
    /// </summary>
    public static class Objects
    {
        #region 数据转换函数块
        /// <summary>
        /// 将某个对象转换为某种类型对象数据。
        /// 如果类型转换失败则返回对应目标类型的默认值，如果目标类型是枚举值，则返回第一个枚举值
        /// </summary>
        /// <typeparam name="T">需要转换的目标类型</typeparam>
        /// <param name="obj">需要转换的对象</param>
        /// <returns>转换后的类型数据</returns>
        /// <example>
        /// <code>
        /// int i = "1".As&lt;int&gt;();
        /// float f = "0.32".As&lt;float&gt;();
        /// DayOfWeek dayOfWeek = "Sunday".As&lt;DayOfWeek&gt;();
        /// DateTime time = "2011-01-01 23:00".As&lt;DateTime&gt;();
        /// </code>
        /// </example>
        public static T As<T>(this object obj)
        {
            Type t = typeof(T);
            T replacement = default(T);
            if (t.IsEnum)
            {
                //枚举类型。则获取第一个默认项
                replacement = (T)Enum.GetValues(t).GetValue(0);
            }
            //else if (t == typeof(string))
            //{
            //    //字符串，则以空字符串为默认值
            //    //replacement = (T)(object)string.Empty;
            //}
            return As<T>(obj, replacement);
        }
        /// <summary>
        /// 将某个对象转换为某种类型对象数据。 如果类型转换失败则返回替换值
        /// </summary>
        /// <typeparam name="T">需要转换的目标类型</typeparam>
        /// <param name="obj">对象</param>
        /// <param name="replacement">如果转换失败则返回此替换值</param>
        /// <returns>转换后的类型数据, 如果类型转换失败则返回<paramref name="replacement"/>表示的替换值</returns>
        /// <example>
        /// <code>
        /// object v = null;
        /// int i = v.As&lt;int&gt;(0);                     //i = 0;
        /// float f = "0.32".As&lt;float&gt;(0);            //f = 0.32;
        /// string s = v.As&lt;string&gt;("null");          //s = "null";
        /// DateTime time = v.As&lt;DateTime&gt;(DateTime.Now); //time = DateTime.Now;
        /// </code>
        /// </example>
        public static T As<T>(this object obj, T replacement)
        {
            return (T)obj.As(typeof(T), replacement);
        }

        /// <summary>
        /// 转换为某种类型
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="replacement">如果转换失败则返回此替换值</param>
        /// <returns></returns>
        public static object As(this object obj, Type targetType, object replacement)
        {
            if (obj == null) return replacement;

            TypeCode targetTC = Type.GetTypeCode(targetType);
            Type sourceType = obj.GetType();

            if (!targetType.IsInstanceOfType(obj))
            {
                if (sourceType.IsEnum)
                {
                    //枚举类型，则特殊对待
                    try
                    {
                        return Convert.ChangeType(obj, targetType);
                    }
                    catch
                    {
                        return replacement;
                    }
                }
                else
                {
                    switch (targetTC)
                    {
                        case TypeCode.Empty:
                            return null;
                        case TypeCode.Object:
                            return replacement;
                        case TypeCode.DBNull:
                            return DBNull.Value;
                        case TypeCode.String:
                            return obj.ToString();
                        default:
                            bool error;
                            var v = obj.ToString().ConvertTo(targetType, out error);
                            return error ? replacement : v;
                    }
                }
            }
            else
            {
                return obj;
            }
        }

        /// <summary>
        /// 将某个对象转换为字符串对象
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="empty">如果对象为null则返回此字符</param>
        /// <returns>对象的字符串表示方式。如果对象为null则返回<paramref name="empty"/>表示的字符串</returns>
        public static string ToString(this object obj, string empty)
        {
            if (obj == null) return empty;
            return obj.ToString();
        }
        /// <summary>
        /// 将某个对象转换为Json字符数据
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>对象的Json格式的字符串。
        /// 对于DateTime类型的数据则返回“\/Date(时间戳)\/”的字符串数据；
        /// 而对于字符串，则对于非英文字母、数字的字符则返回utf码表示的字符。如对于中文的“我”则返回“\u6211”</returns>
        public static string ToJson(this object obj)
        {
            if (obj == null) return "null";
            TypeCode code = Convert.GetTypeCode(obj);
            switch (code)
            {
                case TypeCode.Boolean:
                    return ((bool)obj) ? "true" : "false";
                case TypeCode.DateTime:
                    DateTime d = (DateTime)obj;
                    return string.Format("\"\\/Date({0})\\/\"", d.ToTimestamp());
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return "null";
                case TypeCode.String:
                case TypeCode.Char:
                    return string.Format("\"{0}\"", obj.ToString().ToJavaScriptString());
                case TypeCode.Object:
                    return ObjectToJson(obj);
                default:
                    return obj.ToString();
            }
        }

        /// <summary>
        /// 将某个对象转换为Json数据
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string ObjectToJson(object obj)
        {
            //处理数组的情况
            if (obj is IEnumerable) return ListObjectToJson((IEnumerable)obj);

            StringBuilder buffer = new StringBuilder(64);
            buffer.Append("{");
                        

            //取得公共属性
            PropertyDescriptorCollection pros = TypeDescriptor.GetProperties(obj);
            foreach (PropertyDescriptor p in pros)
            {
                if (buffer.Length != 1) buffer.Append(",");
                buffer.AppendFormat("\"{0}\":{1}", p.Name.ToJavaScriptString(), p.GetValue(obj).ToJson());
            }

            //取得公共字段
            Type type = obj.GetType();
            foreach (FieldInfo f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (buffer.Length != 1) buffer.Append(",");
                buffer.AppendFormat("\"{0}\":{1}", f.Name.ToJavaScriptString(), f.GetValue(obj).ToJson());
            }

            buffer.Append("}");

            return buffer.ToString();
        }
        /// <summary>
        /// 将集合、列表对象转换为Json数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static string ListObjectToJson(IEnumerable list)
        {
            StringBuilder buffer = new StringBuilder(64);
            buffer.Append("[");
            foreach (object v in list)
            {
                if (buffer.Length != 1) buffer.Append(",");
                buffer.Append(v.ToJson());
            }
            buffer.Append("]");
            return buffer.ToString();
        }
        #endregion

        /// <summary>
        /// 如果对象为null则调用函数委托并返回函数委托的返回值。否则返回对象本身
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="func">对象为null时用于调用的函数委托</param>
        /// <returns>如果对象不为null则返回对象本身，否则返回<paramref name="func"/>函数委托的返回值</returns>
        /// <example>
        /// <code>
        /// string v = null;
        /// string d = v.IfNull&lt;string&gt;(()=>"v is null");  //d = "v is null";
        /// string t = d.IfNull(() => "d is null");              //t = "v is null";
        /// </code>
        /// </example>
        public static T IfNull<T>(this T obj, Func<T> func)
            where T : class
        {
            if (obj == null)
            {
                return func == null ? default(T) : func();
            }
            else
            {
                return obj;
            }
        }
    }
}
