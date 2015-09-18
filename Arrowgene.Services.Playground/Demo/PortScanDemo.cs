namespace Arrowgene.Services.Playground.Demo
{
    using Network.PortScan;
    using System.Diagnostics;
    using System.Net;

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
            bool anyPortOpen = false;
            foreach (PortScannerResult psResult in e.PortScanResults)
            {
                if (psResult.IsOpen)
                {
                    Debug.WriteLine("Open Port: " + psResult.Port);
                    anyPortOpen = true;
                }
            }

            if(!anyPortOpen)
            {
                Debug.WriteLine("No Open Port found.");
            }

        }

    }
}
