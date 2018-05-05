using Arrowgene.Services.Buffers;
using Xunit;

namespace Arrowgene.Services.Test
{
    public class BufferTest
    {
        [Fact]
        public void TestWriteFixedString()
        {
            string textExact = "1234567890";
            string textLess = "123456789";
            string textMore = "12345678901";
            int length = 10;

            Buffer bufferExact = new StreamBuffer();
            bufferExact.WriteFixedString(textExact, length);
            Assert.True(bufferExact.GetAllBytes().Length == length);
            
            Buffer bufferLess = new StreamBuffer();
            bufferLess.WriteFixedString(textLess, length);
            Assert.True(bufferLess.GetAllBytes().Length == length);
            
            Buffer bufferMore = new StreamBuffer();
            bufferMore.WriteFixedString(textMore, length);
            Assert.True(bufferMore.GetAllBytes().Length == length);
            
        }
    }
}