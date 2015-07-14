namespace SvrKit.Networking
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;


    /// <summary>
    /// Helps dealing with IPAddresses
    /// </summary>
    public static class IPTools
    {

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

                if (ipAdress == null && addressFamily == AddressFamily.InterNetworkV6)
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
                Debug.WriteLine("IPTools::IPAddressLookup:" + ex.Message);
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


    }
}
