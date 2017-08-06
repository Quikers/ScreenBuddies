using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ScreenBuddies {

    public partial class MainWindow {
        public bool FirstLaunch {
            get => ConfigurationManager.AppSettings[ "firstLaunch" ] == null || Convert.ToBoolean( ConfigurationManager.AppSettings[ "firstLaunch" ] );
            set => ConfigurationManager.AppSettings[ "firstLaunch" ] = value.ToString().ToLower();
        }
        public string Username {
            get => ConfigurationManager.AppSettings[ "username" ];
            set { UsernameIsUnique = true; ConfigurationManager.AppSettings[ "username" ] = value; }
        }
        public string Hostname {
            get => ConfigurationManager.AppSettings[ "host" ];
            set => ConfigurationManager.AppSettings[ "host" ] = value;
        }
        public int Port {
            get => Convert.ToInt16( ConfigurationManager.AppSettings[ "port" ] );
            set => ConfigurationManager.AppSettings[ "port" ] = value.ToString();
        }
    }

    public static class FConsole {

        public static bool IsActive { get; private set; }

        private static DispatcherTimer _dp;

        private static List<string> _buffers = new List<string>();

        private static TextBox _textbox;
        public static TextBox Textbox {
            get { try { return _textbox; } catch ( Exception ) { return null; } }
            set {
                if ( value == null ) return;
                _textbox = value;
                IsActive = true;
                _dp = new DispatcherTimer( new TimeSpan( 100000 /* 100000 is 10ms */ ), DispatcherPriority.Normal, DoWork, Textbox.Dispatcher );
                _dp.Start();
            }
        }

        private static void DoWork( object sender, EventArgs e ) {
            if ( Textbox == null ) {
                IsActive = false;
                _dp.Stop();
                throw new Exception( "Textbox has not been set yet." );
            }

            if ( _buffers.Count < 1 || !IsActive )
                return;

            try {
                Textbox.Dispatcher.Invoke(
                    () => {
                        Textbox.AppendText( _buffers[ 0 ] );
                        Textbox.ScrollToEnd();

                        _buffers.RemoveAt( 0 );
                    } );
            } catch ( TaskCanceledException ) { }
        }

        public static void ClearTextBox( TextBox tbx = null ) {
            if ( tbx == null )
                tbx = Textbox;

            tbx.Dispatcher.Invoke( () => { tbx.Text = ""; } );
        }

        public static void WriteLine( string text ) { Write( text + "\n" ); }
        public static void WriteLine( object obj ) { Write( obj + "\n" ); }

        public static void Write( object obj ) { Write( obj.ToString() ); }

        public static void Write( string text ) { _buffers.Add( text ); }

    }
}