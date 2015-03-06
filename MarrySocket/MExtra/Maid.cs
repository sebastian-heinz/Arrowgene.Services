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
    using System.Net;
    using System.Net.Sockets;

    public static class Maid
    {
        public static Random Random = new Random();

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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return ipAdress;
        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes, int srcOffset, int length)
        {
            char[] chars = new char[length / sizeof(char)];
            Buffer.BlockCopy(bytes, srcOffset, chars, 0, length);
            return new string(chars);
        }

        public static string GetString(byte[] bytes)
        {
            return Maid.GetString(bytes, 0, bytes.Length);
        }


        /*
         * Memos..
         * 
         * 
         * According to the StyleCop Rules Documentation the ordering is as follows.
         * Within a class, struct or interface: (SA1201 and SA1203)
         * Constant Fields
         * Fields
         * Constructors
         * Finalizers (Destructors)
         * Delegates
         * Events
         * Enums
         * Interfaces
         * Properties
         * Indexers
         * Methods
         * Structs
         * Classes
         * Within each of these groups order by access: (SA1202)
         * public
         * internal
         * protected internal
         * protected
         * private
         * Within each of the access groups, order by static, then non-static: (SA1204)    
         * static
         * non-static
         * Within each of the static/non-static groups of fields, order by readonly, then non-readonly : (SA1214 and SA1215)
         * readonly
         * non-readonly
         * An unrolled list is 130 lines long, so I won't unroll it here. The methods part unrolled is:
         * public static methods
         * public methods
         * internal static methods
         * internal methods
         * protected internal static methods
         * protected internal methods
         * protected static methods
         * protected methods
         * private static methods
         * private methods
         * The documentation notes that if the prescribed order isn't suitable --- say, multiple interfaces are being implemented, and the interface methods and properties should be grouped together --- then use a partial class to group the related methods and properties together.
         * 
         */



    }
}
