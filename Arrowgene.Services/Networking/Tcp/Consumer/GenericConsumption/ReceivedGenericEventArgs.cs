using System;
using Arrowgene.Networking.Tcp;

namespace Arrowgene.Services.Networking.Tcp.Consumer.GenericConsumption
{
    public class ReceivedGenericEventArgs<T> : EventArgs
    {
        public ReceivedGenericEventArgs(ITcpSocket socket, T generic)
        {
            Socket = socket;
            Generic = generic;
        }

        public ITcpSocket Socket { get; }

        public T Generic { get; }
    }
}