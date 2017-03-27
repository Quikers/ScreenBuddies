using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenBuddies {
    public class UdpSocket {

        private Socket _listener;
        private Socket _talker;
        private IPEndPoint _remoteEP;
        private IPEndPoint _anyEP;

        private void InitializeUdpSocket() {
            _talker = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
            _listener = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
            _listener.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );
            _listener.Bind( _anyEP );
        }

        public UdpSocket( string hostname, int port ) {
            _anyEP = new IPEndPoint( IPAddress.Any, port == 0 ? 20000 : port );
            _remoteEP = hostname == null ? null : new IPEndPoint( IPAddress.Parse( hostname ), port );

            InitializeUdpSocket();
        }

        public UdpSocket( IPEndPoint ep ) {
            _anyEP = new IPEndPoint( IPAddress.Any, ep.Port );
            _remoteEP = ep;

            InitializeUdpSocket();
        }

        public string Receive( bool debug = true ) {
            byte[] buffer = new byte[ 1024 ];
            int read = _listener.Receive( buffer );
            string message = Encoding.ASCII.GetString( new List<byte>( new List<byte>( buffer ).GetRange( 0, read ) ).ToArray() );

            if ( debug )
                Console.WriteLine( "Received: " + message );

            return message;
        }

        public void Send( string message, bool debug = true ) {
            _talker.SendTo( Encoding.ASCII.GetBytes( message ), _remoteEP );

            if ( debug )
                Console.WriteLine( "Sent: " + message );
        }

        public void Connect() {
            Send( "$IsAvailable" );
        }

    }
}
