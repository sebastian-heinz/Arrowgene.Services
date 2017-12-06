using System;
using Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Client;
using Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Models;
using Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Server;

namespace Arrowgene.Services.Playground.Demo.TcpMessageProtocol
{
    public class TcpMessageProtocolDemo
    {
        public TcpMessageProtocolDemo()
        {
            MpServer server = new MpServer();
            server.Start();

            MpClient client = new MpClient();
            client.Connect();

            Login message = new Login();
            message.Name = "Test Account";
            message.Hash = "Test Hash";
            client.Send(message);

            Console.WriteLine("Demo: Press any key to exit.");
            Console.ReadKey();

            client.Disconnect(); 
            server.Stop();
        }
    }
}