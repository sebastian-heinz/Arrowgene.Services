namespace Arrowgene.Services.Common.Buffers
{
    using System;
    using System.Text;
    
    public abstract class Buffer : IBuffer, ICloneable
    {
        public abstract int Size { get; }
        public abstract int Position { get; set; }
        public abstract void SetPositionStart();
        public abstract void SetPositionEnd();
        public abstract IBuffer Clone(int offset, int length);
        public abstract byte[] GetAllBytes();
        public abstract byte[] GetAllBytes(int offset);
        public abstract void WriteByte(byte value);
        public abstract void WriteByte(int value);
        public abstract void WriteByte(long value);
        public abstract void WriteBytes(byte[] bytes);
        public abstract void WriteBytes(byte[] bytes, int offset, int length);
        public abstract void WriteInt16(short value);
        public abstract void WriteInt16(int value);
        public abstract void WriteInt32(int value);
        public abstract void WriteFloat(float value);
        public abstract void WriteString(string value);
        public abstract void WriteFixedString(string value, int length);
        public abstract void WriteCString(string value);
        public abstract byte ReadByte();
        public abstract byte GetByte(int offset);
        public abstract byte[] ReadBytes(int length);
        public abstract byte[] GetBytes(int offset, int length);
        public abstract short GetInt16(int offset);
        public abstract short ReadInt16();
        public abstract int GetInt32(int offset);
        public abstract int ReadInt32();
        public abstract float GetFloat(int offset);
        public abstract float ReadFloat();
        public abstract string GetString(int offset, int length);
        public abstract string ReadString(int length);
        public abstract string ReadCString();
        public abstract string GetCString(int offset);

        public virtual void WriteBuffer(IBuffer value, int offset, int length)
        {
            WriteBytes(value.GetBytes(offset, length));
        }
        
        public virtual void WriteBuffer(IBuffer value)
        {
            WriteBytes(value.GetAllBytes());
        }

        public virtual IBuffer Clone(int length)
        {
            return Clone(0, length);
        }

        public virtual IBuffer Clone()
        {
            return Clone(Size);
        }
        
        public virtual string ToHexString()
        {
            byte[] buffer = GetAllBytes();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                sb.Append(buffer[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public virtual string ToAsciiString(bool spaced)
        {
            byte[] buffer = GetAllBytes();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                char c = '.';
                if (buffer[i] >= 'A' && buffer[i] <= 'Z') c = (char) buffer[i];
                if (buffer[i] >= 'a' && buffer[i] <= 'z') c = (char) buffer[i];
                if (buffer[i] >= '0' && buffer[i] <= '9') c = (char) buffer[i];
                if (spaced && i != 0)
                {
                    sb.Append("  ");
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            return ToAsciiString(true) +
                   Environment.NewLine +
                   ToHexString();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}