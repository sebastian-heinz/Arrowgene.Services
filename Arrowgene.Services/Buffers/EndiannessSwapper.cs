using System;

namespace Arrowgene.Services.Buffers
{
    public class EndiannessSwapper : IEndiannessSwapper
    {
        public EndiannessSwapper(Endianness endianness)
        {
            Endianness = endianness;
        }

        public Endianness Endianness { get; set; }

        public bool SwapNeeded(Endianness currentEndianness, Endianness targetEndianness)
        {
            return currentEndianness != targetEndianness;
        }

        public ushort SwapBytes(ushort x)
        {
            return (ushort) ((x >> 8) | (x << 8));
        }

        public uint SwapBytes(uint x)
        {
            x = (x >> 16) | (x << 16);
            return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
        }

        public ulong SwapBytes(ulong x)
        {
            x = (x >> 32) | (x << 32);
            x = ((x & 0xFFFF0000FFFF0000) >> 16) | ((x & 0x0000FFFF0000FFFF) << 16);
            return ((x & 0xFF00FF00FF00FF00) >> 8) | ((x & 0x00FF00FF00FF00FF) << 8);
        }

        public float SwapBytes(float input)
        {
            byte[] tmpIn = BitConverter.GetBytes(input);
            byte[] tmpOut = new byte[4];
            tmpOut[0] = tmpIn[3];
            tmpOut[1] = tmpIn[2];
            tmpOut[2] = tmpIn[1];
            tmpOut[3] = tmpIn[0];
            return BitConverter.ToSingle(tmpOut, 0);
        }

        public double SwapBytes(double input)
        {
            byte[] tmpIn = BitConverter.GetBytes(input);
            byte[] tmpOut = new byte[8];
            tmpOut[0] = tmpIn[7];
            tmpOut[1] = tmpIn[6];
            tmpOut[2] = tmpIn[5];
            tmpOut[3] = tmpIn[4];
            tmpOut[4] = tmpIn[3];
            tmpOut[5] = tmpIn[2];
            tmpOut[6] = tmpIn[1];
            tmpOut[7] = tmpIn[0];
            return BitConverter.ToSingle(tmpOut, 0);
        }

        public short SwapBytes(short value)
        {
            return (short) SwapBytes((ushort) value);
        }

        public int SwapBytes(int value)
        {
            return (int) SwapBytes((uint) value);
        }

        public long SwapBytes(long value)
        {
            return (long) SwapBytes((ulong) value);
        }

        public T GetSwap<T>(int offset, Func<int, T> getFunction, Func<T, T> swapFunction, Endianness endianness)
        {
            T value = getFunction(offset);
            if (SwapNeeded(Endianness, endianness))
            {
                value = swapFunction(value);
            }

            return value;
        }

        public T ReadSwap<T>(Func<T> readFunction, Func<T, T> swapFunction, Endianness endianness)
        {
            T value = readFunction();
            if (SwapNeeded(Endianness, endianness))
            {
                value = swapFunction(value);
            }

            return value;
        }

        public void WriteSwap<T>(T value, Action<T> writeFunction, Func<T, T> swapFunction, Endianness endianness)
        {
            if (SwapNeeded(Endianness, endianness))
            {
                value = swapFunction(value);
            }

            writeFunction(value);
        }

        public T GetSwap<T>(int offset, Func<int, T> getFunction, Func<T, T> swapFunction)
        {
            return GetSwap(offset, getFunction, swapFunction, Endianness);
        }

        public T ReadSwap<T>(Func<T> readFunction, Func<T, T> swapFunction)
        {
            return ReadSwap(readFunction, swapFunction, Endianness);
        }

        public void WriteSwap<T>(T value, Action<T> writeFunction, Func<T, T> swapFunction)
        {
            WriteSwap(value, writeFunction, swapFunction, Endianness);
        }
    }
}