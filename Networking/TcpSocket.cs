using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Networking {

    public class TcpSocket {

        private TcpClient _socket;
        private NetworkStream Stream => _socket.GetStream();
        private Thread _sendThread;
        private Thread _receiveThread;
        public EndPoint LocalEndPoint => _socket.Client.LocalEndPoint;
        public EndPoint RemoteEndPoint => _socket.Client.RemoteEndPoint;
        public bool Connected => _socket.Connected;

        public event TcpClientEventHandler ConnectionSuccessful;
        public event TcpClientErrorEventHandler ConnectionFailed;
        public event TcpClientEventHandler ConnectionLost;
        public event TcpDataEventHandler DataReceived;
        public event TcpDataEventHandler DataSent;

        public TcpSocket() { _socket = new TcpClient(); }
        public TcpSocket( TcpClient socket ) { _socket = socket; }

        public void Connect( string hostname, int port ) {
            IPAddress ip;
            if ( !IPAddress.TryParse( hostname, out ip ) )
                ip = Dns.GetHostAddresses( hostname )[ 0 ];
            if ( ip == null )
                throw new Exception( "Could not resolve hostname \"" + hostname + "\"" );

            _socket.BeginConnect( ip.ToString(), port,
                ar => {
                    try {
                        TcpClient client = ( TcpClient )ar.AsyncState;
                        client.EndConnect( ar );

                        ConnectionSuccessful?.Invoke( this );
                    } catch ( Exception ex ) {
                        ConnectionFailed?.Invoke( this, ex );
                    }
                }, _socket );
        }

        public void Close() { _socket.Close(); }

        public void Send( Packet data ) {
            if ( !Connected )
                return;

            _sendThread = new Thread(
                () => {
                    try {
                        byte[] buffer = data.PacketToByteArray();
                        Stream.Write( buffer, 0, buffer.Length );

                        DataSent?.Invoke( this, new Packet( buffer ) );
                    } catch ( SocketException ) {
                        ConnectionLost?.Invoke( this );
                    }
                }
            ); _sendThread.Start();
        }

        public void Receive( int bufferSize = 128 ) {
            if ( !Connected )
                return;

            _receiveThread = new Thread(
                () => {
                    bool error = false;
                    do {
                        try {
                            byte[] temp = new byte[ bufferSize ];
                            int length = Stream.Read( temp, 0, temp.Length );
                            List<byte> buffer = new List<byte>( temp ).GetRange( 0, length );

                            Packet packet = new Packet( buffer.ToArray() );
                            packet.ParseFailed += PacketParseFailed;
                            DataReceived?.Invoke( this, packet );
                        } catch ( Exception ) {
                            error = true;
                            ConnectionLost?.Invoke( this );
                        }
                    } while ( Connected && !error );
                }
            ); _receiveThread.Start();
        }

        private void PacketParseFailed( Packet packet ) {
            Console.WriteLine( "Failed to convert packet with type \"" + packet.Type + "\" to type \"string\"" );
        }

    }

}
