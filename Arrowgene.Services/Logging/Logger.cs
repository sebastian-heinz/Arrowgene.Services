/*
 * MIT License
 * 
 * Copyright (c) 2018 Sebastian Heinz <sebastian.heinz.gt@googlemail.com>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;

namespace Arrowgene.Services.Logging
{
    public class Logger : ILogger
    {
        private string _zone;
        private string _identity;

        public Logger()
        {
        }

        public event EventHandler<LogWriteEventArgs> LogWrite;

        public virtual void Initialize(string identity, string zone, object configuration)
        {
            _identity = identity;
            _zone = zone;
        }

        public void Write(Log log)
        {
            OnLogWrite(log);
        }

        public void Write(LogLevel logLevel, string message, object tag)
        {
            Log log = new Log(logLevel, message, tag, _identity, _zone);
            Write(log);
        }

        public void Trace(string message)
        {
            Write(LogLevel.Trace, message, null);
        }

        public void Info(string message)
        {
            Write(LogLevel.Info, message, null);
        }

        public void Debug(string message)
        {
            Write(LogLevel.Debug, message, null);
        }

        public void Error(string message)
        {
            Write(LogLevel.Error, message, null);
        }

        public void Exception(Exception exception)
        {
            if (exception == null)
            {
                Write(LogLevel.Error, "Exception was null.", null);
                return;
            }

            Write(LogLevel.Error, exception.ToString(), exception);
        }

        private void OnLogWrite(Log log)
        {
            EventHandler<LogWriteEventArgs> logWrite = LogWrite;
            if (logWrite != null)
            {
                LogWriteEventArgs logWriteEventArgs = new LogWriteEventArgs(log);
                logWrite(this, logWriteEventArgs);
            }
        }
    }
}