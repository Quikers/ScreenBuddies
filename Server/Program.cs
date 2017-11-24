using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using UdpNetworking;

namespace Server {

    public static class SConsole {

        public static void WriteLine() => Write( "\n", null, null );
        public static void WriteLine( object value ) => Write( value + "\n", null, null );
        public static void WriteLine( object value, ConsoleColor? textColor ) => Write( value + "\n", textColor, null );
        public static void WriteLine( object value, ConsoleColor? textColor, ConsoleColor? backgroundColor ) => Write( value + "\n", textColor, backgroundColor );
        public static void Write( object value ) => Write( value, null, null );
        public static void Write( object value, ConsoleColor? textColor ) => Write( value, textColor, null );

        public static void Write( object value, ConsoleColor? textColor, ConsoleColor? backgroundColor ) {
            ConsoleColor oldForegroundColor = Console.ForegroundColor;
            ConsoleColor oldBackgroundColor = Console.BackgroundColor;
            Console.ForegroundColor = textColor ?? Console.ForegroundColor;
            Console.BackgroundColor = backgroundColor ?? Console.BackgroundColor;
            Console.Write( value );
            Console.ForegroundColor = oldForegroundColor;
            Console.BackgroundColor = oldBackgroundColor;
        }

    }

    public class Program {

        private static UdpServer _listener;
        private static bool ServerActive;

        private static void Main( string[] args ) {
            // Initialize the server variables
            _listener = new UdpServer( 8080 );
            _listener.StartReceiving();
            ServerActive = true;

            // Set the server's events
            _listener.OnNewClientRequest += NewClientRequested;
            _listener.OnClientDisconnected += ClientDisconnected;
            _listener.OnDataReceived += DataReceived;
            _listener.OnDataSent += DataSent;

            // Set the Clients-list events
            _listener.UserList.OnUsernameChanged += UsernameChanged;
            _listener.UserList.OnUserListChanged += u => Broadcast( _listener.UserList );

            SConsole.WriteLine( $"Server started on { _listener.LocalEndPoint }", ConsoleColor.Blue );

            // Handle continuous user input for server commands
            HandleUserInput();
        }

        private static void ClientDisconnected( User user ) {
            SConsole.WriteLine( $"{user} has disconnected", ConsoleColor.DarkRed );
        }

        private static void NewClientRequested( User user ) {
            SConsole.WriteLine( $"{user} has connected", ConsoleColor.Blue );
        }

        private static void UsernameChanged( User user ) {
            if ( user.Username != "UNKNOWN" )
                SConsole.WriteLine( $"{user} has logged in", ConsoleColor.DarkGreen );

            Broadcast( _listener.UserList );
        }

        private static void DataReceived( UdpSocket socket, Packet packet, IPEndPoint endPoint ) {
            //if ( packet.Type.Name.ToLower() != "ping" && packet.Type.Name.ToLower() != "pong" )
                SConsole.WriteLine( $"RECV \"{( packet.Type.Name.ToLower() == "ping" ? $"PING#{packet.DeserializePacket<Ping>().ID}" : $"{packet.Content}" )}\" FROM {endPoint}", ConsoleColor.DarkMagenta );

            HandlePacket( socket, packet );
        }

        private static void DataSent( UdpSocket socket, Packet packet, IPEndPoint endPoint ) {
            //if ( packet.Type.Name.ToLower() != "ping" && packet.Type.Name.ToLower() != "pong" )
                SConsole.WriteLine( $"SENT \"{( packet.Type.Name.ToLower() == "pong" ? $"PONG#{packet.DeserializePacket<Pong>().ID}" : $"{packet.Content}" )}\" TO {endPoint}", ConsoleColor.DarkCyan );
        }

        private static void HandlePacket( UdpSocket socket, Packet packet ) {
            switch ( packet.Type.Name.ToLower() ) {
                default: SConsole.WriteLine( $"Received packet is of unrecognized type \"{packet.Type.Name}\". Packet is ignored.", ConsoleColor.DarkYellow ); break;
                case "ping": case "pong": case "login": case "newid": case "disconnect": break;
                case "get":
                    if ( !packet.TryDeserializePacket( out GET getRequest ) ) {
                        SConsole.WriteLine( $"Unable to deserialize packet. Packet is possibly corrupted." );
                        break;
                    }

                    HandleGETRequests( socket, getRequest );
                    break;
            }
        }

        private static void HandleGETRequests( UdpSocket socket, GET getRequest ) {
            switch ( getRequest ) {
                default: SConsole.WriteLine( $"Unknown GET request type \"{getRequest}\"" ); break;
                case GET.UserList:
                    socket.Send( _listener.UserList );
                    break;
            }
        }

        private static void Broadcast( object value ) => Broadcast( new Packet( value ) );
        private static void Broadcast( Packet packet ) => _listener.UserList.ToList().ForEach( u => u.Socket.Send( packet ) );

        private static void HandleUserInput() {
            while ( ServerActive ) {
                string input = Console.ReadLine();
                if ( string.IsNullOrWhiteSpace( input ) )
                    continue;

                if ( input[ 0 ] != '/' ) {
                    Broadcast( input );
                    continue;
                }

                List<string> split = new List<string>( input.Split( ' ' ) );

                string cmd = split[ 0 ];
                string[] param = split.GetRange( 1, split.Count - 1 ).ToArray();

                switch ( cmd.ToLower() ) {
                    default: SConsole.WriteLine( $"\"{cmd}\" is not a ScreenBuddies Server command.", ConsoleColor.Red ); break;
                    case "/newclient":
                    case "/nclient":
                    case "/newc":
                    case "/nc":
                        if ( File.Exists( "ScreenBuddies.exe" ) )
                            Process.Start( "ScreenBuddies.exe" );
                        else
                            SConsole.WriteLine( "Could not find the ScreenBuddies.exe file. Please make sure it's in the same folder as this server executable is.", ConsoleColor.Red );
                        break;
                    case "/users":
                    case "/userlist":
                    case "/ul":
                    case "/online":
                        SConsole.WriteLine( "Currently connected users:\n", ConsoleColor.DarkCyan );
                        _listener.UserList.ToList().ForEach( c => SConsole.WriteLine( $"{c.Username} ({c.Socket.RemoteEndPoint})", ConsoleColor.Cyan ) );
                        SConsole.WriteLine();
                        break;
                    case "/tell":
                    case "/say":
                    case "/whisper":
                    case "/talk":
                        if ( param.Length > 0 ) {
                            List<User> foundClients = _listener.UserList.Where( u => u.Socket.RemoteEndPoint.ToString().ToLower() == param[ 0 ].ToLower() ).ToList();
                            if ( foundClients.Count > 0 )
                                foundClients.ForEach( c => c.Socket.Send( param.Length > 2 ? string.Join( " ", param, 1, param.Length - 1 ) : param[ 1 ] ) );
                            else
                                SConsole.WriteLine( $"Client \"{param[ 0 ]}\" is not connected to this server.", ConsoleColor.Red );
                        } else
                            SConsole.WriteLine( $"\"{cmd}\" needs at least 2 parameters: {cmd} [ENDPOINT OF CLIENT] Whatever follows is automatically sent as a message." );
                        break;
                    case "/stop":
                    case "/exit":
                    case "/close":
                        SConsole.WriteLine( "Closing down server...", ConsoleColor.DarkYellow );
                        _listener.StopReceiving();
                        Environment.Exit( 0 );
                        break;
                }
            }
        }

    }
}
