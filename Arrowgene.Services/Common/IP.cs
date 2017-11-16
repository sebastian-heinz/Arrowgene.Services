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

namespace Arrowgene.Services.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;


    /// <summary>
    /// Helps with IPAddress
    /// </summary>
    public static class IP
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
        public const SocketOptionName USE_IPV6_ONLY = (SocketOptionName) 27;

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
            return (IPEndPoint) ep;
        }

        /// <summary>
        /// Determines most possible local <see cref="IPEndPoint"/> for connecting to the specified <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="remoteIp"></param>
        /// <returns></returns>
        public static IPEndPoint QueryRoutingInterface(IPAddress remoteIp)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(remoteIp, 0);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localEndPoint = IP.QueryRoutingInterface(socket, remoteEndPoint);
            return localEndPoint;
        }

        /// <summary>
        /// Determines the <see cref="NetworkInterface"/> of the specified <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static NetworkInterface FindNetworkInterface(IPAddress ipAddress)
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

        /// <summary>
        /// Try to get mac for ip, if not possible get the next best mac.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static string GetMacAddress(IPAddress ipAddress)
        {
            string mac = null;

            if (ipAddress != null)
            {
                NetworkInterface nic = FindNetworkInterface(ipAddress);

                if (nic != null)
                {
                    mac = nic.GetPhysicalAddress().ToString();
                }
            }

            if (mac == null)
            {
                mac = GetMacAddress();
            }

            return mac;
        }

        /// <summary>
        /// Try to get the next best mac
        /// </summary>
        /// <returns></returns>
        public static string GetMacAddress()
        {
            List<NetworkInterfaceType> acceptedNetInterfaceTypes = new List<NetworkInterfaceType>
            {
                NetworkInterfaceType.Ethernet,
                NetworkInterfaceType.Ethernet3Megabit,
                NetworkInterfaceType.FastEthernetFx,
                NetworkInterfaceType.FastEthernetT,
                NetworkInterfaceType.GigabitEthernet,
                NetworkInterfaceType.Wireless80211
            };

            string mac = null;

            List<NetworkInterface> nics = new List<NetworkInterface>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up && acceptedNetInterfaceTypes.Contains(nic.NetworkInterfaceType))
                {
                    nics.Add(nic);
                }
            }

            if (nics.Count > 0)
            {
                nics.Sort((x, y) => y.Speed.CompareTo(x.Speed));
                NetworkInterface nic = nics[0];
                mac = nic.GetPhysicalAddress().ToString();
            }

            return mac;
        }

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
        public static IPAddress AddressLookup(string hostname)
        {
            AddressFamily addressFamily;
            if (IP.V6Support())
            {
                addressFamily = AddressFamily.InterNetworkV6;
            }
            else
            {
                addressFamily = AddressFamily.InterNetwork;
            }
            return IP.AddressLookup(hostname, addressFamily);
        }

        /// <summary>
        /// Returns <see cref="IPAddress"/> of localhost for a given <see cref="AddressFamily"/>
        /// </summary>
        /// <param name="addressFamily"></param>
        /// <returns></returns>
        public static IPAddress AddressLocalhost(AddressFamily addressFamily)
        {
            return IP.AddressLookup("localhost", addressFamily);
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
        public static IPAddress AddressLookup(string hostname, AddressFamily addressFamily)
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

                if (ipAdress == null && IP.V6Support())
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
                Debug.WriteLine("IP::AddressLookup:" + ex.Message);
            }

            return ipAdress;
        }

        /// <summary>
        /// Determines wether IPv6 may be supported.
        /// </summary>
        /// <returns>
        /// Returns <see cref="bool"/>.
        /// </returns>
        public static bool V6Support()
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
        /// Connects the specified socket.
        /// </summary>
        /// <param name="endpoint">The IP endpoint.</param>
        /// <param name="timeout">The timeout.</param>
        public static bool ConnectTest(IPEndPoint endpoint, TimeSpan timeout)
        {
            Socket socket = null;

            if (endpoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            }
            else
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            IAsyncResult result = socket.BeginConnect(endpoint, null, null);

            bool success = result.AsyncWaitHandle.WaitOne(timeout, true);

            if (socket.Connected && success)
            {
                socket.EndConnect(result);
                return true;
            }
            else
            {
                socket.Close();
                return false;
            }
        }

        /// <summary>
        /// Connects the specified socket.
        /// </summary>
        /// <param name="ipAddress">IP endpoint</param>
        /// <param name="port">Port</param>
        /// <param name="timeout">timeout</param>
        public static bool ConnectTest(IPAddress ipAddress, int port, TimeSpan timeout)
        {
            return IP.ConnectTest(new IPEndPoint(ipAddress, port), timeout);
        }
    }
}