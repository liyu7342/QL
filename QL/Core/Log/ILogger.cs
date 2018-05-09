namespace QL.Core.Log
{
    using System;

    public interface ILogger
    {
        void Debug(string message);
        void Debug(Exception ex, string message);
        void Debug(string message, params object[] data);
        void Debug(Exception ex, string message, params object[] data);
        void Error(string message);
        void Error(Exception ex, string message);
        void Error(string message, params object[] data);
        void Error(Exception ex, string message, params object[] data);
        void Fatal(string message);
        void Fatal(Exception ex, string message);
        void Fatal(string message, params object[] data);
        void Fatal(Exception ex, string message, params object[] data);
        void Info(string message);
        void Info(Exception ex, string message);
        void Info(string message, params object[] data);
        void Info(Exception ex, string message, params object[] data);
        void Warn(string message);
        void Warn(Exception ex, string message);
        void Warn(string message, params object[] data);
        void Warn(Exception ex, string message, params object[] data);
    }
}
