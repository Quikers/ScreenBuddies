using System;
using System.Collections.Generic;

using Networking;

namespace UDPHolePunchingServer {
    public class Program {

        private const int Port = 20000;
        private static List<TcpSocket> _clientList = new List<TcpSocket>();

        private static void Main( string[] args ) {
            TcpServer server = new TcpServer( Port );
            server.ClientConnectionRequested += InitNewClient;
            server.Start();

            Console.WriteLine( "Waiting for connections..." );
            bool keepAlive = true;
            do {
                string cmd = Console.ReadLine();

                string[] split = cmd?.Split( ' ' );
                switch ( split?[ 0 ].ToLower() ) {
                    default:
                        Broadcast( cmd );
                        break;
                    case "/help": {
                            Console.WriteLine( "All available commands:\n" );
                            Console.WriteLine( "By writing anything not listed below, you are able to broadcast a message to all connected clients.\nE.G. \"Hello World\" will be broadcasted but \"/help\" will not." );
                            Console.WriteLine( "/help { Shows all available commands in a list format }" );
                            Console.WriteLine( "/show { Shows the server's bound ip, port or both. }" );
                            Console.WriteLine( "/quit { Stops the server and closes the application. }" );
                            Console.WriteLine( "/exit { Stops the server and closes the application. }" );
                            Console.WriteLine( "/stop { Stops the server and closes the application. }" );
                            Console.WriteLine( "/close { Stops the server and closes the application. }" );
                        }
                        break;
                    case "/show":
                        switch ( split[ 1 ].ToLower() ) {
                            default:
                                Console.WriteLine( "\"" + split[ 1 ] + "\" was not recognized as a ScreenBuddiesServer command." );
                                break;
                            case "help":
                                Console.WriteLine( "Available \"/show [cmd]\" commands:\n" );
                                Console.WriteLine( "info" );
                                Console.WriteLine( "port" );
                                Console.WriteLine( "ip" );
                                break;
                            case "info":
                                Console.WriteLine( "Bound on: " + server.LocalEndPoint );
                                break;
                            case "port":
                                Console.WriteLine( "Bound on port: " + server.LocalEndPoint.ToString().Split( ';' )[ 1 ] );
                                break;
                            case "ip":
                                Console.WriteLine( "Bound on ip: " + server.LocalEndPoint.ToString().Split( ';' )[ 0 ] );
                                break;
                        }
                        break;
                    case "/quit":
                    case "/exit":
                    case "/stop":
                    case "/close":
                        keepAlive = false;
                        break;
                }
            } while ( keepAlive );
        }

        private static void Broadcast( string message ) {
            foreach ( TcpSocket socket in _clientList )
                socket.Send( message );
        }

        private static void InitNewClient( TcpSocket newSocket ) {
            Console.WriteLine( "Connection requested! " + newSocket.RemoteEndPoint );

            newSocket.ConnectionSuccessful += ClientConnected;
            newSocket.ConnectionLost += ClientLost;
            newSocket.DataReceived += DataReceived;
            newSocket.DataSent += DataSent;

            newSocket.Receive();

            _clientList.Add( newSocket );
        }

        private static void DataReceived( TcpSocket socket, Packet packet ) {
            string message;
            if ( !Packet.TryParse( packet, out message ) )
                return;

            Console.WriteLine( "Received: " + message );
            socket.Send( message );
        }

        private static void DataSent( TcpSocket socket, Packet packet ) {
            string message;
            if ( !packet.TryParseContent( out message ) )
                return;

            Console.WriteLine( "Sent: " + message );
        }

        private static void ClientConnected( TcpSocket client ) {
            Console.WriteLine( client.RemoteEndPoint + " Connected." );
        }

        private static void ClientLost( TcpSocket client ) { Console.WriteLine( client.RemoteEndPoint + " Lost connection." ); }
    }
}
