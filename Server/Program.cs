using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Networking;

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
            _listener = new UdpServer( 8080, NewClientRequested, DataReceived );
            SConsole.WriteLine( $"Server started on { _listener.LocalEndPoint }", ConsoleColor.DarkCyan );

            ServerActive = true;
            HandleUserInput();
        }

        private static void NewClientRequested( UdpSocket socket ) {
            SConsole.WriteLine( $"A new client connected: {socket.RemoteEndPoint}", ConsoleColor.DarkCyan );
            socket.DataSent += DataSent;
        }

        private static void DataReceived( UdpSocket socket, Packet packet ) {
            SConsole.WriteLine( $"RECV \"{packet.Content}\" FROM {socket.RemoteEndPoint}", ConsoleColor.DarkMagenta );
            HandlePacket( socket, packet );
        }

        private static void DataSent( UdpSocket socket, Packet packet ) {
            SConsole.WriteLine( $"SENT \"{packet.Content}\" TO {socket.RemoteEndPoint}", ConsoleColor.Blue );
            HandlePacket( socket, packet );
        }

        private static void HandlePacket( UdpSocket socket, Packet packet ) {
            switch ( packet.Type.Name.ToLower() ) {
                default: SConsole.WriteLine( $"Received packet is of unrecognized type \"{packet.Type.Name}\". Packet is ignored.", ConsoleColor.DarkYellow ); break;
                case "ping":
                    if ( !packet.TryDeserializePacket( out Ping ping ) ) {
                        SConsole.WriteLine( $"Unable to deserialize packet. Packet is possibly corrupted." );
                        break;
                    }

                    ping.Received = true;
                    socket.Send( ping );
                    break;
            }
        }

        private static void HandleUserInput() {
            while ( ServerActive ) {
                string input = Console.ReadLine()?.ToLower();
                if ( string.IsNullOrWhiteSpace( input ) )
                    continue;

                if ( input[ 0 ] != '/' ) {
                    _listener.Broadcast( input );
                    continue;
                }

                string cmd = input.Split( ' ' )[ 0 ];
                string[] param = input.Split( ' ' );

                switch ( cmd ) {
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
                        _listener.Clients.ToList().ForEach( c => SConsole.WriteLine( c.RemoteEndPoint, ConsoleColor.Cyan ) );
                        SConsole.WriteLine();
                        break;
                    case "/tell":
                    case "/say":
                    case "/whisper":
                    case "/talk":
                        if ( param.Length > 0 ) {
                            List<UdpSocket> foundClients = _listener.Clients.Where( c => c.LocalEndPoint.ToString() == param[ 0 ] ).ToList();
                            if ( foundClients.Count > 0 )
                                foundClients.ForEach( c => c.Send( string.Join( " ", param, 1, param.Length - 2 ) ) );
                            else
                                SConsole.WriteLine( $"Client \"{param[ 0 ]}\" is not connected to this server.", ConsoleColor.Red );
                        } else
                            SConsole.WriteLine( $"\"{cmd}\" needs at least 2 parameters: {cmd} [ENDPOINT OF CLIENT] Whatever follows is automatically sent as a message." );
                        break;
                    case "/stop":
                    case "/exit":
                    case "/close":
                        SConsole.WriteLine( "Closing down server...", ConsoleColor.DarkYellow );
                        _listener.Stop();
                        Environment.Exit( 0 );
                        break;
                }
            }
        }

    }
}
