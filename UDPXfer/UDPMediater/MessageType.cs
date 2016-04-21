using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPMediater
{
    /// <summary>
    /// Messages can be 0..127, Ack ois 127+Msg
    /// </summary>
    public enum MessageType
    {
        //Register me and send public IP in ACK
        RegistrationREQ = 1,
        RegistrationREPLY = 128,

        //Get me list of all hosts
        GetHostsListREQ = 2,
        GetHostsListREPLY = 129,

        //Get a paticular host
        GetHostDetailsREQ = 3,
        GetHostDetailsREPLY = 130,

        //Introduce to a paticular host
        IntroduceREQ = 4,
        IntroduceREPLY = 131,
    }
}
