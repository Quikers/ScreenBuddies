using System;
using System.Diagnostics;
using System.IO;

using Networking;

namespace UDP_client_and_server_test {

    public class Program {

        private static UdpServer _listener;

        public static void Main( string[] args ) {
            _listener = new UdpServer( 8080, NewClientRequest, ReceivedPacket );

            Console.WriteLine( $"Started server on {_listener.LocalEndPoint}" );

            // Handle user input
            HandleUserInput();
        }

        private static void NewClientRequest( UdpSocket socket ) {
            Console.WriteLine( $"A new user ({socket.RemoteEndPoint}) has connected!" );
        }

        private static void ReceivedPacket( UdpSocket socket, Packet packet ) {
            Console.WriteLine( $"RECV \"{packet.Content}\" FROM {socket.RemoteEndPoint}" );

            if ( packet.Type.Name.ToLower() != "ping" )
                return;

            Ping ping = packet.DeserializePacket< Ping >();
            Console.WriteLine( $"RECV PING#{ping.ID} FROM {socket.RemoteEndPoint}" );
            ping.Received = true;
            socket.Send( new Packet( ping ) );
            Console.WriteLine( $"SENT PING#{ping.ID} TO {socket.RemoteEndPoint}" );
        }

        private static void HandleUserInput() {
            while ( _listener.Active ) {
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

        private static void Broadcast( object obj ) => Broadcast( new Packet( obj ), false );
        private static void Broadcast( Packet packet ) => Broadcast( packet, false );
        private static void Broadcast( object obj, bool debug ) => Broadcast( new Packet( obj ), debug );
        private static void Broadcast( Packet packet, bool debug ) {
            // For each ClientInfo in _clientList...
            _listener.Clients.ForEach( ci => {
                // Send the message to the client
                ci.Send( packet.SerializePacket() );

                // Strictly for debugging the sent messages
                if ( debug )
                    Console.WriteLine( $"SENT \"{packet.Content}\" TO {ci.RemoteEndPoint}" );
            } );
        }

    }

}
