using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ScreenBuddies {
    public static class CI {

        public static void Invoke( Action callback ) { try { Application.Current.Dispatcher.Invoke( callback ); } catch( Exception ) {  } }

    }
}
