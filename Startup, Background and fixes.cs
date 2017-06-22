using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ScreenBuddies {

    public static class FConsole {

        public static TextBox Buffer { get; set; }

        public static void WriteLine( string text ) { Write( text + "\n" ); }
        public static void Write( string text ) {
            if ( Buffer == null )
                throw new Exception( "Buffer has not been set yet." );

            try {
                Buffer.Dispatcher.Invoke(
                    () => {
                        Buffer.AppendText( text );
                        Buffer.ScrollToEnd();
                    } );
            } catch ( TaskCanceledException ) { }


        }

    }

    public partial class MainWindow : Window {

        public static string GetLocalIPAddress() {
            IPHostEntry host = Dns.GetHostEntry( Dns.GetHostName() );
            foreach ( IPAddress ip in host.AddressList ) {
                if ( ip.AddressFamily == AddressFamily.InterNetwork ) {
                    return ip.ToString();
                }
            }
            throw new Exception( "Local IP Address Not Found!" );
        }

        public static string GetExternalIPAddress() { return new WebClient().DownloadString( "https://api.ipify.org" ); }
    }
}