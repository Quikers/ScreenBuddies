using System;
using System.Threading.Tasks;

using Networking;

namespace UDP_client_and_server_test {

    public class Program {

        private static UdpSocket _client;
        private static bool _applicationActive;

        public static void Main( string[] args ) {
            _client = new UdpSocket( "quikers.xyz", 8080 );
            _applicationActive = true;

            Console.WriteLine( "Connection setup, sending exploration ping..." );
            Ping ping = new Ping();
            _client.Send( ping );

            // Handle received messages from the server
            StartReceiving();

            // Handle user input
            HandleUserInput();
        }

        private static void StartReceiving() {
            Task.Run( () => {
                while ( _client.Connected ) {
                    if ( !_client.TryReceiveOnce( out Packet packet ) || packet == null )
                        continue;

                    // Strictly for debugging the received messages
                    Console.WriteLine( $"RECV \"{packet.Content}\" FROM {_client.RemoteEndPoint}" );

                    // If the message was not a PING, send a PONG back
                    if ( packet.Content.ToString() != "PING" )
                        _client.Send( "PONG" );
                }
            } );
        }

        private static void HandleUserInput() {
            while ( _applicationActive ) {
                // Read a line from the console and store it in the text variable
                string text = Console.ReadLine();
                // If the input was non-existent or empty (space) ignore it
                if ( string.IsNullOrWhiteSpace( text ) )
                    continue;

                // Send the message to the server
                _client.Send( text );

                // Strictly for debugging the sent messages
                Console.WriteLine( $"SENT \"{text}\" TO {_client.RemoteEndPoint}" );
            }
        }

    }

}
