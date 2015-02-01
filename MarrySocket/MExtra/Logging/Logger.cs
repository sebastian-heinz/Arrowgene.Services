/*
 *  Copyright 2015 Sebastian Heinz <sebastian.heinz.gt@googlemail.com>
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 */
namespace MarrySocket.MExtra.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Threading;


    public enum LogType { NONE, ERROR, INFO, CLIENT, SERVER, PACKET }

    /// <summary>
    /// Logging Class</summary>
    /// <remarks>
    /// Logs message combined with id and type as <see cref="Log"/>
    /// Events for log writings, will only occur if its declared as safe</remarks>
    public class Logger
    {
        /// <summary>
        /// Notifies when a <see cref="Log"/> write occured.
        /// Don't block this Action by the UI thread, use Dispatcher.BeginInvoke.</summary>
        public Action<Log> OnLogWrite { private get; set; }

        private object myLock;
        private Dictionary<int, Log> logs;
        private volatile int count;

        public Logger()
        {
            this.myLock = new object();
            this.logs = new Dictionary<int, Log>();
            this.Clear();
        }

        public int Count { get { return this.count; } }

        /// <summary>
        /// Clears all stored <see cref="Log"/></summary>
        public void Clear()
        {
            lock (this.myLock)
            {
                this.logs.Clear();
                this.count = 0;

            }
        }

        public void Remove(int id)
        {
            lock (this.myLock)
            {
                this.logs.Remove(id);
            }
        }

        /// <summary>
        /// Writes a new <see cref="Log"/></summary>
        /// <param name="log"><see cref="Log"/></param>
        public void Write(Log log)
        {
            lock (this.myLock)
            {
                log.Id = this.count;
                this.logs.Add(log.Id, log);
                this.count++;
                if(this.OnLogWrite != null)
                {
                    this.OnLogWrite(log);
                }
            }
        }

        /// <summary>
        /// Writes a new <see cref="Log"/></summary>
        /// <param name="log">Message {0}</param>
        /// <param name="arg0">Argument</param>
        /// <param name="logType">Log Category</param>
        public void Write(string log, object arg0, LogType logType)
        {
            this.Write(new Log(String.Format(log, arg0), logType));
        }

        /// <summary>
        /// Writes a new <see cref="Log"/></summary>
        /// <param name="log">Message {0}</param>
        /// <param name="arg0">Argument</param>
        /// <param name="arg1">Argument</param>
        /// <param name="logType">Log Category</param>
        public void Write(string log, object arg0, object arg1, LogType logType)
        {
            this.Write(new Log(String.Format(log, arg0, arg1), logType));
        }

        /// <summary>
        /// Writes a new <see cref="Log"/></summary>
        /// <param name="log">Message</param>
        /// <param name="logType">Log Category</param>
        public void Write(string log, LogType logType)
        {
            this.Write(new Log(log, logType));
        }

        /// <summary>
        /// Writes a new <see cref="Log"/></summary>
        /// <param name="log">Message</param>
        public void Write(string log)
        {
            this.Write(new Log(log));
        }

        /// <summary>
        /// Receive all <see cref="Log"/></summary>
        /// <returns>
        /// Dictionary containing id associated by <see cref="Log"/></returns>
        public Dictionary<int, Log> GetLogs()
        {
            Dictionary<int, Log> tmp = null;
            lock (this.myLock)
            {
                tmp = new Dictionary<int, Log>(this.logs);
            }
            return tmp;
        }

    }
}
