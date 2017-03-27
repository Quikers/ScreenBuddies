using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ScreenBuddies {

    public enum PacketType {
        IsAvailable,
        IsAvailableReponse,
        RequestPassword,
        RequestPasswordResponse,
        RequestStartShare,
        RequestStopShare,
        ScreenData
    }

    public class Packet {

        public Packet() {

        }

    }
   
}
