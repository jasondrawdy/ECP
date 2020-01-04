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
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

#endregion
namespace ECP
{
    /// <summary>
    /// Allows communcation with an <see cref="ECPServer"/> using the Encrypted Communications Protocol.
    /// </summary>
    public class ECPClient
    {
        #region Variables

        private string[] commands = { "{HANDSHAKE}", "{HREPLY}", "{SHUTDOWN}" };
        private string Key = null;
        private bool Handshake = false;
        private TcpClient Client = new TcpClient();
        private NetworkStream Stream = default(NetworkStream);
        private ECPDiffieHellman Exchange = new ECPDiffieHellman();
        /// <summary>
        /// Occurs when a client has connected to an <see cref="ECPServer"/>.
        /// </summary>
        public event ClientConnectHandler OnClientConnect;
        /// <summary>
        /// Occurs when a client has disconnected from an <see cref="ECPServer"/>.
        /// </summary>
        public event ClientDisconnectHandler OnClientDisconnect;
        /// <summary>
        /// Occurs when a client has received data.
        /// </summary>
        public event ClientDataReceivedHandler OnDataReceived;
        /// <summary>
        /// Occurs when a log entry has been created.
        /// </summary>
        public event LogOutputHandler OnLogOutput;

        #endregion
        #region Initialization

        /// <summary>
        /// Allows communcation with an <see cref="ECPServer"/> using the Encrypted Communications Protocol.
        /// </summary>
        public ECPClient() { }

        #endregion
        #region Methods

        #region Core

        /// <summary>
        /// Connects to an <see cref="ECPServer"/> at a remote endpoint.
        /// </summary>
        /// <param name="ip">The address that an <see cref="ECPServer"/> is running.</param>
        /// <param name="user">The username to indentify with an <see cref="ECPServer"/> as.</param>
        public void Connect(string ip, string user)
        {
            string address = ip; //IPAddress.Loopback.ToString();
            int port = 80;
            Client.Connect(ip, port);
            Stream = Client.GetStream();
            ClientConnected(ip + ":" + port.ToString());

            byte[] buffer = Encoding.ASCII.GetBytes(user + "$ECP");
            Client.SendBufferSize = buffer.Length;
            Stream.Write(buffer, 0, buffer.Length);
            Stream.Flush();

            Thread t = new Thread(PullData);
            t.Start();
        }

        /// <summary>
        /// Sends a message to an <see cref="ECPServer"/>.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void Send(string message)
        {
            // Check if we're still waiting for a handshake.
            if (Handshake)
            {
                // If we're trying to send a handshake or reply then let it through.
                if (message.Contains(commands[0]) || message.Contains(commands[1]))
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(message + "$ECP");
                    Stream.Write(buffer, 0, buffer.Length);
                    Stream.Flush();
                }
                else
                    LogOutput("A session key could not be established.", EntryType.Error);
            }
            else
            {
                // Send an encrypted message if we have a key.
                if (Key != null)
                {
                    string ciphertext = Convert.ToBase64String(message.ToBytes().Encrypt(Key));
                    byte[] buffer = Encoding.ASCII.GetBytes(ciphertext + "$ECP");
                    Stream.Write(buffer, 0, buffer.Length);
                    Stream.Flush();
                }
                else
                    LogOutput("A session key has not been established.", EntryType.Error);
            }
        }

        private void PullData()
        {
            // Send a handshake request before entering our loop.
            try
            {
                Handshake = true;
                Send("{HANDSHAKE}");
            }
            catch { }

            // Start listening for incoming data.
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[Client.ReceiveBufferSize];
                    Stream = Client.GetStream();
                    Stream.Read(buffer, 0, buffer.Length);
                    ParseData(buffer.TrimArray()); // Trim the array of nulls before parsing further.
                }
                catch
                {
                    ClientDisconnected();
                    break;
                }
            }
        }

        #endregion
        #region Utils

        private void ParseData(byte[] data)
        {
            byte[] x = null;
            if (Key != null)
            {
                try
                {
                    byte[] buffer = Convert.FromBase64String(data.GetString());
                    x = buffer.Decrypt(Key);
                }
                catch { x = data; }
            }
            else
                x = data;

            // Parse any commands from our received data.
            string command = x.GetString();
            try
            {
                if (command.Contains(commands[0]))
                {
                    // Generate a new handshake request for the server.
                    if (command.Substring(0, commands[0].Length) == commands[0])
                    {
                        try
                        {
                            // Create a new response packet.
                            string response = command.Replace(commands[0], null);
                            Exchange = new ECPDiffieHellman(256).GenerateResponse(response);

                            // Generate a new session key from our response.
                            Key = Convert.ToBase64String(Exchange.Key);

                            // Send our reponse packet to the server and log it.
                            string message = "{HREPLY}" + Exchange.ToString();
                            Send(message);
                            Handshake = false;
                            LogOutput("A new session key has been generated!", EntryType.Success);
                        }
                        catch { LogOutput("A handshake could not be sent to the server.", EntryType.Error); }

                        if (Key != null)
                            LogOutput(Key, EntryType.General);
                    }
                }
                else if (command.Contains(commands[2]))
                {
                    if (command.Substring(0, commands[2].Length) == commands[2])
                    {
                        Stream.Close();
                        Client.Close();
                        ClientDisconnected();
                        LogOutput("The connection has been closed by the server.", EntryType.Notice, true);
                    }
                }
                else
                {
                    // Pass our data to our event.
                    DataReceived(x);
                }
            }
            catch { }
        }

        private byte[] CombineArrays(List<byte[]> data)
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

        #endregion
        #region Events

        private void ClientConnected(string server)
        {
            if (OnClientConnect == null) return;
            ClientConnectEventArgs args = new ClientConnectEventArgs(server);
            OnClientConnect(this, args);
        }

        private void ClientDisconnected()
        {
            if (OnClientDisconnect == null) return;
            ClientDisconnectEventArgs args = new ClientDisconnectEventArgs();
            OnClientDisconnect(this, args);
        }

        private void DataReceived(byte[] data)
        {
            if (OnDataReceived == null) return;
            ClientDataReceivedEventArgs args = new ClientDataReceivedEventArgs(data);
            OnDataReceived(this, args);
        }

        private void LogOutput(string output, EntryType type, bool silent = false)
        {
            // Add our output to the log and then return or update.
            ECPLogger.AddEntry(output, type);
            if (!silent)
            {
                if (OnLogOutput == null) return;
                LogOutputEventArgs args = new LogOutputEventArgs(output, type);
                OnLogOutput(this, args);
            }
        }

        #endregion

        #endregion
    }
}