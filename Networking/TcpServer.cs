using System.Net;
using System.Net.Sockets;

namespace Networking {

    public class TcpServer {

        private TcpListener _listener;

        public EndPoint LocalEndPoint => _listener.LocalEndpoint;
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

}
