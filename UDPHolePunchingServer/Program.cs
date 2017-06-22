using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Networking;
using System.Linq;

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
            while ( keepAlive ) {
                string cmd = Console.ReadLine();

                switch ( cmd ) {
                    default:
                        Broadcast( cmd );
                        break;
                    case "/quit":
                    case "/exit":
                    case "/stop":
                        keepAlive = false;
                        break;
                }
            }
        }

        private static void Broadcast( string message ) {
            foreach ( TcpSocket socket in _clientList )
                socket.Send( message );
        }

        private static void InitNewClient( TcpSocket newSocket ) {
            Console.WriteLine( "Connection requested! " + newSocket.LocalEndPoint );

            newSocket.ConnectionSuccessful += ClientConnected;
            newSocket.ConnectionLost += ClientLost;
            newSocket.DataReceived += DataReceived;
            newSocket.DataSent += DataSent;

            newSocket.Receive();

            _clientList.Add( newSocket );
        }

        private static void DataReceived( TcpSocket socket, byte[] data ) {
            string message = TcpSocket.ByteArrayToObject<string>( data );
            Console.WriteLine( "Received: " + message );
            socket.Send( message );
        }

        private static void DataSent( TcpSocket socket, byte[] data ) {
            string message = TcpSocket.ByteArrayToObject<string>( data );
            Console.WriteLine( "Sent: " + message );
        }

        private static void ClientConnected( TcpSocket client ) {
            Console.WriteLine( client.LocalEndPoint + " Connected." );
        }

        private static void ClientLost( TcpSocket client ) { Console.WriteLine( client.LocalEndPoint + " Lost connection." ); }
    }
}
