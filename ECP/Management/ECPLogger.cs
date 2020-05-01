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

using System;
using System.Collections.Generic;

#endregion
namespace ECP
{
    #region Enums

    /// <summary>
    /// Formatting styles to be applied to log entries.
    /// </summary>
    public enum EntryType { General, Notice, Success, Warning, Error }

    #endregion
    internal static class ECPLogger
    {
        #region Variables

        private static List<string> Log = new List<string>();

        #endregion
        #region Methods

        /// <summary>
        /// Creates a stylized log entry and stores it into the entry collection.
        /// </summary>
        /// <param name="message">The entry text to be made.</param>
        /// <param name="type">The style of the entry being made.</param>
        internal static void AddEntry(string message, EntryType type)
        {
            // Check what type of entry we're adding and create it.
            string entry = null;
            switch (type)
            {
                case EntryType.General:
                    entry += Timestamp() + "[-] " + message;
                    break;
                case EntryType.Notice:
                    entry += Timestamp() + "[*] " + message;
                    break;
                case EntryType.Success:
                    entry += Timestamp() + "[+] " + message;
                    break;
                case EntryType.Warning:
                    entry += Timestamp() + "[!] " + message;
                    break;
                case EntryType.Error:
                    entry += Timestamp() + "[x] " + message;
                    break;
            }
            
            // Add our entry if it's not null, otherwise, add our received message.
            if (entry != null)
                Log.Add(entry);
            else
            {
                if (message != null)
                    Log.Add(entry);
            }
        }

        private static string Timestamp()
        {
            return ("(" + DateTime.Now.ToLongTimeString() + " - " + DateTime.Now.ToShortDateString() + ") ");
        }

        #endregion
    }
}
