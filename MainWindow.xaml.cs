using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
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

        public static TcpSocket Socket = new TcpSocket();
        public bool UsernameIsUnique;

        public MainWindow() {
            InitializeComponent();
            
            FConsole.Textbox = tbxConsole;
            Data.Users.AddUsersAutomatically = false;
            
            //Hostname = "Quikers.xyz";
            Hostname = "127.0.0.1";
            Port = 20000;
            Username = "Quikers";

            Settings settingsWindow = new Settings();
            settingsWindow.ShowDialog();
        }

        private void Window_Loaded( object sender, RoutedEventArgs e ) { LoopConnection(); }

        private void LoopConnection() {
            Socket = new TcpSocket();
            ThreadHandler.Create( () => {
                while ( UsernameIsUnique ) {
                    Thread.Sleep( 1000 );

                    try {
                        if ( Socket == null )
                            Socket = new TcpSocket();

                        if ( Socket != null && Socket.Connected )
                            continue;

                        Socket = new TcpSocket();
                        if ( Socket != null ) {
                            Socket.ConnectionSuccessful += InitSocket;
                            Socket.ConnectionFailed += ConnectionFailed;
                            Socket.ConnectionLost += DestroySocket;
                            Socket.DataReceived += DataReceived;
                            Socket.DataSent += DataSent;

                            Socket.Connect( Hostname, Port );
                        }
                    } catch ( Exception ex ) { FConsole.WriteLine( ex ); break; }
                }
            } );
        }

        public void InitSocket( TcpSocket socket ) {
            FConsole.WriteLine( $"Connected successfully to {Hostname}" );

            socket.Receive();
            socket.Send( new User( Username ) );
        }

        public void ConnectionFailed( TcpSocket socket, Exception ex ) { /* Called every second */ }

        public void DestroySocket( TcpSocket socket, Exception ex ) {
            if ( socket == null )
                return;
            try {
                string error = "";
                if ( ex.GetType() != typeof( IOException ) && ex.GetType() != typeof( NullReferenceException ) )
                    error = "\n" + ex;
                FConsole.WriteLine( $"Lost connection to the server.{error}" );
                socket.Close();
            } catch ( Exception ) { /* ignored */ }
        }

        public void DataReceived( TcpSocket socket, Packet packet ) {
            if ( packet.Type == null ) {
                DestroySocket( socket, new IOException( "Empty data received." ) );
                return;
            }

            switch ( packet.Type.Name.ToLower() ) {
                default:
                    FConsole.WriteLine( $"Packet contained an unknown type: {packet.Type.Name}" );
                    break;
                case "string":
                    string message;
                    if ( !packet.TryDeserializePacket( out message ) )
                        break;
                    FConsole.WriteLine( message );
                    break;
                case "command":
                    Command command;
                    if ( !packet.TryDeserializePacket( out command ) )
                        break;
                    FConsole.WriteLine( $"Received a command from the server: {command}" );

                    if ( command != Command.UsernameTaken )
                        break;

                    UsernameIsUnique = false;
                    FConsole.WriteLine( "Username is already taken" );
                    break;
                case "users":
                    Users users;
                    if ( !packet.TryDeserializePacket( out users ) )
                        break;

                    Data.Users = users;
                    lbUsers.Dispatcher.Invoke( () => {
                        lbUsers.Items.Clear();
                        foreach ( User user in Data.Users )
                            lbUsers.Items.Add( new ListBoxItem { Content = user.Username } );
                    } );
                    break;
            }
        }

        public void DataSent( TcpSocket socket, Packet packet ) { FConsole.ClearTextBox( tbxSend ); }

        private void tbxSend_KeyDown( object sender, System.Windows.Input.KeyEventArgs e ) {
            if ( e.Key != System.Windows.Input.Key.Enter )
                return;

            Socket.Send( tbxSend.Text );
        }

        private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e ) {
            ThreadHandler.StopAllThreads();
            Environment.Exit( 0 );
        }

        private void Window_SizeChanged( object sender, SizeChangedEventArgs e ) {
            Width = e.NewSize.Width;
            Height = e.NewSize.Height;
        }
    }
}