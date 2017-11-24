using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using UdpNetworking;

namespace UDP_client_and_server_test {

    public class Program {

        private static void DataReceived( UdpSocket s, Packet p, IPEndPoint endPoint ) {
            if ( p.TypeName != "ping" && p.TypeName != "pong" )
                Console.WriteLine( $"RECV \"{p.Content}\" FROM {endPoint}" );
            else if ( p.TypeName == "ping" )
                Console.WriteLine( $"RECV \"PING#{p.DeserializePacket<Ping>().ID}\" FROM {endPoint}" );
            else
                Console.WriteLine( $"RECV \"PONG#{p.DeserializePacket<Pong>().ID}\" FROM {endPoint} AFTER {Socket.Ping.MsSinceSent}ms" );
        }
        private static void DataSent( UdpSocket s, Packet p, IPEndPoint endPoint ) {
            if ( p.TypeName != "ping" && p.TypeName != "pong" )
                Console.WriteLine( $"SENT \"{p.Content}\" TO {endPoint}" );
            else if ( p.TypeName == "ping" )
                Console.WriteLine( $"SENT \"PING#{p.DeserializePacket<Ping>().ID}\" TO {endPoint}" );
            else
                Console.WriteLine( $"SENT \"PONG#{p.DeserializePacket<Pong>().ID}\" TO {endPoint} AFTER {Socket.Ping.MsSinceSent}ms" );
        }

        public static UdpSocket Socket;

        private static void Main( string[] args ) {
            UdpSocket.OnDataReceived += DataReceived;
            UdpSocket.OnDataSent += DataSent;
            Socket = new UdpSocket();
            Socket.Connect( "127.0.0.1", 8080 );
            //Socket.Connect( "213.46.57.198", 8080 );

            Socket.StartReceiving();

            Task.Run( () => {
                while ( true )
                    Socket.StayAlive();
            } );

            while ( true ) {
                string input = Console.ReadLine();
                if ( string.IsNullOrWhiteSpace( input ) )
                    continue;

                string[] split = input.Split( ' ' );
                string first = split[ 0 ];
                switch ( first.ToLower() ) {
                    default:
                        Socket.Send( input );
                        break;
                    case "/ns":
                        Process.Start( "Server.exe" );
                        continue;
                    case "/nc":
                        Process.Start( "Client.exe" );
                        continue;
                    case "/ping":
                        Socket.SendPing();
                        continue;
                    case "/login":
                        if ( split.Length > 1 && !string.IsNullOrWhiteSpace( split[ 1 ] ) )
                            Socket.Send( new Login( split[1] ) );
                        continue;
                }
            }
        }

    }

}
