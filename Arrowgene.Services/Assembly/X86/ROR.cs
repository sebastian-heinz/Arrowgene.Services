using System;

namespace Arrowgene.Services.Assembly.X86
{
    public class ROR : OpCode
    {
        public static UInt64 RotateRight(UInt64 x, int n)
        {
            return (x >> n) | (x << (64 - n));
        }

        public static UInt32 RotateRight(UInt32 x, int n)
        {
            return (x >> n) | (x << (32 - n));
        }

        public static UInt16 RotateRight(UInt16 x, int n)
        {
            return (ushort) ((x >> n) | (x << (16 - n)));
        }
    }
}