using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using UdpNetworking;

namespace ScreenBuddies {

    public static class SConsole {

        private static TextBlock _console;

        public static void SetConsole( TextBlock console ) => _console = console;

        public static void WriteLine() => Write( "\r\n", null, null );
        public static void WriteLine( object value ) => Write( value + "\r\n", null, null );
        public static void WriteLine( object value, Brush foregroundColor ) => Write( value + "\r\n", foregroundColor, null );
        public static void WriteLine( object value, Brush foregroundColor, Brush backgroundColor ) => Write( value + "\r\n", foregroundColor, backgroundColor );
        public static void Write( object value ) => Write( value, null, null );
        public static void Write( object value, Brush foregroundColor ) => Write( value, foregroundColor, null );
        public static void Write( object value, Brush foregroundColor, Brush backgroundColor ) {
            if ( _console == null ) {
                MessageBox.Show( "The console variable was not set. You were not supposed to see this message." );
                return;
            }

            CI.Invoke( () => _console.Inlines.Add( new Run( value.ToString() ) { Foreground = foregroundColor ?? _console.Foreground, Background = backgroundColor ?? Brushes.Transparent } ) );
        }

    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private UdpSocket _socket;

        public string Username { get => tbxUsername.Text; set => tbxUsername.Dispatcher.Invoke( () => tbxUsername.Text = value ); }

        public UserList UserList = new UserList();

        public MainWindow() {
            InitializeComponent();
            UserList.OnUserListChanged += u => {
                CI.Invoke( () => {
                    lsbUsers.Items.Clear();
                    UserList.ToList().ForEach( user => lsbUsers.Items.Add( $"{user}" ) );
                } );
            };

            SConsole.SetConsole( tblConsole );

            // Text changed event for console textblock
            DependencyPropertyDescriptor dp = DependencyPropertyDescriptor.FromProperty( TextBlock.TextProperty, typeof( TextBlock ) );
            dp.AddValueChanged( tblConsole, ( a, b ) => svrConsole.ScrollToBottom() );
        }

        private void Window_Loaded( object sender, RoutedEventArgs e ) {
            UdpSocket.OnDataReceived += DataReceived;
            UdpSocket.OnDataSent += DataSent;
            _socket = new UdpSocket();
            //_socket.Connect( "127.0.0.1", 8080 );
            _socket.Connect( "quikers.xyz", 8080 );

            CI.Invoke( () => SetStatusLabelText( "Trying to connect to the ScreenBuddies Server...", "#AAAA44" ) );

            _socket.StartReceiving();

            Task.Run( () => {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                Stopwatch stpwatch = new Stopwatch();
                stpwatch.Restart();
                while ( true ) {
                    if (stpwatch.ElapsedMilliseconds < 100)
                        Thread.Sleep( 100 - (int)stpwatch.ElapsedMilliseconds );
                    stpwatch.Restart();

                    CheckServerStatus(); // TODO: THIS IS NOT WORKING YET, UDPSOCKET.ISACTIVE IS NOT WORKING AS INTENDED EITHER.
                }
            } );
        }

        private void DataReceived( UdpSocket socket, Packet packet, IPEndPoint endPoint ) {
            //if ( packet.Type.Name.ToLower() != "ping" && packet.Type.Name.ToLower() != "pong" )
            //    return;

            SConsole.WriteLine( $"RECV \"{packet.Content}\" FROM {endPoint}", Brushes.DarkMagenta );
            HandlePacketTypes( packet );
        }

        private void DataSent( UdpSocket socket, Packet packet, IPEndPoint endPoint ) {
            //if ( packet.Type.Name.ToLower() != "ping" && packet.Type.Name.ToLower() != "pong" )
                SConsole.WriteLine( $"SENT \"{packet.Content}\" TO {endPoint}", Brushes.DarkCyan );
        }

        private void SetStatusLabelText( string message, string color ) {
            lblServerStatus.Foreground = ( Brush )new BrushConverter().ConvertFromString( color );
            lblServerStatus.Content = message;
        }

        private void CheckServerStatus() {
            if ( _socket.IsActive )
                CI.Invoke( () => SetStatusLabelText( "Connected", "#44CC44" ) );
            else
                CI.Invoke( () => SetStatusLabelText( "Trying to connect to the ScreenBuddies Server...", "#AAAA44" ) );
        }

        /// <summary>
        /// Handle any incoming packet.
        /// </summary>
        /// <param name="packet">The packet to handle</param>
        private void HandlePacketTypes( Packet packet ) {
            switch ( packet.Type.Name.ToLower() ) {
                default: SConsole.WriteLine( $"Received a packet with an unknown type: \"{packet.Type.Name}\".\nPlease contact the developer about this bug." ); break;
                case "ping": case "pong": case "newid": break;
                case "string":
                    SConsole.WriteLine( packet.Content );
                    break;
                case "userlist":
                    if ( !packet.TryDeserializePacket( out UserList userList ) )
                        break;

                    UserList.FromIEnumerable( userList );
                    break;
                case "": break;
            }
        }

        private void btnClients_Click( object sender, RoutedEventArgs e ) { _socket.Send( GET.UserList ); }

        private void tbxUsername_KeyDown( object sender, System.Windows.Input.KeyEventArgs e ) {
            if ( e.Key != System.Windows.Input.Key.Enter )
                return;

            if ( string.IsNullOrWhiteSpace( tbxUsername.Text ) ) {
                MessageBox.Show( "Username has to contain at least 1 symbol.", "Cannot change username", MessageBoxButton.OK, MessageBoxImage.Information );
                return;
            }

            _socket.Send( new Login( Username ) );
        }

        private void Window_Closing( object sender, CancelEventArgs e ) {
            if ( _socket != null && _socket.IsActive )
                _socket.Send( new Disconnect() );
        }
    }
}
