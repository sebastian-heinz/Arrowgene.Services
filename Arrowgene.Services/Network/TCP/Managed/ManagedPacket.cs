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
namespace Arrowgene.Services.Network.TCP.Managed
{
    /// <summary>
    /// Class to manage packet content
    /// </summary>
    public class ManagedPacket
    {
        public const int HeaderSize = 8;

        public ManagedPacket(int id, object content)
        {
            this.Id = id;
            this.Object = content;
        }

        /// <summary>
        /// Id to identify the packet
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Transfered Object
        /// </summary>
        public object Object { get; internal set; }

        /// <summary>
        /// Returns concrete class or value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetObject<T>()
        {
            T myObject = (T)this.Object;
            return myObject;
        }
    }
}