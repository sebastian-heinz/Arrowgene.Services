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
namespace Arrowgene.Services.Network.PortScan
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;

    /// <summary>
    /// Scan for open ports.
    /// </summary>
    public class PortScanner
    {
        private const int TICKS_PER_MS = 10000;

        private object sync;
        private int threadFinishedCount;
        private ushort port;
        private List<ushort> portRange;
        private IPAddress ipAddress;
        private List<IPAddress> ipAddressPool;
        private List<PortScannerResult> portScanResults;
        private TimeSpan timeout;
        private bool isRunning;

        /// <summary>
        /// Initializes a new PortScan.
        /// </summary>
        /// <param name="connections">Simultaneous connections</param>
        /// <param name="timeoutMs">Time to wait for a response, before a port counts as closed</param>
        public PortScanner(int connections, int timeoutMs)
        {
            long ticks = timeoutMs * TICKS_PER_MS;

            this.timeout = new TimeSpan(ticks);
            this.Connections = connections;
            this.sync = new object();
            this.isRunning = false;
        }

        /// <summary>
        /// Scan Completed
        /// </summary>
        public event EventHandler<PortScannerCompletedEventArgs> PortScanCompleted;

        /// <summary>
        /// Simultaneous connections.
        /// </summary>
        public int Connections { get; private set; }

        /// <summary>
        /// Scan a given <see cref="IPAddress"/> for a port range.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="startPort"></param>
        /// <param name="endPort"></param>
        public void Scan(IPAddress ipAddress, ushort startPort, ushort endPort)
        {
            if (startPort <= 0 || endPort <= 0)
            {
                throw new Exception("Invalid port number supplied.");
            }

            if (this.isRunning)
            {
                throw new Exception("Scan is already in Progress.");
            }

            this.isRunning = true;
            this.portScanResults = new List<PortScannerResult>();
            this.ipAddress = ipAddress;
            this.portRange = new List<ushort>();
            this.threadFinishedCount = 0;

            for (ushort i = startPort; i < endPort; i++)
            {
                this.portRange.Add(i);
            }

            for (int i = 0; i < this.Connections; i++)
            {
                Thread portScan = new Thread(this.ScanPortRange);
                portScan.Name = string.Format("PortScan PortRange {0}", i);
                portScan.Start();
            }
        }

        /// <summary>
        /// Scan a pool of <see cref="IPAddress"/> for a port.
        /// </summary>
        /// <param name="ipAddressPool"></param>
        /// <param name="port"></param>
        public void Scan(List<IPAddress> ipAddressPool, ushort port)
        {
            if (port <= 0)
            {
                throw new Exception("Invalid port number supplied.");
            }

            if (this.isRunning)
            {
                throw new Exception("Scan is already in Progress.");
            }

            this.isRunning = true;
            this.portScanResults = new List<PortScannerResult>();
            this.ipAddressPool = new List<IPAddress>(ipAddressPool);
            this.port = port;
            this.threadFinishedCount = 0;

            for (int i = 0; i < this.Connections; i++)
            {
                Thread portScan = new Thread(this.ScanIPRange);
                portScan.Name = string.Format("PortScan IPRange {0}", i);
                portScan.Start();
            }
        }

        private void ScanPortRange()
        {
            ushort processPort = 0;

            lock (this.sync)
            {
                if (this.portRange.Count > 0)
                {
                    processPort = this.portRange[0];
                    this.portRange.RemoveAt(0);
                }
            }

            while (processPort > 0)
            {
                bool isOpen = IP.ConnectTest(this.ipAddress, processPort, this.timeout);

                PortScannerResult portScanResult = new PortScannerResult(this.ipAddress, processPort, isOpen);

                processPort = 0;

                lock (this.sync)
                {
                    this.portScanResults.Add(portScanResult);

                    if (this.portRange.Count > 0)
                    {
                        processPort = this.portRange[0];
                        this.portRange.RemoveAt(0);
                        Debug.WriteLine(string.Format("Scanning Port: {0}", processPort));
                    }
                }
            }

            lock (this.sync)
            {
                this.threadFinishedCount++;

                if (this.Connections == this.threadFinishedCount)
                {
                    this.OnPortScanCompleted();
                }
            }
        }

        private void ScanIPRange()
        {
            IPAddress processIPAddress = null;

            lock (this.sync)
            {
                if (this.ipAddressPool.Count > 0)
                {
                    processIPAddress = this.ipAddressPool[0];
                    this.ipAddressPool.RemoveAt(0);
                }
            }

            while (processIPAddress != null)
            {
                bool isOpen = IP.ConnectTest(processIPAddress, this.port, this.timeout);

                PortScannerResult portScanResult = new PortScannerResult(processIPAddress, this.port, isOpen);

                processIPAddress = null;

                lock (this.sync)
                {
                    this.portScanResults.Add(portScanResult);

                    if (this.ipAddressPool.Count > 0)
                    {
                        processIPAddress = this.ipAddressPool[0];
                        this.ipAddressPool.RemoveAt(0);
                        Debug.WriteLine(string.Format("Scanning IP: {0}", processIPAddress));
                    }
                }
            }

            lock (this.sync)
            {
                this.threadFinishedCount++;

                if (this.Connections == this.threadFinishedCount)
                {
                    this.OnPortScanCompleted();
                }
            }
        }

        private void OnPortScanCompleted()
        {
            this.isRunning = false;

            EventHandler<PortScannerCompletedEventArgs> portScanCompleted = this.PortScanCompleted;

            if (portScanCompleted != null)
            {
                this.portScanResults.Sort((x, y) => x.Port.CompareTo(y.Port));
                PortScannerCompletedEventArgs portScanCompletedEventArgs = new PortScannerCompletedEventArgs(this.portScanResults);
                portScanCompleted(this, portScanCompletedEventArgs);
            }
        }


    }
}
