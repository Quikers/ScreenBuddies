using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Networking {

    public class Packet {

        // EVENT HANDLERS

        public event TcpPacketEventHandler ParseFailed;

        // VARIABLES

        public Type Type => Content.GetType();
        public string SuggestedType;
        public object Content;

        // CONSTRUCTORS

        /// <summary>
        /// The standard <see cref="Packet"/>. Contains no information.
        /// </summary>
        public Packet() {}
        /// <summary>
        /// Create a <see cref="Packet"/> from any type of object.
        /// </summary>
        /// <param name="obj">The object to create the <see cref="Packet"/> from</param>
        public Packet( object obj ) { Content = obj; }
        public static Packet FromByteArray<T>( byte[] bytes ) {
            Packet p = new Packet {
                Content = ByteArrayToObject<T>( bytes ),
                SuggestedType = typeof( T ).Name
            };

            return p;
        }

        // FUNCTIONS

        /// <summary>
        /// Converts the contens of a <see cref="Packet"/> into a <see cref="byte"/> array.
        /// </summary>
        /// <returns>The <see cref="byte"/> array containing the converted object</returns>
        public byte[] ContentToByteArray() { return ObjectToByteArray( Content ); }
        /// <summary>
        /// Converts the whole <see cref="Packet"/> to a <see cref="byte"/> array that contains a string with the suggested type of the contents and the contents into a <see cref="byte"/> array.
        /// </summary>
        /// <returns>The <see cref="byte"/> array containing the converted <see cref="Packet"/></returns>
        public byte[] PacketToByteArray() { return Encoding.ASCII.GetBytes( Type + "\\\\0\\\\" + Encoding.ASCII.GetString( ContentToByteArray() ) ); }
        /// <summary>
        /// Tries to parse the contents of the <see cref="Packet"/> into the given <see cref="System.Type"/>.
        /// </summary>
        /// <typeparam name="T">The type to try to convert the contents of the <see cref="Packet"/> into</typeparam>
        /// <param name="to">The variable that will hold the value of the newly converted contents</param>
        /// <returns>True or false depending on whether the conversion was successfull or not</returns>
        public bool TryParseContent<T>( out T to ) {
            to = default( T );

            if ( Type != typeof( T ) ) {
                ParseFailed?.Invoke( this );
                return false;
            }

            try {
                to = ( T )Content;
                return true;
            } catch ( Exception ) { /* Ignored */ }

            ParseFailed?.Invoke( this );
            return false;
        }

        // STATIC FUNCTIONS

        /// <summary>
        /// Converts an object to a <see cref="byte"/> array.
        /// </summary>
        /// <param name="obj">The object to convert to bytes</param>
        /// <returns><see cref="byte"/> array containing the given object</returns>
        public static byte[] ToByteArray( object obj ) { return ObjectToByteArray( obj ); }
        /// <summary>
        /// Tries to convert the given object to the given type.
        /// </summary>
        /// <typeparam name="T">The type to convert the given object to</typeparam>
        /// <param name="from">The object to convert to the given type</param>
        /// <param name="to">The variable to store the newly converted object in</param>
        /// <returns>True or false depending on whether the conversion was successfull or not</returns>
        public static bool TryParse<T>( object from, out T to ) {
            to = default( T );

            if ( from.GetType() != typeof( T ) ) {
                return false;
            }

            try {
                to = ( T )from;
                return true;
            } catch ( Exception ) { /* Ignored */ }

            return false;
        }

        // INTERNAL CLASS TOOLS

        /// <summary>
        /// Converts an object of any type to a <see cref="byte"/> array.
        /// </summary>
        /// <param name="obj">The object to convert</param>
        /// <returns>The converted object as a <see cref="byte"/> array</returns>
        private static byte[] ObjectToByteArray( object obj ) {
            BinaryFormatter bf = new BinaryFormatter();
            using ( MemoryStream ms = new MemoryStream() ) {
                bf.Serialize( ms, obj );
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Converts <see cref="byte"/> array to the specified object
        /// </summary>
        /// <typeparam name="T">The type to convert the <see cref="byte"/> array into</typeparam>
        /// <param name="arrBytes">The <see cref="byte"/> array to convert to an object of the specified type</param>
        /// <returns>The converted <see cref="byte"/> array as an object of the specified type</returns>
        private static T ByteArrayToObject<T>( byte[] arrBytes ) {
            using ( MemoryStream memStream = new MemoryStream() ) {
                BinaryFormatter bf = new BinaryFormatter();
                memStream.Write( arrBytes, 0, arrBytes.Length );
                memStream.Seek( 0, SeekOrigin.Begin );
                object obj = bf.Deserialize( memStream );
                return ( T )obj;
            }
        }

    }

}
