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


// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Runtime.Serialization;

namespace Arrowgene.Services.Networking.Tcp.Server.AsyncEvent
{
    [DataContract]
    public class AsyncEventSettings : ICloneable
    {
        [DataMember(Order = 0)]
        public string Identity { get; set; }

        [DataMember(Order = 1)]
        public int MaxConnections { get; set; }

        [DataMember(Order = 2)]
        public int NumSimultaneouslyWriteOperations { get; set; }

        [DataMember(Order = 3)]
        public int BufferSize { get; set; }

        [DataMember(Order = 4)]
        public int Retries { get; set; }
        
        [DataMember(Order = 5)]
        public int MaxUnitOfOrder { get; set; }

        [DataMember(Order = 10)]
        public SocketSettings SocketSettings { get; set; }
        
        public AsyncEventSettings()
        {
            BufferSize = 2000;
            MaxConnections = 100;
            NumSimultaneouslyWriteOperations = 100;
            Retries = 10;
            SocketSettings = new SocketSettings();
            Identity = "";
            MaxUnitOfOrder = 1;
        }

        public AsyncEventSettings(AsyncEventSettings settings)
        {
            Identity = settings.Identity;
            BufferSize = settings.BufferSize;
            MaxConnections = settings.MaxConnections;
            NumSimultaneouslyWriteOperations = settings.NumSimultaneouslyWriteOperations;
            Retries = settings.Retries;
            SocketSettings = new SocketSettings(settings.SocketSettings);
            MaxUnitOfOrder = settings.MaxUnitOfOrder;
        }

        public object Clone()
        {
            return new AsyncEventSettings(this);
        }
    }
}