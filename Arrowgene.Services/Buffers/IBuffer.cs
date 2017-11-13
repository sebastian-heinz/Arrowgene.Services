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

namespace Arrowgene.Services.Common.Buffers
{
    public interface IBuffer
    {
        /// <summary>
        /// The current buffer size.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Gets or Sets the cursor position.
        /// </summary>
        int Position { get; set; }

        /// <summary>
        /// Gets or Sets the byte order.
        /// </summary>
        Endianness Endianness { get; set; }

        /// <summary>
        /// Set the cursor to the beginning of the buffer.
        /// </summary>
        void SetPositionStart();

        /// <summary>
        /// Set the cursor to the end of the buffer.
        /// </summary>
        void SetPositionEnd();

        /// <summary>
        /// Clone this buffer from an offset till the specified length.
        /// </summary>
        IBuffer Clone(int offset, int length);

        /// <summary>
        /// Clone this buffer from start till the specified length.
        /// </summary>
        IBuffer Clone(int length);

        /// <summary>
        /// Clone this buffer from start till the specified length.
        /// </summary>
        IBuffer Clone();

        /// <summary>
        /// Returns all written bytes without affecting the position.
        /// </summary>
        byte[] GetAllBytes();

        /// <summary>
        /// Returns all written bytes from a specific offset, without affecting the position.
        /// </summary>
        byte[] GetAllBytes(int offset);

        void WriteByte(byte value);

        void WriteByte(int value);

        void WriteByte(long value);

        void WriteBytes(byte[] bytes);

        /// <summary>
        /// Writes a part of an byte array to this buffer.
        /// </summary>
        /// <param name="bytes">Source</param>
        /// <param name="offset">Offset in source</param>
        /// <param name="length">Byte count to copy from source</param>
        void WriteBytes(byte[] bytes, int offset, int length);

        void WriteInt16(short value);

        void WriteInt16(int value);

        void WriteInt32(int value);

        void WriteFloat(float value);

        void WriteString(string value);

        void WriteFixedString(string value, int length);

        void WriteBuffer(IBuffer value);

        void WriteBuffer(IBuffer value, int offset, int length);

        /// <summary>
        /// Write a Nul-Terminated-String.
        /// Advances the cursor.
        /// </summary>
        void WriteCString(string value);

        /// <summary>
        /// Read byte.
        /// Advances the cursor.
        /// </summary>
        byte ReadByte();

        /// <summary>
        /// Get byte at specified offset.
        /// Doesn't advance the cursor.
        /// </summary>
        byte GetByte(int offset);

        /// <summary>
        /// Read bytes.
        /// Advances the cursor.
        /// </summary>
        byte[] ReadBytes(int length);

        /// <summary>
        /// Get bytes at specified offset.
        /// Doesn't advance the cursor.
        /// </summary>
        byte[] GetBytes(int offset, int length);

        /// <summary>
        /// Get Int16 at specified offset.
        /// Doesn't advance the cursor.
        /// </summary>
        short GetInt16(int offset);

        /// <summary>
        /// Read Int16.
        /// Advances the cursor.
        /// </summary>
        short ReadInt16();

        /// <summary>
        /// Get Int32 at specified offset.
        /// Doesn't advance the cursor.
        /// </summary>
        int GetInt32(int offset);

        /// <summary>
        /// Read Int32.
        /// Doesn't advance the cursor.
        /// </summary>
        int ReadInt32();

        /// <summary>
        /// Get Float at specified offset.
        /// Doesn't advance the cursor.
        /// </summary>
        float GetFloat(int offset);

        /// <summary>
        /// Read Float
        /// Doesn't advance the cursor.
        /// </summary>
        float ReadFloat();

        /// <summary>
        /// Get a String at specified offset with a specific length.
        /// Doesn't advance the cursor.
        /// </summary>
        string GetString(int offset, int length);

        /// <summary>
        /// Read a String with a specific length.
        /// Advances the cursor.
        /// </summary>
        string ReadString(int length);

        /// <summary>
        /// Read a Nul-Terminated-String.
        /// Advances the cursor.
        /// </summary>
        string ReadCString();

        /// <summary>
        /// Get a Nul-Terminated-String from a specified offset.
        /// Doesn't advance the cursor.
        /// </summary>
        string GetCString(int offset);

        /// <summary>
        /// Hex representation of the buffer.
        /// </summary>
        string ToHexString();

        /// <summary>
        /// Ascii representation of the buffer.
        /// </summary>
        string ToAsciiString(bool spaced);
    }
}