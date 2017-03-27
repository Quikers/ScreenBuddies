using System;
using System.Net;
using System.Windows;
using System.Net.Sockets;
using System.Threading;

namespace ScreenBuddies {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private UdpSocket _socket;

        public MainWindow() {
            InitializeComponent();

            AddPortToMapping(GetLocalIPAddress(), 20000, NetworkProtocol.UDP, "Screen Buddies");

            _socket = new UdpSocket(null, 20000);
            Thread t = new Thread(() => {
                string message = "";
                while (message != "$quit") {
                    message = _socket.Receive();
                    AddLine(message);
                }
            });
            t.Start();

            AddLine("Waiting for incoming connections.");
        }

        private void btnStartClient_Click(object sender, RoutedEventArgs e) {
            IPAddress IP;
            if ( !IPAddress.TryParse( tbxIP.Text, out IP ) ) {
                AddLine( "\"" + tbxIP.Text + "\" was not recognised as a valid IP Address." );
                return;
            }

            _socket.Connect();
        }

        private async void tbxSend_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;

            string data = tbxSend.Text;
            tbxSend.Text = "";

            await _socket.Send(data);
        }
    }
}