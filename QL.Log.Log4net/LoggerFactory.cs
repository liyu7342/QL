namespace QL.Log.Log4Net
{
    using QL.Core.Log;
    using System;

    public class LoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger()
        {
            return QL.Log.Log4Net.Logger.Default;
        }
    }
}
