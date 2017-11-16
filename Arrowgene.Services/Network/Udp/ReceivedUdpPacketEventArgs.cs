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

namespace Arrowgene.Services.Network.Udp
{
    using Buffers;
    using System;
    using System.Net;


    public class ReceivedUdpPacketEventArgs : EventArgs
    {
        private IBuffer readableBuffer;


        public ReceivedUdpPacketEventArgs(int size, byte[] received, IPEndPoint remoteIPEndPoint)
        {
            this.Size = size;
            this.Received = received;
            this.RemoteIPEndPoint = remoteIPEndPoint;
        }


        public IBuffer ReadableBuffer
        {
            get { return this.GetReadableBuffer(); }
        }


        public IPEndPoint RemoteIPEndPoint { get; private set; }


        public byte[] Received { get; private set; }


        public int Size { get; private set; }

        private IBuffer GetReadableBuffer()
        {
            if (this.readableBuffer == null)
            {
                readableBuffer = new ByteBuffer(this.Received);
            }
            return this.readableBuffer;
        }
    }
}