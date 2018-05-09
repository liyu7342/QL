/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  EmailServiceConfiguration
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QL.Core.Net.Mail
{
    /// <summary>
    /// 邮件服务器
    /// </summary>
    public class MailServer
    {
        /// <summary>
        /// 默认端口
        /// </summary>
        internal const int DefaultPort = 25;
        /// <summary>
        /// 默认实例
        /// </summary>
        public MailServer()
        {
            this.Port = MailServer.DefaultPort;
            this.UseLocalService = false;
            this.Timeout = 60000;
            this.EnableSsl = false;
            this.AuthenticationType = "ntlm";
        }
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string HostName
        {
            get;
            set;
        }
        /// <summary>
        /// 服务器的端口
        /// </summary>
        public int Port
        {
            get;
            set;
        }
        /// <summary>
        /// 服务器的连接超时设置，单位：毫秒,默认值是1分钟
        /// </summary>
        public int Timeout
        {
            get;
            set;
        }
        /// <summary>
        /// 是否使用SSL验证
        /// </summary>
        public bool EnableSsl
        {
            get;
            set;
        }
        /// <summary>
        /// 服务器的验证方式,分别有以下几种:login  gssapi  ntlm  WDigest  注:SSL验证方式下无效
        /// </summary>
        public string AuthenticationType
        {
            get;
            set;
        }
        /// <summary>
        /// 服务器的登录帐号
        /// </summary>
        public string UserName
        {
            get;
            set;
        }
        /// <summary>
        /// 服务器的登录帐号密码
        /// </summary>
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// 是否开启了登录授权
        /// </summary>
        public bool EnableAuth
        {
            get
            {
                //非采用本地邮件服务，并且用户名不为空
                return !this.UseLocalService && !string.IsNullOrEmpty(this.UserName);
            }
        }

        /// <summary>
        /// 是否使用本地邮件服务.如果值为true则直接将邮件投递到对方邮件服务器上,而不是通过SMTP邮件服务器中转
        /// </summary>
        public bool UseLocalService { get; set; }

        /// <summary>
        /// 设置或返回邮件信使的域名名称.在SMPT交互中的EHLO或HELO命令时用到,如果不设置默认是发送机器的机器名
        /// </summary>
        public string MessagerHostName { get; set; }
    }
}
