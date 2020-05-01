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
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections;

#endregion
namespace ECP
{
    /// <summary>
    /// Allows communcation with an <see cref="ECPClient"/> using the Encrypted Communications Protocol.
    /// </summary>
    public class ECPServer
    {
        #region Variables
        
        /// <summary>
        /// Collection of currently connected <see cref="ECPClient"/> objects.
        /// </summary>
        public static Hashtable Clients = new Hashtable();
        /// <summary>
        /// Occurs when an <see cref="ECPClient"/> has connected to the server.
        /// </summary>
        public event ServerConnectHandler OnServerConnect;
        /// <summary>
        /// Occurs when an <see cref="ECPClient"/> has disconnected from the server.
        /// </summary>
        public event ServerDisconnectHandler OnServerDisconnect;
        /// <summary>
        /// Occurs when the server has received data from an <see cref="ECPClient"/>.
        /// </summary>
        public event ServerDataReceivedHandler OnDataReceived;
        /// <summary>
        /// Occurs when a log entry has been created.
        /// </summary>
        public event LogOutputHandler OnLogOutput;

        #endregion
        #region Initialization

        /// <summary>
        /// Allows communcation with an <see cref="ECPClient"/> using the Encrypted Communications Protocol.
        /// </summary>
        public ECPServer() { }

        #endregion
        #region Methods

        #region Core

        /// <summary>
        /// Allows the server to begin listening for incoming connections and data on a specified port.
        /// </summary>
        /// <param name="port">The port number to listen on.</param>
        public void Start(int port)
        {
            // Create our TCP objects.
            TcpListener Listener = new TcpListener(IPAddress.Any, port);
            TcpClient Client = default(TcpClient);

            // Start the socket.
            Listener.Start();

            // Tell the user that our server has connected.
            ServerConnected();
            //LogOutput("ECPServer has been started...", EntryType.General);

            // Loop our socket to await data.
            while (true)
            {
                // Accept any incoming clients.
                Client = Listener.AcceptTcpClient();

                // Create some data buffers.
                byte[] buffer = new byte[1024];
                string data = null;

                // Create a stream to read data from.
                NetworkStream stream = Client.GetStream();
                stream.Read(buffer, 0, buffer.Length);

                // Get the clients info
                data = Encoding.ASCII.GetString(buffer);
                data = data.Substring(0, data.IndexOf("$ECP"));

                // Check if our connecting client's name is null or not.
                if (data == "" || data == " " || data == null)
                {
                    // Since we errored out, our client obviously already exists.
                    buffer = ("You must identify yourself to the server!").ToBytes();
                    Client.SendBufferSize = buffer.Length;
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();

                    // Write a shutdown command to the client.
                    buffer = ("{SHUTDOWN}").ToBytes();
                    Client.SendBufferSize = buffer.Length;
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();

                    // Close our stream and client socket. 
                    stream.Close();
                    Client.Close();
                    Client.Client.Close();
                }
                else
                {
                    // Check if the connecting client already exists.
                    try
                    {
                        // Add them to our hashtable
                        Clients.Add(data, Client);

                        // Broadcast that our client connected.
                        byte[] received = Encoding.ASCII.GetBytes(data);
                        LogOutput(data + " has connected.", EntryType.Notice);
                        //DataReceived(received);
                        //Broadcast(data + " Joined ", data, false);
                        //Console.WriteLine(Timestamp() + "[+] {0} has joined.", data);

                        // Handle any incoming data from the client.
                        ECPServerHandler handler = new ECPServerHandler();
                        handler.Handle(this, Client, data);
                    }
                    catch
                    {
                        // Since we errored out, our client obviously already exists.
                        buffer = ("Client already exists in the table.").ToBytes();
                        Client.SendBufferSize = buffer.Length;
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Flush();

                        // Write a shutdown command to the client.
                        buffer = ("{SHUTDOWN}").ToBytes();
                        Client.SendBufferSize = buffer.Length;
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Flush();

                        // Close our stream and client socket. 
                        stream.Close();
                        Client.Close();
                        Client.Client.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Sends a message to currently connected <see cref="ECPClient"/>.
        /// </summary>
        /// <param name="command">Data to send the client.</param>
        /// <param name="id">The ID of the client to send data to.</param>
        public void Broadcast(string command, string id)
        {
            TcpClient client = null;
            foreach(DictionaryEntry x in Clients)
            {
                if (id == x.Key.ToString())
                {
                    client = (TcpClient)x.Value;
                    break;
                }
            }

            if (client != null)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = command.ToBytes();
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
        }

        /// <summary>
        /// Sends a message from a client to all other clients.
        /// </summary>
        /// <param name="command">The message to broadcast to each client.</param>
        /// <param name="uName">The client id that is trying to broadcast.</param>
        /// <param name="flag">Broadcast telling each client who sent the message.</param>
        public void BroadcastAll(string command, string uName, bool flag)
        {
            // Send a message to all clients in our table.
            foreach (DictionaryEntry Item in Clients)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)Item.Value;
                NetworkStream broadcastStream = broadcastSocket.GetStream();
                Byte[] broadcastBytes = null;

                if (flag == true)
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(uName + " says: " + command);
                }
                else
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(command);
                }
                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        }

        #endregion
        #region Utils
        
        /// <summary>
        /// Removes an <see cref="ECPClient"/> from the current collection of connected clients.
        /// </summary>
        /// <param name="id">The ID of the <see cref="ECPClient"/> to remove.</param>
        internal void RemoveClient(string id)
        {
            try
            {
                if (Clients.ContainsKey(id))
                    Clients.Remove(id);
            }
            catch { LogOutput("The client could not be removed from the table.", EntryType.Error); }
        }

        private string Timestamp()
        {
            return ("(" + DateTime.Now.ToLongTimeString() + " - " + DateTime.Now.ToShortDateString() + ") ");
        }
        

        #endregion
        #region Events

        internal void ServerConnected()
        {
            if (OnServerConnect == null) return;
            ServerConnectEventArgs args = new ServerConnectEventArgs();
            OnServerConnect(this, args);
        }

        internal void ServerDisconnected()
        {
            if (OnServerDisconnect == null) return;
            ServerDisconnectEventArgs args = new ServerDisconnectEventArgs();
            OnServerDisconnect(this, args);
        }

        internal void DataReceived(string user, byte[] data)
        {
            if (OnDataReceived == null) return;
            ServerDataReceivedEventArgs args = new ServerDataReceivedEventArgs(user, data);
            OnDataReceived(this, args);
        }

        internal void LogOutput(string output, EntryType type, bool silent = false)
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

    internal class ECPServerHandler
    {
        #region Variables

        private string[] commands = { "{HANDSHAKE}", "{HREPLY}" };
        private ECPServer Server;
        private TcpClient Client;
        private string ClientID;
        private ECPDiffieHellman Exchange;
        private string Key = null;

        #endregion
        #region Methods

        #region Core

        /// <summary>
        /// Starts a new thread to handle client data.
        /// </summary>
        /// <param name="client">The client to await incoming data.</param>
        /// <param name="id">The name of the client.</param>
        internal void Handle(ECPServer server, TcpClient client, string id)
        {
            // Set our client and its id.
            Server = server;
            Client = client;
            ClientID = id;

            // Create a new thread to pull data.
            Thread t = new Thread(PullData);
            t.Start();
        }

        /// <summary>
        /// Reads incoming data from the client stream.
        /// </summary>
        internal void PullData()
        {
            // Create our buffers.
            byte[] buffer = new byte[1024];
            string data = null;

            // Start reading incoming data.
            while (true)
            {
                try
                {
                    // Create a new buffer and read the incoming bytes.
                    buffer = new byte[Client.ReceiveBufferSize];
                    NetworkStream networkStream = Client.GetStream();
                    networkStream.Read(buffer, 0, buffer.Length);
                    data = Encoding.ASCII.GetString(buffer.TrimArray());
                    data = data.Substring(0, data.IndexOf("$ECP"));

                    // Create an array with the processed data..
                    byte[] received = Encoding.ASCII.GetBytes(data);
                    ParseData(received);
                }
                catch (Exception ex)
                {
                    // Check if our client is still connected.
                    if (ClientConnected())
                        Server.LogOutput(ClientID + ": " + ex.ToString(), EntryType.Error);
                    else
                    {
                        // Log that our user has disconnected and break from our loop.
                        Server.LogOutput(ClientID + " has disconnected.", EntryType.Warning);
                        Server.RemoveClient(ClientID);
                        Client.Close();
                        break;
                    }
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
                    // Generate a new handshake request for the client.
                    if (command.Substring(0, commands[0].Length) == commands[0])
                    {
                        try
                        {
                            // Log that we've received a handshake request.
                            Server.LogOutput(ClientID + " has requested a handshake.", EntryType.Notice);

                            // Jit our diffie-hellman before sending the actual request packet.
                            ECPDiffieHellman Jitter = new ECPDiffieHellman(32).GenerateRequest();

                            // Create a new request packet.
                            Exchange = new ECPDiffieHellman(256).GenerateRequest();

                            // Send our packet to the client and log it.
                            string message = "{HANDSHAKE}" + Exchange.ToString();
                            Server.Broadcast(message, ClientID);
                            Server.LogOutput("A handshake has been sent to " + ClientID + ".", EntryType.Notice);
                        }
                        catch //catch(Exception ex)
                        {
                            //Server.LogOutput(ex.StackTrace, EntryType.Error);
                            Server.LogOutput("A handshake could not be sent to " + ClientID + ".", EntryType.Error);
                        }
                    }
                }
                else if (command.Contains(commands[1]))
                {
                    // Generate a new encryption key using our handshake response.
                    if (command.Substring(0, commands[1].Length) == commands[1])
                    {
                        try
                        {
                            // Parse our response to get our handshake reply.
                            string response = command.Replace(commands[1], null);

                            // Generate a new session key from our response.
                            Exchange.HandleResponse(response);
                            Key = Convert.ToBase64String(Exchange.Key);
                            Server.LogOutput("A new session key has been generated!", EntryType.Success);
                        }
                        catch { Server.LogOutput("The handshake response could not be processed.", EntryType.Error); }

                        if (Key != null)
                        {
                            string message = Convert.ToBase64String(("Hello World!").ToBytes().Encrypt(Key));
                            Server.Broadcast(message, ClientID);
                            Server.LogOutput(Key, EntryType.General);
                        }

                    }
                }
                else
                {
                    // Pass our data to our event.
                    Server.DataReceived(ClientID, x);
                }
            }
            catch { }
        }

        private bool ClientConnected()
        {
            try
            {
                Server.Broadcast("1", ClientID);
                return true;
            }
            catch { return false; }
        }

        private string Timestamp()
        {
            return ("(" + DateTime.Now.ToLongTimeString() + " - " + DateTime.Now.ToShortDateString() + ") ");
        }

        #endregion

        #endregion
    }
}
