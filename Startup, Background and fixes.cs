using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using NATUPNPLib;

namespace ScreenBuddies {
    internal enum NetworkProtocol {
        UDP,
        TCP
    }

    public partial class MainWindow : Window {

        public static string GetLocalIPAddress() {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        /// <summary>
        /// Add a line of text to the textbox control "tbxConsole".
        /// </summary>
        /// <param name="text">The string of text to add</param>
        private void AddLine(string text) {
            tbxConsole.AppendText(text + "\n");
            tbxConsole.ScrollToEnd();
        }

        /// <summary>
        /// Forwards a port using UPnP.
        /// </summary>
        /// <param name="IP">The IP the forwarded port is bound to</param>
        /// <param name="port">The port to forward</param>
        /// <param name="protocol">The protocol used for the forwarded port</param>
        /// <param name="description">The title to describe the forwarded port</param>
        /// <param name="enabled">If true, the port is forwarded, if false the port is not forwarded</param>
        private void AddPortToMapping(string IP, int port, NetworkProtocol protocol, string description, bool enabled = true) {
            IStaticPortMappingCollection mappings = new UPnPNATClass().StaticPortMappingCollection;
            try { mappings.Remove(port, protocol.ToString()); } catch (Exception) { /* ignored */ }
            mappings.Add(port, protocol.ToString(), port, IP, enabled, description);
        }

        /// <summary>
        ///     Prints the ports that are forwarded using UPnP.
        /// </summary>
        /// <param name="showOnlyLocalIPMappings">If true, only prints the forwarded port on this pc's local IP address.</param>
        private void PrintMappings(bool showOnlyLocalIPMappings = false) {
            var mappings = new UPnPNATClass().StaticPortMappingCollection;
            foreach (IStaticPortMapping portMapping in mappings) {
                if (showOnlyLocalIPMappings && portMapping.InternalClient != GetLocalIPAddress())
                    continue;

                AddLine(portMapping.Description);
                AddLine(portMapping.Protocol);
                AddLine(portMapping.InternalClient + ":" + portMapping.InternalPort);
                AddLine(portMapping.ExternalIPAddress + ":" + portMapping.ExternalPort);
                AddLine(portMapping.Enabled + "\n");
            }
        }
    }
}