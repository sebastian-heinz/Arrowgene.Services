using System.Net.Sockets;
using Arrowgene.Services.Networking;
using Arrowgene.Services.Serialization;
using Xunit;

namespace Arrowgene.Services.Test
{
    public class SocketOptionTest
    {
        [Fact]
        public void TestSocketOptionSerialisation()
        {
            SocketOption option = new SocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
            string json = JsonSerializer.Serialize(option);
            SocketOption newOption  = JsonSerializer.Deserialize<SocketOption>(json);
            Assert.Equal(option.Name, newOption.Name);
            Assert.Equal(option.Level, newOption.Level);
            Assert.Equal(option.Value, newOption.Value);
        }
    }
}