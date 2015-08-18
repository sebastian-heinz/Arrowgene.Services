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

        public static IPEndPoint QueryRoutingInterface()
        {
            IPAddress remoteIp = IPAddress.Parse("173.194.112.8"); //server ip
            IPEndPoint remoteEndPoint = new IPEndPoint(remoteIp, 0);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localEndPoint = QueryRoutingInterface(socket, remoteEndPoint);

            return localEndPoint;
        }


        public NetworkInterface GetNetworkInterface(IPAddress ipAddress)
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