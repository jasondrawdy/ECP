/*
==============================================================================
Copyright © Jason Drawdy (CloneMerge)

All rights reserved.

The MIT License (MIT)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

Except as contained in this notice, the name of the above copyright holder
shall not be used in advertising or otherwise to promote the sale, use or
other dealings in this Software without prior written authorization.
==============================================================================
*/

#region Imports

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using SecurityDriven.Inferno;

#endregion
namespace ECP
{
    public static class ECPExtensions
    {
        #region Methods

        //public static byte[] ToPacket(this string command, PacketType type = PacketType.Message)
        //{
        //    try
        //    {
        //        List<byte[]> parts = new List<byte[]>();
        //        byte[] length = command.Length.ToBytes();
        //        parts.Add(length);
        //        //byte[] header = Convert.ToInt32(type).ToBytes();
        //        //parts.Add(header);
        //        byte[] contents = command.ToBytes();
        //        parts.Add(contents);
        //        return CombineArrays(parts);
        //    }
        //    catch { return null; }
        //}

        /// <summary>
        /// Parses a command from their containing identifiers.
        /// </summary>
        /// <param name="input">The command to parse.</param>
        internal static string TrimCommand(this string input)
        {
            try { return input.Split('}')[1]; }
            catch { return null; }// All commands end with a curly bracket.
        }

        /// <summary>
        /// Encodes a byte array into its string representation.
        /// </summary>
        /// <param name="data">The array to be encoded.</param>
        internal static string GetString(this byte[] data)
        {
            try { return Encoding.ASCII.GetString(data); }
            catch { return null; }
        }
        
        /// <summary>
        /// Converts a string into an array of bytes.
        /// </summary>
        /// <param name="input">The string to encode.</param>
        internal static byte[] ToBytes(this string input)
        {
            try { return Encoding.ASCII.GetBytes(input); }
            catch { return null; }
        }

        //public static byte[] GetBytes(this PacketType header)
        //{
        //    try { return BitConverter.GetBytes((int)header); }
        //    catch { return null; }
        //}

        /// <summary>
        /// Converts an integer into a byte array.
        /// </summary>
        /// <param name="value">The integer to convert.</param>
        internal static byte[] ToBytes(this int value)
        {
            try { return BitConverter.GetBytes(value); }
            catch { return null; }
        }

        /// <summary>
        /// Merges two byte arrays into a single array.
        /// </summary>
        /// <param name="first">The first array to be merged.</param>
        /// <param name="second">The second array to be merged.</param>
        internal static byte[] Concatenate(this byte[] first, byte[] second)
        {
            try
            {
                int index = first.Length;
                Array.Resize(ref first, index + second.Length);
                Array.Copy(second, 0, first, index, second.Length);
                return null;
            }
            catch { return null; }
        }

        //internal static byte GetByte(this byte[] array, int index)
        //{
        //    try
        //    {
        //        byte header = array[index];

        //        List<byte> bytes = array.ToList();
        //        bytes.RemoveAt(index);
        //        array = bytes.ToArray();

        //        return header;
        //    }
        //    catch { return 0; }
        //}

        /// <summary>
        /// Removes trailing zeroes from a byte array.
        /// </summary>
        /// <param name="data">The aray to trim.</param>
        internal static byte[] TrimArray(this byte[] data)
        {
            try
            {
                int i = data.Length - 1;
                while (data[i] == 0)
                {
                    if ((i - 1) != 0) { i--; }
                }
                byte[] trimmed = new byte[i + 1];
                Array.Copy(data, trimmed, trimmed.Length);
                return trimmed;
            }
            catch { return null; }
        }

        /// <summary>
        /// Combines a list of arrays into a single byte array.
        /// </summary>
        /// <param name="data">The collection of arrays to combine.</param>
        internal static byte[] CombineArrays(this List<byte[]> data)
        {
            try
            {
                long size = 0;
                int offset = 0;
                foreach (byte[] x in data) { size += x.Length; }
                byte[] combined = new byte[size];
                for (int i = 0; i < data.Count; i++)
                {
                    Buffer.BlockCopy(data[i], 0, combined, offset, data[i].Length);
                    offset += data[i].Length;
                }
                return combined;
            }
            catch { return null; }
        }
        
        /// <summary>
        /// Selects a sequence of bytes within a byte array and returns it.
        /// </summary>
        /// <param name="bytes">The original array to select bytes from.</param>
        /// <param name="index">The location of the array to start selecting bytes.</param>
        /// <param name="length">The amount of bytes to select in the array.</param>
        /// <returns></returns>
        internal static byte[] PickBytes(this byte[] bytes, int index, int length)
        {
            try
            {
                List<byte> data = bytes.ToList();
                List<byte> header = new List<byte>();
                for (int i = index; i < length; i++)
                {
                    header.Add(bytes[i]);
                    data.RemoveAt(i);
                }
                return header.ToArray();
            }
            catch { return null; }
        }

        /// <summary>
        /// Transforms a byte array into its cryptographically strong equivalent using AES.
        /// </summary>
        /// <param name="data">The data to transform.</param>
        /// <param name="key">The password to use during the transformation.</param>
        internal static byte[] Encrypt(this byte[] data, string key)
        {
            // Create a new random object and memory stream.
            CryptoRandom random = new CryptoRandom();
            MemoryStream ciphertext = new MemoryStream();

            // Transform our plaintext into ciphertext.
            using (MemoryStream plaintext = new MemoryStream(data))
            using (EtM_EncryptTransform transform = new EtM_EncryptTransform(key: key.ToBytes()))
            using (CryptoStream crypto = new CryptoStream(ciphertext, transform, CryptoStreamMode.Write))
            {
                plaintext.CopyTo(crypto);
            }

            // Return our encrypted text.
            return ciphertext.ToArray();
        }

        /// <summary>
        /// Transforms a cryptographically strong sequence of bytes into its plainbyte equivalent using AES.
        /// </summary>
        /// <param name="data">The data to transform.</param>
        /// <param name="key">The password to use during the transformation.</param>
        internal static byte[] Decrypt(this byte[] data, string key)
        {
            // Create a new memory stream.
            MemoryStream plaintext = new MemoryStream();

            // Transform our ciphertext into plaintext.
            using (MemoryStream ciphertext = new MemoryStream(data))
            using (EtM_DecryptTransform transform = new EtM_DecryptTransform(key: key.ToBytes()))
            {
                using (CryptoStream crypto = new CryptoStream(ciphertext, transform, CryptoStreamMode.Read))
                    crypto.CopyTo(plaintext);
                if (!transform.IsComplete) throw new Exception("The data could not be decrypted.");
            }

            // Return our decrypted text.
            return plaintext.ToArray();
        }

        #endregion
    }
}
