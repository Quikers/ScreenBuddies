using System;
using System.Collections.Generic;
using System.Threading;

namespace Networking {

    [Serializable]
    public enum Command {
        None,
        Disconnect,
        Kick,
        UsernameTaken
    }

    public delegate void TcpSocketEventHandler( TcpSocket socket );
    public delegate void TcpSocketErrorEventHandler( TcpSocket socket, Exception ex );
    public delegate void TcpPacketEventHandler( Packet packet );
    public delegate void TcpDataEventHandler( TcpSocket socket, Packet packet );

    public static class ThreadHandler {
        private static List<Thread> _threadList = new List<Thread>();

        public static Thread Create( ParameterizedThreadStart callback ) {
            Thread t = new Thread( callback );
            t.Start();

            _threadList.Add( t );
            return t;
        }
        public static Thread Create( ThreadStart callback ) {
            Thread t = new Thread( callback );
            t.Start();

            _threadList.Add( t );
            return t;
        }

        public static void Remove( Thread t ) {
            _threadList[ _threadList.IndexOf( t ) ].Abort();
            _threadList.Remove( t );
        }
        public static void RemoveAt( int index ) {
            _threadList[ index ].Abort();
            _threadList.RemoveAt( index );
        }

        public static void StopAllThreads() {
            foreach ( Thread t in _threadList )
                t.Abort();
        }
    }

}
