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
    public static class LogProvider<T> where T : ILogger, new()
    {
        private static LogProvider _instance = new LogProvider();

        static LogProvider()
        {
            _instance.SetProducer(new T());
        }

        public static T GetLogger(object instance)
        {
            return _instance.GetLogger<T>(instance);
        }

        public static T GetLogger(Type type)
        {
            return _instance.GetLogger<T>(type);
        }

        public static T GetLogger(string identity, string zone = null)
        {
            return _instance.GetLogger<T>(identity, zone);
        }
    }

    public class LogProvider
    {
        private readonly Dictionary<string, ILogger> Loggers = new Dictionary<string, ILogger>();
        private readonly object Lock = new object();
        private ILogger _producer = new Logger("Producer");

        /// <summary>
        /// Notifies about any logging event from every LogProvider instance
        /// </summary>
        public static event EventHandler<LogWriteEventArgs> GlobalLogWrite;

        /// <summary>
        /// Notifies about logging events from this LogProvider instance
        /// </summary>
        public event EventHandler<LogWriteEventArgs> ProviderLogWrite;

        /// <summary>
        /// Provide an implementation of ILogger.
        /// All logging will be handled by the provided implementation.
        /// </summary>
        /// <param name="logger"></param>
        public void SetProducer(ILogger logger)
        {
            _producer = logger;
        }

        public ILogger GetLogger(object instance)
        {
            return GetLogger(instance.GetType());
        }

        public T GetLogger<T>(object instance) where T : ILogger
        {
            return GetLogger<T>(instance.GetType());
        }

        public ILogger GetLogger(Type type)
        {
            return GetLogger(type.FullName, type.Name);
        }

        public T GetLogger<T>(Type type) where T : ILogger
        {
            return GetLogger<T>(type.FullName, type.Name);
        }

        public ILogger GetLogger(string identity, string zone = null)
        {
            ILogger logger;
            lock (Lock)
            {
                if (!Loggers.TryGetValue(identity, out logger))
                {
                    logger = _producer.Produce(identity, zone);
                    logger.LogWrite += LoggerOnLogWrite;
                    Loggers.Add(identity, logger);
                }
            }
            return logger;
        }

        public T GetLogger<T>(string identity, string zone = null) where T : ILogger
        {
            ILogger logger = GetLogger(identity, zone);
            if (!(logger is T))
            {
                throw new Exception("Call to 'GetLogger<T>' failed because the producer is not an instance of T. Use 'SetProducer()' to set the correct instance for T.");
            }
            return (T) logger;
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