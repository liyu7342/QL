/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  PageLinkHelper
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QL.Core;
using System.Collections.Specialized;
using System.Web;
using QL.Core.Extensions;
using VTemplate.Engine;
namespace QL.Web
{
    /// <summary>
    /// 页面链接的帮助器
    /// </summary>
    public static class PageLinkHelper
    {
        /// <summary>
        /// 重置当前Request URL的查询参数的某个值，并且返回最新已重设参数值的地址。
        /// 比如原Request URL： http://www.host.com/?index=detail&amp;page=2
        /// 现将page重设值为3，即调用ResetCurrentUrlQueryParam("page", "3")方法后，将返回类似地址：
        /// http://www.host.com/?index=detail&amp;page=3
        /// </summary>
        /// <param name="key">要重设置的参数名称。</param>
        /// <param name="value">要重设置的参数值，如果值为null或空值，则表示将此参数去除</param>
        /// <returns></returns>
        public static string ResetCurrentUrlQueryParam(string key, string value)
        {
            var request = WebContext.Request.Current;
            if (request == null) return string.Empty;

            string rawUrl = WebContext.Request.RawUrl;
            return ResetUrlQueryParam(rawUrl, request.ContentEncoding, key, value);
        }
        /// <summary>
        /// 重置URL的查询参数的某个值，并且返回最新已重设参数值的地址。
        /// 比如原URL： http://www.host.com/?index=detail&amp;page=2
        /// 现将page重设值为3，即调用ResetCurrentUrlQueryParam("page", "3")方法后，将返回类似地址：
        /// http://www.host.com/?index=detail&amp;page=3
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <param name="encoding">URL地址参数的编码</param>
        /// <param name="key">要重设置的参数名称。</param>
        /// <param name="value">要重设置的参数值，如果值为null或空值，则表示将此参数去除</param>
        /// <returns></returns>
        public static string ResetUrlQueryParam(string url, Encoding encoding, string key, string value)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            NameValueCollection queryString = null;
            if (!string.IsNullOrEmpty(url))
            {
                Uri uri = null;
                if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
                {
                    queryString = HttpUtility.ParseQueryString(uri.Query, encoding);
                }
            }
            if (queryString == null) return string.Empty;

            bool find = false;
            StringBuilder buffer = new StringBuilder();
            foreach (string k in queryString)
            {
                if (!find && !k.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    string[] values = queryString.GetValues(k);
                    foreach (string v in values)
                    {
                        if (buffer.Length > 0) buffer.Append("&");
                        buffer.AppendFormat("{0}={1}", HttpUtility.UrlEncode(k, encoding), HttpUtility.UrlEncode(v.ToString(""), encoding));
                    }
                }
                else
                {
                    find = true;
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (buffer.Length > 0) buffer.Append("&");
                        buffer.AppendFormat("{0}={1}", HttpUtility.UrlEncode(k, encoding), HttpUtility.UrlEncode(value, encoding));
                    }
                }
            }
            if (!find && !string.IsNullOrEmpty(value)) buffer.AppendFormat("{0}={1}", HttpUtility.UrlEncode(key, encoding), HttpUtility.UrlEncode(value, encoding));

            int p = url.IndexOf("?");
            if (p != -1) url = url.Remove(p);

            return buffer.Length > 0 ? string.Concat(url, "?", buffer.ToString()) : url;
        }

        /// <summary>
        /// 解析分页列表标签. 调用此方法解析后，模板里将会对“PageListTag”变量赋值，结构如下：
        /// PageListTag = new { PageNumber = 当前页号, PageSize = 页码, PageCount = 总页码数量, RecordCount = 总记录数量, StartIndex = 当前页号内数据的起始位置, EndIndex = 当前页号内数据的结束位置 }
        /// 。并且还会在模板里注册一个“CreatePageListUrl”方法，用于获取分页列表地址。此方法可在变量标签里直接call调用，如：{$n call="CreatePageListUrl"}，将输出n表示的页号地址，
        /// 其中n必须为数字值或者“first”、“首页”、“previous”、“上页”、“next”、“下页”、“last”、“尾页”这几个特殊值。
        /// </summary>
        /// <param name="template">分页列表标签所在的模板</param>
        /// <param name="pageNumber">当前页号</param>
        /// <param name="pageSize">页码</param>
        /// <param name="recordCount">数量总数</param>
        /// <param name="pageListUrlCreator">生成分页列表地址的委托函数</param>
        public static void ParsePageListTag(Template template, int pageNumber, int pageSize, int recordCount, Func<int, string> pageListUrlCreator)
        {
            int pageCount = pageSize > 0 ? (int)Math.Ceiling((double)recordCount / pageSize) : recordCount;
            int startIndex = (pageNumber - 1) * pageSize + 1;
            int endIndex = pageNumber * pageSize;
            endIndex = endIndex > recordCount ? recordCount : endIndex;
            if (startIndex > endIndex) startIndex = endIndex;
            template.Variables.SetValue("PageListTag", new { PageNumber = pageNumber, PageSize = pageSize, PageCount = pageCount, RecordCount = recordCount, StartIndex = startIndex, EndIndex = endIndex });
            if (pageListUrlCreator != null)
            {
                //注册两个用于获取分页地址的变量方法
                template.RegisterGlobalFunction("CreatePageListUrl", (o =>
                {
                    if (o[0] is LoopIndex)
                    {
                        LoopIndex li = (LoopIndex)o[0];
                        if (li != null)
                        {
                            return pageListUrlCreator(li.ToInt32(null));
                        }
                        else
                        {
                            return pageListUrlCreator(o[0].As<int>());
                        }
                    }else{
                        switch(o[0].ToString("")){
                            case "first":
                            case "首页":
                                return pageListUrlCreator(1);
                            case "previous":
                            case "上页":
                                return pageListUrlCreator(pageNumber > 1 ? pageNumber - 1 : 1);
                            case "next":
                            case "下页":
                                return pageListUrlCreator(pageNumber < pageCount ? pageNumber + 1 : pageCount);
                            case "last":
                            case "尾页":
                                 return pageListUrlCreator(pageCount);
                            default:
                                return pageListUrlCreator(o[0].As<int>(1));
                        }
                    }
                }));
            }
        }
    }
}
