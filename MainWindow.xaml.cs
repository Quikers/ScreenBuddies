using System;
using System.Windows;
using System.Threading;
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
            btnStartClient.Dispatcher.Invoke( () => { btnStartClient.IsEnabled = state; } );
        }

        public void InitSocket( TcpSocket socket ) {
            FConsole.WriteLine( "Connected successfully! " + socket.RemoteEndPoint );

            FConsole.WriteLine( "Receiving..." );
            socket.Receive();
            Thread t = new Thread( () => {

                while ( socket.Connected ) {
                    FConsole.WriteLine( "Still connected." );
                    Thread.Sleep( 100 );
                }
                ToggleConnectButtonEnabled( true );
            } ); t.Start();
        }

        public void ConnectionFailed( TcpSocket socket, Exception ex ) {
            FConsole.WriteLine( "Connected failed! " + socket.RemoteEndPoint );
            FConsole.WriteLine( ex.ToString() );

            socket.Close();
        }

        public void DestroySocket( TcpSocket socket ) {
            FConsole.WriteLine( "Lost connection to the server. " + socket.RemoteEndPoint );
            socket.Close();
        }

        public void DataReceived( TcpSocket socket, Packet packet ) {
            string message;
            if ( !packet.TryParseContent( out message ) )
                return;

            FConsole.WriteLine( "Received: " + message );
        }

        public void DataSent( TcpSocket socket, Packet packet ) {
            string message;
            if ( !packet.TryParseContent( out message ) )
                return;

            FConsole.WriteLine( "Sent: " + message );
            FConsole.ClearTextBox( tbxSend );
        }

        private void btnStartClient_Click( object sender, RoutedEventArgs e ) {
            if (_socket == null)
                _socket = new TcpSocket();

            if ( _socket.Connected )
                return;

            string ip = tbxIP.Text;

            ToggleConnectButtonEnabled( true );

            _socket = new TcpSocket();
            _socket.ConnectionSuccessful += InitSocket;
            _socket.ConnectionFailed += ConnectionFailed;
            _socket.ConnectionLost += DestroySocket;
            _socket.DataReceived += DataReceived;
            _socket.DataSent += DataSent;

            _socket.Connect( ip, 20000 );
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
    }
}