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

    public enum LogType { NONE, ERROR, ENTRY }

    public class Logger
    {
        public Action<Log> OnLogWrite { private get; set; }

        private object myLock;
        private Dictionary<int, Log> logs;
        private int count;

        public Logger()
        {
            this.myLock = new object();
            this.logs = new Dictionary<int, Log>();
            this.Clear();
        }

        public void Clear()
        {
            lock (this.myLock)
            {
                this.logs.Clear();
                this.count = 0;
            }
        }

        public void Write(Log log)
        {
            lock (this.myLock)
            {
                this.count++;
                log.Id = this.count;
                this.logs.Add(log.Id, log);
                if (this.OnLogWrite != null)
                    this.OnLogWrite(log);
            }
        }

        public void Write(string log, LogType logType)
        {
            this.Write(new Log(log, logType));
        }

        public void Write(string log)
        {
            this.Write(new Log(log));
        }

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
