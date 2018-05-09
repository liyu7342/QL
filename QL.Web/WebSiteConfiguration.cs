/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  WebSiteConfiguration
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QL.Core;
using QL.Core.Extensions;
using System.IO;
using QL.Database;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;
using System.Text.RegularExpressions;
using QL.Core.Data;
using QL.Core.Caching;
namespace QL.Web
{
    /// <summary>
    /// Web站点的配置文件. 此配置的参考示例文件，可在"doc"目录下查看"website.default.config"文件
    /// </summary>
    public class WebSiteConfiguration
        : DbObject
    {
        private ICaching _cache;
        /// <summary>
        /// 
        /// </summary>
        public WebSiteConfiguration()
        {
            //处理几个内部特有的属性值
            this.AddInternalData("HostName", () => this.HostName, (o => this.HostName = o.As<string>()));
            this.AddInternalData("RootPath", () => this.RootPath, (o => this.RootPath = o.As<string>()));
            this.AddInternalData("AppPath", () => this.AppPath, (o => this.AppPath = o.As<string>()));
            this.AddInternalData("TemplatePath", () => this.TemplatePath, (o => this.TemplatePath = o.As<string>()));
            this.AddInternalData("UploadFilePath", () => this.UploadFilePath, (x) => this.UploadFilePath = x.As<string>());
            this.AddInternalData("UploadFileUrl", () => this.UploadFileUrl, (x) => this.UploadFileUrl = x.As<string>());
            this.AddInternalData("SystemLogPath", () => this.SystemLogPath, (x) => this.SystemLogPath = x.As<string>());
        }

        #region 站点的配置参数
        /// <summary>
        /// 
        /// </summary>
        private string _HostName;
        /// <summary>
        ///设置或返回站点的域名,如: "http://www.host.com/"
        /// </summary>
        public string HostName
        {
            get
            {
                string hostName = this._HostName;
                if (string.IsNullOrEmpty(hostName))
                {
                    hostName = QL.Core.WebContext.Request.HostName;
                }
                return hostName;
            }
            set
            {
                _HostName = value;
                if (!string.IsNullOrEmpty(_HostName))
                {
                    _HostName = _HostName.Replace("\\", "/").TrimEnd('/');
                }
            }
        }

        private string _RootPath;
        /// <summary>
        ///设置或返回站点所在的根目录,如: "/"
        /// </summary>
        public string RootPath
        {
            get
            {
                string path = this._RootPath;
                if (string.IsNullOrEmpty(path))
                {
                    path = "/";
                }
                return path;
            }
            set
            {
                _RootPath = value;
                if (!string.IsNullOrEmpty(_RootPath))
                {
                    _RootPath = _RootPath.Replace("\\", "/");
                    if (!_RootPath.EndsWith("/")) _RootPath += "/";
                    if (!_RootPath.StartsWith("/")) _RootPath = "/" + _RootPath;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private string _AppPath;
        /// <summary>
        ///设置或返回站点的绝对路径地址,如: "c:\wwwroot\web\"
        /// </summary>
        public string AppPath
        {
            get
            {
                string path = this._AppPath;
                if (string.IsNullOrEmpty(path))
                {
                    path = this.RootPath;
                }
                return Utility.ToAbsolutePath(path);
            }
            set
            {
                this._AppPath = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private string _TemplatePath;
        /// <summary>
        ///设置或返回站点模板文件存放的目录,如: "template/default/"
        /// </summary>
        public string TemplatePath
        {
            get
            {
                string path = this._TemplatePath;
                if (string.IsNullOrEmpty(path))
                {
                    path = this.ToRelativeSitePath("template/default/");
                }
                return path;
            }
            set
            {
                _TemplatePath = value;
                if (!string.IsNullOrEmpty(_TemplatePath))
                {
                    _TemplatePath = _TemplatePath.Replace("\\", "/");
                    if (!_TemplatePath.EndsWith("/")) _TemplatePath += "/";
                }
            }
        }
        /// <summary>
        /// 设置或返回站点模板文件的编码,如: "gb2312"
        /// </summary>
        public string TemplateFileCharset
        {
            get
            {
                return this.GetData<string>("TemplateFileCharset", null);
            }
            set
            {
                this["TemplateFileCharset"] = value;
            }
        }

        /// <summary>
        /// 获取站点模板文件所用的编码
        /// </summary>
        public Encoding TemplateFileEncoding
        {
            get
            {
                string charset = this.TemplateFileCharset;
                if (string.IsNullOrEmpty(charset)) return Encoding.Default;

                return Encoding.GetEncoding(charset);
            }
        }

        /// <summary>
        ///设置或返回站点模板文件的扩展名,如: ".html"
        /// </summary>
        public string TemplateFileExt
        {
            get
            {
                return this.GetData<string>("TemplateFileExt", null);
            }
            set
            {
                this["TemplateFileExt"] = value;
            }
        }
        #endregion

        #region 日记目录
        private string _SystemLogPath;
        /// <summary>
        /// 设置或返回系统日志存储目录，默认为，站点目录下的"logfiles"
        /// </summary>
        public string SystemLogPath
        {
            get
            {
                string path = this._SystemLogPath;
                if (string.IsNullOrEmpty(path))
                {
                    path = this.ToAbsoluteAppPath("logfiles/");
                }
                return path;
            }
            set
            {
                this._SystemLogPath = value;
                if (!string.IsNullOrEmpty(value))
                {
                    _SystemLogPath = _SystemLogPath.Replace("/", "\\");
                    if (!_SystemLogPath.EndsWith("\\")) _SystemLogPath += "\\";
                    _SystemLogPath = this.ToAbsoluteAppPath(_SystemLogPath);
                }
            }
        }
        #endregion

        #region 上传文件的设置
        /// <summary>
        /// 
        /// </summary>
        private string _UploadFilePath;
        /// <summary>
        /// 设置或返回保存站点上传文件的绝对地址 "c:\wwwroot\web\uploadfiles\"
        /// </summary>
        public string UploadFilePath
        {
            get
            {
                string path = this._UploadFilePath;
                if (string.IsNullOrEmpty(path))
                {
                    path = this.ToAbsoluteAppPath("uploadfiles/");
                }
                return path;
            }
            set
            {
                this._UploadFilePath = value;
                if (!string.IsNullOrEmpty(value))
                {
                    _UploadFilePath = _UploadFilePath.Replace("/", "\\");
                    if (!_UploadFilePath.EndsWith("\\")) _UploadFilePath += "\\";
                    _UploadFilePath = this.ToAbsoluteAppPath(_UploadFilePath);
                }
            }
        }
        private string _UploadFileUrl;
        /// <summary>
        ///设置或返回站点上传文件的URL访问地址,如: "http://files.xx.com/"
        /// </summary>
        public string UploadFileUrl
        {
            get
            {
                string url = this._UploadFileUrl;
                if (string.IsNullOrEmpty(url))
                {
                    url = this.ToRelativeSiteUrl("uploadfiles/");
                }
                return url;
            }
            set
            {
                this._UploadFileUrl = value;
                if (!string.IsNullOrEmpty(value))
                {
                    _UploadFileUrl = _UploadFileUrl.Replace("\\", "/");
                    if (!_UploadFileUrl.EndsWith("/")) _UploadFileUrl += "/";
                    _UploadFileUrl = this.ToRelativeSiteUrl(_UploadFileUrl);
                }
            }
        }
        /// <summary>
        /// 返回站点上传文件保存时的文件名地址格式模式。
        /// 可带以下标记字符:
        /// {y} = 当前年份;
        /// {m} = 当前两位数字的月份;
        /// {d} = 当前两位数字的日份;
        /// {h} = 当前两位数字的小时;
        /// {n} = 当前两位数字的分钟;
        /// {s} = 当前两位数字的秒钟;
        /// {ms} = 当前的毫秒;
        /// {name} = 当前图片文件的原文件名(不带扩展名};
        /// {ext}  = 当前图片文件的扩展名(不带"."号);
        /// {guid} = GUID值;
        /// {rnd}  = 一个随机数字;
        /// </summary>
        public string UploadFileNamePattern
        {
            get
            {
                string pattern = this.GetData<string>("UploadFileNamePattern");
                if (string.IsNullOrEmpty(pattern))
                {
                    pattern = "{y}/{m}/{d}/{y}{m}{d}{h}{n}{rnd}.{ext}";
                }
                return pattern;
            }
            set
            {
                this["UploadFileNamePattern"] = value;
            }
        }

        /// <summary>
        /// 设置或返回站点上传文件的最大大小(单位:KB)
        /// </summary>
        public int UploadFileMaxSize
        {
            get
            {
                return this.GetData<int>("UploadFileMaxSize");
            }
            set
            {
                this["UploadFileMaxSize"] = value;
            }
        }

        /// <summary>
        /// 返回站点允许上传的文件扩展名。各扩展名之间采用“;”号相隔开，如".jpg;.jpeg;.png;.gif"
        /// </summary>
        public string UploadFileAllowExts
        {
            get
            {
                return this.GetData<string>("UploadFileAllowExts");
            }
            set
            {
                this["UploadFileAllowExts"] = value;
            }
        }

        /// <summary>
        /// 设置或返回水印图片地址
        /// </summary>
        public string WatermarkImage
        {
            get
            {
                return this.GetData<string>("WatermarkImage");
            }
            set
            {
                this["WatermarkImage"] = value;
            }
        }
        #endregion

        #region 项目的配置参数
        /// <summary>
        /// 设置或返回数据访问者驱动的实例，格式: "类名,程序集"
        /// 例如: "QL.Project.SqlServerDataAccessor,QL.Project"
        /// </summary>
        public string DbAccessorInstance
        {
            get
            {
                return this.GetData<string>("DbAccessorInstance", null);
            }
            set
            {
                this["DbAccessorInstance"] = value;
            }
        }

        /// <summary>
        /// 设置或返回数据库的连接字符串,如SQL Server源: "Data Source=(local);User Id=用户;Password=密码;Initial Catalog=表名;Pooling=true"
        /// </summary>
        public string DbConnectionString
        {
            get
            {
                return this.GetData<string>("DbConnectionString", null);
            }
            set
            {
                this["DbConnectionString"] = value;
            }
        }
        #endregion

        #region 页面发生错误时的处理
        /// <summary>
        /// 页面发生错误时的跳转地址
        /// </summary>
        public string PageErrorRedirectUrl
        {
            get
            {
                return this.GetData<string>("PageErrorRedirectUrl", null);
            }
            set
            {
                this["PageErrorRedirectUrl"] = value;
            }
        }
        #endregion

        #region 地址转换
        /// <summary>
        /// 将文件路径转换为相对于站点的绝对路径
        /// </summary>
        /// <param name="path">路径地址,如"news/view.aspx"</param>
        /// <returns>绝对文件地址,如"d:\wwwroot\news\view.aspx"</returns>
        public string ToAbsoluteAppPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return this.AppPath;
            if (!Utility.IsAbsolutePath(path))
            {
                path = System.IO.Path.Combine(this.AppPath, ToRelativeSitePath(path).TrimStart('\\', '/'));
            }
            return path;
        }
        /// <summary>
        /// 将文件路径转换为相对于站点根路径的相对路径
        /// </summary>
        /// <param name="path">路径地址,如"news/view.aspx"</param>
        /// <returns>相对于根目录的地址,如"/news/view.aspx"</returns>
        public string ToRelativeSitePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return this.RootPath;
            if (!Utility.IsAbsolutePath(path))
            {
                if (!path.StartsWith("/") && !path.StartsWith("\\"))
                {
                    path = string.Concat(this.RootPath, path);
                }
            }
            return path;
        }
        /// <summary>
        /// 将文件路径转换为相对于站点的URL地址
        /// </summary>
        /// <param name="path">路径地址,如"news/view.aspx"</param>
        /// <returns>相对站点地址,如"http://www.host.com/news/view.aspx"</returns>
        public string ToRelativeSiteUrl(string path)
        {
            if (string.IsNullOrEmpty(path)) return this.HostName;
            if (!Utility.IsAbsolutePath(path))
            {
                path = (new Uri(new Uri(this.HostName), this.ToRelativeSitePath(path))).ToString();
            }
            return path;
        }

        /// <summary>
        /// 将文件路径转换为相对于站点前台模板地址的相对路径
        /// </summary>
        /// <param name="path">路径地址,如"news/view.html"</param>
        /// <returns>相对模板路径的地址,如"/template/default/news/view.html"</returns>
        public string ToRelativeTemplatePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return this.ToRelativeSitePath(this.TemplatePath);
            if (!Utility.IsAbsolutePath(path))
            {
                if (!path.StartsWith("/") && !path.StartsWith("\\"))
                {
                    path = this.ToRelativeSitePath(string.Concat(this.TemplatePath, path));
                }
            }
            return path;
        }

        /// <summary>
        /// 将文件路径转换为相对于站点前台模板地址的绝对路径
        /// </summary>
        /// <param name="path">路径地址,如"news/view.html"</param>
        /// <returns>相对模板路径的地址,如"/template/default/news/view.html"</returns>
        public string ToAbsoluteTemplatePath(string path)
        {
            return this.ToAbsoluteAppPath(this.ToRelativeTemplatePath(path));
        }

        /// <summary>
        /// 获取某个文件对应的模板文件绝对地址
        /// </summary>
        /// <param name="file">文件地址（可为相对或绝对地址），如果非以模板扩展名结尾则会自动补上。如："article/list"</param>
        /// <returns>绝对地址，如："c:\wwwroot\web\template\default\article\list.html"</returns>
        public string GetTemplateFile(string file)
        {
            if (string.IsNullOrEmpty(file)) return string.Empty;

            if (!string.IsNullOrEmpty(this.TemplateFileExt))
            {
                if (!file.EndsWith(this.TemplateFileExt, StringComparison.OrdinalIgnoreCase))
                    file += this.TemplateFileExt;
            }
            return this.ToAbsoluteTemplatePath(file);
        }
        #endregion

        #region 上传文件处理
        /// <summary>
        /// 将文件路径转换为相对于站点日志存储路径的绝对地址
        /// </summary>
        /// <param name="file">文件地址,如"logs/20110603.log" 返回的路径为 "c:\wwwroot\logs\20110603.log"</param>
        /// <returns>绝对文件地址,如"c:\wwwroot\logs\20110603.log"</returns>
        public string ToAbsoluteLogFilePath(string file)
        {
            if (string.IsNullOrEmpty(file)) return this.SystemLogPath;
            if (!Utility.IsAbsolutePath(file))
            {
                file = Path.Combine(this.SystemLogPath, file.TrimStart('\\', '/'));
            }
            return file;
        }
        /// <summary>
        /// 将文件路径转换为相对于站点上传文件保存路径的相对地址
        /// </summary>
        /// <param name="file">文件地址,如"news/f3ddd.jpg" 返回的地址为"http://files.hostname.com/news/f3ddd.jpg"</param>
        /// <returns>相对于站点上传目录的文件地址,如"http://files.hostname.com/news/f3ddd.jpg"</returns>
        public string ToRelativeUploadFileUrl(string file)
        {
            if (string.IsNullOrEmpty(file)) return this.UploadFileUrl;

            if (Utility.IsAbsolutePath(file)) return file;

            return (new Uri(new Uri(this.UploadFileUrl), file)).ToString();
        }
        /// <summary>
        /// 将文件路径转换为相对于站点上传文件保存路径的绝对地址
        /// </summary>
        /// <param name="file">文件地址,如"news/f3ddd.jpg" 返回的路径为 "c:\wwwroot\uploadfile\news\f3ddd.jpg"</param>
        /// <returns>绝对文件地址,如"c:\wwwroot\uploadfile\news\f3ddd.jpg"</returns>
        public string ToAbsoluteUploadFilePath(string file)
        {
            if (string.IsNullOrEmpty(file)) return this.UploadFilePath;
            if (!Utility.IsAbsolutePath(file))
            {
                file = Path.Combine(this.UploadFilePath, file.TrimStart('\\', '/'));
            }
            return file;
        }
        /// <summary>
        /// 建立某个文件对应的上传文件地址名
        /// </summary>
        /// <param name="fileName">文件名,如:files.jpg则解析后的文件名为"200702303212.jpeg"</param>
        /// <returns></returns>
        public string CreateUploadFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return string.Empty;
            fileName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(this.UploadFileNamePattern)) return fileName;
            
            DateTime time = DateTime.Now;
            return this.UploadFileNamePattern.Replace((key) =>
            {
                switch (key.ToLower())
                {
                    case "y": return time.Year.ToString();
                    case "m": return time.Month.ToString("00");
                    case "d": return time.Day.ToString("00");
                    case "h": return time.Hour.ToString("00");
                    case "n": return time.Minute.ToString("00");
                    case "s": return time.Second.ToString("00");
                    case "ms": return time.Millisecond.ToString();
                    case "name": return Path.GetFileNameWithoutExtension(fileName);
                    case "ext": return Path.GetExtension(fileName).TrimStart('.');
                    case "guid": return Guid.NewGuid().ToString();
                    case "rnd": return Utility.Random.Next(100, 100000).ToString();
                    default:
                        return null;
                }
            }, "{", "}");
        }

        /// <summary>
        /// 是否是上传文件(判断是否和上传文件保存地址模式相同,如地址模式为:{y}/{y}{m}{d}.{ext},则2008/20080112.gif符合,而2008/022343small.gif不符合)
        /// </summary>
        /// <param name="fileName">经过解析过路径模式的上传文件名,如:2008/20080112.gif</param>
        /// <returns></returns>
        public bool IsUploadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(this.UploadFileNamePattern)) return false;

            string pattern = Regex.Escape(this.UploadFileNamePattern);
            pattern = pattern.Replace((key) =>
            {
                switch (key.ToLower())
                {
                    case "y": return @"\d{4}";
                    case "m":
                    case "d":
                    case "h":
                    case "n":
                    case "s": return @"\d{2}";
                    case "ms": return @"\d{1,3}";
                    case "name": return @"[^\\/\:\*\?\""<>\|\.]+";
                    case "ext": return @"[^\\/\:\*\?\""<>\|\.]+";
                    case "guid": return @"\d{3,7}";
                    case "rnd": return @"\{[\dA-Za-z]{8}\-[\dA-Za-z]{4}\-[\dA-Za-z]{4}\-[\dA-Za-z]{4}\-[\dA-Za-z]{12}\}";
                    default:
                        return null;
                }
            }, @"\{", "}");

            pattern += "^" + pattern + "$";
            return fileName.IsMatch(pattern);
        }

        /// <summary>
        /// 删除已上传的文件
        /// </summary>
        /// <param name="fileName">文件名,格式可以为与地址模式相同的地址(如:2008/01/01/2008010121233.jpg),也可以为绝对路径(如:http://files.hostname.com/2008/01/01/2008010121233.jpg)</param>
        /// <returns></returns>
        public bool RemoveUploadFile(string fileName)
        {
            bool flag = false;
            if (!string.IsNullOrEmpty(fileName))
            {
                if (fileName.Length > this.UploadFileUrl.Length
                    && fileName.StartsWith(this.UploadFileUrl, StringComparison.OrdinalIgnoreCase))
                {
                    //绝对路径则转换为与地址模式相同的路径
                    fileName = fileName.Substring(this.UploadFileUrl.Length);
                }
                if (this.IsUploadFile(fileName))
                {
                    //与地址模式相同的地址
                    try
                    {
                        File.Delete(this.ToAbsoluteUploadFilePath(fileName));
                        flag = true;
                    }
                    catch
                    {
                        flag = false;
                    }
                }
            }
            return flag;
        }

        /// <summary>
        /// 保存从客户端上传的文件
        /// </summary>
        /// <param name="postedFile">要保存的上传文件</param>
        /// <param name="fileName">返回的最终保存在服务器上的地址,如:2008/01/01/2008010121233.jpg</param>
        /// <returns></returns>
        //public UploadFileSaveResult SaveUploadFile(HttpPostedFile postedFile, out string fileName)
        //{
        //    fileName = null;
        //    if (postedFile == null || postedFile.ContentLength < 1) return UploadFileSaveResult.EmptyFile;

        //    //判断是否超出大小
        //    if (this.UploadFileMaxSize != 0 && postedFile.ContentLength > (this.UploadFileMaxSize * 1024))
        //        return UploadFileSaveResult.DeniableFileSize;

        //    //判断格式是否正确
        //    if (!string.IsNullOrEmpty(this.UploadFileAllowExts)
        //        && !this.UploadFileAllowExts.IsContain(Path.GetExtension(postedFile.FileName), ";"))
        //        return UploadFileSaveResult.DeniableFileExt;

        //    //保存文件
        //    fileName = this.CreateUploadFileName(postedFile.FileName);

        //    try
        //    {
        //        string filePath = this.ToAbsoluteUploadFilePath(fileName);
        //        string fileDirectory = Path.GetDirectoryName(filePath);
        //        //建立目录
        //        if (!Directory.Exists(fileDirectory)) Directory.CreateDirectory(fileDirectory);
        //        postedFile.SaveAs(filePath);
        //    }
        //    catch
        //    {
        //        fileName = null;
        //        return UploadFileSaveResult.Error;
        //    }
        //    return UploadFileSaveResult.Success;
        //}


        public UploadFileSaveResult SaveUploadFile(HttpPostedFile postedFile, out string fileName)
        {
            return this.SaveUploadFile(postedFile, null, out fileName);
        }

        public UploadFileSaveResult SaveUploadFile(HttpPostedFile postedFile, string pathName, out string fileName)
        {
            fileName = null;
            if (!string.IsNullOrEmpty(pathName))
            {
                pathName = pathName.Replace("..", "").Trim(@"/\".ToCharArray());
            }
            if ((postedFile == null) || (postedFile.ContentLength < 1))
            {
                return UploadFileSaveResult.EmptyFile;
            }
            if ((this.UploadFileMaxSize != 0) && (postedFile.ContentLength > (this.UploadFileMaxSize * 0x400)))
            {
                return UploadFileSaveResult.DeniableFileSize;
            }
            if (!string.IsNullOrEmpty(this.UploadFileAllowExts) && !Strings.IsContain(this.UploadFileAllowExts, Path.GetExtension(postedFile.FileName), ";", true))
            {
                return UploadFileSaveResult.DeniableFileExt;
            }
            fileName = this.CreateUploadFileName(postedFile.FileName);
            if (!string.IsNullOrEmpty(pathName))
            {
                fileName = pathName + "/" + fileName;
            }
            try
            {
                string str = this.ToAbsoluteUploadFilePath(fileName);
                string directoryName = Path.GetDirectoryName(str);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                postedFile.SaveAs(str);
            }
            catch
            {
                fileName = null;
                return UploadFileSaveResult.Error;
            }
            return UploadFileSaveResult.Success;
        }
        #endregion

        #region 记录日记
        /// <summary>
        /// 增加日记
        /// </summary>
        /// <param name="logFile">日志文件，可以是相对地址或绝对地址</param>
        /// <param name="logs"></param>
        public void AddLogs(string logFile, string logs)
        {
            string file = this.ToAbsoluteLogFilePath(logFile);
            try
            {
                string p = Path.GetDirectoryName(file);
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);

                File.AppendAllLines(file, new string[]{
                    new string('-', 50),
                    string.Format("记录时间：{0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    logs
                }, Encoding.UTF8);
            }
            catch { }
        }
        #endregion

        #region 打图片水印
        /// <summary>
        /// 对某张图片打水印。注意：如果图片的宽度或高度小于水印图片的宽度或高度，将不会被打水印
        /// </summary>
        /// <param name="imageFile">需要打水印的图片文件地址，可以为绝对或相对地址(相对于站点地址)</param>
        /// <param name="transparence">透明度，值范围0-1（小数)，0：全透明; 1：不透明</param>
        /// <param name="position">水印图片的位置</param>
        public void AddWatermarkImage(string imageFile, float transparence, WatermarkPosition position)
        {
            string watermarkImage = this.WatermarkImage;
            if (string.IsNullOrEmpty(watermarkImage)) return;

            watermarkImage = this.ToAbsoluteAppPath(watermarkImage);
            if (!File.Exists(watermarkImage)) return;

            if (!File.Exists(imageFile)) return;

            try
            {
                using (Stream stream = new MemoryStream(File.ReadAllBytes(imageFile)))
                {
                    using (Bitmap bitmap = (Bitmap)Bitmap.FromStream(stream))
                    {
                        if (bitmap.Watermark(watermarkImage, transparence, position))
                        {
                            bitmap.Save(imageFile);
                        }
                    }
                }
            }
            catch { }
        }
        #endregion

        /// <summary>
        /// 从配置文件里获取站点配置实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configFile">配置文件地址，可以为绝对路径或相对路径(相对于Settings里的基路径)</param>
        /// <returns></returns>
        public static T From<T>(string configFile)
            where T : WebSiteConfiguration, new()
        {
            return Settings.GetFrom<T>(configFile);
        }
 
        public bool DbConnectionKeepAlive
        {
            get
            {
                return base.GetData<bool>("DbConnectionKeepAlive", true);
            }
            set
            {
                base.SetData("DbConnectionKeepAlive", (bool)value);
            }
        }


        public string SlaveDbConnectionStrings
        {
            get
            {
                return base.GetData<string>("SlaveDbConnectionStrings", null);
            }
            set
            {
                base.SetData("SlaveDbConnectionStrings", value);
            }
        }


        public int SlowDbCommandExecuteTime
        {
            get
            {
                return base.GetData<int>("SlowDbCommandExecuteTime", 0);
            }
            set
            {
                base.SetData("SlowDbCommandExecuteTime", (int)value);
            }
        }


        public ICaching Cache
        {
            get
            {
                if (this._cache == null)
                {
                    lock (this)
                    {
                        if (this._cache == null)
                        {
                            this._cache = string.IsNullOrEmpty(this.CachingName) ? QL.Core.Cache.Current : QL.Core.Cache.CreateCaching(this.CachingName);
                        }
                    }
                }
                return this._cache;
            }
        }

        public string CacheKeyPrefix
        {
            get
            {
                return base.GetData<string>("CacheKeyPrefix", null);
            }
            set
            {
                base.SetData("CacheKeyPrefix", value);
            }
        }

        public string CachingName
        {
            get
            {
                return base.GetData<string>("CachingName", null);
            }
            set
            {
                base.SetData("CachingName", value);
                this._cache = null;
            }
        }

        public string CreateCacheKey(string cacheKey)
        {
            if (string.IsNullOrEmpty(this.CacheKeyPrefix))
            {
                return cacheKey;
            }
            return (this.CacheKeyPrefix + cacheKey);
        }
    }
}
