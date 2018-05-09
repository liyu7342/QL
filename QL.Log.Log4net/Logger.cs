namespace QL.Log.Log4Net
{
    using log4net;
    using log4net.Config;
    using log4net.Layout;
    using QL.Core.Log;
    using QL.Log.Log4Net.Pattern;
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class Logger : ILogger
    {
        private static object _initLock = new object();
        private static ILogger _loggerInstance = null;

        static Logger()
        {
            PatternLayout layout = new PatternLayout();
            FieldInfo field = layout.GetType().GetField("s_globalRulesRegistry", BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
            {
                ((Hashtable)field.GetValue(null)).Add("aspnet-form", typeof(AspNetFormPatternConverter));
            }
            layout = null;
        }

        public Logger(ILog log)
        {
            this.Log = log;
        }

        public void Debug(string message)
        {
            this.Log.Debug(message);
        }

        public void Debug(Exception ex, string message)
        {
            this.Log.Debug(message, ex);
        }

        public void Debug(string message, params object[] data)
        {
            this.Log.DebugFormat(message, data);
        }

        public void Debug(Exception ex, string message, params object[] data)
        {
            this.Log.Debug(string.Format(message, data), ex);
        }

        public void Error(string message)
        {
            this.Log.Error(message);
        }

        public void Error(Exception ex, string message)
        {
            this.Log.Error(message, ex);
        }

        public void Error(string message, params object[] data)
        {
            this.Log.ErrorFormat(message, data);
        }

        public void Error(Exception ex, string message, params object[] data)
        {
            this.Log.Error(string.Format(message, data), ex);
        }

        public void Fatal(string message)
        {
            this.Log.Fatal(message);
        }

        public void Fatal(Exception ex, string message)
        {
            this.Log.Fatal(message, ex);
        }

        public void Fatal(string message, params object[] data)
        {
            this.Log.FatalFormat(message, data);
        }

        public void Fatal(Exception ex, string message, params object[] data)
        {
            this.Log.Fatal(string.Format(message, data), ex);
        }

        public void Info(string message)
        {
            this.Log.Info(message);
        }

        public void Info(Exception ex, string message)
        {
            this.Log.Info(message, ex);
        }

        public void Info(string message, params object[] data)
        {
            this.Log.InfoFormat(message, data);
        }

        public void Info(Exception ex, string message, params object[] data)
        {
            this.Log.Info(string.Format(message, data), ex);
        }

        public void Warn(string message)
        {
            this.Log.Warn(message);
        }

        public void Warn(Exception ex, string message)
        {
            this.Log.Warn(message, ex);
        }

        public void Warn(string message, params object[] data)
        {
            this.Log.WarnFormat(message, data);
        }

        public void Warn(Exception ex, string message, params object[] data)
        {
            this.Log.Warn(string.Format(message, data), ex);
        }

        public static ILogger Default
        {
            get
            {
                if (_loggerInstance == null)
                {
                    lock (_initLock)
                    {
                        if (_loggerInstance == null)
                        {
                            string str = ConfigurationManager.AppSettings["QL.Log.Log4Net.Name"];
                            if (string.IsNullOrEmpty(str))
                            {
                                str = "QL.Log.Log4Net";
                            }
                            XmlConfigurator.Configure();
                            _loggerInstance = new QL.Log.Log4Net.Logger(LogManager.GetLogger(str));
                        }
                    }
                }
                return _loggerInstance;
            }
        }

        public ILog Log { get; private set; }
    }
}