using beRemote.Core.ProtocolSystem.ProtocolBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beeVNC.beRemoteProtocol
{
    public class VNCSession : Session
    {
     
        public VNCSession(beRemote.Core.ProtocolSystem.ProtocolBase.Interfaces.IServer server, VNCProtocol vNCProtocol, long connSettingsID) : base(server, vNCProtocol, connSettingsID) { }
        
        public override void CloseConnection()
        {
            

            this.TriggerCloseConnectionEvent();
        }


        public override System.Windows.Controls.UserControl GetSessionWindow()
        {
            if (_sessionWindow == null)
            {
                _sessionWindow = new SessionPresenter();
            }
            return _sessionWindow;
        }

        public override void OnOpenConnection(string username, string password)
        {
            //userControl11.ServerAddress = txtServer.Text;
            //userControl11.ServerPassword = txtPassword.Text;
            //userControl11.ServerPort = Convert.ToInt32(txtPort.Text);
            //userControl11.Connect();

            //((SessionPresenter)_sessionWindow).vncView.ServerAddress = "192.168.32.158";
            //((SessionPresenter)_sessionWindow).vncView.ServerPassword = "bene1905";
            //((SessionPresenter)_sessionWindow).vncView.ServerPort = 5900;
            //((SessionPresenter)_sessionWindow).vncView.Connect();
            //String ip = this.GetSessionServer().GetRemoteIP().ToString();
            //int port = this.GetProtocolPort();

            //((SessionPresenter)_sessionWindow).DoConnect(ip, password, port);

            this.password = password;
        }

        private String password;

        public override void OnOpenConnectionPostProcessor(params object[] args)
        {
            base.OnOpenConnectionPostProcessor(args);

            ((SessionPresenter)_sessionWindow).DoConnect(this.GetSessionServer().GetRemoteIP().ToString(), password, this.GetProtocolPort());
        }
    }
}
