/*
==============================================================================
Copyright © Jason Drawdy 

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

using System.Threading;
using System.Collections.Generic;

#endregion
namespace ECP
{
    /// <summary>
    /// Allows management of threads through instancing and collections.
    /// </summary>
    internal class ECPThreadManager
    {
        #region Variables

        private static ECPThreadManager instance;
        /// <summary>
        /// Collection of multistate threads.
        /// </summary>
        internal List<ECPThread> threads = new List<ECPThread>();

        #endregion
        #region Initialization

        /// <summary>
        /// Allows management of threads through instancing and collections.
        /// </summary>
        private ECPThreadManager() { } // Nada por ahora.

        #endregion
        #region Methods

        /// <summary>
        /// Returns a new <see cref="ECPThreadManager"/> instance.
        /// </summary>
        public static ECPThreadManager Instance { get { return instance ?? (instance = new ECPThreadManager()); } }

        /// <summary>
        /// Starts a new thread and adds it to the current collection.
        /// </summary>
        /// <param name="thread">The thread to start.</param>
        internal void StartThread(Thread thread)
        {
            string name = thread.Name;
            string id = ECPStringGenerator.GenerateString(8);
            ECPThread t = new ECPThread(name, id, thread);
            threads.Add(t);
            t.Method.Start();
        }

        /// <summary>
        /// Stops a specified thread abruptly.
        /// </summary>
        /// <param name="thread">The thread to stop.</param>
        internal void StopThread(ECPThread thread)
        {
            threads[threads.IndexOf(thread)].Method.Abort();
        }

        #endregion
    }
}
