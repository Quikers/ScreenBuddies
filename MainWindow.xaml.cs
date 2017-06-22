using System;
using System.Net;
using System.Windows;
using System.Net.Sockets;
using System.Threading;

using Networking;

namespace ScreenBuddies {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private TcpSocket _socket;

        public MainWindow() {
            InitializeComponent();

            FConsole.Buffer = tbxConsole;

            _socket = new TcpSocket();
            _socket.ConnectionSuccessful += InitSocket;
            _socket.ConnectionLost += DestroySocket;
            _socket.DataReceived += DataReceived;
            _socket.DataSent += DataSent;
        }

        public void InitSocket( TcpSocket newConnection ) {
            FConsole.WriteLine( "Connected successfully! " + newConnection.RemoteEndPoint );

            _socket.Receive();
        }

        public void DestroySocket( TcpSocket socket ) {
            FConsole.WriteLine( "Lost connection to the server. " + socket.RemoteEndPoint );
            socket.Close();
        }

        public void DataReceived( TcpSocket socket, byte[] data ) {
            FConsole.WriteLine( "Received: " + TcpSocket.ByteArrayToObject<string>( data ) );
        }

        public void DataSent( TcpSocket socket, byte[] data ) {
            FConsole.WriteLine( "Sent: " + TcpSocket.ByteArrayToObject<string>( data ) );

            tbxSend.Dispatcher.Invoke(
                () => {
                    tbxSend.Text = "";
                }
            );
        }

        private void btnStartClient_Click( object sender, RoutedEventArgs e ) {
            Thread t = new Thread(
                () => {
                    _socket.Connect( "127.0.0.1", 20000 );
                    
                    while ( _socket.Connected ) {
                        _socket.Receive();
                    }
                }
            );
            t.Start();
        }

        private void tbxSend_KeyDown( object sender, System.Windows.Input.KeyEventArgs e ) {
            if ( e.Key != System.Windows.Input.Key.Enter )
                return;

            _socket.Send( tbxSend.Text );
        }
    }
}