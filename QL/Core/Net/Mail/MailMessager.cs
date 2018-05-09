/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  MailMessager
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Reflection;
using System.Net;
using System.ComponentModel;
using QL.Core.Extensions;
namespace QL.Core.Net.Mail
{
    /// <summary>
    /// 邮件信使
    /// </summary>
    public class MailMessager
    {
        /// <summary>
        /// 采用本地邮件服务实例化
        /// </summary>
        public MailMessager()
        {
            MailServer server = new MailServer();
            server.UseLocalService = true;
            this._Server = server;
        }
        /// <summary>
        /// 构造实例
        /// </summary>
        /// <param name="serverHostName">SMTP服务器地址</param>
        /// <param name="serverUserName">服务器的用户帐号</param>
        /// <param name="serverPassword">服务器的用户密码</param>
        public MailMessager(string serverHostName, string serverUserName, string serverPassword)
        {
            MailServer server = new MailServer();
            server.HostName = serverHostName;
            server.UserName = serverUserName;
            server.Password = serverPassword;
            this._Server = server;
        }
        /// <summary>
        /// 构造实例
        /// </summary>
        /// <param name="serverHostName">SMTP服务器地址</param>
        /// <param name="serverPort">服务器端口</param>
        /// <param name="serverUserName">服务器的用户帐号</param>
        /// <param name="serverPassword">服务器的用户密码</param>
        public MailMessager(string serverHostName, int serverPort, string serverUserName, string serverPassword)
        {
            MailServer server = new MailServer();
            server.HostName = serverHostName;
            server.Port = serverPort;
            server.UserName = serverUserName;
            server.Password = serverPassword;
            this._Server = server;
        }
        /// <summary>
        /// 根据服务器配置构造实例
        /// </summary>
        /// <param name="server">发信的邮件服务器</param>
        public MailMessager(MailServer server)
        {
            this._Server = server;
            if (server != null && !string.IsNullOrEmpty(server.MessagerHostName))
            {
                this.HostName = server.MessagerHostName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private MailServer _Server;
        /// <summary>
        /// 返回当前邮件信使的服务器配置
        /// </summary>
        public MailServer Server
        {
            get
            {
                return _Server;
            }
            set
            {
                _Server = value;
            }
        }

        /// <summary>
        /// 设置或返回邮件信使的域名名称.在SMPT交互中的EHLO或HELO命令时用到,如果不设置默认是发送机器的机器名
        /// </summary>
        public string HostName
        {
            get;
            set;
        }
        #region 发送邮件
        /// <summary>
        /// 获取SMTP Client实例
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private SmtpClient GetSmtpClient(string server, int port)
        {
            //构造SMTP的连接器
            SmtpClient smtp = new SmtpClient(server, port);
            if (!string.IsNullOrEmpty(this.HostName))
            {
                FieldInfo field = smtp.GetType().GetField("localHostName", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null) field.SetValue(smtp, this.HostName);
            }
            //超时设置
            smtp.Timeout = this.Server.Timeout;
            if (this.Server.EnableAuth)
            {
                //设置凭据
                NetworkCredential credential = new NetworkCredential(this.Server.UserName, this.Server.Password);

                if (this.Server.EnableSsl)
                {
                    //SSL验证
                    smtp.Credentials = credential;
                    smtp.EnableSsl = this.Server.EnableSsl;
                }
                else
                {
                    //普通验证
                    smtp.Credentials = credential.GetCredential(server, port, this.Server.AuthenticationType);
                }
            }
            return smtp;
        }

        /// <summary>
        /// 采用本地发送之前需要处理
        /// </summary>
        /// <param name="message"></param>
        protected void BeforeLocalSend(MailMessage message)
        {
            //加入Received Header头;
            string received = null;
            try
            {
                var ips = Dns.GetHostAddresses(message.From.Host);
                string ip = ips.Length > 0 ? ips[0].ToString() : "127.0.0.1";
                received = string.Format("from {0} ([{1}]) by {2} with MailMessager; {3}",
                        this.HostName.IfEmpty(Dns.GetHostName),
                        ip,
                        message.From.Host,
                        DateTime.Now.ToRFC822Time());
                message.Headers.Add("Received", received);
            }
            catch { }
        }


        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="from">发件人</param>
        /// <param name="recipients">收件人</param>
        /// <param name="subject">主题</param>
        /// <param name="body">邮件正文</param>
        public void Send(string from, string recipients, string subject, string body)
        {
            Send(new MailMessage(from, recipients, subject, body));
        }

        /// <summary>
        /// 发送邮件,如果发送过程出现错误则会抛出异常
        /// </summary>
        /// <param name="message">邮件内容</param>
        public void Send(MailMessage message)
        {
            if (this.Server.UseLocalService)   //采用本地邮件服务
            {
                if (message.To.Count < 1) return;

                Exception ex = null ;
                try
                {
                    string[] mailMX = DnsMxRecord.Query(message.To[0]);
                    if (mailMX != null)
                    {
                        this.BeforeLocalSend(message);
                        foreach (string mx in mailMX)
                        {
                            //构造SMTP的连接器
                            SmtpClient smtp = this.GetSmtpClient(mx, MailServer.DefaultPort);                            
                            try
                            {
                                smtp.Send(message);
                                ex = null;
                                break;
                            }
                            catch (Exception e)
                            {
                                ex = e;
                            }
                        }
                    }
                }
                catch { }

                //如果没有发送成功.则抛出最后一个错误
                if (ex != null) throw ex;
            }
            else
            {
                //构造SMTP的连接器
                SmtpClient smtp = this.GetSmtpClient(this.Server.HostName, this.Server.Port);
                smtp.Send(message);
            }
        }
        /// <summary>
        /// 发送邮件,如果发送过程出现错误则会抛出异常
        /// </summary>
        /// <param name="message">邮件内容</param>
        public void Send(EmailMessage message)
        {
            if (message == null) return;
            Send(message.GetMailMessage());
        }

        /// <summary>
        /// 发送邮件完成时触发的事件
        /// </summary>
        public event SendCompletedEventHandler SendCompleted;
        /// <summary>
        /// 异步发送邮件
        /// </summary>
        /// <param name="from">发件人</param>
        /// <param name="recipients">收件人</param>
        /// <param name="subject">主题</param>
        /// <param name="body">邮件正文</param>
        /// <param name="userToken">一个用户定义对象，此对象将被传递给完成异步操作时所调用的方法。</param>
        public void SendAsync(string from, string recipients, string subject, string body, object userToken)
        {
            //构造SMTP的连接器
            SendAsync(new MailMessage(from, recipients, subject, body), userToken);
        }

        /// <summary>
        /// 发送邮件,如果发送过程出现错误则会抛出异常
        /// </summary>
        /// <param name="message">邮件内容</param>
        /// <param name="userToken">一个用户定义对象，此对象将被传递给完成异步操作时所调用的方法。</param>
        public void SendAsync(EmailMessage message, object userToken)
        {
            if (message == null) return;
            SendAsync(message.GetMailMessage(), userToken);
        }

        /// <summary>
        /// 异步发送邮件,如果发送过程出现错误则会抛出异常
        /// </summary>
        /// <param name="message">邮件内容</param>
        /// <param name="userToken">一个用户定义对象，此对象将被传递给完成异步操作时所调用的方法。</param>
        public void SendAsync(MailMessage message, object userToken)
        {
            if (this.Server.UseLocalService)   //采用本地邮件服务
            {
                if (message.To.Count < 1) return;
                try
                {
                    string[] mailMX = DnsMxRecord.Query(message.To[0]);
                    if (mailMX != null && mailMX.Length > 0)
                    {
                        this.BeforeLocalSend(message);
                        SendCompletedToken token = new SendCompletedToken()
                        {
                            MxRecords = mailMX,
                            Index = 0,
                            Port = MailServer.DefaultPort,
                            Message = message,
                            UserToken = userToken
                        };
                        //构造SMTP的连接器
                        SmtpClient smtp = this.GetSmtpClient(mailMX[token.Index], token.Port);                        
                        smtp.SendCompleted += MailSendCompleted;
                        smtp.SendAsync(message, token);
                    }
                }
                catch { }
            }
            else
            {
                //构造SMTP的连接器
                SmtpClient smtp = this.GetSmtpClient(this.Server.HostName, this.Server.Port);
                smtp.SendCompleted += this.SendCompleted;
                smtp.SendAsync(message, userToken);
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MailSendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            SmtpClient client = sender as SmtpClient;
            if (client != null) client.SendCompleted -= MailSendCompleted;

            object userToken = e.UserState;
            SendCompletedToken token = e.UserState as SendCompletedToken;
            if (token != null) userToken = token.UserToken;
            if (e.Error != null && !e.Cancelled && token != null)
            {
                if ((++token.Index) < token.MxRecords.Length)
                {
                    //继续连接下一个MX Record
                    SmtpClient smtp = this.GetSmtpClient(token.MxRecords[token.Index], token.Port);
                    smtp.SendCompleted += MailSendCompleted;
                    smtp.SendAsync(token.Message, token);
                    return;
                }
            }
            AsyncCompletedEventArgs args = new AsyncCompletedEventArgs(e.Error, e.Cancelled, userToken);
            if (this.SendCompleted != null) this.SendCompleted(sender, args);
        }

        /// <summary>
        /// 发送完成的会话
        /// </summary>
        private class SendCompletedToken
        {
            /// <summary>
            /// 对应的邮件记录
            /// </summary>
            public string[] MxRecords { get; set; }
            /// <summary>
            /// 端口
            /// </summary>
            public int Port { get; set; }
            /// <summary>
            /// 当前索引值
            /// </summary>
            public int Index { get; set; }
            /// <summary>
            /// 邮件消息
            /// </summary>
            public MailMessage Message { get; set; }
            /// <summary>
            /// 用户的标识对象
            /// </summary>
            public object UserToken { get; set; }
        }
    }
}
