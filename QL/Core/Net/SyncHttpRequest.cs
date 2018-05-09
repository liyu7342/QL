/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  SyncHttpRequest
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace QL.Core.Net
{
    /// <summary>
    /// 同步的HTTP请求
    /// </summary>
    public class SyncHttpRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public SyncHttpRequest(string url)
            : this(url, Encoding.UTF8)
        { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        public SyncHttpRequest(string url, Encoding encoding)
        {
            this.Url = url;
            Uri uri;
            this.Uri = Uri.TryCreate(url, UriKind.Absolute, out uri) ? uri : null;
            this.Timeout = 30000;
            this.Encoding = encoding;
            this.Headers = new Parameters();
            this.Parameters = new Parameters();
            this.Cookies = new CookieCollection();
        }

        /// <summary>
        /// 超时,单位:毫秒
        /// </summary>
        public int Timeout { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        public Encoding Encoding { get; private set; }

        /// <summary>
        /// 需要请求的地址
        /// </summary>
        public string Url { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// 引用来源地址
        /// </summary>
        public string Referer { get; set; }

        /// <summary>
        /// 要附加的HTTP头
        /// </summary>
        public Parameters Headers { get; private set; }

        /// <summary>
        /// 查询参数
        /// </summary>
        public Parameters Parameters { get; private set; }

        /// <summary>
        /// Cookies
        /// </summary>
        public CookieCollection Cookies { get; private set; }

        /// <summary>
        /// 添加Cookie项
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddCookie(string name, string value)
        {
            this.Cookies.Add(new Cookie(name, value, "/", this.Uri.Host));
        }
        #region 方法动作
        /// <summary>
        /// 建立HttpRequest实例
        /// </summary>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        private HttpWebRequest CreateWebRequest(RequestMethod method, Uri uri)
        {
            var request = HttpHelper.CreateRequest(method.ToString(), uri, this.Timeout);
            if (!string.IsNullOrEmpty(this.Referer)) request.Referer = this.Referer;
            foreach (var p in this.Headers.Items)
            {
                request.Headers.Add(p.Key, p.Value);
            }
            if (Cookies.Count > 0)
            {
                var cc = new CookieContainer();
                cc.Add(this.Cookies);
                request.CookieContainer = cc;
            }
            return request;
        }
        /// <summary>
        /// GET请求
        /// </summary>
        /// <returns></returns>
        public string Get()
        {
            string queryString = this.Parameters.BuildQueryString(true, this.Encoding);
            Uri uri = this.Uri;
            if (!string.IsNullOrEmpty(queryString))
            {
                uri = new Uri(string.Concat(this.Url, this.Url.IndexOf('?') == -1 ? '?' : '&', queryString));
            }
            var request = this.CreateWebRequest(RequestMethod.GET, uri);
            return HttpHelper.ReadAllResponseText(request, this.Encoding);
        }

        /// <summary>
        /// POST请求
        /// </summary>
        /// <returns></returns>
        public string Post()
        {
            var request = this.CreateWebRequest(RequestMethod.POST, this.Uri);
            request.ContentType = "application/x-www-form-urlencoded";

            if (this.Parameters.Items.Count != 0)
            {
                string queryString = this.Parameters.BuildQueryString(true, this.Encoding);
                byte[] data = this.Encoding.GetBytes(queryString);
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            return HttpHelper.ReadAllResponseText(request, this.Encoding);
        }

        /// <summary>
        /// 提交文件
        /// </summary>
        /// <param name="files">要提交上传的文件列表</param>
        /// <returns></returns>
        public string PostFile(Files files)
        {
            var request = this.CreateWebRequest(RequestMethod.POST, this.Uri);

            string boundary = string.Concat("--", Utility.CreateRndCode(20));
            request.ContentType = string.Concat("multipart/form-data; boundary=", boundary);
            request.KeepAlive = true;

            using (MemoryStream ms = new MemoryStream())
            {
                byte[] boundaryData = this.Encoding.GetBytes("\r\n--" + boundary + "\r\n");
                if (this.Parameters.Items.Count != 0)
                {
                    //写入参数
                    string parameterData = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                    foreach (var p in this.Parameters.Items)
                    {
                        string item = string.Format(parameterData, p.Key, p.Value);
                        byte[] data = this.Encoding.GetBytes(item);
                        ms.Write(boundaryData, 0, boundaryData.Length);
                        ms.Write(data, 0, data.Length);
                    }
                }

                if (files != null)
                {
                    //写入文件数据
                    string fileData = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                    foreach (var p in files.Items)
                    {
                        if (p.Value != null)
                        {
                            string item = string.Format(fileData, p.Key, p.Value.FileName, p.Value.ContentType);
                            byte[] data = this.Encoding.GetBytes(item);
                            ms.Write(boundaryData, 0, boundaryData.Length);
                            ms.Write(data, 0, data.Length);
                            p.Value.WriteTo(ms);
                        }
                    }
                }

                //写入结束线
                boundaryData = this.Encoding.GetBytes("\r\n--" + boundary + "--\r\n");
                ms.Write(boundaryData, 0, boundaryData.Length);

                request.ContentLength = ms.Length;
                using (var stream = request.GetRequestStream())
                {
                    ms.WriteTo(stream);
                }
            }

            return HttpHelper.ReadAllResponseText(request, this.Encoding);
        }
        #endregion
    }
}
