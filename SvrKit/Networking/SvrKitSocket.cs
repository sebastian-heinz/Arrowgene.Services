namespace SvrKit.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    public class SvrKitSocket : Socket
    {
        /// <summary>
        ///https://msdn.microsoft.com/en-us/library/system.net.sockets.socketoptionname.aspx
        /// IPv6Only	
        /// Indicates if a socket created for the AF_INET6 address family is restricted to IPv6 communications only.
        /// Sockets created for the AF_INET6 address family may be used for both IPv6 and IPv4 communications.
        /// Some applications may want to restrict their use of a socket created for the AF_INET6 address family to IPv6 communications only.
        /// When this value is non-zero (the default on Windows), a socket created for the AF_INET6 address family can be used to send and receive IPv6 packets only.
        /// When this value is zero, a socket created for the AF_INET6 address family can be used to send and receive packets to and from an IPv6 address or an IPv4 address.
        /// Note that the ability to interact with an IPv4 address requires the use of IPv4 mapped addresses.
        /// This socket option is supported on Windows Vista or later.
        /// </summary>
        public const SocketOptionName USE_IPV6_ONLY = (SocketOptionName)27;

        public SvrKitSocket(AddressFamily adressFamily)
        {
      
        }

        public Socket Socket { get; private set; }

        private Socket CreateSocket(IPAddress ipAddress)
        {
            Socket socket = null;
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.IPv6, USE_IPV6_ONLY, false);
            }
            else
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            return socket;
        }

        private Socket CreateSocket(AddressFamily adressFamily)
        {
            Socket socket = null;
            if (adressFamily == AddressFamily.InterNetworkV6)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.IPv6, USE_IPV6_ONLY, false);
            }
            else
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            return socket;
        }

        internal void Send(byte[] data)
        {
            throw new NotImplementedException();
        }

        public bool Connected { get; set; }

        internal bool Poll(int p, SelectMode selectMode)
        {
            throw new NotImplementedException();
        }

        internal int Receive(byte[] p1, int p2, int p3, SocketFlags socketFlags)
        {
            throw new NotImplementedException();
        }
    }
}
