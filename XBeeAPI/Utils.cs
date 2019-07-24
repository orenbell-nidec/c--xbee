using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XBeeAPI
{
    public static class Utils
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Returns true if the nth bit of number is on
        /// </summary>
        /// <param name="number">The number in question</param>
        /// <param name="position">The "nth" in "nth bit". 0 = LSB</param>
        /// <returns>True if bit is on, false if not</returns>
        public static bool IsBitEnabled(int number, int position)
        {
            return ((number >> position) & 0x01) == 0x01;
        }

        /// <summary>
        /// Converts a string with hexadecimal alphanumerics into a byte array
        /// </summary>
        /// <param name="hex">The string (eg 48F73A01)</param>
        /// <returns>Byte array</returns>
        /// <exception cref="NullReferenceException">Thrown if argument is null</exception>
        public static byte[] HexStringToBytes(string hex)
        {
            // Remove white space
            hex = hex.Replace(" ", "");

            // Check the string exists and has an acceptable number of digits
            if (hex.Length % 2 != 0 || hex.Length == 0) throw new ArgumentException("Invalid hex string");

            // Split into couplets and convert each couplet to it's numeric byte value
            int i = 0;
            byte[] bytes = new byte[hex.Length / 2];
            foreach (string chunk in hex.SplitInParts(2))
            {
                bytes[i++] = Convert.ToByte(chunk, 16);
            }

            return bytes;
        }

        /// <summary>
        /// Returns big-endian byte array of a 64bit int
        /// </summary>
        /// <param name="num">Integer to be converted</param>
        /// <returns>Big-endian byte array</returns>
        public static byte[] IntToBytes(long num)
        {
            // Bytes array returned is little endian, so be sure to reverse it
            return BitConverter.GetBytes(num).Reverse().ToArray();
        }
        /// <summary>
        /// Returns big-endian byte array of a 64bit int
        /// </summary>
        /// <param name="num">Integer to be converted</param>
        /// <returns>Big-endian byte array</returns>
        public static byte[] IntToBytes(ulong num)
        {
            // Bytes array returned is little endian, so be sure to reverse it
            return BitConverter.GetBytes(num).Reverse().ToArray();
        }
        /// <summary>
        /// Returns big-endian byte array of a 32bit int
        /// </summary>
        /// <param name="num">Integer to be converted</param>
        /// <returns>Big-endian byte array</returns>
        public static byte[] IntToBytes(int num)
        {
            // Bytes array returned is little endian, so be sure to reverse it
            return BitConverter.GetBytes(num).Reverse().ToArray();
        }
        /// <summary>
        /// Returns big-endian byte array of a 32bit int
        /// </summary>
        /// <param name="num">Integer to be converted</param>
        /// <returns>Big-endian byte array</returns>
        public static byte[] IntToBytes(uint num)
        {
            // Bytes array returned is little endian, so be sure to reverse it
            return BitConverter.GetBytes(num).Reverse().ToArray();
        }
        /// <summary>
        /// Returns big-endian byte array of a 16bit int
        /// </summary>
        /// <param name="num">Integer to be converted</param>
        /// <returns>Big-endian byte array</returns>
        public static byte[] IntToBytes(short num)
        {
            // Bytes array returned is little endian, so be sure to reverse it
            return BitConverter.GetBytes(num).Reverse().ToArray();
        }
        /// <summary>
        /// Returns big-endian byte array of a 16bit int
        /// </summary>
        /// <param name="num">Integer to be converted</param>
        /// <returns>Big-endian byte array</returns>
        public static byte[] IntToBytes(ushort num)
        {
            // Bytes array returned is little endian, so be sure to reverse it
            return BitConverter.GetBytes(num).Reverse().ToArray();
        }

        /// <summary>
        /// Returns big-endian byte array of 16bit int, used for computing length bytes in XBee frame
        /// </summary>
        /// <param name="num">16 bit value</param>
        /// <returns>2 byte representation</returns>
        public static byte[] IntToLength(short num)
        {
            return BitConverter.GetBytes(num).Reverse().ToArray();
        }

        /// <summary>
        /// Takes a big-endian byte array and converts it to a 64bit integer
        /// </summary>
        /// <param name="bytes">Byte array to convert, must not exceed 8 bytes</param>
        /// <returns>Integer representation</returns>
        /// <exception cref="NullReferenceException">Thrown if argument is null</exception>
        public static long BytesToInt(byte[] bytes)
        {
            if (bytes.Length > 8) throw new ArgumentException("Too many bytes to convert to integer");

            byte[] zeros = new byte[8 - bytes.Length];
            Array.Clear(zeros, 0, zeros.Length);

            return BitConverter.ToInt64(bytes.Reverse().Concat(zeros).ToArray(), 0);
        }

        /// <summary>
        /// Takes a big-endian array of 2 bytes and converts it to an int.
        /// </summary>
        /// <param name="bytes">Bytes to convert</param>
        /// <returns>Integer equivalent</returns>
        /// <exception cref="NullReferenceException">Thrown if argument is null</exception>
        public static int LengthToInt(byte[] bytes)
        {
            if (bytes.Length != 2) throw new ArgumentException("Invalid number of bytes");

            return BitConverter.ToInt16(bytes.Reverse().ToArray(), 0);
        }

        /// <summary>
        /// Finds a string's byte array representation, and then converts that to a 64 bit integer
        /// </summary>
        /// <param name="ascii">String to convert, but be fewer than 8 bytes</param>
        /// <returns>Integer representation</returns>
        /// <exception cref="NullReferenceException">Thrown if argument is null</exception>
        public static long AsciiToInt(string ascii)
        {
            return BytesToInt(AsciiToBytes(ascii));
        }

        /// <summary>
        /// Converts a string to it's byte array representation
        /// </summary>
        /// <param name="ascii">String to convert</param>
        /// <returns>Byte-wise representation</returns>
        /// <exception cref="NullReferenceException">Thrown if argument is null</exception>
        public static byte[] AsciiToBytes(string ascii)
        {
            return Encoding.GetEncoding("UTF-8").GetBytes(ascii.ToCharArray());
        }

        /// <summary>
        /// Converts a byte array to it's ASCII representation
        /// </summary>
        /// <param name="bytes">String to convert</param>
        /// <returns>Byte-wise representation</returns>
        /// <exception cref="NullReferenceException">Thrown if argument is null</exception>
        public static string BytesToAscii(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// Splits an integer into it's respective bytes and interprets that as a string
        /// </summary>
        /// <param name="num">64bit integer input</param>
        /// <returns>String, no more than 8 bytes</returns>
        public static string IntToAscii(long num)
        {
            return Encoding.GetEncoding("UTF-8").GetString(BitConverter.GetBytes(num));
        }

        /// <summary>
        /// Converts a byte array to it's hexadecimal representation, and returns that as a string
        /// </summary>
        /// <param name="bytes">Byte array input</param>
        /// <returns>Hexadecimal string</returns>
        /// <exception cref="NullReferenceException">Thrown if argument is null</exception>
        public static string BytesToHexString(byte[] bytes, bool insertSpace=false)
        {
            char[] hexString = new char[bytes.Length * 2];
            string[] snippets = new string[bytes.Length];

            int i = 0;
            foreach (byte b in bytes)
            {
                snippets[i++] = b.ToString("X2");
            }

            if (insertSpace)
                return snippets.Aggregate((x, y) => x + ' ' + y).ToUpper();
            else
                return snippets.Aggregate((x, y) => x + y).ToUpper();
        }

        /// <summary>
        /// Returns the current time in seconds since the Epoch (Jan 1, 1970)
        /// </summary>
        /// <returns></returns>
        public static double TimeNow()
        {
            return (DateTime.Now - epoch).TotalSeconds;
        }
    }

    static class StringExtensions
    {

        public static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

    }
    
    /// <summary>
    /// This attribute is used to represent the description of frame types
    /// </summary>
    class DescriptionAttribute : Attribute
    {

        #region Properties

        /// <summary>
        /// Holds the stringvalue for a value in an enum.
        /// </summary>
        public string Description { get; protected set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor used to init a Description Attribute
        /// </summary>
        /// <param name="value"></param>
        public DescriptionAttribute(string value)
        {
            this.Description = value;
        }
        #endregion

    }
}
