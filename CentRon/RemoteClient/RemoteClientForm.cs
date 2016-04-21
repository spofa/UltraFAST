using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using tcpServer;
using TravelObjects;

namespace RemoteClient
{
    public partial class RemoteClientForm : Form
    {

        public ScreenCapture screenForm = null;

        delegate void SetTextCallback(TextBox tb, string text);
        int listenport = 65446;

        // UdpClient udpClient;

        public RemoteClientForm()
        {
            InitializeComponent();
            ListenService.Partner = new ExchangeClient();
            ListenService.startListen(listenport);
            ListenService.UdpSent += ListenService_UdpSent;
            Globals.AppForm = this;
            if (screenForm == null) { screenForm = new ScreenCapture(); }

            ListenService.CloseAllConnections();

        }

        private void ListenService_UdpSent(object sender, UdpEventArgs e)
        {

        }

        #region "UDP"

        public void OnData(TransferData data)
        {

            if (data == null) { return; }

            switch (data.cmdCommand)
            {

                case Command.Login:
                    SetText(txtMyID, data.strName);
                    break;

                case Command.Message:
                    SetText(txtRelayBox, data.strMessage + Environment.NewLine);
                    break;

                case Command.Ping:

                    //  MessageBox.Show(data.strName); 
                    string pdata = data.strMessage;
                    string[] lst = pdata.Split(':');
                    string ip = lst[0];
                    string port = lst[1];
                    string rid = lst[2];
                    string lip = lst[3];
                    string lport = lst[4];

                    int cport = int.Parse(port.ToString());

                    ListenService.Partner.PublicEndPoint = new IPEndPoint(System.Net.IPAddress.Parse(ip), cport);

                    if (!string.IsNullOrEmpty(lip))
                    {
                        int lcport = 0;
                        int.TryParse(lport.ToString(), out lcport);
                        ListenService.Partner.LocalEndPoint = new IPEndPoint(System.Net.IPAddress.Parse(lip), lcport);
                    }

                    if (data.strName == "host")
                    {
                        ListenService.IsHost = "yes";
                        ListenService.InitializeService();
                    }


                    if (data.strName == "client")
                    {
                        ListenService.IsHost = "no";


                        ListenService.InitializeService();
                    }




                    SetText(txtRelayBox, "Connecting to " + ListenService.Partner.PublicEndPoint.Address.ToString() + ":" + ListenService.Partner.PublicEndPoint.Port.ToString() + Environment.NewLine);

                    Thread.Sleep(1000);
                    ListenService.LClient.Connections[0].Disconnect(""); // DISCONNECT SERVER
                    TransferData PartnerData = new TransferData();
                    PartnerData.cmdCommand = Command.NULL;
                    PartnerData.strMessage = "handshake";
                    PartnerData.strName = "handshake";

                    // ListenService.SendUDPMessage(PartnerData);

                    ListenService.SendLidgrenMessage(PartnerData, ListenService.GetPartnerEndPoint());

                    //ListenService.ConnectPartner(PartnerData);

                    //  SetText(txtRelayBox, "Send to Partner " + PartnerData.strMessage + Environment.NewLine);


                    break;
                case Command.NULL:
                    if (data.strName == "handshake")
                    {
                        ListenService.ConnectedToPartner = true;

                        if (ListenService.IsHost == "no")
                        {
                            Globals.service.SendSettings();
                        }
                        SetText(txtRelayBox, "handshake from Partner " + data.strName + Environment.NewLine);
                        //MessageBox.Show("Connected To Partner");
                    }
                    else
                    {
                        SetText(txtRelayBox, "Ping from Partner " + data.strName + Environment.NewLine);
                    }
                    break;
            }

        }

        private IPAddress LocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        private void btnConnectServer_Click(object sender, EventArgs e)
        {

            if (ListenService.ConnectedToExchangeServer == true) return;

            IPAddress serverIP = IPAddress.Parse(txtServerIP.Text);     // Server IP
            int port = 80;
            int.TryParse(txtServerPort.Text, out port);

            // Server port
            ListenService.serverEndPoint = new IPEndPoint(serverIP, port);
            string serverResponse = string.Empty;       // The variable which we will use to store the server response

            // using (UdpClient client = new UdpClient())
            {
                IPAddress endp = LocalIPAddress();
                TravelObjects.TransferData test = new TravelObjects.TransferData();

                TravelObjects.TransferData LoginData = new TravelObjects.TransferData();
                LoginData.cmdCommand = Command.Login;
                LoginData.strMessage = "Hi";
                LoginData.strName = listenport.ToString();

                //LoginData.strName = endp.ToString() + ":" + ((IPEndPoint)ListenService.udpClient.Client.LocalEndPoint).Port.ToString();
                //LoginData.port = ((IPEndPoint)ListenService.udpClient.Client.LocalEndPoint).Port.ToString();
                //LoginData.IPAddress = endp.ToString();

                LoginData.strName = endp.ToString() + ":" + ListenService.LClient.Port.ToString();
                LoginData.port = ListenService.LClient.Port.ToString();
                LoginData.IPAddress = endp.ToString();


                byte[] data = LoginData.ToByte();      // Convert our message to a byte array
                                                       //ListenService.udpClient.Send(data, data.Length, serverEndPoint);      // Send the date to the server



                ListenService.ConnectedToExchangeServer = ListenService.SendLidgrenMessage(LoginData, ListenService.serverEndPoint);



                //data = client.Receive(ref serverEndPoint);
                //LoginData = new Data(data);
                //txtMyID.Text = LoginData.strName;

            }




        }

        private void SetText(TextBox tb, string text)
        {
            if (tb.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { tb, text });
            }
            else
            {
                tb.AppendText(text);
            }
        }

        private void btnRequestPartnerInfo_Click(object sender, EventArgs e)
        {
            if (ListenService.ConnectedToExchangeServer == false) { return; }



            //  using (UdpClient client = new UdpClient())
            {
                TransferData PartnerData = new TransferData();
                PartnerData.cmdCommand = Command.ConnectPartner;
                PartnerData.strMessage = txtMyID.Text;
                PartnerData.strName = txtPartnerID.Text;
                byte[] data = PartnerData.ToByte();      // Convert our message to a byte array
                //ListenService.udpClient.Send(data, data.Length, serverEndPoint);      // Send the date to the server
                ListenService.SendLidgrenMessage(PartnerData, ListenService.serverEndPoint);

                //  data = client.Receive(ref serverEndPoint);
                //PartnerData = new Data(data);
                //txtPartnerDynamicIP.Text = PartnerData.strName;
            }


        }



        private void btnSendPartner_Click(object sender, EventArgs e)
        {

            TransferData PartnerData = new TransferData();
            PartnerData.cmdCommand = Command.NULL;
            PartnerData.strMessage = txtpartnerMessage.Text;
            PartnerData.strName = txtpartnerMessage.Text;

            //ListenService.SendUDPMessage(PartnerData);

            ListenService.SendLidgrenMessage(PartnerData, ListenService.GetPartnerEndPoint());

            SetText(txtRelayBox, "Send to Partner " + txtpartnerMessage.Text + Environment.NewLine);
        }

        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {


                ListenService.CloseAllConnections();

            }
            catch (Exception ex)
            {


            }
        }

        private void Partnerclient_DataReceived(TransferData data)
        {
            try
            {
                switch (data.cmdCommand)
                {
                    case Command.Message:
                        SetText(txtRelayBox, data.strMessage + Environment.NewLine);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static bool CheckMask(IPAddress address, IPAddress mask, IPAddress target)
        {
            if (mask == null)
                return false;

            var ba = address.GetAddressBytes();
            var bm = mask.GetAddressBytes();
            var bb = target.GetAddressBytes();

            if (ba.Length != bm.Length || bm.Length != bb.Length)
                return false;

            for (var i = 0; i < ba.Length; i++)
            {
                int m = bm[i];

                int a = ba[i] & m;
                int b = bb[i] & m;

                if (a != b)
                    return false;
            }

            return true;
        }

        public void PrintMessage(string message)
        {
            SetText(txtRelayBox, message + Environment.NewLine);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            Globals.TravelImage = new TravelImage();


            if (!File.Exists(Globals.SettingsPath))
            {
                Globals.SaveDefaultConnectionSettings();
            }
            else
            {

                Globals.LoadConnectionSettings();

                if (Globals.Settings != null && Globals.Settings.ResetDefault == true)
                {
                    Globals.SaveDefaultConnectionSettings();
                    Globals.LoadConnectionSettings();
                }
            }


            if (Globals.Settings.AutoConnectServer == true)
            {

                if (!string.IsNullOrEmpty(Globals.Settings.ServerIP))
                {
                    txtServerIP.Text = Globals.Settings.ServerIP;
                }

                if (!string.IsNullOrEmpty(txtServerIP.Text))
                {
                    btnConnectServer_Click(null, null);
                }
                

            }


        }


        private int resolutionX;
        private int resolutionY;


        delegate void SetScreenUpdate(TransferData data);

        public void ScreenUpdate(TransferData data)
        {
            if (this.screenForm == null) return;


            try
            {
                if (this.screenForm.InvokeRequired)
                {
                    SetScreenUpdate d = new SetScreenUpdate(ScreenUpdate);
                    this.Invoke(d, new object[] { data });
                }
                else
                {
                    //this.screenBox.Dock = DockStyle.Fill;

                    bool IsOpen = false;
                    foreach (Form form in Application.OpenForms)
                    {
                        if (form.GetType().Name == this.screenForm.GetType().Name)
                        {
                            IsOpen = true;
                            break;
                        }
                    }

                    if (IsOpen == false) { this.screenForm.Show(); }


                    screenForm.RenderImage(data);

                    //BinaryFormatter bFormat = new BinaryFormatter();
                    //MemoryStream ms1 = new MemoryStream(data.ByteArray);
                    //Image inImage = bFormat.Deserialize(ms1) as Image;


                    ////  if (Settings.Scale) inImage = ResizeImage(inImage, 1920, 1080);
                    //resolutionX = inImage.Width;
                    //resolutionY = inImage.Height;
                    //if (resolutionX > 5) this.screenBox.Image = (Image)inImage;
                }

            }
            catch (Exception ex) { }

        }

        private void ketch_Click(object sender, EventArgs e)
        {
            Globals.SaveDefaultConnectionSettings();


        }

        private void btnConnectPartnerTCP_Click(object sender, EventArgs e)
        {

        }

        private void btnSendImage_Click(object sender, EventArgs e)
        {
            Globals.service.SendImage();
        }
    }

}
