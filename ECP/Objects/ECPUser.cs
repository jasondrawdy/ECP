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

using System.Net.Sockets;

#endregion
namespace ECP
{
    #region Enums

    /// <summary>
    /// The authority level of a connected user.
    /// </summary>
    public enum UserType { Guest, Client, Admin }

    #endregion
    public class ECPUser
    {
        #region Variables

        /// <summary>
        /// The username of the connected client.
        /// </summary>
        public string Username { get; private set; } = ECPStringGenerator.GenerateString(25);
        /// <summary>
        /// The ID of the connected client.
        /// </summary>
        public string UserID { get; private set; } = ECPStringGenerator.GenerateString(10);
        /// <summary>
        /// The authentication level of the connected client.
        /// </summary>
        public UserType UserRole { get; private set; } = UserType.Guest;
        /// <summary>
        /// The underlying network stream for the connected client.
        /// </summary>
        public TcpClient Client { get; private set; }

        #endregion
        #region Initialization

        /// <summary>
        /// Provides an interface for managing connected clients to an <see cref="ECPServer".
        /// </summary>
        /// <param name="client">The underlying network stream of the connected client.</param>
        public ECPUser(TcpClient client)
        {
            Client = client;
        }

        /// <summary>
        /// Provides an interface for managing connected clients to an <see cref="ECPServer".
        /// </summary>
        /// <param name="username">The username of the connected client.</param>
        /// <param name="id">The ID of the connected client.</param>
        /// <param name="role">The authentication level of the connected client.</param>
        /// <param name="client">The underlying network stream of the connected client.</param>
        public ECPUser(string username, string id, UserType role, TcpClient client)
        {
            Username = username;
            UserID = id;
            UserRole = role;
            Client = client;
        }

        /// <summary>
        /// Changes the username of the connected client.
        /// </summary>
        /// <param name="name"></param>
        public void SetUsername(string name) { Username = name; }
        /// <summary>
        /// Changes the ID of the connected client.
        /// </summary>
        /// <param name="id"></param>
        public void SetUserID(string id) { UserID = id; }
        /// <summary>
        /// Changes the authentication level of the connected client.
        /// </summary>
        /// <param name="role"></param>
        public void SetUserRole(UserType role) { UserRole = role; }

        #endregion
    }
}
