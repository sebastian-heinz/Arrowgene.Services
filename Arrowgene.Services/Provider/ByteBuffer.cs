namespace ArrowgeneServices.Provider
{
    using System.IO;

    public class ByteBuffer
    {
        private MemoryStream memoryStream;
        private BinaryWriter binaryWriter;
        private BinaryReader binaryReader;

        public ByteBuffer()
        {
            this.memoryStream = new MemoryStream();
            this.binaryReader = new BinaryReader(this.memoryStream);
            this.binaryWriter = new BinaryWriter(this.memoryStream);
        }

        public ByteBuffer(byte[] buffer)
            : this()
        {
            this.binaryWriter.Write(buffer);
        }

        public ByteBuffer(string filePath)
            : this()
        {
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                int read = 0;
                while ((read = fileStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    this.binaryWriter.Write(buffer, 0, read);
                }
            }
        }

        public long Position { get { return this.memoryStream.Position; } set { this.memoryStream.Position = value; } }

        public byte[] GetBytesTillPosition()
        {
            int length = (int)this.Position;
            this.Position = 0;
            return this.ReadBytes(length);
        }


        #region read
        public byte ReadByte()
        {
            return this.binaryReader.ReadByte();
        }

        public byte[] ReadBytes(int length)
        {
            return this.binaryReader.ReadBytes(length);
        }

        public int ReadInt16()
        {
            return this.binaryReader.ReadInt16();
        }

        public int ReadInt32()
        {
            return this.binaryReader.ReadInt32();
        }

        public float ReadFloat()
        {
            return this.binaryReader.ReadSingle();
        }

        public string ReadZeroString()
        {
            string s = string.Empty;
            bool read = true;
            while (this.memoryStream.Position < this.memoryStream.Length && read)
            {
                byte b = this.binaryReader.ReadByte();
                if (b > 0)
                {
                    s = s + ((char)b);
                }
                else
                {
                    read = false;
                }
            }
            return s;
        }

        public string ReadString(int length)
        {
            string s = string.Empty;

            for (int i = 0; i < length; i++)
            {
                if (this.memoryStream.Position < this.memoryStream.Length)
                {
                    byte b = this.binaryReader.ReadByte();
                    if (b > 0)
                    {
                        s = s + ((char)b);
                    }
                }
            }
            return s;
        }
        #endregion read

        #region write
        public void WriteByte(byte b)
        {
            this.binaryWriter.Write(b);
        }

        public void WriteByte(int i)
        {
            this.binaryWriter.Write((byte)i);
        }

        public void WriteBytes(byte[] b)
        {
            this.binaryWriter.Write(b);
        }

        public void WriteBytes(byte[] b, int index, int count)
        {
            this.binaryWriter.Write(b, index, count);
        }

        public void WriteInt16(short i)
        {
            this.binaryWriter.Write(i);
        }

        public void WriteInt16(int i)
        {
            this.binaryWriter.Write((short)i);
        }

        public void WriteInt32(int i)
        {
            this.binaryWriter.Write(i);
        }

        public void WriteString(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                this.binaryWriter.Write((int)s[i]);
            }
        }

        public void WriteFixedString(string s, int length)
        {
            int sLength = s.Length;
            int diff = length - sLength;
            if (diff >= 0)
            {
                for (int i = 0; i < sLength; i++)
                {
                    this.binaryWriter.Write((int)s[i]);
                }
                if (diff > 0)
                {
                    this.binaryWriter.Write(new byte[diff]);
                }
            }
        }

        #endregion write

    }
}