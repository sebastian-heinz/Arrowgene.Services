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
namespace Arrowgene.Services.Network.PortScan
{
    using System.Diagnostics;
    using System.Net;

    [DebuggerDisplay("{Port} open={IsConnected} {IPAddress}")]
    public class PortScannerResult
    {
        public PortScannerResult(IPAddress ipAddress, ushort port, bool isOpen)
        {
            this.IPAddress = ipAddress;
            this.Port = port;
            this.IsOpen = isOpen;
        }

        public IPAddress IPAddress { get; private set; }
        public bool IsOpen { get; private set; }
        public ushort Port { get; private set; }
    }
}