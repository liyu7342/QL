namespace QL.Core.Log
{
    using System;

    public class FileLoggerFactory : ILoggerFactory
    {
        public static readonly ILoggerFactory Default = new FileLoggerFactory();

        public ILogger CreateLogger()
        {
            return FileLogger.Default;
        }
    }
}
