using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Networking {

    public class TcpSocket : TcpClient {

        public event TcpSocketEventHandler ConnectionSuccessful;
        public event TcpSocketErrorEventHandler ConnectionFailed;
        public event TcpSocketErrorEventHandler ConnectionLost;
        public event TcpDataEventHandler DataReceived;
        public event TcpDataEventHandler DataSent;

        public NetworkStream Stream { get { try { return GetStream(); } catch (Exception) { return null; } } }
        public EndPoint LocalEndPoint { get { try { return Client.LocalEndPoint; } catch ( Exception ) { return null; } } }
        public EndPoint RemoteEndPoint { get { try { return Client.RemoteEndPoint; } catch ( Exception ) { return null; } } }

        /// <summary>
        /// Creates a new instance of the <see cref="TcpSocket"/> class.
        /// </summary>
        public TcpSocket() { }
        /// <summary>
        /// Creates a new instance of the <see cref="TcpSocket"/> class and automatically connects it to the given IP and port.
        /// </summary>
        public TcpSocket( string hostname, int port ) { Connect( hostname, port ); }
        /// <summary>
        /// Converts a TcpClient to a TcpSocket.
        /// </summary>
        public TcpSocket( TcpClient socket ) { Client = socket.Client; }

        public new void Connect( string hostname, int port ) {
            if ( !IPAddress.TryParse( hostname, out IPAddress ip ) )
                ip = Dns.GetHostAddresses( hostname )[ 0 ];
            if ( ip == null )
                throw new Exception( "Could not resolve hostname \"" + hostname + "\"" );

            BeginConnect( ip.ToString(), port, ar => {
                try {
                    EndConnect( ar );

                    if ( ( ( TcpSocket )ar.AsyncState ).Connected ) {
                        ConnectionSuccessful?.Invoke( this );
                    } else
                        ConnectionFailed?.Invoke( this, null );
                } catch ( Exception ex ) {
                    ConnectionFailed?.Invoke( this, ex );
                }
            }, this
            );
        }

        public new void Close() {
            base.Close();
            Stream?.Close();
        }

        public void Send( object data ) { Send( new Packet( data ) ); }

        public void Send( Packet data ) {
            if ( Stream == null || !Connected )
                return;

            ThreadHandler.Create( // Initiate the sender thread
                () => {
                    try {
                        byte[] buffer = data.SerializePacket();
                        Stream?.Write( buffer, 0, buffer.Length );

                        DataSent?.Invoke( this, data );
                    } catch ( Exception ex ) {
                        ConnectionLost?.Invoke( this, ex );
                    }
                }
            );
        }

        public void Receive( int bufferSize = 128 ) {
            if ( Stream == null || !Connected )
                return;

            ThreadHandler.Create( // Initiate the receiver thread
                () => {
                    bool error = false;
                    do {
                        try {
                            Packet packet = ReceiveOnce();
                            packet.ParseFailed += PacketParseFailed;
                            DataReceived?.Invoke( this, packet );
                        } catch ( Exception ex ) {
                            error = true;
                            if ( ex.ToString().Contains( "WSACancelBlockingCall" ) )
                                break;
                            
                            ConnectionLost?.Invoke( this, ex );
                        }
                    } while ( Connected && !error );
                }
            );
        }

        public Packet ReceiveOnce( int bufferSize = 4096 ) {
            if ( Stream == null || !Connected )
                return null;

            byte[] bytes = new byte[ bufferSize ];
            int length = Stream.Read( bytes, 0, bytes.Length );

            return new Packet( new List<byte>( bytes ).GetRange( 0, length ) );
        }

        private void PacketParseFailed( Packet packet ) { Console.WriteLine( "Failed to convert packet with type \"" + packet.Type + "\" to type \"string\"" ); }

    }

}