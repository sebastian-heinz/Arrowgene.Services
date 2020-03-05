using System;
using System.Collections.Generic;
using Arrowgene.Networking.Tcp;
using Arrowgene.Services.Networking.Tcp.Consumer.GenericConsumption;

namespace Arrowgene.Services.Networking.Tcp.Consumer.Messages
{
    public class MessageConsumer : GenericConsumer<Message>, IMessageSerializer
    {
        private readonly Dictionary<int, IMessageHandle> _handles;

        public MessageConsumer()
        {
            _handles = new Dictionary<int, IMessageHandle>();
        }

        public void AddHandle(IMessageHandle handle)
        {
            if (_handles.ContainsKey(handle.Id))
            {
                throw new Exception(string.Format("Handle for id: {0} already defined.", handle.Id));
            }

            handle.SetMessageSerializer(this);
            _handles.Add(handle.Id, handle);
        }

        protected override void OnReceivedGeneric(ITcpSocket socket, Message message)
        {
            base.OnReceivedGeneric(socket, message);
            if (_handles.ContainsKey(message.Id))
            {
                _handles[message.Id].Process(message, socket);
            }
        }
    }
}