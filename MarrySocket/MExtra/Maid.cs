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
namespace MarrySocket.MExtra
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// TODO SUMMARY
    /// </summary>
    public static class Maid
    {
        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public static Random Random = new Random();

        /// <summary>
        /// Returns IP Address for given hostname.
        /// If Supported, returns IPv6 IP, 
        /// if no IPv6 IP was found or IPv6 is not Supported,
        /// it will try to return a IPv4 IP address.
        /// </summary>
        /// <param name="hostname">Name of host.</param>
        /// <returns>
        /// Returns <see cref="IPAddress"/> on success,
        /// nuöö on failure
        /// </returns>
        public static IPAddress IPAddressLookup(string hostname)
        {
            AddressFamily addressFamily;
            if (Maid.IPv6Support())
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
                Debug.Write("Maid:IPAddressLookup::" + ex.Message);
            }

            return ipAdress;
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public enum OsVersion
        {
            /// <summary>UNKNOWN</summary>
            UNKNOWN = 1,
            /// <summary>WIN_3_1</summary>
            WIN_3_1,
            /// <summary>WIN_95</summary>
            WIN_95,
            /// <summary>WIN_98</summary>
            WIN_98,
            /// <summary>WIN_ME</summary>
            WIN_ME,
            /// <summary>WIN_NT_3_5</summary>
            WIN_NT_3_5,
            /// <summary>WIN_NT_4</summary>
            WIN_NT_4,
            /// <summary>WIN_2000</summary>
            WIN_2000,
            /// <summary>WIN_XP</summary>
            WIN_XP,
            /// <summary>WIN_2003</summary>
            WIN_2003,
            /// <summary>WIN_VISTA</summary>
            WIN_VISTA,
            /// <summary>WIN_2008</summary>
            WIN_2008,
            /// <summary>WIN_7</summary>
            WIN_7,
            /// <summary>WIN_2008_R2</summary>
            WIN_2008_R2,
            /// <summary>WIN_8</summary>
            WIN_8,
            /// <summary>WIN_8_1</summary>
            WIN_8_1,
            /// <summary>WIN_10</summary>
            WIN_10,
            /// <summary>WIN_CE</summary>
            WIN_CE,
            /// <summary>UNIX</summary>
            UNIX,
            /// <summary>XBOX</summary>
            XBOX,
            /// <summary>MAX_OSX</summary>
            MAX_OSX
        };

        /// <summary>
        /// Returns version of OS.
        /// </summary>
        /// <returns>
        /// Returns <see cref="OsVersion"/>.
        /// </returns>
        /// <remarks>
        /// In order to detect cetain windows versions,
        /// it is necessary to add a custom .manifest file to the project.
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/dn481241%28v=vs.85%29.aspx
        /// Otherwise win 8.1 will be reconized as win 8.0 for example.
        /// </remarks>
        public static OsVersion GetOperatingSystemVersion()
        {
            int major = Environment.OSVersion.Version.Major;
            int minor = Environment.OSVersion.Version.Minor;
            PlatformID platformId = Environment.OSVersion.Platform;
            OsVersion osVersion = OsVersion.UNKNOWN;

            switch (platformId)
            {
                case PlatformID.Win32S:
                    osVersion = OsVersion.WIN_3_1;
                    break;

                case PlatformID.Win32Windows:
                    switch (major)
                    {
                        case 4:
                            switch (minor)
                            {
                                case 0:
                                    osVersion = OsVersion.WIN_95;
                                    break;
                                case 10:
                                    osVersion = OsVersion.WIN_98;
                                    break;
                                case 90:
                                    osVersion = OsVersion.WIN_ME;
                                    break;
                            }
                            break;
                    }
                    break;

                case PlatformID.Win32NT:
                    switch (major)
                    {
                        case 3:
                            osVersion = OsVersion.WIN_NT_3_5;
                            break;

                        case 4:
                            osVersion = OsVersion.WIN_NT_4;
                            break;

                        case 5:
                            switch (minor)
                            {
                                case 0:
                                    osVersion = OsVersion.WIN_2000;
                                    break;
                                case 1:
                                    osVersion = OsVersion.WIN_XP;
                                    break;
                                case 2:
                                    osVersion = OsVersion.WIN_2003;
                                    break;
                            }
                            break;

                        case 6:
                            switch (minor)
                            {
                                case 0:
                                    osVersion = OsVersion.WIN_VISTA;
                                    break;
                                case 1:
                                    osVersion = OsVersion.WIN_7;
                                    break;
                                case 2:
                                    osVersion = OsVersion.WIN_8;
                                    break;
                                case 3:
                                    osVersion = OsVersion.WIN_8_1;
                                    break;
                            }
                            break;
                    }
                    break;

                case PlatformID.WinCE:
                    osVersion = OsVersion.WIN_CE;
                    break;

                case PlatformID.Unix:
                    osVersion = OsVersion.UNIX;
                    break;

                case PlatformID.Xbox:
                    osVersion = OsVersion.XBOX;
                    break;

                case PlatformID.MacOSX:
                    osVersion = OsVersion.MAX_OSX;
                    break;
            }

            return osVersion;
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
        /// TODO SUMMARY
        /// </summary>
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public static string GetString(byte[] bytes, int srcOffset, int length)
        {
            char[] chars = new char[length / sizeof(char)];
            Buffer.BlockCopy(bytes, srcOffset, chars, 0, length);
            return new string(chars);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public static string GetString(byte[] bytes)
        {
            return Maid.GetString(bytes, 0, bytes.Length);
        }
    }
}
