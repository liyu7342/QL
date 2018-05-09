namespace QL.Core.Log
{
    using QL.Core;
    using System;
    using System.Configuration;

    public static class Logger
    {
        private static object _initLock = new object();
        private static ILoggerFactory _loggerFactory = null;
        [ThreadStatic]
        private static ILogger loggerInstance;

        public static void Debug(string message)
        {
            Current.Debug(message);
        }

        public static void Debug(Exception ex, string message)
        {
            Current.Debug(message, new object[] { ex });
        }

        public static void Debug(string message, params object[] data)
        {
            Current.Debug(message, data);
        }

        public static void Debug(Exception ex, string message, params object[] data)
        {
            Current.Debug(ex, message, data);
        }

        public static void Error(string message)
        {
            Current.Error(message);
        }

        public static void Error(Exception ex, string message)
        {
            Current.Error(ex, message);
        }

        public static void Error(string message, params object[] data)
        {
            Current.Error(message, data);
        }

        public static void Error(Exception ex, string message, params object[] data)
        {
            Current.Error(ex, message, data);
        }

        public static void Fatal(string message)
        {
            Current.Fatal(message);
        }

        public static void Fatal(Exception ex, string message)
        {
            Current.Fatal(ex, message);
        }

        public static void Fatal(string message, params object[] data)
        {
            Current.Fatal(message, data);
        }

        public static void Fatal(Exception ex, string message, params object[] data)
        {
            Current.Fatal(ex, message, data);
        }

        private static ILoggerFactory GetLoggerFactory()
        {
            if (_loggerFactory == null)
            {
                lock (_initLock)
                {
                    if (_loggerFactory == null)
                    {
                        string instance = ConfigurationManager.AppSettings["QL.Log.Factory"];
                        _loggerFactory = Utility.CreateInstance<ILoggerFactory>(instance) ?? FileLoggerFactory.Default;
                    }
                }
            }
            return _loggerFactory;
        }

        public static void Info(string message)
        {
            Current.Info(message);
        }

        public static void Info(Exception ex, string message)
        {
            Current.Info(message, new object[] { ex });
        }

        public static void Info(string message, params object[] data)
        {
            Current.Info(message, data);
        }

        public static void Info(Exception ex, string message, params object[] data)
        {
            Current.Info(ex, message, data);
        }

        public static void Set(ILogger logger)
        {
            loggerInstance = logger;
        }

        public static void Warn(string message)
        {
            Current.Warn(message);
        }

        public static void Warn(Exception ex, string message)
        {
            Current.Warn(ex, message);
        }

        public static void Warn(string message, params object[] data)
        {
            Current.Warn(message, data);
        }

        public static void Warn(Exception ex, string message, params object[] data)
        {
            Current.Warn(ex, message, data);
        }

        private static ILogger Current
        {
            get
            {
                if (loggerInstance != null)
                {
                    return loggerInstance;
                }
                return GetLoggerFactory().CreateLogger();
            }
        }
    }
}