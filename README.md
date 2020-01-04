<p align="center">
    <img width="256" height="256" src="https://user-images.githubusercontent.com/40871836/43414723-3fac85b2-93f9-11e8-9794-61e8a4281465.png">
<p>

# ECP
<p align="left">
    <!-- Version -->
    <img src="https://img.shields.io/badge/version-1.0.0-brightgreen.svg">
    <!-- Docs -->
    <img src="https://img.shields.io/badge/docs-not%20found-lightgrey.svg">
    <!-- License -->
    <img src="https://img.shields.io/badge/license-MIT-blue.svg">
</p>

A lightweight, flexible, and extensible network communications protocol created with security in mind and focuses on the productivity of both potential clients and servers. ECP is built on top of multiple layers of security and is meant to be a base for anyone looking to implement networking into their own applications; ECP comes bundled with AES in CBC mode for general data encryption, SHA256 for checksum generation and validation, and Diffie-Hellman is used as the main key exchange algorithm. Given that the library is a base for other networking applications, ECP comes with some of the basic features and tools that are normally included in likewise software such as logging and packet management.

### Requirements
- .NET Framework 4.6.1

# Features
- A completely event based and object oriented library
- Accept socket connections from the *Local Area Network*
- Accept socket connections from the *Wide Area Network* (Internet)
- Listen for data on a custom port number
- Connect to an `ECPServer` using a username
- Broadcast data to a specific user
- Broadcast data to all currently connected users
- All data is encrypted with AES256 in CBC mode with SHA2 hashing support
- Multiple hashing algorithms and encoding algorithms have been incorporated
    - #### Hashing
        - MD5
        - RIPEMD160
        - SHA1
        - SHA256
        - SHA384
        - SHA256
    - #### Encoding
        - Base64
        - UTF8
        - ASCII
        <br><br>
- Allows multiple instances of both `ECPClient` and `ECPServer`
- Multithreaded and thread handling with a multi-instanced thread manager
- Logging capabilities with timestamps and entry levels
- Simple packet management with parsing and encryption

# TODO
##### Updates
- [ ] Allows both IPv4 and IPv6 addresses
- [ ] Thread manager needs optimizing
- [ ] Most constructors and some methods need overloading with proper properties
- [ ] `ECPUser` objects should be generated on connection as well as their temporary names until user authentication

##### New Features
- [ ] File Transfers
- [ ] All methods now have async versions

##### Bugs
- [x] "*Broadcast All*" doesn't encrypt text with client keys
- [x] Keep-alive packets aren't sent encrypted even if the key isn't null
- [x] Shutdown commands aren't sent encrypted even if the key isn't null
- [x] Packets from both server and client do not have any structure to them; they only have termination chars
- [x] Sending a `{SHUTDOWN}` command from the client causes a loop if ECPClient.Disconnect() is called afterwards
- [ ] Client doesn't process text before initial shutdown command from server
- [ ] Closing the server before sending a `{SHUTDOWN}` command to the Client will cause the Client to infinitely loop
- [ ] Sending broken handshake requests like `xxx{HANDSHAKE}` breaks the tunnel requiring another handshake
- [ ] Sending broken handshake replies like `{HREPLY}` or `xxx{HREPLY}` breaks the tunnel requiring another handshake

# Examples
### Server
Creating a new server using the ECP library is fairly easy. In this example, we'll create a new `ECPServer` CLI application which will just greet the user and begin accepting connections on port 80 while listening for data from those sockets.
```c#
using System;
using System.Text;
using ECP; // Always remember to import our library.

namespace ECPServer
{
    class Program
    {
        /// <summary>
        /// The server that will be used for all incoming socket connections.
        /// </summary>
        static ECPServer server = new ECPServer();

        /// <summary>
        /// The main entry point for our application.
        /// </summary>
        static void Main(string[] args)
        {
            // Greet our user with any relevant information.
            PrintGreeting();

            // Create a new ECP server and set our event handlers.
            server.OnServerConnect += OnConnect;
            server.OnServerDisconnect += OnDisconnect;
            server.OnDataReceived += OnDataReceived;
            server.OnLogOutput += OnLogOutput;

            // Start listening for data on the provided port.
            server.Start(80); // Any general or unused port is valid.
        }

        /// <summary>
        /// Prints a generic user greeting to the console.
        /// </summary>
        static void PrintGreeting()
        {
            Console.WriteLine("============================================");
            Console.WriteLine("            Welcome to ECPServer            ");
            Console.WriteLine("          Written By: Jason Drawdy          ");
            Console.WriteLine("============================================");
        }

        /// <summary>
        /// Returns a string formatted with the current time of day and date.
        /// </summary>
        static string GetTimestamp()
        {
            return ("(" + DateTime.Now.ToLongTimeString() + " - " + DateTime.Now.ToShortDateString() + ") ");
        }

        /// <summary>
        /// Handles the socket connection of an incoming user.
        /// </summary>
        static void OnConnect(object sender, ServerConnectEventArgs args)
        {
            Console.WriteLine(GetTimestamp() + "ECPServer has been initialized!");
        }

        /// <summary>
        /// Handles the closing of a socket from a disconnecting user.
        /// </summary>
        static void OnDisconnect(object sender, ServerDisconnectEventArgs args)
        {
            Console.WriteLine(GetTimestamp() + "ECPServer has been stopped!");
        }

        /// <summary>
        /// Handles incoming data from a currently connected user.
        /// </summary>
        static void OnDataReceived(object sender, ServerDataReceivedEventArgs args)
        {
            // Simply reads the incoming data and rebroadcasts it to all connected users.
            Console.WriteLine(GetTimestamp() + args.User + ": " + Encoding.ASCII.GetString(args.Data));
            server.BroadcastAll(Encoding.ASCII.GetString(args.Data), args.User);
        }

        /// <summary>
        /// Handles the logging of any internal data passed to the current server instance.
        /// </summary>
        static void OnLogOutput(object sender, LogOutputEventArgs args)
        {
            // Print any errors or log events to the console.
            if (args.Error != null)
                Console.WriteLine(GetTimestamp() + args.Error.Message);
            else
                Console.WriteLine(GetTimestamp() + args.Output);
        }
    }
}
```


### Client
Creating an `ECPClient` is just as easy as creating an `ECPServer`. In this example, we'll create a client with a specified username and connect to the previously created `ECPServer`, send a *"Hello, World!"* message, and await any incoming data that would normally be sent from the server to the current client.

```c#
using System;
using System.Text;
using ECP; // Always remember to import our library.

namespace ECPClient
{
    class Program
    {
        /// <summary>
        /// The username which will be used to identify to the server.
        /// </summary>
        static string username = "Jason";

        /// <summary>
        /// The main entry point for our application.
        /// </summary>
        static void Main(string[] args)
        {
            // Print a greeting to our user along with any relevant information.
            PrintGreeting();

            // Allow any IPv4 address to be entered.
            Console.Write("Enter an IP: ");
            string ip = Console.ReadLine();

            // Try connecting to the remote ECPServer instance.
            try
            {
                // Create a client, hook-up events, and connect to our server.
                ECPClient client = new ECPClient();
                client.OnClientConnect += OnConnect;
                client.OnClientDisconnect += OnDisconnect;
                client.OnDataReceived += OnDataReceived;
                client.OnLogOutput += OnLogOutput;
                client.Connect(ip, username);

                // Say hello to our server.
                client.Send("Hello, World!");

                // Make a prompt to send our server a command.
                Console.Write("~{0}: ", username);

                // Create a loop in order to talk with our server.
                string input = null;
                while (!string.IsNullOrEmpty((input = Console.ReadLine())))
                {
                    // Get user input and send it off.
                    client.Send(input);
                    Console.Write("~{0}: ", username);
                }

                // Tell the user the server has disconnected.
                Console.WriteLine(Timestamp() + "[-] Server has disconnected...");
            }
            catch { Console.WriteLine(Timestamp() + "[x] Connection to server could not be established."); }

            // Wait for user interaction.
            Console.Read();
        }

        /// <summary>
        /// Prints a generic user greeting to the console.
        /// </summary>
        static void PrintGreeting()
        {
            Console.WriteLine("============================================");
            Console.WriteLine("            Welcome to ECPClient            ");
            Console.WriteLine("          Written By: Jason Drawdy          ");
            Console.WriteLine("============================================");
        }

        /// <summary>
        /// Returns a string formatted with the current time of day and date.
        /// </summary>
        static string Timestamp()
        {
            return ("(" + DateTime.Now.ToLongTimeString() + " - " +
            DateTime.Now.ToShortDateString() + ") ");
        }

        /// <summary>
        /// Handles the connection to an <see cref="ECPServer"/> instance.
        /// </summary>
        static void OnConnect(object sender, ClientConnectEventArgs args)
        {
            Console.WriteLine("\n" + Timestamp() + "[+] You have connected to {0}.", args.Server);
        }

        /// <summary>
        /// Handles the disconnection from an <see cref="ECPServer"/> instance.
        /// </summary>
        static void OnDisconnect(object sender, ClientDisconnectEventArgs args)
        {
            Console.WriteLine("\n" + Timestamp() + "[!] You have been disconnected from the server.");
            Console.Write("~{0}: ", username);
        }

        /// <summary>
        /// Handles the incoming data from a socket connected to an <see cref="ECPServer"/> instance.
        /// </summary>
        static void OnDataReceived(object sender, ClientDataReceivedEventArgs args)
        {
            Console.WriteLine("\n" + Timestamp() + "[+] {0}", Encoding.ASCII.GetString(args.Data));
            Console.Write("~{0}: ", username);
        }

        /// <summary>
        /// Handles the logging of any internal data passed to the current client instance.
        /// </summary>
        static void OnLogOutput(object sender, LogOutputEventArgs args)
        {
            Console.WriteLine("\n" + Timestamp() + "[+] {0}", args.Output);
            Console.Write("~{0}: ", username);
        }
    }
}
```

# Credits
**Icon:** `WEWEKA DESiGNERS` <br>
https://www.iconfinder.com/weweka <br>

**Encryption:** `sdrapkin` <br>
https://github.com/sdrapkin/SecurityDriven.Inferno <br>

# License
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
