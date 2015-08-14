﻿/*
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

    /// <summary>
    /// TODO SUMMARY
    /// </summary>
    public class Log
    {
        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public Log(string text)
        {
            this.Text = text;
            this.LogType = LogType.NONE;
            this.DateTime = DateTime.Now;
            this.Id = -1;
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public Log(string text, LogType logType) : this(text)
        {
            this.LogType = logType;
        }

        /// <summary>TODO SUMMARY</summary>
        public int Id { get; set; }
        /// <summary>TODO SUMMARY</summary>
        public string Text { get; private set; }
        /// <summary>TODO SUMMARY</summary>
        public LogType LogType { get; private set; }
        /// <summary>TODO SUMMARY</summary>
        public DateTime DateTime { get; private set; }

    }
}
