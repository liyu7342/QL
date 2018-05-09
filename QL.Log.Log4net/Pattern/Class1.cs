namespace QL.Log.Log4Net.Pattern
{
    using log4net.Core;
    using log4net.Layout.Pattern;
    using log4net.Util;
    using System;
    using System.IO;
    using System.Web;

    public class AspNetFormPatternConverter : PatternLayoutConverter
    {
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            if (HttpContext.Current == null)
            {
                writer.Write(SystemInfo.NotAvailableText);
            }
            else
            {
                HttpRequest request = null;
                try
                {
                    request = HttpContext.Current.Request;
                }
                catch (HttpException)
                {
                }
                if (request != null)
                {
                    if (this.Option != null)
                    {
                        PatternConverter.WriteObject(writer, loggingEvent.Repository, request.Form[this.Option]);
                    }
                    else
                    {
                        PatternConverter.WriteObject(writer, loggingEvent.Repository, request.Form.ToString());
                    }
                }
                else
                {
                    writer.Write(SystemInfo.NotAvailableText);
                }
            }
        }
    }
}
