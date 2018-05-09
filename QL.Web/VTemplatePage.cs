/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  VTemplatePage
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.SessionState;
using System.Web;
using System.ComponentModel;
using System.Threading;
using QL.Core;
using QL.Core.Data;
using QL.Core.Extensions;
using System.Reflection;
using VTemplate.Engine;
using System.IO;
using System.Collections;
using System.IO.Compression;
namespace QL.Web
{
    /// <summary>
    /// 采用VT模模板的Web页面基类
    /// </summary>
    public abstract class VTemplatePage
        : IHttpHandler, IRequiresSessionState
    {
        #region IHttpHandler 成员
        /// <summary>
        /// 是否可重用
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        bool IHttpHandler.IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// 处理每个请求
        /// </summary>
        /// <param name="context"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            this.InitHttpContext(context);

            try
            {
                this.OnInit(EventArgs.Empty);
                if (!this.OnProcessAjaxHandler())
                {
                    //如果未处理AjaxHandler则继续处理流程
                    this.OnLoad(EventArgs.Empty);
                    this.OnRender(EventArgs.Empty);
                }
            }
            catch (ThreadAbortException)
            {
                if (this.DocumentIsLoaded && !_IsPageRender && !Response.IsRequestBeingRedirected)
                {
                    //输出最后的数据
                    this.Document.Render(Response.Output);
                }
            }
            catch (Exception ex)
            {
                //触发错误通知事件
                this.Context.AddError(ex);
                this.OnError(this, EventArgs.Empty);
                if (this.Context.Error != null) throw;
            }
            finally
            {
                this.OnEnd(EventArgs.Empty);
            }
        }
        #endregion

        #region  Context、Request、Response、Server、Session、Application
        /// <summary>
        /// 初始化HTTP上下文参数
        /// </summary>
        /// <param name="context"></param>
        private void InitHttpContext(HttpContext context)
        {
            this.Context = context;
            this.Application = context.Application;
            this.Request = context.Request;
            this.Response = context.Response;
            this.Server = context.Server;
            this.Session = context.Session;
        }
        /// <summary>
        /// 获取与该页关联的 HttpContext 对象。
        /// </summary>
        public HttpContext Context { get; private set; }
        /// <summary>
        /// 获取请求的页的 HttpRequest 对象。
        /// </summary>
        public HttpRequest Request { get; private set; }
        /// <summary>
        /// 获取与该 Page 对象关联的 HttpResponse 对象。该对象使您得以将 HTTP 响应数据发送到客户端，并包含有关该响应的信息。
        /// </summary>
        public HttpResponse Response { get; private set; }
        /// <summary>
        /// 获取 Server 对象
        /// </summary>
        public HttpServerUtility Server { get; private set; }
        /// <summary>
        /// 获取 ASP.NET 提供的当前 Session 对象。
        /// </summary>
        public HttpSessionState Session { get; private set; }
        /// <summary>
        /// 为当前 Web 请求获取 HttpApplicationState 对象。
        /// </summary>
        public HttpApplicationState Application { get; private set; }
        #endregion
        
        #region 获取Ajax委托方法
        /// <summary>
        /// 
        /// </summary>
        private static Hashtable AjaxHandlerMethods = new Hashtable();
        /// <summary>
        /// 根据名称获取对应的Ajax委托方法
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual List<MethodInfo> GetAjaxHandlerMethods(string name)
        {
            var type = this.GetType();
            Dictionary<string, List<MethodInfo>> cacheItems = null;
            if (!AjaxHandlerMethods.ContainsKey(type))
            {
                lock (AjaxHandlerMethods)
                {
                    if (!AjaxHandlerMethods.ContainsKey(type))
                    {
                        Dictionary<string, List<MethodInfo>> items = new Dictionary<string, List<MethodInfo>>(10, StringComparer.OrdinalIgnoreCase);
                        MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        foreach (MethodInfo method in methodInfos)
                        {
                            var attributes = method.GetCustomAttributes(typeof(AjaxHandlerMethodAttribute), false);
                            if (attributes.Length > 0)
                            {
                                foreach (AjaxHandlerMethodAttribute attr in attributes)
                                {
                                    string methodName = string.IsNullOrEmpty(attr.Name) ? method.Name : attr.Name;
                                    var ms = items.GetOrAdd<string, List<MethodInfo>>(methodName, () =>
                                    {
                                        return new List<MethodInfo>();
                                    });
                                    if (!ms.Contains(method)) ms.Add(method);
                                }
                            }
                        }
                        AjaxHandlerMethods.Add(type, items);
                        cacheItems = items;
                    }
                }
            }
            if (cacheItems == null)
                cacheItems = AjaxHandlerMethods[type] as Dictionary<string, List<MethodInfo>>;

            return cacheItems.ContainsKey(name) ? cacheItems[name] : null;
        }
        #endregion

        #region 页面处理流程
        /// <summary>
        /// 页面初始化开始
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnInit(EventArgs e) { }


        /// <summary>
        /// 处理Ajax委托事件
        /// </summary>
        /// <returns>如果返回true则将终止页面执行,否则不会终止</returns>
        protected virtual bool OnProcessAjaxHandler()
        {
            string ajaxHandler = this.Request.QueryString["ajaxhandler"];
            if (string.IsNullOrEmpty(ajaxHandler)) ajaxHandler = this.Request.Form["ajaxhandler"];
            if (!string.IsNullOrEmpty(ajaxHandler))
            {
                List<MethodInfo> methods = this.GetAjaxHandlerMethods(ajaxHandler);
                if (methods == null) return false;

                foreach (MethodInfo method in methods)
                {
                    object[] attributes = method.GetCustomAttributes(typeof(AjaxHandlerMethodAttribute), false);
                    foreach(AjaxHandlerMethodAttribute att in attributes)
                    {
                        if (!att.AllowBrowserCache)
                        {
                            Response.Cache.SetAllowResponseInBrowserHistory(false);
                            Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        }
                        if (!string.IsNullOrEmpty(att.ContentType))
                        {
                            Response.ContentType = att.ContentType;
                        }
                        else
                        {
                            switch (att.ReturnType)
                            {
                                case AjaxHandlerMethodReturnType.Text:
                                    Response.ContentType = "text/plain";
                                    break;
                                case AjaxHandlerMethodReturnType.Json:
                                    Response.ContentType = "application/x-javascript";
                                    break;
                            }
                        }

                        object result = method.Invoke(method.IsStatic ? null : this, null);
                        switch (att.ReturnType)
                        {
                            case AjaxHandlerMethodReturnType.Text:
                                if (result != null) Response.Write(result.ToString());
                                break;
                            case AjaxHandlerMethodReturnType.Json:
                                Response.Write(result.ToJson());
                                break;
                            case AjaxHandlerMethodReturnType.Auto:
                                if (result != null)
                                {
                                    //有返回值
                                    if (string.IsNullOrEmpty(att.ContentType))
                                        Response.ContentType = "application/x-javascript";
                                    Response.Write(result.ToJson());
                                }
                                break;
                        }
                        if (att.IsTerminative)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 装载页面数据
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnLoad(EventArgs e)
        {
            if (this.IsPostBack)
            {
                //POST方式则不处理缓存数据
                this.InitPageTemplate();
            }
            else
            {
                //优先装载缓存数据
                if (!this.LoadPageCache())
                {
                    this.InitPageTemplate();
                }
            }
        }
        /// <summary>
        /// 是否已呈现过页面
        /// </summary>
        private bool _IsPageRender = false;
        /// <summary>
        /// 呈现页面数据
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnRender(EventArgs e)
        {
            _IsPageRender = true;
            if (!this.IsPostBack)
            {
                //GET方式访问才保存页面缓存
                if (this.SavePageCache())
                {
                    //直接从缓存里输出，避免再次解析数据
                    if(this.LoadPageCache()) return;
                }
            }
            //输出页面数据
            if (this.DocumentIsLoaded)
            {
                this.Document.Render(Response.Output);
            }
        }

        /// <summary>
        /// 页面已结束
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnEnd(EventArgs e)
        {

        }

        /// <summary>
        /// 初始化当前访问页面的模版数据
        /// </summary>
        public abstract void InitPageTemplate();

        /// <summary>
        /// 当页面发生错误时的事件
        /// </summary>
        protected virtual void OnError(object sender, EventArgs e)
        {
            Exception ex = this.Context.Error;
            if (ex != null)
            {
                //进行页面跳转
                string url = this.Configuration.PageErrorRedirectUrl;
                if (!string.IsNullOrEmpty(url))
                {
                    this.Context.ClearError();

                    if (!Request.Url.ToString().EndsWith(url, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //当前发生错误的页面不是错误提示页则跳转
                        Response.Redirect(url);
                    }
                }
            }
        }
        #endregion

        #region 模版处理
        private TemplateDocument _Document = null;
        /// <summary>
        /// 当前页面的模版文档对象
        /// </summary>
        public TemplateDocument Document
        {
            get
            {
                return _Document;
            }
            protected set
            {
                _Document = value;
            }
        }

        /// <summary>
        /// 当前页面的模版文档的配置参数
        /// </summary>
        protected virtual TemplateDocumentConfig DocumentConfig
        {
            get
            {
                return TemplateDocumentConfig.Default;
            }
        }

        /// <summary>
        /// 装载页面模版文件
        /// </summary>
        /// <param name="filename">要读取的文件地址</param>
        public virtual void LoadTemplateFromFile(string filename)
        {
            LoadTemplateFromFile(filename, HttpContext.Current.Request.ContentEncoding);
        }

        /// <summary>
        /// 装载页面模版文件
        /// </summary>
        /// <param name="filename">要读取的文件地址</param>
        /// <param name="encoding">读取文件时使用的编码</param>
        public virtual void LoadTemplateFromFile(string filename, Encoding encoding)
        {
            filename = Utility.ToAbsolutePath(filename);
            this._Document = TemplateDocument.FromFileCache(filename, encoding, this.DocumentConfig);

            this._Document.Variables.SetValue("Page", this);
            this._Document.Variables.SetValue("Page.SitePath", this.Configuration.ToRelativeSitePath(""));
            this._Document.Variables.SetValue("Page.SiteUrl", this.Configuration.ToRelativeSiteUrl(""));
            this._Document.Variables.SetValue("Page.TemplatePath", this.Configuration.ToRelativeTemplatePath(""));
        }

        /// <summary>
        /// 装载当前访问页面的模版
        /// </summary>
        protected virtual void LoadCurrentPageTemplate()
        {
            string filePath = VirtualPathUtility.GetDirectory(this.Request.FilePath);
            string fileName = Path.GetFileNameWithoutExtension(this.Request.PhysicalPath);

            string rootPath = this.Configuration.RootPath;

            //去掉站点根路径部分，以便转换为相对于模板路径，如将"/products/"转换为"products/"
            if (filePath.Length >= rootPath.Length) filePath = filePath.Remove(0, rootPath.Length);
            fileName = string.Concat(VirtualPathUtility.AppendTrailingSlash(filePath), fileName);

            this.LoadTemplateFromFile(this.Configuration.GetTemplateFile(fileName), this.Configuration.TemplateFileEncoding);
        }
        #endregion

        #region 常用属性
        /// <summary>
        /// 返回判断当前页面模板文档是否已装载
        /// </summary>
        public bool DocumentIsLoaded
        {
            get
            {
                return _Document != null;
            }
        }

        /// <summary>
        /// 当前页面是否是以POST方式访问
        /// </summary>
        public bool IsPostBack
        {
            get
            {
                return "POST".Equals(this.Request.HttpMethod, StringComparison.InvariantCultureIgnoreCase);
            }
        }
        #endregion

        #region 处理页面缓存
        /// <summary>
        /// 当前页面的缓存过期时效(单位:秒钟,默认为1小时,如果小于等于0则不缓存)
        /// </summary>
        protected virtual int PageCacheExpireTime
        {
            get
            {
                return 3600;
            }
        }
        /// <summary>
        /// 当前页面的缓存文件绝对地址(如果需要使用页面缓存则必须重写此属性并返回真实地址)
        /// </summary>
        protected virtual string PageCacheFileName
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// 当前页面的缓存文件是否采用Gzip压缩
        /// </summary>
        protected virtual bool UseGzipCompressedPageCache
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 装载页面缓存,此方法默认在OnLoad事件发生时自动调用
        /// </summary>
        /// <returns>是否装载页面缓存成功</returns>
        protected virtual bool LoadPageCache()
        {
            string pageCacheFileName = this.PageCacheFileName;
            if (this.PageCacheExpireTime > 0 && !string.IsNullOrEmpty(pageCacheFileName))
            {
                try
                {
                    string cacheFileName = this.UseGzipCompressedPageCache ? pageCacheFileName + ".gz" : pageCacheFileName;
                    FileInfo cacheFile = new FileInfo(cacheFileName);
                    if (cacheFile.Exists)
                    {
                        //判断文件是否已过期
                        var lastWriteTime = cacheFile.LastWriteTime;
                        TimeSpan span = DateTime.Now.Subtract(lastWriteTime);
                        int totalSpanSeconds = (int)span.TotalSeconds;
                        if (totalSpanSeconds >= 0 && totalSpanSeconds <= this.PageCacheExpireTime)
                        {
                            //输出缓存文件数据
                            this.Response.Clear();
                            this.Response.Cache.SetLastModified(lastWriteTime);
                            this.Response.Cache.SetCacheability(HttpCacheability.Public);
                            this.Response.Cache.SetExpires(DateTime.Now.AddSeconds(this.PageCacheExpireTime - totalSpanSeconds));

                            if (this.UseGzipCompressedPageCache)
                            {
                                //判断客户端浏览器是否支持gzip流
                                string acceptEncoding = Request.ServerVariables["HTTP_ACCEPT_ENCODING"];
                                if (string.IsNullOrEmpty(acceptEncoding) || acceptEncoding.IndexOf("gzip") == -1)
                                {
                                    //不支持gzip流，所以需要解压
                                    using (GZipStream gzipStream = new GZipStream(cacheFile.OpenRead(), CompressionMode.Decompress, false))
                                    {
                                        int count;
                                        byte[] buffer = new byte[4096];
                                        while ((count = gzipStream.Read(buffer, 0, buffer.Length)) != 0)
                                        {
                                            Response.OutputStream.Write(buffer, 0, count);
                                        }
                                        buffer = null;
                                    }
                                }
                                else
                                {
                                    this.Response.AppendHeader("Content-Encoding", "gzip");
                                    this.Response.WriteFile(cacheFileName);
                                }
                            }
                            else
                            {
                                this.Response.WriteFile(cacheFileName);
                            }
                            return true;
                        }
                    }
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// 保存页面缓存,此方法默认在OnRender事件发生时自动调用
        /// </summary>
        protected virtual bool SavePageCache()
        {
            string pageCacheFileName = this.PageCacheFileName;
            if (this.PageCacheExpireTime > 0 && !string.IsNullOrEmpty(pageCacheFileName))
            {
                //获取模版文档的数据
                if (this.DocumentIsLoaded)
                {
                    try
                    {
                        string path = Path.GetDirectoryName(pageCacheFileName);
                        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                        if (this.UseGzipCompressedPageCache)
                        {
                            using (FileStream stream = new FileStream(pageCacheFileName + ".gz", FileMode.Create, FileAccess.ReadWrite))
                            {
                                using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Compress))
                                {
                                    using (StreamWriter writer = new StreamWriter(gzipStream, this.Document.Charset))
                                    {
                                        this.Document.Render(writer);
                                    }
                                }
                            }
                        }
                        else
                        {
                            this.Document.RenderTo(pageCacheFileName, this.Document.Charset);
                        }

                        return true;
                    }
                    catch { }
                }
            }
            return false;
        }
        #endregion

        #region 返回当前Web站点的配置
        ///<summary>
        ///返回当前Web站点的配置
        ///</summary>
        public abstract WebSiteConfiguration Configuration
        {
            get;
        }
        #endregion
    }
}
