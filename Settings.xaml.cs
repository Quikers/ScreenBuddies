using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScreenBuddies {
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window {

        public string Host {
            get => ConfigurationManager.AppSettings[ "host" ];
            set => ConfigurationManager.AppSettings[ "host" ] = value;
        }
        public string Port {
            get => ConfigurationManager.AppSettings[ "port" ];
            set => ConfigurationManager.AppSettings[ "port" ] = value;
        }
        public string Username {
            get => ConfigurationManager.AppSettings[ "username" ];
            set => ConfigurationManager.AppSettings[ "username" ] = value;
        }

        public Settings() {
            InitializeComponent();

            LoadSettings();
        }

        private void LoadSettings() {
            tbxHost.Text = Host;
            tbxPort.Text = Port;
            tbxUsername.Text = Username;
        }

        private void btnSave_Click( object sender, RoutedEventArgs e ) {
            if ( tbxHost.Text == "" || tbxPort.Text == "" || tbxUsername.Text == "" ) {
                MessageBox.Show( "Please fill in all input fields.", "Screen Buddies" );
                return;
            }

            Host = tbxHost.Text;
            Port = tbxPort.Text;
            Username = tbxUsername.Text;

            Close();
        }

        private void btnCancel_Click( object sender, RoutedEventArgs e ) {
            if ( Host == "" || Port == "" || Username == "" ) {
                MessageBox.Show( "All settings must have a value, please fill in the input fields and press Save.", "Screen Buddies" );
                return;
            }

            Close(); }

        private void btnDefault_Click( object sender, RoutedEventArgs e ) {
            Host = "Quikers.xyz";
            Port = "20000";
            Username = "";

            LoadSettings();
        }

        private void tbx_KeyDown( object sender, KeyEventArgs e ) {
            if ( e.Key != System.Windows.Input.Key.Enter )
                return;

            btnSave.RaiseEvent( new RoutedEventArgs( ButtonBase.ClickEvent ) );
        }
    }
}
