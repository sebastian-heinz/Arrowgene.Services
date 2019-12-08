using System;

namespace Arrowgene.Services.Logging
{
    public interface ILogger
    {
        event EventHandler<LogWriteEventArgs> LogWrite;
        void Initialize(string identity, string name, object configuration);
        void Write(LogLevel logLevel, string message, object tag);
        void Trace(string message);
        void Info(string message);
        void Debug(string message);
        void Error(string message);
        void Exception(Exception exception);
    }
}