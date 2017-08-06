using System;
using System.Net;
using System.Net.Sockets;

namespace Networking {

    public class TcpServer : TcpListener {

        public event TcpSocketEventHandler ClientConnectionRequested;

        public EndPoint LocalEndPoint => LocalEndpoint;

        public TcpServer( int port, TcpSocketEventHandler callback ) : base( IPAddress.Any, port ) {
            ClientConnectionRequested += callback;
            BeginAccepting();
        }
        public TcpServer( IPAddress ip, int port, TcpSocketEventHandler callback ) : base( ip, port ) {
            ClientConnectionRequested += callback;
            BeginAccepting();
        }

        private void BeginAccepting() {
            Start();
            BeginAcceptTcpClient( ar => {
                TcpSocket socket = new TcpSocket( ( ( TcpListener )ar.AsyncState ).EndAcceptTcpClient( ar ) );
                ClientConnectionRequested?.Invoke( socket );

                BeginAccepting();
            }, this );
        }

    }

}
