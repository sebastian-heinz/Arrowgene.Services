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
namespace Arrowgene.Services.Network.Proxy
{
    using Exceptions;
    using Arrowgene.Services.Logging;

    using System.Net;

    public class ProxyConfig
    {
        public Logger Logger { get; private set; }
        public IPAddress ProxyListenIP { get; private set; }
        public int ProxyPort { get; private set; }
        public IPAddress ServerIP { get; private set; }
        public int ServerPort { get; private set; }
        public int BufferSize { get; set; }
        public IPEndPoint ProxyEndPoint { get { return new IPEndPoint(this.ProxyListenIP, this.ProxyPort); } }
        public IPEndPoint ServerEndPoint { get { return new IPEndPoint(this.ServerIP, this.ServerPort); } }
        public int ReadTimeout { get; set; }
        public int Backlog { get; set; }
        public int PollResponseWait { get; set; }

        public ProxyConfig(IPAddress proxyListenIP, int proxyPort, IPAddress serverIP, int serverPort)
        {
            if (proxyListenIP == null || proxyPort <= 0 || serverIP == null || serverPort <= 0)
                throw new InvalidParameterException(string.Format("ProxyServer(IP:{0}/Port:{1}) or ProxyDestination(IP:{2}/Port:{3}) invalid",
                    proxyListenIP, proxyPort, serverIP, serverPort));

            this.Logger = new Logger("ProxyServer");
            this.ProxyPort = proxyPort;
            this.ProxyListenIP = proxyListenIP;
            this.ServerIP = serverIP;
            this.ServerPort = serverPort;

            this.LoadDefaultConfig();
        }

        public void LoadDefaultConfig()
        {
            this.BufferSize = 10000;
            this.ReadTimeout = 100;
            this.Backlog = 10;
            this.PollResponseWait = 100;
        }

    }
}