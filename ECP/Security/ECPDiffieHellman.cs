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
using System.Text;

#endregion
namespace ECP
{
    /// <summary>
    /// Represents the Diffie-Hellman algorithm.
    /// </summary>
    public class ECPDiffieHellman : IDisposable
    {
        #region Util

        static ECPRandomNumberGenerator _strongRng = new ECPRandomNumberGenerator();

        #endregion
        #region Fields

        /// <summary>
        /// The number of bits to generate.
        /// </summary>
        private int bits = 256;

        /// <summary>
        /// The shared prime.
        /// </summary>
        BigInteger prime;
        /// <summary>
        /// The shared base.
        /// </summary>
        BigInteger g;
        /// <summary>
        /// The private prime.
        /// </summary>
        BigInteger mine;

        /// <summary>
        /// The final key.
        /// </summary>
        byte[] key;
        /// <summary>
        /// The string representation/packet.
        /// </summary>
        string representation;

        #endregion
        #region Properties

        /// <summary>
        /// Gets the final key to use for encryption.
        /// </summary>
        public byte[] Key
        {
            get { return key; }
        }

        #endregion
        #region Ctor

        public ECPDiffieHellman() { }
        public ECPDiffieHellman(int bits)
        {
            this.bits = bits;
        }

        ~ECPDiffieHellman()
        {
            Dispose();
        }

        #endregion
        #region Implementation Methods

        #region Flow

        /// <summary>
        /// Generates a request packet.
        /// </summary>
        /// <returns></returns>
        public ECPDiffieHellman GenerateRequest()
        {
            // Generate the parameters.
            prime = BigInteger.GenPseudoPrime(bits, 30, _strongRng);
            mine = BigInteger.GenPseudoPrime(bits, 30, _strongRng);
            g = (BigInteger)5;

            // Gemerate the string.
            StringBuilder rep = new StringBuilder();
            rep.Append(prime.ToString(36));
            rep.Append("|");
            rep.Append(g.ToString(36));
            rep.Append("|");

            // Generate the send BigInt.
            using (BigInteger send = g.ModPow(mine, prime))
            {
                rep.Append(send.ToString(36));
            }

            representation = rep.ToString();
            return this;
        }

        /// <summary>
        /// Generate a response packet.
        /// </summary>
        /// <param name="request">The string representation of the request.</param>
        /// <returns></returns>
        public ECPDiffieHellman GenerateResponse(string request)
        {
            string[] parts = request.Split('|');

            // Generate the would-be fields.
            using (BigInteger prime = new BigInteger(parts[0], 36))
            using (BigInteger g = new BigInteger(parts[1], 36))
            using (BigInteger mine = BigInteger.GenPseudoPrime(bits, 30, _strongRng))
            {
                // Generate the key.
                using (BigInteger given = new BigInteger(parts[2], 36))
                using (BigInteger key = given.ModPow(mine, prime))
                {
                    this.key = key.GetBytes();
                }
                // Generate the response.
                using (BigInteger send = g.ModPow(mine, prime))
                {
                    this.representation = send.ToString(36);
                }
            }

            return this;
        }

        /// <summary>
        /// Generates the key after a response is received.
        /// </summary>
        /// <param name="response">The string representation of the response.</param>
        public void HandleResponse(string response)
        {
            // Get the response and modpow it with the stored prime.
            using (BigInteger given = new BigInteger(response, 36))
            using (BigInteger key = given.ModPow(mine, prime))
            {
                this.key = key.GetBytes();
            }
            Dispose();
        }

        #endregion

        public override string ToString()
        {
            return representation;
        }

        #endregion
        #region IDisposable Members

        /// <summary>
        /// Ends the calculation. The key will still be available.
        /// </summary>
        public void Dispose()
        {
            if (!Object.ReferenceEquals(prime, null))
                prime.Dispose();
            if (!Object.ReferenceEquals(mine, null))
                mine.Dispose();
            if (!Object.ReferenceEquals(g, null))
                g.Dispose();

            prime = null;
            mine = null;
            g = null;

            representation = null;
            GC.Collect();
            GC.Collect();
        }

        #endregion
    }
}
