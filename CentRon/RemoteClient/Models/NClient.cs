using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RemoteClient
{
    public class ExchangeClient
    {
        public IPEndPoint PublicEndPoint { get; set; }
        public IPEndPoint LocalEndPoint { get; set; }

    }
}
