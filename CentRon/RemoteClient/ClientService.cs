using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TravelObjects;

namespace RemoteClient
{
    public class ClientService : IService
    {
        private bool run = true;
        private Thread theThread;
        private double lastRequest = 0;
        private int counterTiles = 0;
        private bool requestCyceComplete = true;

        private DateTime lastSend = DateTime.Now;
        public ClientService() { }

        private DateTime LastTileRecievedTime = DateTime.Now;
        public void OnData(TransferData data)
        {
            //If data is empty don't process
            if (data == null) { return; }

            switch (data.cmdCommand)
            {
                case Command.SettingsReceived:
                    Globals.AppForm.PrintMessage(data.strMessage);
                    this.start();


                    break;
                case Command.ReadResponse:
                    if (lastRequest == data.ProcessedRequestNo)
                    {
                        counterTiles = counterTiles + 1;
                    }
                    if (counterTiles == data.TotalTiles)
                    {
                        requestCyceComplete = true;
                        counterTiles = 0;
                    }

                    Globals.AppForm.PrintMessage(data.strMessage + " Req No " + data.ProcessedRequestNo);
                    Globals.AppForm.ScreenUpdate(data);
                    break;

                default: break;
            }
        }

        public void stop()
        {
            ListenService.SendLidgrenMessage(new TransferData() { cmdCommand = Command.Disconnect, strMessage = "Disconnect", RequestNo = lastRequest }, ListenService.GetPartnerEndPoint());

            run = false;
        }

        public void start()
        {
            if (Globals.Settings.SendingMethod == SendingMethods.Pull)
            { 
                if (theThread == null)
                {
                    run = true;
                    theThread = new Thread(new ThreadStart(startRead));
                    theThread.Start();
                }
            }
        }


        public void RequestFrames()
        {

        }


        private void startRead()
        {
            try
            {
                while (run)
                {
                    //Thread.Sleep(1000);

                    TimeSpan diff = DateTime.Now.Subtract(lastSend);

                    if (diff.TotalMilliseconds > Globals.Settings.UpdateFrequency)
                    {
                        requestCyceComplete = true;
                    }

                    if (requestCyceComplete == true)
                    {
                        lastRequest = lastRequest + 1;
                        requestCyceComplete = false;
                        lastSend = DateTime.Now;
                        ListenService.SendLidgrenMessage(new TransferData() { cmdCommand = Command.ReadPartial, strMessage = "Read screen request", RequestNo = lastRequest }, ListenService.GetPartnerEndPoint());
                    }
                }

            }
            catch (Exception Ex)
            {

            }
        }

        public void SendImage()
        {


        }
        public void SendSettings()
        {
            Globals.AppForm.PrintMessage("Sending setings..");

            System.Net.IPEndPoint ip = ListenService.GetPartnerEndPoint();
            ListenService.SendLidgrenMessage(new TransferData() { cmdCommand = Command.Settings, Settings= Globals.Settings },ip );

        }

    }
}
