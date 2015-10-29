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
namespace Arrowgene.Services.Network.UDP
{
    using Common;
    using System;
    using System.Net;


    public class ReceivedUDPPacketEventArgs : EventArgs
    {
        private ByteBuffer readableBuffer;


        public ReceivedUDPPacketEventArgs(int size, byte[] received, IPEndPoint remoteIPEndPoint)
        {
            this.Size = size;
            this.Received = received;
            this.RemoteIPEndPoint = remoteIPEndPoint;
        }


        public ByteBuffer ReadableBuffer { get { return this.GetReadableBuffer(); } }


        public IPEndPoint RemoteIPEndPoint { get; private set; }


        public byte[] Received { get; private set; }


        public int Size { get; private set; }

        private ByteBuffer GetReadableBuffer()
        {
            if (this.readableBuffer == null)
            {
                readableBuffer = new ByteBuffer(this.Received);
            }
            return this.readableBuffer;
        }

    }
}
