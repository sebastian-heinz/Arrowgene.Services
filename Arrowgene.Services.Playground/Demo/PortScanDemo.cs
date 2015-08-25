namespace Arrowgene.Services.Playground.Demo
{
    using Network.PortScan;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Text;

    public class PortScanDemo
    {

        public PortScanDemo()
        {
            PortScanner ps = new PortScanner(8, 5000);
            ps.PortScanCompleted += Ps_PortScanCompleted;
            ps.Scan(IPAddress.Loopback, 1, 100);

        }

        private void Ps_PortScanCompleted(object sender, PortScannerCompletedEventArgs e)
        {
        
            foreach(PortScannerResult psResult in e.PortScanResults)
            {
                if (psResult.IsOpen)
                    Debug.WriteLine("Open Port: " + psResult.Port);
            }


        }
    }
}
