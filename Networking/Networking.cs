using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace Networking {
    public delegate void TcpClientEventHandler( TcpSocket socket );
    public delegate void TcpPacketEventHandler( TcpSocket socket, Packet packet );

    public class TcpServer {

        private TcpListener _listener;

        public event TcpClientEventHandler ClientConnectionRequested;

        public TcpServer( int port ) {
            _listener = new TcpListener( IPAddress.Any, port );
        }

        private void BeginAccepting() {
            _listener.BeginAcceptTcpClient(
                ar => {
                    TcpListener listener = ( TcpListener )ar.AsyncState;
                    TcpSocket socket = new TcpSocket( listener.EndAcceptTcpClient( ar ) );
                    ClientConnectionRequested?.Invoke( socket );

                    BeginAccepting();
                }, _listener );
        }

        public void Start() {
            _listener.Start();
            BeginAccepting();
        }

    }

    public class TcpSocket {

        private TcpClient _socket;
        private NetworkStream Stream => _socket.GetStream();
        private Thread _sendThread;
        private Thread _receiveThread;
        public EndPoint LocalEndPoint => _socket.Client.LocalEndPoint;
        public EndPoint RemoteEndPoint => _socket.Client.RemoteEndPoint;
        public bool Connected => _socket.Connected;

        public event TcpClientEventHandler ConnectionSuccessful;
        public event TcpClientEventHandler ConnectionLost;
        public event TcpPacketEventHandler DataReceived;
        public event TcpPacketEventHandler DataSent;

        public TcpSocket() { _socket = new TcpClient(); }
        public TcpSocket( TcpClient socket ) { _socket = socket; }

        public void Connect( string hostname, int port ) {
            if ( !IPAddress.TryParse( hostname, out IPAddress ip ) )
                ip = Dns.GetHostAddresses( hostname )[ 0 ];
            if ( ip == null )
                throw new Exception( "Could not resolve hostname \"" + hostname + "\"" );

            _socket.BeginConnect( ip.ToString(), port,
                ar => {
                    TcpClient client = ( TcpClient )ar.AsyncState;
                    client.EndConnect( ar );
                    ConnectionSuccessful?.Invoke( this );
                }, _socket );
        }

        public void Close() { _socket.Close(); }

        public void Send( object data ) {
            if ( !Connected )
                return;

            _sendThread = new Thread(
                () => {
                    try {
                        byte[] buffer = Packet.ToByteArray( data );
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
                    while ( Connected && !error ) {
                        try {
                            byte[] temp = new byte[ bufferSize ];
                            int length = Stream.Read( temp, 0, temp.Length );
                            List<byte> buffer = new List<byte>( temp ).GetRange( 0, length );

                            DataReceived?.Invoke( this, new Packet( buffer.ToArray() ) );
                        } catch ( Exception ) {
                            error = true;
                            ConnectionLost?.Invoke( this );
                        }
                    }
                }
            ); _receiveThread.Start();
        }

    }

    public class Packet {

        public string Type;
        public object Content;

        public Packet() {
            Type = typeof( string ).Name;
            Content = "UNINITIALIZED PACKET";
        }
        public Packet( object obj ) {
            Content = obj;
            Type = obj.GetType().Name;
        }
        public Packet( byte[] bytes ) {
            Content = ByteArrayToObject<object>( bytes );
            Type = Content.GetType().Name;
        }

        public byte[] ToByteArray() {
            return ObjectToByteArray( this );
        }

        public static byte[] ToByteArray( Packet packet ) {
            return ObjectToByteArray( packet.Content );
        }

        public static byte[] ToByteArray( object obj ) {
            return ObjectToByteArray( obj );
        }

        public static bool TryParse<T>( Packet packet, out T obj ) {
            obj = default( T );

            if ( packet.Type != typeof( T ).Name )
                return false;

            try {
                obj = ( T )packet.Content;
                return true;
            } catch ( Exception ) { /* Ignored */ }

            return false;
        }

        public bool TryParse<T>( out T obj ) { return TryParse( this, out obj ); }

        private static byte[] ObjectToByteArray( object obj ) {
            BinaryFormatter bf = new BinaryFormatter();
            using ( MemoryStream ms = new MemoryStream() ) {
                bf.Serialize( ms, obj );
                return ms.ToArray();
            }
        }

        private static T ByteArrayToObject<T>( byte[] arrBytes ) {
            using ( MemoryStream memStream = new MemoryStream() ) {
                BinaryFormatter bf = new BinaryFormatter();
                memStream.Write( arrBytes, 0, arrBytes.Length );
                memStream.Seek( 0, SeekOrigin.Begin );
                object obj = bf.Deserialize( memStream );
                return ( T )obj;
            }
        }

    }
}
