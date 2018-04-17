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
    public static class LogProvider<T> where T : Logger, new()
    {
        private static readonly LogProvider Instance;

        static LogProvider()
        {
            Instance = new LogProvider();
        }

        public static LogProvider Provider => Instance;

        public static T GetLogger(object instance)
        {
            return Instance.GetLogger<T>(instance);
        }

        public static T GetLogger(Type type)
        {
            return Instance.GetLogger<T>(type);
        }

        public static T GetLogger(string identity, string zone = null)
        {
            return Instance.GetLogger<T>(identity, zone);
        }
    }

    public class LogProvider
    {
        private readonly Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>();
        private readonly object _lock = new object();
        private object _configuration;

        /// <summary>
        /// Notifies about any logging event from every LogProvider instance
        /// </summary>
        public static event EventHandler<LogWriteEventArgs> GlobalLogWrite;

        /// <summary>
        /// Notifies about logging events from this LogProvider instance
        /// </summary>
        public event EventHandler<LogWriteEventArgs> ProviderLogWrite;

        /// <summary>
        /// Provide a confugration object that will be passed to every <see cref="Logger"/> instance
        /// by calling <see cref="Logger.Configure(object)"/> on it.
        /// </summary>
        public void Configure(object configuration)
        {
            _configuration = configuration;
        }

        public T GetLogger<T>(object instance) where T : Logger, new()
        {
            return GetLogger<T>(instance.GetType());
        }

        public T GetLogger<T>(Type type) where T : Logger, new()
        {
            return GetLogger<T>(type.FullName, type.Name);
        }

        public T GetLogger<T>(string identity, string zone = null) where T : Logger, new()
        {
            Logger logger;
            lock (_lock)
            {
                if (!_loggers.TryGetValue(identity, out logger))
                {
                    logger = new T();
                    logger.Initialize(identity, zone, _configuration);
                    logger.LogWrite += LoggerOnLogWrite;
                    _loggers.Add(identity, logger);
                }
            }

            return logger is T ? (T) logger : default(T);
        }

        private void LoggerOnLogWrite(object sender, LogWriteEventArgs writeEventArgs)
        {
            EventHandler<LogWriteEventArgs> providerLogWrite = ProviderLogWrite;
            if (providerLogWrite != null)
            {
                providerLogWrite(sender, writeEventArgs);
            }

            EventHandler<LogWriteEventArgs> globalLogWrite = GlobalLogWrite;
            if (globalLogWrite != null)
            {
                globalLogWrite(sender, writeEventArgs);
            }
        }
    }
}