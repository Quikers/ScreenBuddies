using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
                            Console.WriteLine( "By writing anything not listed below, you are able to broadcast a packet to all connected clients.\nE.G. \"Hello World\" will be broadcasted but \"/help\" will not." );
                            Console.WriteLine( "/help { Shows all available commands in a list format }" );
                            Console.WriteLine( "/newclient, /newc, /nclient, /nc { Opens a new client window, if available. (DEBUGGING ONLY) }" );
                            Console.WriteLine( "/show { Shows the server's bound ip, port or both. }" );
                            Console.WriteLine( "/quit, /exit, /stop, /close { Stops the server and closes the application. }" );
                        }
                        break;
                    case "/show":
                        if ( split.Length < 2 ) {
                            Console.WriteLine( "\"/show\" requires (at least) one parameter." );
                            break;
                        }

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
                                Console.WriteLine( "Bound on port: " + server.LocalEndPoint.ToString().Split( ':' )[ 1 ] );
                                break;
                            case "ip":
                                Console.WriteLine( "Bound on ip: " + server.LocalEndPoint.ToString().Split( ':' )[ 0 ] );
                                break;
                        }
                        break;
                    case "/newclient":
                    case "/newc":
                    case "/nclient":
                    case "/nc":
                        if ( File.Exists( "ScreenBuddies.exe" ) )
                            Process.Start( "ScreenBuddies.exe" );
                        break;
                    case "/quit":
                    case "/exit":
                    case "/stop":
                    case "/close":
                        keepAlive = false;
                        break;
                }
            } while ( keepAlive );

            ThreadHandler.StopAllThreads();
            Environment.Exit(0);
        }

        private static void Broadcast( object obj ) { Broadcast( new Packet( obj ) ); }
        private static void Broadcast( Packet packet ) {
            foreach ( TcpSocket socket in _clientList )
                socket.Send( packet );
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
            switch ( packet.Type.Name.ToLower() ) {
                default:
                    Console.WriteLine( "Packet contained an unknown type: {0}", packet.Type.Name );
                    break;
                case "string":
                    string message;
                    if ( !packet.TryDeserializePacket( out message ) )
                        return;

                    Console.WriteLine( "Received: " + message );
                    Broadcast( packet );
                    //socket.Send( packet );
                    break;
            }
        }

        private static void DataSent( TcpSocket socket, Packet packet ) {
            string message;
            if ( !packet.TryDeserializePacket( out message ) )
                return;

            Console.WriteLine( "Sent: " + message );
        }

        private static void ClientConnected( TcpSocket client ) {
            Console.WriteLine( client.RemoteEndPoint + " Connected." );
        }

        private static void ClientLost( TcpSocket client ) {
            Console.WriteLine( client.RemoteEndPoint + " Lost connection." );

            _clientList.Remove( client );
        }
    }
}
