using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPClient
{
    public class APeerOnNw
    {
        public APeerOnNw(String _PeerName)
        {
            //Default Data To Avoid Null Exceptions
            PeerName = _PeerName;
            UniqueIdentifier = _PeerName;
            InternalEndPoint = new IPEndPoint(NetUtility.Resolve("127.0.0.1"), 162);
            ExternalEndPoint = new IPEndPoint(NetUtility.Resolve("127.0.0.1"), 162);
        }

        public String PeerName { get; set; }

        public String UniqueIdentifier { get; set; }

        /// <summary>
        /// Network card (local endpoint)
        /// </summary>
        public IPEndPoint InternalEndPoint { get; set; } 

        /// <summary>
        /// Routers (public endpoint)
        /// </summary>
        public IPEndPoint ExternalEndPoint { get; set; }

        public override string ToString()
        {
            var Internal = (InternalEndPoint == null) ? "?" : InternalEndPoint.ToString();
            var External = (ExternalEndPoint == null) ? "?" : ExternalEndPoint.ToString();
            return String.Format("{0} <ID: {3}, INT: {1}, EXT: {2}>", PeerName, Internal, External, UniqueIdentifier);
        }
    }
}
