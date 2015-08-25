namespace Arrowgene.Services.Network.Discovery
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading;

    public class PortScan
    {

        public PortScan(int connections)
        {
            this.timeout = new TimeSpan(5000);
            this.Connections = connections;
            this.sync = new object();
        }

        public event EventHandler<PortScanCompletedEventArgs> PortScanCompleted;

        public int Connections { get; private set; }

        public void Scan(IPAddress ipAddress, int startPort, int endPort)
        {
            this.portScanResults = new List<PortScanResult>();
            this.ipAddress = ipAddress;
            this.portRange = new List<int>();

            for (int i = startPort; startPort < endPort; i++)
            {
                portRange.Add(i);
            }

            for (int i = 0; i < this.Connections; i++)
            {
                Thread portScan = new Thread(this.ScanPortRange);
                portScan.Name = string.Format("PortScan {0}", i);
                portScan.Start();
            }

        }

        public void Scan(IPAddress startIPAddress, IPAddress sendIPAddress, int port)
        {



        }

        private object sync;
        private List<int> portRange;
        private IPAddress ipAddress;
        private TimeSpan timeout;
        private List<PortScanResult> portScanResults;

        private void ScanPortRange()
        {
            int count = 0;
            lock (this.sync)
            {
                count = portRange.Count;
            }

            while (count > 0)
            {
                int processPort = 0;

                lock (this.sync)
                {
                    processPort = portRange[0];
                    portRange.RemoveAt(0);
                }

                bool isConnected = AGSocket.ConnectTest(this.ipAddress, processPort, this.timeout);

                PortScanResult portScanResult = new PortScanResult(this.ipAddress, processPort, isConnected);
                this.portScanResults.Add(portScanResult);
            }



        }

        private void OnPortScanCompleted()
        {
            EventHandler<PortScanCompletedEventArgs> portScanCompleted = this.PortScanCompleted;

            if (portScanCompleted != null)
            {
                PortScanCompletedEventArgs portScanCompletedEventArgs = new PortScanCompletedEventArgs(this.portScanResults);
                portScanCompleted(this, portScanCompletedEventArgs);
            }
        }


    }
}
