using System;
using System.Collections.Generic;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Networking;

namespace ScreenBuddies {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private TcpSocket _socket;

        public MainWindow() {
            InitializeComponent();

            FConsole.Textbox = tbxConsole;
        }

        private void ToggleConnectButtonEnabled( bool state ) {
            try {
                btnStartClient.Dispatcher.Invoke( () => {
                    btnStartClient.Content = state ? "Connect" : "Disconnect";
                } );
            } catch ( TaskCanceledException ) { /* Ignored */ }
        }

        public void InitSocket( TcpSocket socket ) {
            FConsole.WriteLine( "Connected successfully! " + socket.RemoteEndPoint );

            FConsole.WriteLine( "Receiving..." );
            socket.Receive();
            ThreadHandler.Create( () => {
                while ( socket.Connected ) {
                    ToggleConnectButtonEnabled( false );
                    Thread.Sleep( 100 );
                } // Check if the socket is connected every 100ms
                ToggleConnectButtonEnabled( true ); // If the socket disconnected, reenable the connect button.
            } );
        }

        public void ConnectionFailed( TcpSocket socket, Exception ex ) {
            if ( socket == null || socket.IsClientNull )
                return;
            try {
                FConsole.WriteLine( ex.ToString() );
                FConsole.WriteLine( "Connected failed! " + socket.RemoteEndPoint );
                socket.Close();
            } catch ( Exception ) { /* ignored */ }
        }

        public void DestroySocket( TcpSocket socket ) {
            if ( socket == null || socket.IsClientNull )
                return;
            try {
                FConsole.WriteLine( "Lost connection to the server. " + socket.RemoteEndPoint );
                socket.Close();
            } catch ( Exception ) { /* ignored */ }
        }

        public void DataReceived( TcpSocket socket, Packet packet ) {
            string message;
            if ( !packet.TryDeserializePacket( out message ) )
                return;

            FConsole.WriteLine( "Received: " + message );
        }

        public void DataSent( TcpSocket socket, Packet packet ) {
            string message;
            if ( !packet.TryDeserializePacket( out message ) )
                return;

            if ( message == "/disconnect" )
                socket.Close();

            FConsole.WriteLine( "Sent: " + message );
            FConsole.ClearTextBox( tbxSend );
        }

        private void btnStartClient_Click( object sender, RoutedEventArgs e ) {
            if ((string) btnStartClient.Content == "Connect") {
                if (_socket == null)
                    _socket = new TcpSocket();

                if (_socket.Connected)
                    return;

                string ip = tbxIP.Text;

                ToggleConnectButtonEnabled(true);

                _socket = new TcpSocket();
                _socket.ConnectionSuccessful += InitSocket;
                _socket.ConnectionFailed += ConnectionFailed;
                _socket.ConnectionLost += DestroySocket;
                _socket.DataReceived += DataReceived;
                _socket.DataSent += DataSent;

                _socket.Connect(ip, 20000);
            } else
                DestroySocket( _socket );
        }

        private void tbxIP_KeyDown( object sender, System.Windows.Input.KeyEventArgs e ) {
            if ( e.Key != System.Windows.Input.Key.Enter )
                return;

            btnStartClient.RaiseEvent( new RoutedEventArgs( Button.ClickEvent ) );
        }

        private void tbxSend_KeyDown( object sender, System.Windows.Input.KeyEventArgs e ) {
            if ( e.Key != System.Windows.Input.Key.Enter )
                return;

            _socket.Send( tbxSend.Text );
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            ThreadHandler.StopAllThreads();
            Environment.Exit(0);
        }
    }
}