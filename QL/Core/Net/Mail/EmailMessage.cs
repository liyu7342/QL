/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  EmailMessage
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace QL.Core.Net.Mail
{
    /// <summary>
    /// 电子邮件
    /// </summary>
    [Serializable]
    public class EmailMessage
    {
        /// <summary>
        /// 构造默认实例
        /// </summary>
        public EmailMessage()
        {
            this.Charset = Encoding.UTF8.WebName;
            this.IsBodyHtml = false;
            this.Priority = MailPriority.Normal;
        }

        /// <summary>
        /// 构造实例
        /// </summary>
        /// <param name="from">发件人</param>
        /// <param name="recipients">收件人</param>
        /// <param name="subject">主题</param>
        /// <param name="body">邮件内容</param>
        public EmailMessage(string from, string recipients, string subject, string body)
            : this()
        {
            this.From = from;
            this.Recipients = recipients;
            this.Subject = subject;
            this.Body = body;
        }
        /// <summary>
        /// 邮件编码
        /// </summary>
        public string Charset { get; set; }
        /// <summary>
        /// 发件人
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 发件人名
        /// </summary>
        public string FromName { get; set; }
        /// <summary>
        /// 收件人
        /// </summary>
        public string Recipients { get; set; }
        /// <summary>
        /// 回复地址
        /// </summary>
        public string ReplyTo { get; set; }
        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 是否是HTML邮件内容
        /// </summary>
        public bool IsBodyHtml { get; set; }
        /// <summary>
        /// 邮件优先级
        /// </summary>
        public MailPriority Priority { get; set; }

        #region 返回转换后的邮件内容
        /// <summary>
        /// 返回转换后的邮件内容
        /// </summary>
        /// <returns></returns>
        public MailMessage GetMailMessage()
        {
            if (string.IsNullOrEmpty(this.From) || string.IsNullOrEmpty(this.Recipients)) return null;

            Encoding encoding = string.IsNullOrEmpty(this.Charset) ? Encoding.UTF8 : Encoding.GetEncoding(this.Charset);
            MailAddress from = string.IsNullOrEmpty(this.FromName) ? (new MailAddress(this.From)) : (new MailAddress(this.From, this.FromName, encoding));

            MailMessage message = new MailMessage();
            message.From = from;
            message.Subject = this.Subject;
            message.Body = this.Body;
            message.IsBodyHtml = this.IsBodyHtml;
            message.Priority = this.Priority;
            message.To.Add(this.Recipients);
            if(!string.IsNullOrEmpty(this.ReplyTo))
                message.ReplyToList.Add(this.ReplyTo);
            message.SubjectEncoding = message.BodyEncoding = encoding;

            return message;
        }
        #endregion
    }
}
