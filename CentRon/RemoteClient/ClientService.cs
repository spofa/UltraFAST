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
        public ClientService() { }
        
        public void OnData(Data data)
        {
            if (data == null) { return; }

            switch (data.cmdCommand)
            {
                case Command.ReadResponse:
                    Globals.AppForm.PrintMessage(data.strMessage);
                   Globals.AppForm.ScreenUpdate(data);
                    break;

                default: break;
            }
        }



        public void stop()
        {
            ListenService.SendLidgrenMessage(new Data() { cmdCommand = Command.Disconnect, strMessage="Disconnect"}, ListenService.GetPartnerEndPoint());

            run = false;
        }

        public void start()
        {
            if (theThread == null)
            {
                run = true;
                theThread = new Thread(new ThreadStart(startRead));
                theThread.Start();
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

                    //if (Globals.primaryImage != null) { break; }

                    Thread.Sleep(500);
                    string reqType = "pri";
                    if (Globals.primaryImage == null)
                    {
                        reqType = "pri";
                    }
                    else {
                        reqType = "pri";
                    }

                    ListenService.SendLidgrenMessage(new Data() { cmdCommand = Command.Read , strMessage ="Rad screen request", strName= reqType }, ListenService.GetPartnerEndPoint());
                 
                }
            }
            catch (Exception Ex)
            {

            }
        }

    }
}
