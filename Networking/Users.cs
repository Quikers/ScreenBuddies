using System;
using System.Collections.Generic;
using System.Linq;

namespace Networking {

    /// <summary>
    /// An indexed collection class to keep track of all the <see cref="Users"/>.
    /// </summary>
    [ Serializable ]
    public class Users : List< User > {

        /// <summary>
        /// This variable will keep track of the last used ID and increments it, so there will never be a duplicate ID.
        /// </summary>
        private static int _idAutoIncrement;

        /// <summary>
        /// A boolean that controls whether new <see cref="User"/>-instances are to be added to the <see cref="Users"/>-list or not.
        /// </summary>
        public bool AddUsersAutomatically = true;

        /// <summary>
        /// Gets or sets a <see cref="User"/> by finding their ID.
        /// </summary>
        /// <param name="userID">The ID to search for</param>
        /// <returns>The <see cref="User"/> if found, null if not</returns>
        public new User this[ int userID ] => this.FirstOrDefault( user => user.ID == userID );

        /// <summary>
        /// Gets or sets a <see cref="User"/> by finding their username.
        /// </summary>
        /// <param name="username">The username to search for</param>
        /// <returns>The <see cref="User"/> if found, null if not</returns>
        public User this[ string username ] => this.FirstOrDefault( user => user.Username.ToLower() == username.ToLower() );

        /// <summary>
        /// Gets or sets a <see cref="User"/> by finding their username.
        /// </summary>
        /// <param name="socket">The <see cref="TcpSocket"/> to search for</param>
        /// <returns>The <see cref="User"/> if found, null if not</returns>
        public User this[ TcpSocket socket ] => this.FirstOrDefault( user => user.Socket == socket );

        /// <summary>
        /// Creates a new instance of the <see cref="Users"/> class
        /// </summary>
        public Users() { }

        private Users( IEnumerable< User > userList ) : base( userList ) { }

        /// <summary>
        /// Checks if the given <see cref="User"/> exists.
        /// </summary>
        /// <param name="user">The <see cref="User"/> to check</param>
        /// <returns>True if the <see cref="User"/> was found, false if not</returns>
        public bool Exists( User user ) => Contains( user );

        /// <summary>
        /// Checks if the given <see cref="User"/> exists.
        /// </summary>
        /// <param name="userID">The <see cref="User"/>'s ID to check</param>
        /// <returns>True if the <see cref="User"/> was found, false if not</returns>
        public bool Exists( int userID ) => this[ userID ] != null;

        /// <summary>
        /// Checks if the given <see cref="User"/> exists.
        /// </summary>
        /// <param name="username">The <see cref="User"/>'s username to check</param>
        /// <returns>True if the <see cref="User"/> was found, false if not</returns>
        public bool Exists( string username ) => this[ username ] != null;

        /// <summary>
        /// Checks if the given <see cref="User"/> exists.
        /// </summary>
        /// <param name="socket">The <see cref="User"/>'s <see cref="TcpSocket"/> to check</param>
        /// <returns>True if the <see cref="User"/> was found, false if not</returns>
        public bool Exists( TcpSocket socket ) => this[ socket ] != null;

        /// <summary>
        /// Creates and adds a new <see cref="User"/> to the <see cref="Users"/>-list.
        /// </summary>
        /// <param name="user">The <see cref="User"/> to add</param>
        /// <returns>True if the user was successfully added, false if not</returns>
        public new bool Add( User user ) => Add( user, false );

        /// <summary>
        /// Creates and adds a new <see cref="User"/> to the <see cref="Users"/>-list.
        /// </summary>
        /// <param name="username">The username of the <see cref="User"/> to add</param>
        /// <returns>True if the user was successfully added, false if not</returns>
        public bool Add( string username ) => Add( new User( username ), false );

        /// <summary>
        /// Adds a <see cref="User"/> to the <see cref="Users"/>-list.
        /// </summary>
        /// <param name="user">The <see cref="User"/> to add</param>
        /// <param name="overwrite">Whether to overwrite if the user already exists or not (default: false)</param>
        /// <returns>True if the user was successfully added, false if not</returns>
        public bool Add( User user, bool overwrite ) {
            if ( user.ID < 0 )
                user.ID = _idAutoIncrement++;

            if ( this[ user.Username ] != null ) {
                if ( !overwrite )
                    return Contains( user );
                Remove( user );
                base.Add( user );
            } else {
                base.Add( user );
            }

            return Contains( user );
        }

        /// <summary>
        /// Removes a <see cref="User"/> from the <see cref="Users"/>-list.
        /// </summary>
        /// <param name="user">The <see cref="User"/> to remove from the list</param>
        /// <returns>True if the <see cref="User"/> does not exist in the <see cref="Users"/>-list (anymore), false if the <see cref="User"/> still exists</returns>
        public new bool Remove( User user ) {
            if ( !Exists( user ) )
                return true;

            base.Remove( user );
            return !Exists( user );
        }

        /// <summary>
        /// Removes a <see cref="User"/> from the <see cref="Users"/>-list.
        /// </summary>
        /// <param name="username">The username of the <see cref="User"/> to remove from the list</param>
        /// <returns>True if the <see cref="User"/> does not exist in the <see cref="Users"/>-list (anymore), false if the <see cref="User"/> still exists</returns>
        public bool Remove( string username ) => Remove( this[ username ] );

        /// <summary>
        /// Removes a <see cref="User"/> from the <see cref="Users"/>-list.
        /// </summary>
        /// <param name="userID">The ID of the <see cref="User"/> to remove from the list</param>
        /// <returns>True if the <see cref="User"/> does not exist in the <see cref="Users"/>-list (anymore), false if the <see cref="User"/> still exists</returns>
        public bool Remove( int userID ) => Remove( this[ userID ] );

        /// <summary>
        /// Removes a <see cref="User"/> from the <see cref="Users"/>-list.
        /// </summary>
        /// <param name="socket">The socket of the <see cref="User"/> to remove from the list</param>
        /// <returns>True if the <see cref="User"/> does not exist in the <see cref="Users"/>-list (anymore), false if the <see cref="User"/> still exists</returns>
        public bool Remove( TcpSocket socket ) => Remove( this[ socket ] );

        /// <summary>
        /// Clears the <see cref="Users"/>-list of all <see cref="Users"/> and <see cref="TcpSocket"/>s./>
        /// </summary>
        /// <returns>True if successfully cleared, false if not</returns>
        public new bool Clear() {
            base.Clear();
            return Count == 0;
        }

        public void ClearDisconnectedUsers() {
            // Checks which users are either not connected or have null as a socket value and removes those
            foreach ( User user in this.Where( user => user.Socket == null || !user.Socket.Connected ) ) {
                Remove( user );
            }
        }

        /// <summary>
        /// Replaces the <see cref="Users"/>-list with the given <see cref="List{User}"/>
        /// </summary>
        /// <param name="users">The <see cref="Users"/>-list to replace</param>
        /// <param name="userList">The <see cref="List{User}"/> to replace the <see cref="Users"/>-list with</param>
        /// <returns>The replaced <see cref="Users"/>-list instance</returns>
        public static Users operator -( Users users, IEnumerable< User > userList ) {
            return new Users( userList );
        }

        /// <summary>
        /// Merges a <see cref="List{User}"/> and a <see cref="Users"/>-list into a <see cref="Users"/>-list
        /// </summary>
        /// <param name="users">The <see cref="Users"/>-list to merge the <see cref="List{User}"/> into</param>
        /// <param name="userList">The <see cref="List{User}"/> to merge into the <see cref="Users"/>-list</param>
        /// <returns>The merged <see cref="Users"/>-list instance</returns>
        public static Users operator +( Users users, IEnumerable< User > userList ) {
            foreach ( User user in userList )
                users.Add( user );

            return users;
        }
    }

    /// <summary>
    /// A data container class used to save the user's info like their username and socket information.
    /// </summary>
    [ Serializable ]
    public class User {
        public int ID = -1;
        public string Username;
        [ NonSerialized ] public TcpSocket Socket;

        public User() { }

        public User( string username ) {
            Username = username;

            if ( !Data.Users.AddUsersAutomatically )
                Data.Users.Add( this );
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="socket"></param>
        public User( string username, TcpSocket socket ) {
            Username = username;
            Socket = socket;

            if ( !Data.Users.AddUsersAutomatically )
                Data.Users.Add( this );
        }
    }

}