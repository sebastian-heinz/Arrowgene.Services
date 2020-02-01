using System;

namespace Arrowgene.Services.Assembly.X86
{
    public class ROL : OpCode
    {
        public static UInt64 RotateLeft(UInt64 x, int n) {
            return (x << n) | (x >> (64-n));
        }
        
        public static UInt32 RotateLeft(UInt32 x, int n) {
            return (x << n) | (x >> (32-n));
        }
        
        public static UInt16 RotateLeft(UInt16 x, int n) {
            return (ushort) ((x << n) | (x >> (16-n)));
        }
    }
}