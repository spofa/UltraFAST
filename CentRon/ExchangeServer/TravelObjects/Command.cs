using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelObjects
{
    public enum Command
    {
        NULL,
        //Log into the server
        Login,
        //Logout of the server
        Logout,
        //Send a text message to all the chat clients     
        Message,
        //Get a list of users in the chat room from the server
        ConnectPartnerCallback,
        ConnectPartner,
        Ping,
        FullRead,
        ReadPartial,
        ReadTile,
        ReadResponse,
        Disconnect,


        Move,
        LClick,
        RClick,
        LDown,
        RDown,
        LUp,
        RUp,



        Settings,
        SettingsReceived,




    }
}
