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
namespace Arrowgene.Services.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

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

        public event EventHandler<LogWriteEventArgs> LogWrite;
        private object myLock;
        private Dictionary<int, Log> logs;
        private volatile int currentId;

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public Logger(string name)
        {
            this.Name = name;
            this.myLock = new object();
            this.logs = new Dictionary<int, Log>();
            this.Clear();
            this.WriteDebug = true;
        }

        /// <summary>
        /// Write Logs to Debug output.
        /// </summary>
        public bool WriteDebug { get; set; }

        public string Name { get; set; }
        public int Count { get { return this.logs.Count; } }

        internal void OnLogWrite(Log log)
        {
            EventHandler<LogWriteEventArgs> logWrite = this.LogWrite;
            if (logWrite != null)
            {
                LogWriteEventArgs logWriteEventArgs = new LogWriteEventArgs(log);
                logWrite(this, logWriteEventArgs);
            }
        }

        /// <summary>
        /// Clears all stored <see cref="Log"/></summary>
        public void Clear()
        {
            lock (this.myLock)
            {
                this.logs.Clear();
                this.currentId = 0;
            }
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
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
                log.Id = this.currentId;
                this.logs.Add(log.Id, log);
                this.DebugWrite(log);
                this.currentId++;
                this.OnLogWrite(log);
            }
        }

        private void DebugWrite(Log log)
        {
            if (this.WriteDebug)
            {
                Debug.WriteLine(log.Text, this.Name);
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
