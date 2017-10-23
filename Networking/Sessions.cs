﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Networking {

    [Serializable]
    public enum SessionState {
        Idle,
        PlayRequested,
        Playing,
        PauseRequested,
        Paused,
        StopRequested,
        Stopped
    }

    [Serializable]
    public enum SessionCommand {
        RequestPlay,
        RequestPause,
        RequestStop
    }

    [Serializable]
    public class Session {

        public delegate void SessionEvent( Session session );
        public event SessionEvent OnSessionStateChanged;

        public List<User> UserList = new List<User>();
        private SessionState _state = SessionState.Idle;
        public SessionState State {
            get => _state;
            set {
                OnSessionStateChanged?.Invoke( this );
                _state = value;
            }
        }

        public void Broadcast( Packet packet ) {
            foreach ( User user in UserList.ToList().Where( u => u.ConnectionInfo != null && u.ConnectionInfo.Connected ) )
                user.ConnectionInfo.Send( packet );
        }

    }

}