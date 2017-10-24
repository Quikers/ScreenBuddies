using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Networking;

namespace UDP_client_and_server_test {

    public class ClientInfo {

        public Socket Client;
        public EndPoint RemoteEndPoint;

        public ClientInfo( EndPoint remoteEndPoint ) {
            // Create a new socket and initialize it
            Client = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp ) { ExclusiveAddressUse = false, EnableBroadcast = true };
            // Copy the EndPoint information to store it
            RemoteEndPoint = remoteEndPoint;
        }

    }

    public class Program {

        private static List<ClientInfo> _clientList = new List<ClientInfo>();
        private static IPEndPoint _listenEP = new IPEndPoint( IPAddress.Any, 8080 );

        private static Socket _listener;
        private static bool _serverActive;

        public static void Main( string[] args ) {
            // Initialize the listener socket
            _listener = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp ) { ExclusiveAddressUse = false, EnableBroadcast = true };
            // Start listening to any connection made to port 8080
            _listener.Bind( _listenEP );
            
            Console.WriteLine( $"Started server on {_listenEP}" );
            _serverActive = true;

            // Turn on the keep alive loop
            KeepAlive();

            // Handle received messages and new clients
            Task.Run( () => {
                while ( _serverActive ) {
                    // Create a temp EndPoint to store the sender's connection information in
                    EndPoint newClient = new IPEndPoint( IPAddress.Any, 0 );
                    // Create a temp buffer to store the message in
                    byte[] buffer = new byte[ 128 ];
                    // Receive the message and take the actual length of the message (not the whole 128 bytes)
                    int length = _listener.ReceiveFrom( buffer, ref newClient );

                    // Convert and store the message in a variable
                    string message = Encoding.ASCII.GetString( buffer.ToList().GetRange( 0, length ).ToArray() );

                    // If client does not exist yet, add it to the _clientList
                    if ( _clientList.Where( ci => ci.RemoteEndPoint.ToString() == newClient.ToString() ).ToList().Count == 0 )
                        _clientList.Add( new ClientInfo( newClient ) );

                    // Strictly for debugging the received messages
                    Console.WriteLine( $"RECV \"{message}\" FROM {newClient}" );
                }
            } );

            // Handle user input
            while ( _serverActive ) {
                // Read a line from the console and store it in the text variable
                string text = Console.ReadLine();
                // If the input was non-existent or empty (space) ignore it
                if ( string.IsNullOrWhiteSpace( text ) )
                    continue;

                // Else if the text contained "/nc" then start a new client (DEBUGGING PURPOSES)
                if ( text == "/nc" && File.Exists( "Client.exe" ) )
                    Process.Start( "Client.exe" );
                else // Else broadcast the message to all the connected clients
                    Broadcast( text, true );
            }
        }

        private static void KeepAlive() {
            // Handle the keep-alive packages every 10 seconds
            Task.Run( () => {
                while ( _serverActive ) {
                    // Broadcast a PING message to all clients and debug it
                    Broadcast( "PING", true );
                    // Wait 10 seconds
                    Thread.Sleep( 10000 );
                }
            } );
        }

        private static void Broadcast( string message ) => Broadcast( message, false );
        private static void Broadcast( string message, bool debug ) {
            // For each ClientInfo in _clientList...
            _clientList.ForEach( ci => {
                // Create a temp buffer to store the message in
                byte[] buffer = Encoding.ASCII.GetBytes( message );
                // Send the message to the client
                ci.Client.SendTo( buffer, ci.RemoteEndPoint );

                // Strictly for debugging the sent messages
                if ( debug )
                    Console.WriteLine( $"SENT \"{message}\" TO {ci.RemoteEndPoint}" );
            } );
        }

    }

}
