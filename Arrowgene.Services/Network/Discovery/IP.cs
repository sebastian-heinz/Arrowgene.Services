namespace Arrowgene.Services.Network.Discovery
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;


    public class IP
    {

        private static IPEndPoint QueryRoutingInterface(Socket socket, IPEndPoint remoteEndPoint)
        {
            SocketAddress address = remoteEndPoint.Serialize();

            byte[] remoteAddrBytes = new byte[address.Size];
            for (int i = 0; i < address.Size; i++)
            {
                remoteAddrBytes[i] = address[i];
            }

            byte[] outBytes = new byte[remoteAddrBytes.Length];
            socket.IOControl(IOControlCode.RoutingInterfaceQuery, remoteAddrBytes, outBytes);
            for (int i = 0; i < address.Size; i++)
            {
                address[i] = outBytes[i];
            }

            EndPoint ep = remoteEndPoint.Create(address);
            return (IPEndPoint)ep;
        }


        /// <summary>
        /// Trys to determine the most possible <see cref="IPEndPoint"/> for connecting to the specified <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="remoteIp"></param>
        /// <returns></returns>
        public static IPEndPoint QueryRoutingInterface(IPAddress remoteIp)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(remoteIp, 0);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localEndPoint = QueryRoutingInterface(socket, remoteEndPoint);
            return localEndPoint;
        }

        /// <summary>
        /// Trys to determine the best suited <see cref="NetworkInterface"/> for connecting to the specified <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static NetworkInterface GetNetworkInterface(IPAddress ipAddress)
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                IPInterfaceProperties ipProps = nic.GetIPProperties();

                foreach (UnicastIPAddressInformation ip in ipProps.UnicastAddresses)
                { 
                    if (ip.Address.AddressFamily == ipAddress.AddressFamily && ip.Address.GetAddressBytes() == ipAddress.GetAddressBytes())
                    {
                        return nic;
                    }
                }
            }
            return null;
        }

    }
}