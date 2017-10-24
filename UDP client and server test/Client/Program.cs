using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Networking;

namespace UDP_client_and_server_test {

    public class Program {

        private static Socket _listener;
        private static Socket _client;
        private static EndPoint _serverEP = new IPEndPoint( IPAddress.Parse( "127.0.0.1" ), 8080 );
        private static bool _clientActive;

        public static void Main( string[] args ) {
            // Initialize both client and listener sockets
            _client = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp ) { ExclusiveAddressUse = false, EnableBroadcast = true };
            _listener = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp ) { ExclusiveAddressUse = false, EnableBroadcast = true };
            // Prepare for connections to the server
            _client.Connect( _serverEP );
            // Start listening to the newly created connection the the server
            _listener.Bind( _client.LocalEndPoint );

            _clientActive = true;
            // Handle received messages from the server
            Task.Run( () => {
                while ( _clientActive ) {
                    // Create a temp EndPoint to store the sender's connection information in
                    EndPoint tmp = new IPEndPoint( IPAddress.Any, 0 );
                    // Create a temp buffer to store the message in
                    byte[] buffer = new byte[ 128 ];
                    // Receive the message and take the actual length of the message (not the whole 128 bytes)
                    int length = _listener.ReceiveFrom( buffer, ref tmp );

                    // Convert and store the message in a variable
                    string message = Encoding.ASCII.GetString( buffer.ToList().GetRange( 0, length ).ToArray() );

                    // Strictly for debugging the received messages
                    Console.WriteLine( $"RECV \"{message}\" FROM {tmp}" );

                    // If the message was not a PING, skip it
                    if ( message != "PING" )
                        continue;

                    // If it was, send a PONG back
                    _client.Send( Encoding.ASCII.GetBytes( "PONG" ) );

                    // Strictly for debugging the sent messages
                    Console.WriteLine( $"SENT \"PONG\" TO {_serverEP}" );
                }
            } );

            // Handle user input
            while ( _clientActive ) {
                // Read a line from the console and store it in the text variable
                string text = Console.ReadLine();
                // If the input was non-existent or empty (space) ignore it
                if ( string.IsNullOrWhiteSpace( text ) )
                    continue;

                // Else create a temporary byte buffer to send to the server
                byte[] buffer = Encoding.ASCII.GetBytes( text );
                // Send the message to the server
                _client.Send( buffer );

                // Strictly for debugging the sent messages
                Console.WriteLine( $"SENT \"{text}\" TO {_serverEP}" );
            }
        }

    }

}
