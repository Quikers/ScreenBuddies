using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using UdpNetworking;

namespace UDP_client_and_server_test {

    public class Program {

        private static void DataReceived( UdpSocket s, Packet p, IPEndPoint endPoint ) {
            if ( p.TypeName != "ping" && p.TypeName != "pong" )
                Console.WriteLine( $"RECV \"{p.Content}\" FROM {endPoint}" );
            else if ( p.TypeName == "ping" )
                Console.WriteLine( $"RECV \"PING#{p.DeserializePacket<Ping>().ID}\" FROM {endPoint}" );
            else
                Console.WriteLine( $"RECV \"PONG#{p.DeserializePacket<Pong>().ID}\" FROM {endPoint}" );
        }
        private static void DataSent( UdpSocket s, Packet p, IPEndPoint endPoint ) {
            if ( p.TypeName != "ping" && p.TypeName != "pong" )
                Console.WriteLine( $"SENT \"{p.Content}\" TO {endPoint}" );
            else if ( p.TypeName == "ping" )
                Console.WriteLine( $"SENT \"PING#{p.DeserializePacket<Ping>().ID}\" TO {endPoint}" );
            else
                Console.WriteLine( $"SENT \"PONG#{p.DeserializePacket<Pong>().ID}\" TO {endPoint}" );
        }
        private static void ClientConnected( User user ) {
            Console.WriteLine( $"A new client has connected! {user.ReceiverEndPoint}" );
        }

        private static void Main( string[] args ) {
            UdpServer server = new UdpServer( IPAddress.Any, 8080 );
            server.OnDataReceived += DataReceived;
            server.OnDataSent += DataSent;
            server.OnNewClientRequest += ClientConnected;

            server.StartReceiving();

            Console.WriteLine( $"Server is bound ({server.LocalEndPoint}) and ready for incoming packets!" );

            while ( true ) {
                string input = Console.ReadLine();
                if ( string.IsNullOrWhiteSpace( input ) )
                    continue;

                switch ( input.ToLower() ) {
                    default:
                        foreach ( User u in server.UserList )
                            u.Send( input );
                        break;
                    case "/ns":
                        Process.Start( "Server.exe" );
                        continue;
                    case "/nc":
                        Process.Start( "Client.exe" );
                        continue;
                    case "/ul":
                        Console.WriteLine( $"All online users({server.UserList.Count}):\n" );
                        server.UserList.ToList().ForEach( Console.WriteLine );
                        continue;
                }
            }
        }

    }

}
