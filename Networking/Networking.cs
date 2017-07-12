using System;

namespace Networking {

    public delegate void TcpClientEventHandler( TcpSocket socket );
    public delegate void TcpClientErrorEventHandler( TcpSocket socket, Exception ex );
    public delegate void TcpPacketEventHandler( Packet packet );
    public delegate void TcpDataEventHandler( TcpSocket socket, Packet packet );
    
}
