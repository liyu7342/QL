namespace QL.Core.Log
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using QL.Core.Extensions;

    public class FileLogger : ILogger, IDisposable
    {
        private static DateTime _createFileLoggerTime = DateTime.MinValue;
        private static FileLogger _fileLogger;
        private static bool _fileLoggerIsDaySplit = "true".Equals(ConfigurationManager.AppSettings["QL.Logger.DaySplit"], StringComparison.OrdinalIgnoreCase);
        private static object _fileLoggerLock = new object();
        private static int _fileLoggerStoreDays = ConfigurationManager.AppSettings["QL.Logger.StoreDays"].As<int>(30);
        private StreamWriter _LogWriter;
        private StringWriter Buffer;
        private Encoding charset;
        private string logFileName;

        public FileLogger(string fileName)
            : this(fileName, Encoding.UTF8)
        {
        }

        public FileLogger(string fileName, bool enableDebug)
            : this(fileName, Encoding.UTF8, enableDebug)
        {
        }

        public FileLogger(string fileName, Encoding charset)
            : this(fileName, charset, true)
        {
        }

        public FileLogger(string fileName, Encoding charset, bool enableDebug)
        {
            this.Buffer = new StringWriter(new StringBuilder(0x80));
            this.logFileName = fileName;
            this.charset = charset;
            this.EnableDebug = enableDebug;
        }

        private static void CleanLogFiles(string fileName)
        {
            if (_fileLoggerStoreDays >= 1)
            {
                DirectoryInfo info = new DirectoryInfo(Path.GetDirectoryName(fileName));
                foreach (FileInfo info2 in info.GetFiles("*" + Path.GetExtension(fileName), SearchOption.TopDirectoryOnly))
                {
                    int totalDays = (int)DateTime.Today.Subtract(info2.CreationTime.Date).TotalDays;
                    if (totalDays > _fileLoggerStoreDays)
                    {
                        try
                        {
                            info2.Delete();
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private static FileLogger CreateInstance(string fileName)
        {
            Encoding charset = Encoding.UTF8;
            string str = ConfigurationManager.AppSettings["QL.Logger.Charset"];
            if (!string.IsNullOrEmpty(str))
            {
                charset = Encoding.GetEncoding(str);
            }
            return new FileLogger(fileName, charset, "true".Equals(ConfigurationManager.AppSettings["QL.Logger.EnableDebug"], StringComparison.OrdinalIgnoreCase)) { ConsoleOutput = "true".Equals(ConfigurationManager.AppSettings["QL.Logger.ConsoleOutput"], StringComparison.OrdinalIgnoreCase) };
        }

        public void Debug(string message)
        {
            this.Debug(null, message, null);
        }

        public void Debug(Exception ex, string message)
        {
            this.Debug(ex, message, null);
        }

        public void Debug(string message, params object[] data)
        {
            this.Debug(null, message, data);
        }

        public void Debug(Exception ex, string message, params object[] data)
        {
            if (this.EnableDebug)
            {
                lock (this)
                {
                    this.WriteLog("DEBUG", ex, message, data);
                }
            }
        }

        public void Dispose()
        {
            if (this._LogWriter != null)
            {
                this._LogWriter.Flush();
                this._LogWriter.Close();
                this._LogWriter = null;
                this.Buffer.Close();
            }
        }

        public void Error(string message)
        {
            this.Error(null, message, null);
        }

        public void Error(Exception ex, string message)
        {
            this.Error(ex, message, null);
        }

        public void Error(string message, params object[] data)
        {
            this.Error(null, message, data);
        }

        public void Error(Exception ex, string message, params object[] data)
        {
            lock (this)
            {
                this.WriteLog("ERROR", ex, message, data);
            }
        }

        public void Fatal(string message)
        {
            this.Fatal(null, message, null);
        }

        public void Fatal(Exception ex, string message)
        {
            this.Fatal(ex, message, null);
        }

        public void Fatal(string message, params object[] data)
        {
            this.Fatal(null, message, data);
        }

        public void Fatal(Exception ex, string message, params object[] data)
        {
            lock (this)
            {
                this.WriteLog("FATAL", ex, message, data);
            }
        }

        private void Flush()
        {
            string str = this.Buffer.ToString();
            this.Buffer.GetStringBuilder().Length = 0;
            this.LogWriter.Write(str);
            this.LogWriter.Flush();
            if (this.ConsoleOutput)
            {
                Console.Write(str);
            }
        }

        private static string GetLogFileName()
        {
            string str = ConfigurationManager.AppSettings["QL.Logger.File"];
            if (string.IsNullOrEmpty(str))
            {
                str = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs/app.log");
            }
            string directoryName = Path.GetDirectoryName(str);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            if (_fileLoggerIsDaySplit)
            {
                str = Path.Combine(directoryName, Path.GetFileNameWithoutExtension(str) + DateTime.Now.ToString("-yyyyMMdd") + Path.GetExtension(str));
            }
            return str;
        }

        public void Info(string message)
        {
            this.Info(null, message, null);
        }

        public void Info(Exception ex, string message)
        {
            this.Info(ex, message, null);
        }

        public void Info(string message, params object[] data)
        {
            this.Info(null, message, data);
        }

        public void Info(Exception ex, string message, params object[] data)
        {
            lock (this)
            {
                this.WriteLog("INFO", ex, message, data);
            }
        }

        public void Warn(string message)
        {
            this.Warn(null, message, null);
        }

        public void Warn(Exception ex, string message)
        {
            this.Warn(ex, message, null);
        }

        public void Warn(string message, params object[] data)
        {
            this.Warn(null, message, data);
        }

        public void Warn(Exception ex, string message, params object[] data)
        {
            lock (this)
            {
                this.WriteLog("WARN", ex, message, data);
            }
        }

        private void Write(string message, params object[] data)
        {
            if (data == null)
            {
                this.Buffer.Write(message);
            }
            else
            {
                this.Buffer.Write(message, data);
            }
        }

        private void WriteLine()
        {
            this.Buffer.WriteLine();
        }

        private void WriteLine(string message, params object[] data)
        {
            if (data == null)
            {
                this.Buffer.WriteLine(message);
            }
            else
            {
                this.Buffer.WriteLine(message, data);
            }
        }

        private void WriteLog(string head, Exception ex, string message, params object[] data)
        {
            this.Write("{0} [{1}] ", new object[] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), head });
            if (!string.IsNullOrEmpty(message))
            {
                this.Write(message, data);
                if (ex != null)
                {
                    this.WriteLine();
                }
            }
            if (ex != null)
            {
                if (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
                this.Write("Exception: {0}\r\n{1}", new object[] { ex.Message, ex.StackTrace });
            }
            this.WriteLine();
            this.Flush();
        }

        public bool ConsoleOutput { get; set; }

        public static FileLogger Default
        {
            get
            {
                if (_fileLogger == null)
                {
                    lock (_fileLoggerLock)
                    {
                        if (_fileLogger == null)
                        {
                            _fileLogger = CreateInstance(GetLogFileName());
                            _createFileLoggerTime = DateTime.Today;
                        }
                        goto Label_0106;
                    }
                }
                if (_fileLoggerIsDaySplit && (DateTime.Today.Subtract(_createFileLoggerTime.Date).TotalDays != 0.0))
                {
                    lock (_fileLoggerLock)
                    {
                        if (DateTime.Today.Subtract(_createFileLoggerTime.Date).TotalDays != 0.0)
                        {
                            if (_fileLogger != null)
                            {
                                _fileLogger.Dispose();
                                _fileLogger = null;
                            }
                            string logFileName = GetLogFileName();
                            try
                            {
                                CleanLogFiles(logFileName);
                            }
                            catch
                            {
                            }
                            _fileLogger = CreateInstance(logFileName);
                            _createFileLoggerTime = DateTime.Today;
                        }
                    }
                }
            Label_0106:
                return _fileLogger;
            }
        }

        public bool EnableDebug { get; private set; }

        private StreamWriter LogWriter
        {
            get
            {
                if (this._LogWriter == null)
                {
                    lock (this)
                    {
                        if (this._LogWriter == null)
                        {
                            this._LogWriter = new StreamWriter(new FileStream(this.logFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 0x200, FileOptions.None | FileOptions.WriteThrough), this.charset);
                            this._LogWriter.AutoFlush = true;
                        }
                    }
                }
                return this._LogWriter;
            }
        }
    }
}