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
using System.Collections.Generic;

namespace Arrowgene.Services.Logging
{
    public class Logger : ILogger
    {
        public event EventHandler<LogWriteEventArgs> LogWrite;

        private readonly string _zone;
        private readonly string _identity;
        private readonly object _lock;
        private readonly Dictionary<int, Log> _logs;
        private volatile int _currentId;

        public Logger() : this(null)
        {
            
        }

        public Logger(string identity = null, string zone = null)
        {
            _zone = zone;
            _identity = identity;
            _lock = new object();
            _logs = new Dictionary<int, Log>();
            _currentId = 0;
        }

        public ILogger Produce(string identity, string zone = null)
        {
            return new Logger(identity, zone);
        }

        public void Write(Log log)
        {
            lock (_lock)
            {
                _logs.Add(_currentId++, log);
            }
            OnLogWrite(log);
        }

        public void Write(LogLevel logLevel, string message, params object[] args)
        {
            string msg = string.Format(message, args);
            Log log = new Log(logLevel, msg, _identity, _zone);
            Write(log);
        }

        public void Info(string message, params object[] args)
        {
            Write(LogLevel.Info, message, args);
        }

        public void Debug(string message, params object[] args)
        {
            Write(LogLevel.Debug, message, args);
        }

        public void Error(string message, params object[] args)
        {
            Write(LogLevel.Error, message, args);
        }

        public void Exception(Exception exception)
        {
            Write(LogLevel.Error, exception.ToString());
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

        public Dictionary<int, Log> GetLogs()
        {
            Dictionary<int, Log> tmp;
            lock (_lock)
            {
                tmp = new Dictionary<int, Log>(_logs);
            }
            return tmp;
        }
    }
}