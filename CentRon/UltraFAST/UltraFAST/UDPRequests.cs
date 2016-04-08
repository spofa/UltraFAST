using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraFAST
{
    public class UDPRequests
    {
        public static string CONNECT
        {
            get { return "CONNECT";  }
        }

        public static string DISCONNECT
        {
            get { return "DISCONNECT"; }
        }
        
        public static string PACKET
        {
            get { return "PACKET";  }
        }
    }

}
