namespace Arrowgene.Services.Network.Tcp.Server.AsyncEvent
{
    public class AsyncEventSettings
    {
        public int MaxConnections { get; set; }
        public int NumSimultaneouslyWriteOperations { get; set; }
        public int BufferSize { get; set; }
        public int Backlog { get; set; }

        public AsyncEventSettings()
        {
            BufferSize = 2000;
            MaxConnections = 100;
            NumSimultaneouslyWriteOperations = 100;
            Backlog = 5;
        }
    }
}