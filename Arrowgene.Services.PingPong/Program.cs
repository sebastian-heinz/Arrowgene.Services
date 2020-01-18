using System;
using System.Net;
using Arrowgene.Services.Logging;
using Arrowgene.Services.Networking.Tcp.Server.AsyncEvent;
using Arrowgene.Services.PingPong.Handler;

namespace Arrowgene.Services.PingPong
{
    class Program
    {
        static void Main(string[] args)
        {
            int threads = 1;
            int timeout = -1;
            int connections = 100;
            int buffer = 2000;
            ushort port = 9999;
            
            foreach (string arg in args)
            {
                if (arg.StartsWith('-'))
                {
                    string argument = arg.Substring(1);
                    string[] keyValue = argument.Split('=');
                    if (keyValue.Length != 2)
                    {
                        continue;
                    }

                    string key = keyValue[0];
                    string value = keyValue[1];

                    switch (key)
                    {
                        case "threads":
                        {
                            if (!int.TryParse(value, out threads))
                            {
                                Console.WriteLine($"Invalid parameter for threads {value}");
                            }
                            Console.WriteLine($"Threads={threads}");
                            break;
                        }
                        case "timeout":
                        {
                            if (!int.TryParse(value, out timeout))
                            {
                                Console.WriteLine($"Invalid parameter for timeout {value}");
                            }
                            Console.WriteLine($"Timeout={timeout}");
                            break;
                        }
                        case "connections":
                        {
                            if (!int.TryParse(value, out connections))
                            {
                                Console.WriteLine($"Invalid parameter for connections {value}");
                            }
                            Console.WriteLine($"Connections={connections}");
                            break;
                        }
                        case "buffer":
                        {
                            if (!int.TryParse(value, out buffer))
                            {
                                Console.WriteLine($"Invalid parameter for buffer {value}");
                            }
                            Console.WriteLine($"Connections={buffer}");
                            break;
                        }
                        case "port":
                        {
                            if (!ushort.TryParse(value, out port))
                            {
                                Console.WriteLine($"Invalid parameter for port {value}");
                            }
                            Console.WriteLine($"Port={port}");
                            break;
                        }
                    }
                }
            }
            
            Console.WriteLine($"Threads: {threads}");
            Console.WriteLine($"Timeout: {timeout}");
            Console.WriteLine($"Connections: {connections}");
            Console.WriteLine($"Buffer: {buffer}");
            
            LogProvider.GlobalLogWrite += LogProviderOnLogWrite;
            
            AsyncEventSettings settings = new AsyncEventSettings();
            settings.MaxUnitOfOrder = threads;
            settings.SocketTimeoutSeconds = timeout;
            settings.MaxConnections = connections;
            settings.NumSimultaneouslyWriteOperations = connections;
            settings.BufferSize = buffer;
            
            PingPongConsumer consumer = new PingPongConsumer(settings);
            consumer.AddHandler(new PingPongHandler());
            AsyncEventServer server = new AsyncEventServer(IPAddress.Any, port, consumer, settings);

            server.Start();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            server.Stop();
        }
        
        private static void LogProviderOnLogWrite(object sender, LogWriteEventArgs logWriteEventArgs)
        {
            Console.WriteLine(logWriteEventArgs.Log);
        }
    }
}