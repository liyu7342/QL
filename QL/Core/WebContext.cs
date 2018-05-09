/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  Web
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using QL.Core.Extensions;
namespace QL.Core
{
    /// <summary>
    /// Web上下文的封装。注意：此类只能在WEB运行环境中使用
    /// </summary>
    public static class WebContext
    {
        /// <summary>
        /// 当前HTTP请求上下文
        /// </summary>
        public static HttpContext Current
        {
            get
            {
                return HttpContext.Current;
            }
        }

        #region HttpRequest的相关封装
        /// <summary>
        /// HTTP请求的相关封装
        /// </summary>
        public static class Request
        {
            /// <summary>
            /// 获取当前HTTP请求的System.Web.HttpRequest对象
            /// </summary>
            public static HttpRequest Current
            {
                get
                {
                    return WebContext.Current == null ? null : WebContext.Current.Request;
                }
            }

            #region 参数获取
            /// <summary>
            /// 获取HTTP查询参数值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="name">参数名称</param>
            /// <returns></returns>
            public static T GetQuery<T>(string name)
            {
                return Current.QueryString.Get<T>(name);
            }
            /// <summary>
            /// 获取HTTP查询参数值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="name">参数名称</param>
            /// <param name="replacement">如果没有此参数或者参数值类型转换则失败则返回此替换值</param>
            /// <returns></returns>
            public static T GetQuery<T>(string name, T replacement)
            {
                return Current.QueryString.Get<T>(name, replacement);
            }
            /// <summary>
            /// 获取HTTP窗体参数值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="name">参数名称</param>
            /// <returns></returns>
            public static T GetForm<T>(string name)
            {
                return Current.Form.Get<T>(name);
            }
            /// <summary>
            /// 获取HTTP窗体参数值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="name">参数名称</param>
            /// <param name="replacement">如果没有此参数或者参数值类型转换则失败则返回此替换值</param>
            /// <returns></returns>
            public static T GetForm<T>(string name, T replacement)
            {
                return Current.Form.Get<T>(name, replacement);
            }
            /// <summary>
            /// 获取HttpRequest请求参数值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="name">参数名称</param>
            /// <returns></returns>
            public static T Get<T>(string name)
            {
                return Current[name].As<T>();
            }
            /// <summary>
            /// 获取HttpRequest请求参数值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="name">参数名称</param>
            /// <param name="replacement">如果没有此参数或者参数值类型转换则失败则返回此替换值</param>
            /// <returns></returns>
            public static T Get<T>(string name, T replacement)
            {
                return Current[name].As<T>(replacement);
            }


            /// <summary>
            /// 获取HTTP查询参数值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="name">参数名称</param>
            /// <returns></returns>
            public static T TrimGetQuery<T>(string name)
            {
                return Current.QueryString.TrimGet<T>(name);
            }
            /// <summary>
            /// 获取HTTP查询参数值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="name">参数名称</param>
            /// <param name="replacement">如果没有此参数或者参数值类型转换则失败则返回此替换值</param>
            /// <returns></returns>
            public static T TrimGetQuery<T>(string name, T replacement)
            {
                return Current.QueryString.TrimGet<T>(name, replacement);
            }
            /// <summary>
            /// 获取HTTP窗体参数值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="name">参数名称</param>
            /// <returns></returns>
            public static T TrimGetForm<T>(string name)
            {
                return Current.Form.TrimGet<T>(name);
            }
            /// <summary>
            /// 获取HTTP窗体参数值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="name">参数名称</param>
            /// <param name="replacement">如果没有此参数或者参数值类型转换则失败则返回此替换值</param>
            /// <returns></returns>
            public static T TrimGetForm<T>(string name, T replacement)
            {
                return Current.Form.TrimGet<T>(name, replacement);
            }
            /// <summary>
            /// 获取HttpRequest请求参数值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="name">参数名称</param>
            /// <returns></returns>
            public static T TrimGet<T>(string name)
            {
                return Current[name].IfEmpty("").Trim().As<T>();
            }
            /// <summary>
            /// 获取HttpRequest请求参数值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="name">参数名称</param>
            /// <param name="replacement">如果没有此参数或者参数值类型转换则失败则返回此替换值</param>
            /// <returns></returns>
            public static T TrimGet<T>(string name, T replacement)
            {
                return Current[name].IfEmpty("").Trim().As<T>(replacement);
            }
            #endregion

            /// <summary>
            /// 客户端的IP地址，如果客户端使用了代理则将忽略代理的IP
            /// </summary>
            public static string RemoteAddress
            {
                get
                {
                    string ip = Current.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (!string.IsNullOrEmpty(ip))
                    {
                        int p = ip.IndexOfAny(new char[] { ',', ';' });
                        if (p != -1)
                        {
                            ip = ip.Substring(0, p);
                        }
                    }
                    else
                    {
                        ip = Current.ServerVariables["REMOTE_ADDR"];
                    }
                    if (!Utility.IsValidIP(ip)) ip = Current.UserHostAddress;
                    if (string.IsNullOrEmpty(ip)) ip = "0.0.0.0";

                    return ip;
                }
            }

            /// <summary>
            /// 获取页面的来源地址
            /// </summary>
            public static string Referrer
            {
                get
                {
                    //不使用Request.UrlReferrer是因为有时客户端恶意提交非法格式的来源地址时，使用Request.UrlReferrer获取将会抛错！
                    return Current.ServerVariables["HTTP_REFERER"] ?? "";
                }
            }

            /// <summary>
            /// 获取当前请求的原始Url（已处理了地址重写的情况，即原地址存储于“HTTP_X_REWRITE_URL”服务器变量的情况)
            /// </summary>
            public static string RawUrl
            {
                get
                {
                    string rawUrl = Current.ServerVariables["HTTP_X_REWRITE_URL"];
                    return rawUrl.IfEmpty(Current.RawUrl);
                }
            }

            /// <summary>
            /// 返回是否以POST方式访问页面
            /// </summary>
            public static bool IsPost
            {
                get
                {
                    return "POST".Equals(Current.HttpMethod, StringComparison.InvariantCultureIgnoreCase);
                }
            }


            /// <summary>
            /// 获取当前服务器的域名地址,如: http://www.host.com 或 http://www.host.com:8102
            /// </summary>
            /// <returns></returns>
            public static string HostName
            {
                get
                {
                    string name = Current.ServerVariables["SERVER_NAME"];
                    string port = Current.ServerVariables["SERVER_PORT"];
                    string url = string.Empty;
                    if ("on".Equals(Current.ServerVariables["HTTPS"], StringComparison.OrdinalIgnoreCase))
                    {
                        url = string.Concat("https://", name);
                        if ("443" == port) port = string.Empty;
                    }
                    else
                    {
                        url = string.Concat("http://", name);
                        if ("80" == port) port = string.Empty;
                    }
                    if (!string.IsNullOrEmpty(port)) url += ":" + port;

                    return url;
                }
            }
        }
        #endregion

        #region HttpResponse的相关封装
        /// <summary>
        /// HTTP响应的相关封装
        /// </summary>
        public static class Response
        {
            /// <summary>
            /// 获取当前HTTP响应的System.Web.HttpResponse对象
            /// </summary>
            public static HttpResponse Current
            {
                get
                {
                    return WebContext.Current == null ? null : WebContext.Current.Response;
                }
            }

            /// <summary>
            /// 将请求重定向到新 URL 并指定该新 URL。
            /// </summary>
            /// <param name="url"></param>
            public static void Redirect(string url)
            {
                Response.Current.Redirect(url, true);
            }

            /// <summary>
            /// 输出页面未找到错误(404)信息
            /// </summary>
            public static void NotFound()
            {
                var response = Response.Current;
                response.Clear();
                response.StatusCode = 404;
                EndWrite(@"<html>
<head>
<title>404 - File not found.</title>
</head>
<body>
<h2>404 - File not found.</h2>
</body>
<html>");
            }


            /// <summary>
            /// 输出数据并且结束当前页面执行
            /// </summary>
            /// <param name="value"></param>
            public static void EndWrite(object value)
            {
                var response = Response.Current;
                response.Write(value);
                response.End();
            }
            /// <summary>
            /// 输出一行数据
            /// </summary>
            /// <param name="value"></param>
            public static void WriteLine(object value)
            {
                var r = Current;
                r.Write(value);
                r.Write(Environment.NewLine);
            }

            /// <summary>
            /// 输出数据与&lt;br /&gt;字符
            /// </summary>
            /// <param name="value"></param>
            public static void WriteHtmlLine(object value)
            {
                var r = Current;
                r.Write(value);
                r.Write("<br />");
            }


        }
        #endregion

        #region Cookie封装
        /// <summary>
        /// Cookie封装
        /// </summary>
        public static class Cookies
        {
            /// <summary>
            /// 获取当前HTTP请求的某个Cookie
            /// </summary>
            /// <param name="name">Cookie名称</param>
            /// <returns></returns>
            public static HttpCookie Get(string name)
            {
                return Request.Current.Cookies[name];
            }
            /// <summary>
            /// 获取当前HTTP请求的某个Cookie的值
            /// </summary>
            /// <param name="name">Cookie名称</param>
            /// <returns>如果Cookie不存在则返回null，否则返回Cookie的值</returns>
            public static string GetValue(string name)
            {
                var cookie = Get(name);
                return cookie == null ? null : cookie.Value;
            }
            /// <summary>
            /// 将某个Cookie输出到当前HTTP会话中
            /// </summary>
            /// <param name="cookie">Cookie</param>
            public static void Put(HttpCookie cookie)
            {
                Response.Current.Cookies.Set(cookie);
            }
            /// <summary>
            /// 设置某个Cookie的值，并将其输出到当前HTTP会话中
            /// </summary>
            /// <param name="name">Cookie名称</param>
            /// <param name="value">Cookie的值</param>
            public static void PutValue(string name, string value)
            {
                HttpCookie cookie = Get(name);
                if (cookie == null) cookie = new HttpCookie(name);
                cookie.Value = value;
                Put(cookie);
            }
            /// <summary>
            /// 移除某个Cookie
            /// </summary>
            /// <param name="name">Cookie名称</param>
            public static bool Remove(string name)
            {
                var cookie = Get(name);
                if (cookie != null)
                {
                    cookie.Expires = DateTime.MinValue;
                    Put(cookie);
                }
                return cookie != null;
            }
        }
        #endregion
    }
}
