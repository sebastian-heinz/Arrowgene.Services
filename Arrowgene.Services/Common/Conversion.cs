namespace ArrowgeneServices.Common
{
    using System;
  

    /// <summary>
    /// Converts
    /// </summary>
    public static class Conversion
    {

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public static string GetString(byte[] bytes, int srcOffset, int length)
        {
            char[] chars = new char[length / sizeof(char)];
            Buffer.BlockCopy(bytes, srcOffset, chars, 0, length);
            return new string(chars);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public static string GetString(byte[] bytes)
        {
            return GetString(bytes, 0, bytes.Length);
        }

    }
}
