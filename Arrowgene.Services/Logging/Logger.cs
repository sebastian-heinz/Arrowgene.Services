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
using System.Diagnostics;

namespace Arrowgene.Services.Logging
{
    public class Logger
    {
        public event EventHandler<LogWriteEventArgs> LogWrite;

        private readonly object _lock;
        private readonly Dictionary<int, Log> _logs;
        private volatile int _currentId;
        private string _zone;
        private string _identity;

        public Logger() : this(null)
        {
        }

        public Logger(string identity, string zone = null, object configuration = null)
        {
            _lock = new object();
            _logs = new Dictionary<int, Log>();
            _currentId = 0;
            Initialize(identity, zone, configuration);
        }

        public void Write(Log log)
        {
            lock (_lock)
            {
                _logs.Add(_currentId++, log);
            }

            OnLogWrite(log);
        }

        public void Write(LogLevel logLevel, object tag, string message, params object[] args)
        {
            string msg = string.Format(message, args);
            Log log = new Log(logLevel, msg, tag, _identity, _zone);
            Write(log);
        }

        public void Info(string message, params object[] args)
        {
            Write(LogLevel.Info, null, message, args);
        }

        public void Debug(string message, params object[] args)
        {
            Write(LogLevel.Debug, null, message, args);
        }

        public void Error(string message, params object[] args)
        {
            Write(LogLevel.Error, null, message, args);
        }

        public void Exception(Exception exception)
        {
            Write(LogLevel.Error, null, exception.ToString());
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

        protected virtual void Configure(object configuration)
        {
        }

        internal void Initialize(string identity, string zone, object configuration)
        {
            _identity = identity;
            _zone = zone;
            Configure(configuration);
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

        private string GetCallingMethodInfo()
        {
            return GetCallingMethodInfo(1);
        }

        private string GetCallingMethodInfo(int depth)
        {
            string result = "";
            StackTrace trace = new StackTrace();
            if (trace.FrameCount > 1)
            {
                if (trace.FrameCount < depth)
                {
                    depth = trace.FrameCount;
                }

                for (int i = 1; i < depth; i++)
                {
                    StackFrame frame = trace.GetFrame(i);
                    result += GetCallingMethod(frame);
                }
            }

            return result;
        }

        private string GetCallingMethod(StackFrame stackFrame)
        {
            return stackFrame.GetMethod().DeclaringType.FullName + "::" + stackFrame.GetMethod().Name;
        }
    }
}