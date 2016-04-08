using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TravelObjects
{
    public class RemoteClient
    {

        public IPEndPoint IPEndPoint
        {
            get
            {
                int cport = int.Parse(Port.ToString());
                IPEndPoint clientAddress = new IPEndPoint(System.Net.IPAddress.Parse(IPAddress), cport);
                return clientAddress;
            }
        }


        public IPEndPoint LocalEndPoint
        {

            get
            {
                int cport = int.Parse(LocalPort.ToString());
                IPEndPoint clientAddress = new IPEndPoint(System.Net.IPAddress.Parse(LocalIPAddress), cport);
                return clientAddress;

            }

        }

        public string Display
        {
            get
            {
                return IPAddress + ":" + Port + ":" + RemoteID + ":" + LocalIPAddress + ":" + LocalPort;
            }
        }
        public string IPAddress { get; set; }
        public string Port { get; set; }


        public string LocalIPAddress { get; set; }
        public string LocalPort { get; set; }
        public string RemoteID { get; set; }

    }
}
