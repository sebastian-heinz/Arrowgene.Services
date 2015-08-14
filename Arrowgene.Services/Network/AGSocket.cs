namespace Arrowgene.Services.Network
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// 
    /// </summary>
    public class AGSocket
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

        private Socket socket;


        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public AGSocket()
        {
            this.socket = CreateSocket();
        }

        /// <summary>
        /// Creates a new instance with an existing socket.
        /// </summary>
        public AGSocket(Socket socket)
        {
            this.socket = socket;
        }

        /// <summary>
        /// Gets a value that indicates whether a Socket is connected to a remote host as of the last Send or Receive operation.
        /// </summary>
        public bool Connected { get { return this.socket.Connected; } }

        /// <summary>
        /// Returns IP Address for given hostname.
        /// If Supported, returns IPv6 IP, 
        /// if no IPv6 IP was found or IPv6 is not Supported,
        /// it will try to return a IPv4 IP address.
        /// </summary>
        /// <param name="hostname">Name of host.</param>
        /// <returns>
        /// Returns <see cref="IPAddress"/> on success,
        /// null on failure
        /// </returns>
        public static IPAddress IPAddressLookup(string hostname)
        {
            AddressFamily addressFamily;
            if (IPv6Support())
            {
                addressFamily = AddressFamily.InterNetworkV6;
            }
            else
            {
                addressFamily = AddressFamily.InterNetwork;
            }
            return IPAddressLookup(hostname, addressFamily);
        }

        public static IPAddress IPAddressLocalhost(AddressFamily addressFamily)
        {
            return IPAddressLookup("localhost", addressFamily);
        }

        /// <summary>
        /// Returns IP Address for given hostname.
        /// Tries to return the IP of specified IP version,
        /// if a IPv6 IP can not be retrived,
        /// it will be tried to return a IPv4 IP.
        /// </summary>
        /// <param name="hostname">Name of host.</param>
        /// <param name="addressFamily">Specific IP version.</param>
        /// <returns>
        /// Returns <see cref="IPAddress"/> on success,
        /// null on failure.
        /// </returns>
        public static IPAddress IPAddressLookup(string hostname, AddressFamily addressFamily)
        {
            IPAddress ipAdress = null;

            try
            {
                IPAddress[] ipAddresses = Dns.GetHostAddresses(hostname);

                foreach (IPAddress ipAddr in ipAddresses)
                {
                    if (ipAddr.AddressFamily == addressFamily)
                    {
                        ipAdress = ipAddr;
                        break;
                    }
                }

                if (ipAdress == null && IPv6Support())
                {
                    foreach (IPAddress ipAddr in ipAddresses)
                    {
                        if (ipAddr.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            ipAdress = ipAddr;
                            break;
                        }
                    }
                }

                if (ipAdress == null)
                {
                    foreach (IPAddress ipAddr in ipAddresses)
                    {
                        if (ipAddr.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAdress = ipAddr;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AGSocket::IPAddressLookup:" + ex.Message);
            }

            return ipAdress;
        }

        /// <summary>
        /// Tries to determine wether IPv6 may be supported.
        /// </summary>
        /// <returns>
        /// Returns <see cref="bool"/>.
        /// </returns>
        public static bool IPv6Support()
        {
            bool result = false;
            int major = Environment.OSVersion.Version.Major;
            PlatformID platformId = Environment.OSVersion.Platform;

            if (platformId == PlatformID.Win32NT && major >= 6)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Creates a default .net TCP Stream Socket with the specified adress family.
        /// </summary>
        public static Socket CreateSocket(AddressFamily adressFamily)
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

        /// <summary>
        /// Creates a default .net TCP Stream Socket.
        /// If creation of an IPv6 socket fails, an IPv4 socket will be created.
        /// </summary>
        public static Socket CreateSocket()
        {
            Socket socket = null;
            if (IPv6Support())
            {
                socket = CreateSocket(AddressFamily.InterNetworkV6);
            }
            else
            {
                socket = CreateSocket(AddressFamily.InterNetwork);
            }
            return socket;
        }

        /// <summary>
        /// Sends data to a connected socket.
        /// </summary>
        public void Send(byte[] buffer)
        {
            this.socket.Send(buffer);
        }

        /// <summary>
        /// Determines the status of the Socket.
        /// </summary>
        public bool Poll(int microSeconds, SelectMode mode)
        {
            return this.socket.Poll(microSeconds, mode);
        }

        /// <summary>
        /// Receives data from a bound Socket into a receive buffer.
        /// </summary>
        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            return this.socket.Receive(buffer, offset, size, socketFlags);
        }

        /// <summary>
        /// Establishes a connection to a remote host.
        /// </summary>
        public void Connect(IPEndPoint remoteEp)
        {
            this.socket.Connect(remoteEp);
        }

        /// <summary>
        /// Associates a Socket with a local endpoint.
        /// </summary>
        public void Bind(IPEndPoint localEP)
        {
            this.socket.Bind(localEP);
        }

        /// <summary>
        /// Places a Socket in a listening state.
        /// </summary>
        public void Listen(int backlog)
        {
            this.socket.Listen(backlog);
        }

        /// <summary>
        /// Creates a new Socket for a newly created connection.
        /// </summary>
        public AGSocket Accept()
        {
            return new AGSocket(this.socket.Accept());
        }



    }
}
