using System;
using System.IO;
using System.Threading;
using Networking;

namespace ScreenBuddiesServer {
    public partial class Program {

        public static void InitClient( TcpSocket socket ) {
            SBConsole.WriteLine( "New connection requested." );
            Packet packet = socket.ReceiveOnce();
            if ( packet.Type.Name.ToLower() != "user" ) {
                SBConsole.WriteLine( $"Unexpected type \"{packet.Type.Name}\", expected \"{typeof( User ).Name}\" instead.", ConsoleColor.Red );
                return;
            }
            if ( !packet.TryDeserializePacket( out User user ) ) {
                SBConsole.WriteLine( $"Could not deserialize packet into \"{typeof( User ).Name}\". Are you using outdated software?", ConsoleColor.Red );
                return;
            }

            if ( !Data.Users.Exists( user.Username ) ) {
                socket.ConnectionLost += ClientDisconnected;
                socket.DataReceived += DataReceived;
                socket.DataSent += DataSent;
                socket.Receive();

                user.Socket = socket;

                Data.Users.Add( user );
                SBConsole.WriteLine( $"{user.Username} has connected.", ConsoleColor.Green );
            } else {
                User oldUser = Data.Users[ user.Username ];

                if ( oldUser.Socket.RemoteEndPoint.ToString().Split( ':' )[ 0 ] != socket.RemoteEndPoint.ToString().Split( ':' )[ 0 ] )
                    SBConsole.WriteLine( $"Duplicate username in login attempt: \"{user.Username}\", connection rejected.", ConsoleColor.Red );

                socket.Send( Command.UsernameTaken );

                Thread.Sleep( 10 ); // Make sure that all messages have been sent before disconnecting
                socket.Close();
            }

            Broadcast( Data.Users );
        }

        public static void ClientDisconnected( TcpSocket socket, Exception ex ) {
            socket.ConnectionLost -= ClientDisconnected;

            string error = "";
            if ( ex != null && ex.GetType() != typeof( IOException ) && ex.GetType() != typeof( NullReferenceException ) && ex.GetType() != typeof( ObjectDisposedException ) )
                error = "\n" + ex;
            if ( !Data.Users.Exists( socket ) ) {
                SBConsole.WriteLine( $"A user lost connection.{error}", ConsoleColor.Red );

                Data.Users.ClearDisconnectedUsers();
                Broadcast( Data.Users );
                return;
            }
            User user = Data.Users[ socket ];

            SBConsole.WriteLine( $"{user.Username}({socket.LocalEndPoint}) Lost connection.{error}", ConsoleColor.Red );

            socket.Close();
            Data.Users.Remove( socket );

            Broadcast( Data.Users );

            socket.ConnectionLost += ClientDisconnected;
        }

        public static void DataReceived( TcpSocket socket, Packet packet ) {
            SBConsole.WriteLine( $"Received \"{packet.Type.Name}\" packet from {Data.Users[ socket ].Username}({socket.LocalEndPoint})", ConsoleColor.DarkMagenta );

            switch ( packet.Type.Name.ToLower() ) {
                default:
                    SBConsole.WriteLine( $"Received a packet of unknown type \"{packet.Type.Name}\".", ConsoleColor.Red );
                    break;
                case "string":
                    if ( !packet.TryDeserializePacket( out string message ) )
                        break;

                    SBConsole.WriteLine( message );
                    Broadcast( Data.Users[ socket ].Username + ": " + message );
                    break;
            }
        }

        public static void DataSent( TcpSocket socket, Packet packet ) {
            /* Ignored for now... */
        }
    }
}