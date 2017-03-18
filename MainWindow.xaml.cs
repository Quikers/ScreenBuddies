using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NATUPNPLib;

namespace ScreenBuddies {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
        }

        public static string GetLocalIPAddress() {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        private void AddLine(string text) {
            tbx.Text += text + "\n";
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e) {
            tbx.Text = "";

            UPnPNATClass upnpnat = new UPnPNATClass();
            IStaticPortMappingCollection mappings = upnpnat.StaticPortMappingCollection;
            try { mappings.Remove(20000, "UDP"); } catch (Exception) { /* ignored */ }
            mappings.Add(20000, "UDP", 20000, GetLocalIPAddress(), true, "Screen Buddies");
            foreach (IStaticPortMapping portMapping in mappings) {
                if (portMapping.InternalClient != GetLocalIPAddress())
                    continue;

                AddLine(portMapping.Description);
                AddLine(portMapping.ExternalIPAddress + ":" + portMapping.ExternalPort + " @ " +
                        portMapping.Protocol);
                AddLine(portMapping.Enabled + "\n");
            }
        }
    }
}
