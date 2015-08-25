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
namespace Arrowgene.Services.Network
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;

    /// <summary>
    /// Helps with IPAddress
    /// </summary>
    public static class IP
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

    }
}