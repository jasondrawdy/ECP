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

#endregion
namespace ECP
{
    #region Delegates

    /// <summary>
    /// Represents the method that will handle the <see cref="ECPServer.OnServerConnect"/> event of an <see cref="ECPServer"/> object.
    /// </summary>
    public delegate void ServerConnectHandler(object sender, ServerConnectEventArgs args);
    /// <summary>
    /// Represents the method that will handle the <see cref="ECPServer.OnServerDisconnect"/> event of an <see cref="ECPServer"/> object.
    /// </summary>
    public delegate void ServerDisconnectHandler(object sender, ServerDisconnectEventArgs args);
    /// <summary>
    /// Represents the method that will handle the <see cref="ECPServer.OnDataReceived"/> event of an <see cref="ECPServer"/> object.
    /// </summary>
    public delegate void ServerDataReceivedHandler(object sender, ServerDataReceivedEventArgs args);
    /// <summary>
    /// Represents the method that will handle the <see cref="ECPClient.OnClientConnect"/> event of an <see cref="ECPClient"/> object.
    /// </summary>
    public delegate void ClientConnectHandler(object sender, ClientConnectEventArgs args);
    /// <summary>
    /// Represents the method that will handle the <see cref="ECPClient.OnClientDisconnect"/> event of an <see cref="ECPClient"/> object.
    /// </summary>
    public delegate void ClientDisconnectHandler(object sender, ClientDisconnectEventArgs args);
    /// <summary>
    /// Represents the method that will handle the <see cref="ECPClient.OnDataReceived"/> event of an <see cref="ECPClient"/> object.
    /// </summary>
    public delegate void ClientDataReceivedHandler(object sender, ClientDataReceivedEventArgs args);
    /// <summary>
    /// Represets the method that will handle the <see cref="ECPServer.OnLogOutput"/> and <see cref="ECPClient.OnLogOutput"/> events of an <see cref="ECPServer"/> and <see cref="ECPClient"/> object.
    /// </summary>
    public delegate void LogOutputHandler(object sender, LogOutputEventArgs args);

    #endregion
    #region EventArgs

    /// <summary>
    /// Contains data for the <see cref="ECPServer.OnServerConnect"/> event.
    /// </summary>
    public class ServerConnectEventArgs : EventArgs
    {
        public ServerConnectEventArgs() { }
    }
    /// <summary>
    /// Contains data for the <see cref="ECPServer.OnServerDisconnect"/> event.
    /// </summary>
    public class ServerDisconnectEventArgs : EventArgs
    {
        public ServerDisconnectEventArgs() { }
    }
    /// <summary>
    /// Contains data for the <see cref="ECPServer.OnDataReceived"/> event.
    /// </summary>
    public class ServerDataReceivedEventArgs : EventArgs
    {
        public string User { get; private set; }
        public byte[] Data { get; private set; }
        public ServerDataReceivedEventArgs(string user, byte[] data)
        {
            User = user;
            Data = data;
        }
    }
    /// <summary>
    /// Contains data for the <see cref="ECPClient.OnClientConnect"/> event.
    /// </summary>
    public class ClientConnectEventArgs : EventArgs
    {
        public string Server { get; private set; }
        public ClientConnectEventArgs(string server)
        {
            Server = server;
        }
    }
    /// <summary>
    /// Contains data for the <see cref="ECPClient.OnClientDisconnect"/> event.
    /// </summary>
    public class ClientDisconnectEventArgs : EventArgs
    {
        public ClientDisconnectEventArgs()
        {
        }
    }
    /// <summary>
    /// Contains data for the <see cref="ECPClient.OnDataReceived"/> event.
    /// </summary>
    public class ClientDataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; private set; }
        public ClientDataReceivedEventArgs(byte[] data)
        {
            Data = data;
        }
    }
    /// <summary>
    /// Contains data for the <see cref="ECPServer.OnLogOutput"/> and <see cref="ECPClient.OnLogOutput"/> events.
    /// </summary>
    public class LogOutputEventArgs : EventArgs
    {
        public string Output { get; private set; }
        public EntryType Type { get; private set; }
        public LogOutputEventArgs(string output, EntryType type)
        {
            Output = output;
            Type = type;
        }
    }

    #endregion
}
