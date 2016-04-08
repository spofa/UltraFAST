using System;
using System.Collections.Generic;
using System.Linq;
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

namespace beeVNC.beRemoteProtocol
{
    /// <summary>
    /// Interaction logic for SessionPresenter.xaml
    /// </summary>
    public partial class SessionPresenter : UserControl
    {
        public SessionPresenter()
        {
            InitializeComponent();

        }

        internal void DoConnect(string ip, string password, int port)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    grdContent.Children.Clear();
                    var vnc = new vncControl();
                    vnc.ServerAddress = ip;
                    vnc.ServerPassword = password;
                    vnc.ServerPort = port;

                    grdContent.Children.Add(vnc);

                    vnc.Connect();
                }));
        }
    }
}
