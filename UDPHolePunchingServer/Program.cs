using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Networking;

namespace ScreenBuddiesServer {

    public static class SBConsole {

        public static void WriteLine( string str ) { Write( str + "\n" ); }
        public static void WriteLine( string str, ConsoleColor color ) { Write( str + "\n", color ); }

        public static void Write( string str ) { Console.Write( str ); }

        public static void Write( string str, ConsoleColor color ) {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write( str );
            Console.ForegroundColor = oldColor;
        }

    }

    public partial class Program {

        private const int Port = 20000;

        private static void Main( string[] args ) {
            TcpServer server = new TcpServer( Port, InitClient );

            SBConsole.WriteLine( "Server started, waiting for connections...", ConsoleColor.Green );
            bool keepAlive = true;
            do {
                string cmd = Console.ReadLine();

                string[] split = cmd?.Split( ' ' );
                switch ( split?[ 0 ].ToLower() ) {
                    default:
                        Broadcast( "SERVER: " + cmd );
                        break;
                    case "/help": {
                            SBConsole.WriteLine( "All available commands:\n", ConsoleColor.DarkCyan );
                            SBConsole.WriteLine( "By writing anything not listed below, you are able to broadcast a packet to all connected clients.\nE.G. \"Hello World\" will be broadcasted but \"/help\" will not." );
                            SBConsole.WriteLine( "/help { Shows all available commands in a list format }" );
                            SBConsole.WriteLine( "/newclient, /newc, /nclient, /nc { Opens a new socket window, if available. (DEBUGGING ONLY) }" );
                            SBConsole.WriteLine( "/show { Shows the server's bound ip, port or both. }" );
                            SBConsole.WriteLine( "/quit, /exit, /stop, /close { Stops the server and closes the application. }" );
                        }
                        break;
                    case "/show":
                        if ( split.Length < 2 ) {
                            SBConsole.WriteLine( "\"/show\" requires (at least) one parameter.", ConsoleColor.Red );
                            break;
                        }

                        switch ( split[ 1 ].ToLower() ) {
                            default:
                                SBConsole.WriteLine( "\"" + split[ 1 ] + "\" was not recognized as a ScreenBuddiesServer command.", ConsoleColor.Red );
                                break;
                            case "help":
                                Console.ForegroundColor = ConsoleColor.White;
                                SBConsole.WriteLine( "Available \"/show [cmd]\" commands:\n" );
                                Console.ForegroundColor = ConsoleColor.Gray;
                                SBConsole.WriteLine( "info" );
                                SBConsole.WriteLine( "port" );
                                SBConsole.WriteLine( "ip" );
                                break;
                            case "info":
                                SBConsole.WriteLine( $"Bound on: {server.LocalEndPoint}", ConsoleColor.Yellow );
                                break;
                            case "port":
                                SBConsole.WriteLine( $"Bound on port: {server.LocalEndPoint.ToString().Split( ':' )[ 1 ]}", ConsoleColor.Yellow );
                                break;
                            case "ip":
                                SBConsole.WriteLine( $"Bound on ip: {server.LocalEndPoint.ToString().Split( ':' )[ 0 ]}", ConsoleColor.Yellow );
                                break;
                        }
                        break;
                    case "/newclient":
                    case "/newc":
                    case "/nclient":
                    case "/nc":
                        if ( File.Exists( "ScreenBuddies.exe" ) )
                            Process.Start( "ScreenBuddies.exe", split.Length > 1 ? split[ 1 ] : null );
                        else {
                            SBConsole.WriteLine( "Could not find \"ScreenBuddies.exe\".", ConsoleColor.Red );
                        }
                        break;
                    case "/users":
                    case "/clients":
                    case "/online":
                    case "/list": {
                            SBConsole.WriteLine( "Currently online users:\n", ConsoleColor.DarkCyan );
                            foreach ( User user in Data.Users )
                                SBConsole.WriteLine( $"{user.Username} ({user.Socket.LocalEndPoint})" );
                        }
                        break;
                    case "/kick": {
                            if ( split.Length <= 1 ) {
                                SBConsole.WriteLine( "No parameters given, please specify the user's username like so (without brackets):\r\n/kick [username]", ConsoleColor.Red );
                                break;
                            }
                            if ( !Data.Users.Exists( split[ 1 ] ) ) {
                                SBConsole.WriteLine( $"Could not find user \"{split[ 1 ]}\"", ConsoleColor.Red );
                                break;
                            }

                            User user = Data.Users[ split[ 1 ] ];
                            Broadcast( $"{user.Username} was kicked from the server." );
                            ClientDisconnected( user.Socket, null );
                        }
                        break;
                    case "/quit":
                    case "/exit":
                    case "/stop":
                    case "/close":
                        keepAlive = false;
                        break;
                }
            } while ( keepAlive );

            ThreadHandler.StopAllThreads();
            Environment.Exit( 0 );
        }

        private static void Broadcast( object obj ) { Broadcast( new Packet( obj ) ); }

        private static void Broadcast( Packet packet ) {
            foreach ( User user in Data.Users )
                user.Socket.Send( packet );
        }
    }
}