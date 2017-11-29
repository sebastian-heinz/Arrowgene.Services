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


using System;
using System.Net;

namespace Arrowgene.Services.Networking.Tcp.Client.EventConsumer.EventHandler
{
    public class EventHandlerConsumer : IClientEventConsumer
    {
        public event EventHandler<DisconnectedEventArgs> ClientDisconnected;

        public event EventHandler<ConnectedEventArgs> ClientConnected;

        public event EventHandler<ReceivedPacketEventArgs> ReceivedPacket;

        public event EventHandler<ConnectErrorEventArgs> ConnectError;

        public void OnStart()
        {
        }

        public void OnReceivedPacket(ITcpClient client, byte[] data)
        {
            EventHandler<ReceivedPacketEventArgs> receivedPacket = ReceivedPacket;
            if (receivedPacket != null)
            {
                ReceivedPacketEventArgs receivedPacketEventArgs = new ReceivedPacketEventArgs(client, data);
                receivedPacket(this, receivedPacketEventArgs);
            }
        }

        public void OnClientDisconnected(ITcpClient client)
        {
            EventHandler<DisconnectedEventArgs> clientDisconnected = ClientDisconnected;
            if (clientDisconnected != null)
            {
                DisconnectedEventArgs clientDisconnectedEventArgs = new DisconnectedEventArgs(client);
                clientDisconnected(this, clientDisconnectedEventArgs);
            }
        }

        public void OnClientConnected(ITcpClient client)
        {
            EventHandler<ConnectedEventArgs> clientConnected = ClientConnected;
            if (clientConnected != null)
            {
                ConnectedEventArgs clientConnectedEventArgs = new ConnectedEventArgs(client);
                clientConnected(this, clientConnectedEventArgs);
            }
        }

        public void OnConnectError(ITcpClient client, string reason, IPAddress serverIpAddress, int serverPort, TimeSpan timeout)
        {
            EventHandler<ConnectErrorEventArgs> connectError = ConnectError;
            if (connectError != null)
            {
                ConnectErrorEventArgs connectErrorEventArgs = new ConnectErrorEventArgs(client, reason, serverIpAddress, serverPort, timeout);
                connectError(this, connectErrorEventArgs);
            }
        }

        public void OnStop()
        {
        }
    }
}