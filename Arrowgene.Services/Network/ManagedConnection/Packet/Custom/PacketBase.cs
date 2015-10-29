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
namespace Arrowgene.Services.Network.ManagedConnection.Packet.Custom
{
    using System.IO;

    /// <summary>
    /// TODO SUMMARY
    /// </summary>
    public abstract class PacketBase
    {
        /// <summary>TODO SUMMARY</summary>
        protected MemoryStream memoryBuffer;

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        protected PacketBase()
        {
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public virtual byte[] Buffer
        {
            get
            {
                return this.memoryBuffer.GetBuffer();
            }
            set
            {
            }
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public int BufferPosition
        {
            get
            {
                return (int)this.memoryBuffer.Position;
            }
            set
            {
                this.memoryBuffer.Position = value;
            }
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public int BufferSize
        {
            get
            {
                return (int)this.memoryBuffer.Length;
            }
        }
    }
}

