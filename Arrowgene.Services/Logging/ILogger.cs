using System;

namespace Arrowgene.Services.Logging
{
    public interface ILogger
    {
        event EventHandler<LogWriteEventArgs> LogWrite;
        void Initialize(string identity, string name, object configuration);
        
        void Write(LogLevel logLevel, object tag, string message, params object[] args);
        void Info(string message, params object[] args);

        void Debug(string message, params object[] args);

        void Error(string message, params object[] args);

        void Exception(Exception exception);
    }
}
