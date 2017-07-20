using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ScreenBuddies {

    

    public static class FConsole {

        public static bool IsActive { get; private set; }

        private static DispatcherTimer _dp;

        private static List<string> _buffers = new List<string>();

        private static TextBox _textbox;
        public static TextBox Textbox {
            get { return _textbox; }
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
        public static void Write( string text ) { _buffers.Add( text ); }

    }
}