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
            Client = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp ) { ExclusiveAddressUse = false, EnableBroadcast = true };
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
            _listener.Bind( _listenEP );

            Console.WriteLine( $"Started server on {_listenEP}" );
            _serverActive = true;
            KeepAlive();

            Task.Run( () => {
                while ( _serverActive ) {
                    EndPoint newClient = new IPEndPoint( IPAddress.Any, 0 );
                    byte[] buffer = new byte[ 128 ];
                    int length = _listener.ReceiveFrom( buffer, ref newClient );

                    string message = Encoding.ASCII.GetString( buffer.ToList().GetRange( 0, length ).ToArray() );

                    // If client does not exist yet, add it to the _clientList
                    if ( _clientList.Where( ci => ci.RemoteEndPoint.ToString() == newClient.ToString() ).ToList().Count == 0 )
                        _clientList.Add( new ClientInfo( newClient ) );

                    Console.WriteLine( $"RECV \"{message}\" FROM {newClient}" );
                }
            } );

            while ( _serverActive ) {
                string text = Console.ReadLine();
                if ( string.IsNullOrWhiteSpace( text ) )
                    continue;

                if ( text == "/nc" && File.Exists( "Client.exe" ) )
                    Process.Start( "Client.exe" );
                else
                    Broadcast( text, true );
            }
        }

        private static void KeepAlive() {
            Task.Run( () => {
                while ( _serverActive ) {
                    Broadcast( "PING", true );
                    Thread.Sleep( 10000 );
                }
            } );
        }

        private static void Broadcast( string message ) => Broadcast( message, false );
        private static void Broadcast( string message, bool debug ) {
            _clientList.ForEach( ci => {
                byte[] buffer = Encoding.ASCII.GetBytes( message );
                ci.Client.SendTo( buffer, ci.RemoteEndPoint );

                if ( debug )
                    Console.WriteLine( $"SENT \"{message}\" TO {ci.RemoteEndPoint}" );
            } );
        }

    }

}
