using System;

namespace Arrowgene.Services.Buffers
{
    public interface IEndiannessSwapper
    {
        Endianness Endianness { get; set; }
        bool SwapNeeded(Endianness currentEndianness, Endianness targetEndianness);
        ushort SwapBytes(ushort x);
        uint SwapBytes(uint x);
        ulong SwapBytes(ulong x);
        float SwapBytes(float input);
        double SwapBytes(double input);
        short SwapBytes(short value);
        int SwapBytes(int value);
        long SwapBytes(long value);
        T GetSwap<T>(int offset, Func<int, T> getFunction, Func<T, T> swapFunction, Endianness endianness);
        T ReadSwap<T>(Func<T> readFunction, Func<T, T> swapFunction, Endianness endianness);
        void WriteSwap<T>(T value, Action<T> writeFunction, Func<T, T> swapFunction, Endianness endianness);
        T GetSwap<T>(int offset, Func<int, T> getFunction, Func<T, T> swapFunction);
        T ReadSwap<T>(Func<T> readFunction, Func<T, T> swapFunction);
        void WriteSwap<T>(T value, Action<T> writeFunction, Func<T, T> swapFunction);
    }
}