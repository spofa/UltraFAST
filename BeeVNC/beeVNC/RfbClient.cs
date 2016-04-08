/************************************
 * Developed by Kristian Reukauff
 * License and Project:
 * https://beevnc.codeplex.com/
 * Published under NewBSD-License
 * without any warrenties
 * provided "as is"
 ************************************/

#undef logging

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace beeVNC
{
    internal class RfbClient
    {
#if logging
		 private Logtype _LoggingLevel = Logtype.None; //How much logging should be done
		 private string _LoggingPath = "C:\\temp\\beevnc.log"; //Where should the logfile be saved?
#endif

        private ConnectionProperties _Properties = new ConnectionProperties(); //Contains Properties of the Client
        private bool _DisconnectionInProgress = false; //Flag for getting, if a Disconnection is in Progress
        private Dictionary<RfbEncoding, RfbEncodingDetails> _EncodingDetails = new Dictionary<RfbEncoding, RfbEncodingDetails>(); //Contains the Details for the Encodings
        private UInt16[,] _ColorMap = new UInt16[256,3]; //Stores the Colormap. In Each Dimension is R, G or B
        private Queue<RfbRectangle> newFrameBuffer = new Queue<RfbRectangle>(); //The FramebufferQueue
        private UInt16[] _LargestFrame = new UInt16[2] {0, 0}; //The largest known frame (x/Y)
        private BackgroundWorker _Receiver; //The BackgroundWorkerthread for receiving Data
        private bool _IsConnected = false; //Is the Client connected?
        private DateTime _LastReceive = DateTime.Now; //The Timestamp when the last Received Frame happend
        private int _LastReceiveTimeout = 1000; //If no changes were made, a new Frame will be requested after x ms
		  //private DispatcherTimer _LastReceiveTimer = new DispatcherTimer(); //The timer, that requests new Frames

        private Dictionary<char, UInt32> _KeyCodes = new Dictionary<char, uint>(); //Dictionary for Key-Endcodings (see keys.csv)

        private TcpClient _Client; //TCP-Client for Serverconnection
        private NetworkStream _DataStream; //The Stream to read/write Data

        private int _BackBuffer2RawStride; //How many Bytes a Row have
		  //public byte[] _BackBuffer2PixelData; //The Backbuffer as a Bytearray
        private System.Windows.Media.PixelFormat _BackBuffer2PixelFormat = PixelFormats.Rgb24; //The Pixelformat of the Backbuffer

        #region Constructor

        /// <summary>
        /// Start connection on default port with no password
        /// </summary>
        /// <param name="server"></param>
        public RfbClient(string server)
        {
            if (prepareConnection(server, 5900, "") == false)
                Log(Logtype.User, "Connection to the Server " + Properties.Server + " failed.");
        }

        /// <summary>
        /// Start connection with no password
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        public RfbClient(string server, int port)
        {
            if (prepareConnection(server, port, "") == false)
                Log(Logtype.User, "Connection to the Server " + Properties.Server + " failed.");
        }

        /// <summary>
        /// Start a new connection
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="password"></param>
        public RfbClient(string server, int port, string password)
        {
            if (prepareConnection(server, port, password) == false)
                Log(Logtype.User, "Connection to the Server " + Properties.Server + " failed.");
        }

        private bool loadKeyDictionary()
        {
            try
            {
                //System.IO.StreamReader sR = new System.IO.StreamReader("keys.csv", Encoding.UTF8);
                //while (sR.Peek() >= 0)
                //{
                //    string[] line = sR.ReadLine().Split(';');
                //    if (line[2] == "59") //For Semicolon (bad thing in a CSV-File...)
                //        _KeyCodes.Add(';', 59);
                //    else
                //    {
                //        if (line[4].Length > 0)
                //        {
                //            if (!_KeyCodes.ContainsKey(line[4].ToCharArray()[0]))
                //                _KeyCodes.Add(line[4].ToCharArray()[0], Convert.ToUInt32(line[2]));
                //        }
                //    }
                //}
                //sR.Close();


                if (!_KeyCodes.ContainsKey(' ')) _KeyCodes.Add(' ', 32);
                if (!_KeyCodes.ContainsKey('!')) _KeyCodes.Add('!', 33);
                if (!_KeyCodes.ContainsKey('"')) _KeyCodes.Add('"', 34);
                if (!_KeyCodes.ContainsKey('#')) _KeyCodes.Add('#', 35);
                if (!_KeyCodes.ContainsKey('$')) _KeyCodes.Add('$', 36);
                if (!_KeyCodes.ContainsKey('%')) _KeyCodes.Add('%', 37);
                if (!_KeyCodes.ContainsKey('&')) _KeyCodes.Add('&', 38);
                if (!_KeyCodes.ContainsKey('\'')) _KeyCodes.Add('\'', 39);
                if (!_KeyCodes.ContainsKey('(')) _KeyCodes.Add('(', 40);
                if (!_KeyCodes.ContainsKey(')')) _KeyCodes.Add(')', 41);
                if (!_KeyCodes.ContainsKey('*')) _KeyCodes.Add('*', 42);
                if (!_KeyCodes.ContainsKey('+')) _KeyCodes.Add('+', 43);
                if (!_KeyCodes.ContainsKey(',')) _KeyCodes.Add(',', 44);
                if (!_KeyCodes.ContainsKey('-')) _KeyCodes.Add('-', 45);
                if (!_KeyCodes.ContainsKey('.')) _KeyCodes.Add('.', 46);
                if (!_KeyCodes.ContainsKey('.')) _KeyCodes.Add('.', 46);
                if (!_KeyCodes.ContainsKey('/')) _KeyCodes.Add('/', 47);
                if (!_KeyCodes.ContainsKey('0')) _KeyCodes.Add('0', 48);
                if (!_KeyCodes.ContainsKey('1')) _KeyCodes.Add('1', 49);
                if (!_KeyCodes.ContainsKey('2')) _KeyCodes.Add('2', 50);
                if (!_KeyCodes.ContainsKey('3')) _KeyCodes.Add('3', 51);
                if (!_KeyCodes.ContainsKey('4')) _KeyCodes.Add('4', 52);
                if (!_KeyCodes.ContainsKey('5')) _KeyCodes.Add('5', 53);
                if (!_KeyCodes.ContainsKey('6')) _KeyCodes.Add('6', 54);
                if (!_KeyCodes.ContainsKey('7')) _KeyCodes.Add('7', 55);
                if (!_KeyCodes.ContainsKey('8')) _KeyCodes.Add('8', 56);
                if (!_KeyCodes.ContainsKey('9')) _KeyCodes.Add('9', 57);
                if (!_KeyCodes.ContainsKey(':')) _KeyCodes.Add(':', 58);
                if (!_KeyCodes.ContainsKey(';')) _KeyCodes.Add(';', 59);
                if (!_KeyCodes.ContainsKey('<')) _KeyCodes.Add('<', 60);
                if (!_KeyCodes.ContainsKey('<')) _KeyCodes.Add('<', 60);
                if (!_KeyCodes.ContainsKey('=')) _KeyCodes.Add('=', 61);
                if (!_KeyCodes.ContainsKey('>')) _KeyCodes.Add('>', 62);
                if (!_KeyCodes.ContainsKey('>')) _KeyCodes.Add('>', 62);
                if (!_KeyCodes.ContainsKey('?')) _KeyCodes.Add('?', 63);
                if (!_KeyCodes.ContainsKey('@')) _KeyCodes.Add('@', 64);
                if (!_KeyCodes.ContainsKey('A')) _KeyCodes.Add('A', 65);
                if (!_KeyCodes.ContainsKey('B')) _KeyCodes.Add('B', 66);
                if (!_KeyCodes.ContainsKey('C')) _KeyCodes.Add('C', 67);
                if (!_KeyCodes.ContainsKey('D')) _KeyCodes.Add('D', 68);
                if (!_KeyCodes.ContainsKey('E')) _KeyCodes.Add('E', 69);
                if (!_KeyCodes.ContainsKey('F')) _KeyCodes.Add('F', 70);
                if (!_KeyCodes.ContainsKey('G')) _KeyCodes.Add('G', 71);
                if (!_KeyCodes.ContainsKey('H')) _KeyCodes.Add('H', 72);
                if (!_KeyCodes.ContainsKey('I')) _KeyCodes.Add('I', 73);
                if (!_KeyCodes.ContainsKey('J')) _KeyCodes.Add('J', 74);
                if (!_KeyCodes.ContainsKey('K')) _KeyCodes.Add('K', 75);
                if (!_KeyCodes.ContainsKey('L')) _KeyCodes.Add('L', 76);
                if (!_KeyCodes.ContainsKey('M')) _KeyCodes.Add('M', 77);
                if (!_KeyCodes.ContainsKey('N')) _KeyCodes.Add('N', 78);
                if (!_KeyCodes.ContainsKey('O')) _KeyCodes.Add('O', 79);
                if (!_KeyCodes.ContainsKey('P')) _KeyCodes.Add('P', 80);
                if (!_KeyCodes.ContainsKey('Q')) _KeyCodes.Add('Q', 81);
                if (!_KeyCodes.ContainsKey('R')) _KeyCodes.Add('R', 82);
                if (!_KeyCodes.ContainsKey('S')) _KeyCodes.Add('S', 83);
                if (!_KeyCodes.ContainsKey('T')) _KeyCodes.Add('T', 84);
                if (!_KeyCodes.ContainsKey('U')) _KeyCodes.Add('U', 85);
                if (!_KeyCodes.ContainsKey('V')) _KeyCodes.Add('V', 86);
                if (!_KeyCodes.ContainsKey('W')) _KeyCodes.Add('W', 87);
                if (!_KeyCodes.ContainsKey('X')) _KeyCodes.Add('X', 88);
                if (!_KeyCodes.ContainsKey('Y')) _KeyCodes.Add('Y', 89);
                if (!_KeyCodes.ContainsKey('Z')) _KeyCodes.Add('Z', 90);
                if (!_KeyCodes.ContainsKey('[')) _KeyCodes.Add('[', 91);
                if (!_KeyCodes.ContainsKey('\\')) _KeyCodes.Add('\\', 92);
                if (!_KeyCodes.ContainsKey(']')) _KeyCodes.Add(']', 93);
                if (!_KeyCodes.ContainsKey('^')) _KeyCodes.Add('^', 94);
                if (!_KeyCodes.ContainsKey('_')) _KeyCodes.Add('_', 95);
                if (!_KeyCodes.ContainsKey('_')) _KeyCodes.Add('_', 95);
                if (!_KeyCodes.ContainsKey('`')) _KeyCodes.Add('`', 96);
                if (!_KeyCodes.ContainsKey('a')) _KeyCodes.Add('a', 97);
                if (!_KeyCodes.ContainsKey('b')) _KeyCodes.Add('b', 98);
                if (!_KeyCodes.ContainsKey('c')) _KeyCodes.Add('c', 99);
                if (!_KeyCodes.ContainsKey('d')) _KeyCodes.Add('d', 100);
                if (!_KeyCodes.ContainsKey('e')) _KeyCodes.Add('e', 101);
                if (!_KeyCodes.ContainsKey('f')) _KeyCodes.Add('f', 102);
                if (!_KeyCodes.ContainsKey('g')) _KeyCodes.Add('g', 103);
                if (!_KeyCodes.ContainsKey('h')) _KeyCodes.Add('h', 104);
                if (!_KeyCodes.ContainsKey('i')) _KeyCodes.Add('i', 105);
                if (!_KeyCodes.ContainsKey('j')) _KeyCodes.Add('j', 106);
                if (!_KeyCodes.ContainsKey('k')) _KeyCodes.Add('k', 107);
                if (!_KeyCodes.ContainsKey('l')) _KeyCodes.Add('l', 108);
                if (!_KeyCodes.ContainsKey('m')) _KeyCodes.Add('m', 109);
                if (!_KeyCodes.ContainsKey('n')) _KeyCodes.Add('n', 110);
                if (!_KeyCodes.ContainsKey('o')) _KeyCodes.Add('o', 111);
                if (!_KeyCodes.ContainsKey('p')) _KeyCodes.Add('p', 112);
                if (!_KeyCodes.ContainsKey('q')) _KeyCodes.Add('q', 113);
                if (!_KeyCodes.ContainsKey('r')) _KeyCodes.Add('r', 114);
                if (!_KeyCodes.ContainsKey('s')) _KeyCodes.Add('s', 115);
                if (!_KeyCodes.ContainsKey('t')) _KeyCodes.Add('t', 116);
                if (!_KeyCodes.ContainsKey('u')) _KeyCodes.Add('u', 117);
                if (!_KeyCodes.ContainsKey('v')) _KeyCodes.Add('v', 118);
                if (!_KeyCodes.ContainsKey('w')) _KeyCodes.Add('w', 119);
                if (!_KeyCodes.ContainsKey('x')) _KeyCodes.Add('x', 120);
                if (!_KeyCodes.ContainsKey('y')) _KeyCodes.Add('y', 121);
                if (!_KeyCodes.ContainsKey('z')) _KeyCodes.Add('z', 122);
                if (!_KeyCodes.ContainsKey('{')) _KeyCodes.Add('{', 123);
                if (!_KeyCodes.ContainsKey('|')) _KeyCodes.Add('|', 124);
                if (!_KeyCodes.ContainsKey('}')) _KeyCodes.Add('}', 125);
                if (!_KeyCodes.ContainsKey('~')) _KeyCodes.Add('~', 126);
                if (!_KeyCodes.ContainsKey(' ')) _KeyCodes.Add(' ', 160);
                if (!_KeyCodes.ContainsKey('¡')) _KeyCodes.Add('¡', 161);
                if (!_KeyCodes.ContainsKey('¢')) _KeyCodes.Add('¢', 162);
                if (!_KeyCodes.ContainsKey('£')) _KeyCodes.Add('£', 163);
                if (!_KeyCodes.ContainsKey('¤')) _KeyCodes.Add('¤', 164);
                if (!_KeyCodes.ContainsKey('¥')) _KeyCodes.Add('¥', 165);
                if (!_KeyCodes.ContainsKey('¦')) _KeyCodes.Add('¦', 166);
                if (!_KeyCodes.ContainsKey('§')) _KeyCodes.Add('§', 167);
                if (!_KeyCodes.ContainsKey('¨')) _KeyCodes.Add('¨', 168);
                if (!_KeyCodes.ContainsKey('©')) _KeyCodes.Add('©', 169);
                if (!_KeyCodes.ContainsKey('ª')) _KeyCodes.Add('ª', 170);
                if (!_KeyCodes.ContainsKey('«')) _KeyCodes.Add('«', 171);
                if (!_KeyCodes.ContainsKey('¬')) _KeyCodes.Add('¬', 172);
                if (!_KeyCodes.ContainsKey('­')) _KeyCodes.Add('­', 173);
                if (!_KeyCodes.ContainsKey('®')) _KeyCodes.Add('®', 174);
                if (!_KeyCodes.ContainsKey('¯')) _KeyCodes.Add('¯', 175);
                if (!_KeyCodes.ContainsKey('¯')) _KeyCodes.Add('¯', 175);
                if (!_KeyCodes.ContainsKey('°')) _KeyCodes.Add('°', 176);
                if (!_KeyCodes.ContainsKey('±')) _KeyCodes.Add('±', 177);
                if (!_KeyCodes.ContainsKey('²')) _KeyCodes.Add('²', 178);
                if (!_KeyCodes.ContainsKey('³')) _KeyCodes.Add('³', 179);
                if (!_KeyCodes.ContainsKey('´')) _KeyCodes.Add('´', 180);
                if (!_KeyCodes.ContainsKey('µ')) _KeyCodes.Add('µ', 181);
                if (!_KeyCodes.ContainsKey('¶')) _KeyCodes.Add('¶', 182);
                if (!_KeyCodes.ContainsKey('·')) _KeyCodes.Add('·', 183);
                if (!_KeyCodes.ContainsKey('¸')) _KeyCodes.Add('¸', 184);
                if (!_KeyCodes.ContainsKey('¹')) _KeyCodes.Add('¹', 185);
                if (!_KeyCodes.ContainsKey('º')) _KeyCodes.Add('º', 186);
                if (!_KeyCodes.ContainsKey('»')) _KeyCodes.Add('»', 187);
                if (!_KeyCodes.ContainsKey('¼')) _KeyCodes.Add('¼', 188);
                if (!_KeyCodes.ContainsKey('½')) _KeyCodes.Add('½', 189);
                if (!_KeyCodes.ContainsKey('¾')) _KeyCodes.Add('¾', 190);
                if (!_KeyCodes.ContainsKey('¿')) _KeyCodes.Add('¿', 191);
                if (!_KeyCodes.ContainsKey('À')) _KeyCodes.Add('À', 192);
                if (!_KeyCodes.ContainsKey('Á')) _KeyCodes.Add('Á', 193);
                if (!_KeyCodes.ContainsKey('Â')) _KeyCodes.Add('Â', 194);
                if (!_KeyCodes.ContainsKey('Ã')) _KeyCodes.Add('Ã', 195);
                if (!_KeyCodes.ContainsKey('Ä')) _KeyCodes.Add('Ä', 196);
                if (!_KeyCodes.ContainsKey('Å')) _KeyCodes.Add('Å', 197);
                if (!_KeyCodes.ContainsKey('Æ')) _KeyCodes.Add('Æ', 198);
                if (!_KeyCodes.ContainsKey('Ç')) _KeyCodes.Add('Ç', 199);
                if (!_KeyCodes.ContainsKey('È')) _KeyCodes.Add('È', 200);
                if (!_KeyCodes.ContainsKey('É')) _KeyCodes.Add('É', 201);
                if (!_KeyCodes.ContainsKey('Ê')) _KeyCodes.Add('Ê', 202);
                if (!_KeyCodes.ContainsKey('Ë')) _KeyCodes.Add('Ë', 203);
                if (!_KeyCodes.ContainsKey('Ì')) _KeyCodes.Add('Ì', 204);
                if (!_KeyCodes.ContainsKey('Í')) _KeyCodes.Add('Í', 205);
                if (!_KeyCodes.ContainsKey('Î')) _KeyCodes.Add('Î', 206);
                if (!_KeyCodes.ContainsKey('Ï')) _KeyCodes.Add('Ï', 207);
                if (!_KeyCodes.ContainsKey('Ð')) _KeyCodes.Add('Ð', 208);
                if (!_KeyCodes.ContainsKey('Ñ')) _KeyCodes.Add('Ñ', 209);
                if (!_KeyCodes.ContainsKey('Ò')) _KeyCodes.Add('Ò', 210);
                if (!_KeyCodes.ContainsKey('Ó')) _KeyCodes.Add('Ó', 211);
                if (!_KeyCodes.ContainsKey('Ô')) _KeyCodes.Add('Ô', 212);
                if (!_KeyCodes.ContainsKey('Õ')) _KeyCodes.Add('Õ', 213);
                if (!_KeyCodes.ContainsKey('Ö')) _KeyCodes.Add('Ö', 214);
                if (!_KeyCodes.ContainsKey('×')) _KeyCodes.Add('×', 215);
                if (!_KeyCodes.ContainsKey('Ø')) _KeyCodes.Add('Ø', 216);
                if (!_KeyCodes.ContainsKey('Ø')) _KeyCodes.Add('Ø', 216);
                if (!_KeyCodes.ContainsKey('Ù')) _KeyCodes.Add('Ù', 217);
                if (!_KeyCodes.ContainsKey('Ú')) _KeyCodes.Add('Ú', 218);
                if (!_KeyCodes.ContainsKey('Û')) _KeyCodes.Add('Û', 219);
                if (!_KeyCodes.ContainsKey('Ü')) _KeyCodes.Add('Ü', 220);
                if (!_KeyCodes.ContainsKey('Ý')) _KeyCodes.Add('Ý', 221);
                if (!_KeyCodes.ContainsKey('Þ')) _KeyCodes.Add('Þ', 222);
                if (!_KeyCodes.ContainsKey('ß')) _KeyCodes.Add('ß', 223);
                if (!_KeyCodes.ContainsKey('à')) _KeyCodes.Add('à', 224);
                if (!_KeyCodes.ContainsKey('á')) _KeyCodes.Add('á', 225);
                if (!_KeyCodes.ContainsKey('â')) _KeyCodes.Add('â', 226);
                if (!_KeyCodes.ContainsKey('ã')) _KeyCodes.Add('ã', 227);
                if (!_KeyCodes.ContainsKey('ä')) _KeyCodes.Add('ä', 228);
                if (!_KeyCodes.ContainsKey('å')) _KeyCodes.Add('å', 229);
                if (!_KeyCodes.ContainsKey('æ')) _KeyCodes.Add('æ', 230);
                if (!_KeyCodes.ContainsKey('ç')) _KeyCodes.Add('ç', 231);
                if (!_KeyCodes.ContainsKey('è')) _KeyCodes.Add('è', 232);
                if (!_KeyCodes.ContainsKey('é')) _KeyCodes.Add('é', 233);
                if (!_KeyCodes.ContainsKey('ê')) _KeyCodes.Add('ê', 234);
                if (!_KeyCodes.ContainsKey('ë')) _KeyCodes.Add('ë', 235);
                if (!_KeyCodes.ContainsKey('ì')) _KeyCodes.Add('ì', 236);
                if (!_KeyCodes.ContainsKey('í')) _KeyCodes.Add('í', 237);
                if (!_KeyCodes.ContainsKey('î')) _KeyCodes.Add('î', 238);
                if (!_KeyCodes.ContainsKey('ï')) _KeyCodes.Add('ï', 239);
                if (!_KeyCodes.ContainsKey('ð')) _KeyCodes.Add('ð', 240);
                if (!_KeyCodes.ContainsKey('ñ')) _KeyCodes.Add('ñ', 241);
                if (!_KeyCodes.ContainsKey('ò')) _KeyCodes.Add('ò', 242);
                if (!_KeyCodes.ContainsKey('ó')) _KeyCodes.Add('ó', 243);
                if (!_KeyCodes.ContainsKey('ô')) _KeyCodes.Add('ô', 244);
                if (!_KeyCodes.ContainsKey('õ')) _KeyCodes.Add('õ', 245);
                if (!_KeyCodes.ContainsKey('ö')) _KeyCodes.Add('ö', 246);
                if (!_KeyCodes.ContainsKey('÷')) _KeyCodes.Add('÷', 247);
                if (!_KeyCodes.ContainsKey('ø')) _KeyCodes.Add('ø', 248);
                if (!_KeyCodes.ContainsKey('ø')) _KeyCodes.Add('ø', 248);
                if (!_KeyCodes.ContainsKey('ù')) _KeyCodes.Add('ù', 249);
                if (!_KeyCodes.ContainsKey('ú')) _KeyCodes.Add('ú', 250);
                if (!_KeyCodes.ContainsKey('û')) _KeyCodes.Add('û', 251);
                if (!_KeyCodes.ContainsKey('ü')) _KeyCodes.Add('ü', 252);
                if (!_KeyCodes.ContainsKey('ý')) _KeyCodes.Add('ý', 253);
                if (!_KeyCodes.ContainsKey('þ')) _KeyCodes.Add('þ', 254);
                if (!_KeyCodes.ContainsKey('ÿ')) _KeyCodes.Add('ÿ', 255);
                if (!_KeyCodes.ContainsKey('Ā')) _KeyCodes.Add('Ā', 256);
                if (!_KeyCodes.ContainsKey('ā')) _KeyCodes.Add('ā', 257);
                if (!_KeyCodes.ContainsKey('Ă')) _KeyCodes.Add('Ă', 258);
                if (!_KeyCodes.ContainsKey('ă')) _KeyCodes.Add('ă', 259);
                if (!_KeyCodes.ContainsKey('Ą')) _KeyCodes.Add('Ą', 260);
                if (!_KeyCodes.ContainsKey('ą')) _KeyCodes.Add('ą', 261);
                if (!_KeyCodes.ContainsKey('Ć')) _KeyCodes.Add('Ć', 262);
                if (!_KeyCodes.ContainsKey('ć')) _KeyCodes.Add('ć', 263);
                if (!_KeyCodes.ContainsKey('Ĉ')) _KeyCodes.Add('Ĉ', 264);
                if (!_KeyCodes.ContainsKey('ĉ')) _KeyCodes.Add('ĉ', 265);
                if (!_KeyCodes.ContainsKey('Ċ')) _KeyCodes.Add('Ċ', 266);
                if (!_KeyCodes.ContainsKey('ċ')) _KeyCodes.Add('ċ', 267);
                if (!_KeyCodes.ContainsKey('Č')) _KeyCodes.Add('Č', 268);
                if (!_KeyCodes.ContainsKey('č')) _KeyCodes.Add('č', 269);
                if (!_KeyCodes.ContainsKey('Ď')) _KeyCodes.Add('Ď', 270);
                if (!_KeyCodes.ContainsKey('ď')) _KeyCodes.Add('ď', 271);
                if (!_KeyCodes.ContainsKey('Đ')) _KeyCodes.Add('Đ', 272);
                if (!_KeyCodes.ContainsKey('đ')) _KeyCodes.Add('đ', 273);
                if (!_KeyCodes.ContainsKey('Ē')) _KeyCodes.Add('Ē', 274);
                if (!_KeyCodes.ContainsKey('ē')) _KeyCodes.Add('ē', 275);
                if (!_KeyCodes.ContainsKey('Ė')) _KeyCodes.Add('Ė', 278);
                if (!_KeyCodes.ContainsKey('ė')) _KeyCodes.Add('ė', 279);
                if (!_KeyCodes.ContainsKey('Ę')) _KeyCodes.Add('Ę', 280);
                if (!_KeyCodes.ContainsKey('ę')) _KeyCodes.Add('ę', 281);
                if (!_KeyCodes.ContainsKey('Ě')) _KeyCodes.Add('Ě', 282);
                if (!_KeyCodes.ContainsKey('ě')) _KeyCodes.Add('ě', 283);
                if (!_KeyCodes.ContainsKey('Ĝ')) _KeyCodes.Add('Ĝ', 284);
                if (!_KeyCodes.ContainsKey('ĝ')) _KeyCodes.Add('ĝ', 285);
                if (!_KeyCodes.ContainsKey('Ğ')) _KeyCodes.Add('Ğ', 286);
                if (!_KeyCodes.ContainsKey('ğ')) _KeyCodes.Add('ğ', 287);
                if (!_KeyCodes.ContainsKey('Ġ')) _KeyCodes.Add('Ġ', 288);
                if (!_KeyCodes.ContainsKey('ġ')) _KeyCodes.Add('ġ', 289);
                if (!_KeyCodes.ContainsKey('Ģ')) _KeyCodes.Add('Ģ', 290);
                if (!_KeyCodes.ContainsKey('ģ')) _KeyCodes.Add('ģ', 291);
                if (!_KeyCodes.ContainsKey('Ĥ')) _KeyCodes.Add('Ĥ', 292);
                if (!_KeyCodes.ContainsKey('ĥ')) _KeyCodes.Add('ĥ', 293);
                if (!_KeyCodes.ContainsKey('Ħ')) _KeyCodes.Add('Ħ', 294);
                if (!_KeyCodes.ContainsKey('ħ')) _KeyCodes.Add('ħ', 295);
                if (!_KeyCodes.ContainsKey('Ĩ')) _KeyCodes.Add('Ĩ', 296);
                if (!_KeyCodes.ContainsKey('ĩ')) _KeyCodes.Add('ĩ', 297);
                if (!_KeyCodes.ContainsKey('Ī')) _KeyCodes.Add('Ī', 298);
                if (!_KeyCodes.ContainsKey('ī')) _KeyCodes.Add('ī', 299);
                if (!_KeyCodes.ContainsKey('Ĭ')) _KeyCodes.Add('Ĭ', 300);
                if (!_KeyCodes.ContainsKey('ĭ')) _KeyCodes.Add('ĭ', 301);
                if (!_KeyCodes.ContainsKey('Į')) _KeyCodes.Add('Į', 302);
                if (!_KeyCodes.ContainsKey('į')) _KeyCodes.Add('į', 303);
                if (!_KeyCodes.ContainsKey('İ')) _KeyCodes.Add('İ', 304);
                if (!_KeyCodes.ContainsKey('ı')) _KeyCodes.Add('ı', 305);
                if (!_KeyCodes.ContainsKey('Ĵ')) _KeyCodes.Add('Ĵ', 308);
                if (!_KeyCodes.ContainsKey('ĵ')) _KeyCodes.Add('ĵ', 309);
                if (!_KeyCodes.ContainsKey('Ķ')) _KeyCodes.Add('Ķ', 310);
                if (!_KeyCodes.ContainsKey('ķ')) _KeyCodes.Add('ķ', 311);
                if (!_KeyCodes.ContainsKey('ĸ')) _KeyCodes.Add('ĸ', 312);
                if (!_KeyCodes.ContainsKey('Ĺ')) _KeyCodes.Add('Ĺ', 313);
                if (!_KeyCodes.ContainsKey('ĺ')) _KeyCodes.Add('ĺ', 314);
                if (!_KeyCodes.ContainsKey('Ļ')) _KeyCodes.Add('Ļ', 315);
                if (!_KeyCodes.ContainsKey('ļ')) _KeyCodes.Add('ļ', 316);
                if (!_KeyCodes.ContainsKey('Ľ')) _KeyCodes.Add('Ľ', 317);
                if (!_KeyCodes.ContainsKey('ľ')) _KeyCodes.Add('ľ', 318);
                if (!_KeyCodes.ContainsKey('Ł')) _KeyCodes.Add('Ł', 321);
                if (!_KeyCodes.ContainsKey('ł')) _KeyCodes.Add('ł', 322);
                if (!_KeyCodes.ContainsKey('Ń')) _KeyCodes.Add('Ń', 323);
                if (!_KeyCodes.ContainsKey('ń')) _KeyCodes.Add('ń', 324);
                if (!_KeyCodes.ContainsKey('Ņ')) _KeyCodes.Add('Ņ', 325);
                if (!_KeyCodes.ContainsKey('ņ')) _KeyCodes.Add('ņ', 326);
                if (!_KeyCodes.ContainsKey('Ň')) _KeyCodes.Add('Ň', 327);
                if (!_KeyCodes.ContainsKey('ň')) _KeyCodes.Add('ň', 328);
                if (!_KeyCodes.ContainsKey('Ŋ')) _KeyCodes.Add('Ŋ', 330);
                if (!_KeyCodes.ContainsKey('ŋ')) _KeyCodes.Add('ŋ', 331);
                if (!_KeyCodes.ContainsKey('Ō')) _KeyCodes.Add('Ō', 332);
                if (!_KeyCodes.ContainsKey('ō')) _KeyCodes.Add('ō', 333);
                if (!_KeyCodes.ContainsKey('Ő')) _KeyCodes.Add('Ő', 336);
                if (!_KeyCodes.ContainsKey('ő')) _KeyCodes.Add('ő', 337);
                if (!_KeyCodes.ContainsKey('Œ')) _KeyCodes.Add('Œ', 338);
                if (!_KeyCodes.ContainsKey('œ')) _KeyCodes.Add('œ', 339);
                if (!_KeyCodes.ContainsKey('Ŕ')) _KeyCodes.Add('Ŕ', 340);
                if (!_KeyCodes.ContainsKey('ŕ')) _KeyCodes.Add('ŕ', 341);
                if (!_KeyCodes.ContainsKey('Ŗ')) _KeyCodes.Add('Ŗ', 342);
                if (!_KeyCodes.ContainsKey('ŗ')) _KeyCodes.Add('ŗ', 343);
                if (!_KeyCodes.ContainsKey('Ř')) _KeyCodes.Add('Ř', 344);
                if (!_KeyCodes.ContainsKey('ř')) _KeyCodes.Add('ř', 345);
                if (!_KeyCodes.ContainsKey('Ś')) _KeyCodes.Add('Ś', 346);
                if (!_KeyCodes.ContainsKey('ś')) _KeyCodes.Add('ś', 347);
                if (!_KeyCodes.ContainsKey('Ŝ')) _KeyCodes.Add('Ŝ', 348);
                if (!_KeyCodes.ContainsKey('ŝ')) _KeyCodes.Add('ŝ', 349);
                if (!_KeyCodes.ContainsKey('Ş')) _KeyCodes.Add('Ş', 350);
                if (!_KeyCodes.ContainsKey('ş')) _KeyCodes.Add('ş', 351);
                if (!_KeyCodes.ContainsKey('Š')) _KeyCodes.Add('Š', 352);
                if (!_KeyCodes.ContainsKey('š')) _KeyCodes.Add('š', 353);
                if (!_KeyCodes.ContainsKey('Ţ')) _KeyCodes.Add('Ţ', 354);
                if (!_KeyCodes.ContainsKey('ţ')) _KeyCodes.Add('ţ', 355);
                if (!_KeyCodes.ContainsKey('Ť')) _KeyCodes.Add('Ť', 356);
                if (!_KeyCodes.ContainsKey('ť')) _KeyCodes.Add('ť', 357);
                if (!_KeyCodes.ContainsKey('Ŧ')) _KeyCodes.Add('Ŧ', 358);
                if (!_KeyCodes.ContainsKey('ŧ')) _KeyCodes.Add('ŧ', 359);
                if (!_KeyCodes.ContainsKey('Ũ')) _KeyCodes.Add('Ũ', 360);
                if (!_KeyCodes.ContainsKey('ũ')) _KeyCodes.Add('ũ', 361);
                if (!_KeyCodes.ContainsKey('Ū')) _KeyCodes.Add('Ū', 362);
                if (!_KeyCodes.ContainsKey('ū')) _KeyCodes.Add('ū', 363);
                if (!_KeyCodes.ContainsKey('Ŭ')) _KeyCodes.Add('Ŭ', 364);
                if (!_KeyCodes.ContainsKey('ŭ')) _KeyCodes.Add('ŭ', 365);
                if (!_KeyCodes.ContainsKey('Ů')) _KeyCodes.Add('Ů', 366);
                if (!_KeyCodes.ContainsKey('ů')) _KeyCodes.Add('ů', 367);
                if (!_KeyCodes.ContainsKey('Ű')) _KeyCodes.Add('Ű', 368);
                if (!_KeyCodes.ContainsKey('ű')) _KeyCodes.Add('ű', 369);
                if (!_KeyCodes.ContainsKey('Ų')) _KeyCodes.Add('Ų', 370);
                if (!_KeyCodes.ContainsKey('ų')) _KeyCodes.Add('ų', 371);
                if (!_KeyCodes.ContainsKey('Ŵ')) _KeyCodes.Add('Ŵ', 372);
                if (!_KeyCodes.ContainsKey('ŵ')) _KeyCodes.Add('ŵ', 373);
                if (!_KeyCodes.ContainsKey('Ŷ')) _KeyCodes.Add('Ŷ', 374);
                if (!_KeyCodes.ContainsKey('ŷ')) _KeyCodes.Add('ŷ', 375);
                if (!_KeyCodes.ContainsKey('Ÿ')) _KeyCodes.Add('Ÿ', 376);
                if (!_KeyCodes.ContainsKey('Ź')) _KeyCodes.Add('Ź', 377);
                if (!_KeyCodes.ContainsKey('ź')) _KeyCodes.Add('ź', 378);
                if (!_KeyCodes.ContainsKey('Ż')) _KeyCodes.Add('Ż', 379);
                if (!_KeyCodes.ContainsKey('ż')) _KeyCodes.Add('ż', 380);
                if (!_KeyCodes.ContainsKey('Ž')) _KeyCodes.Add('Ž', 381);
                if (!_KeyCodes.ContainsKey('ž')) _KeyCodes.Add('ž', 382);
                if (!_KeyCodes.ContainsKey('Ə')) _KeyCodes.Add('Ə', 399);
                if (!_KeyCodes.ContainsKey('ƒ')) _KeyCodes.Add('ƒ', 402);
                if (!_KeyCodes.ContainsKey('Ɵ')) _KeyCodes.Add('Ɵ', 415);
                if (!_KeyCodes.ContainsKey('Ơ')) _KeyCodes.Add('Ơ', 416);
                if (!_KeyCodes.ContainsKey('ơ')) _KeyCodes.Add('ơ', 417);
                if (!_KeyCodes.ContainsKey('Ư')) _KeyCodes.Add('Ư', 431);
                if (!_KeyCodes.ContainsKey('ư')) _KeyCodes.Add('ư', 432);
                if (!_KeyCodes.ContainsKey('Ƶ')) _KeyCodes.Add('Ƶ', 437);
                if (!_KeyCodes.ContainsKey('ƶ')) _KeyCodes.Add('ƶ', 438);
                if (!_KeyCodes.ContainsKey('ǒ')) _KeyCodes.Add('ǒ', 466);
                if (!_KeyCodes.ContainsKey('ǒ')) _KeyCodes.Add('ǒ', 466);
                if (!_KeyCodes.ContainsKey('Ǧ')) _KeyCodes.Add('Ǧ', 486);
                if (!_KeyCodes.ContainsKey('ǧ')) _KeyCodes.Add('ǧ', 487);
                if (!_KeyCodes.ContainsKey('ə')) _KeyCodes.Add('ə', 601);
                if (!_KeyCodes.ContainsKey('ɵ')) _KeyCodes.Add('ɵ', 629);
                if (!_KeyCodes.ContainsKey('ˇ')) _KeyCodes.Add('ˇ', 711);
                if (!_KeyCodes.ContainsKey('˘')) _KeyCodes.Add('˘', 728);
                if (!_KeyCodes.ContainsKey('˙')) _KeyCodes.Add('˙', 729);
                if (!_KeyCodes.ContainsKey('˛')) _KeyCodes.Add('˛', 731);
                if (!_KeyCodes.ContainsKey('˝')) _KeyCodes.Add('˝', 733);
                if (!_KeyCodes.ContainsKey('΅')) _KeyCodes.Add('΅', 901);
                if (!_KeyCodes.ContainsKey('Ά')) _KeyCodes.Add('Ά', 902);
                if (!_KeyCodes.ContainsKey('Έ')) _KeyCodes.Add('Έ', 904);
                if (!_KeyCodes.ContainsKey('Ή')) _KeyCodes.Add('Ή', 905);
                if (!_KeyCodes.ContainsKey('Ί')) _KeyCodes.Add('Ί', 906);
                if (!_KeyCodes.ContainsKey('Ό')) _KeyCodes.Add('Ό', 908);
                if (!_KeyCodes.ContainsKey('Ύ')) _KeyCodes.Add('Ύ', 910);
                if (!_KeyCodes.ContainsKey('Ώ')) _KeyCodes.Add('Ώ', 911);
                if (!_KeyCodes.ContainsKey('ΐ')) _KeyCodes.Add('ΐ', 912);
                if (!_KeyCodes.ContainsKey('Α')) _KeyCodes.Add('Α', 913);
                if (!_KeyCodes.ContainsKey('Β')) _KeyCodes.Add('Β', 914);
                if (!_KeyCodes.ContainsKey('Γ')) _KeyCodes.Add('Γ', 915);
                if (!_KeyCodes.ContainsKey('Δ')) _KeyCodes.Add('Δ', 916);
                if (!_KeyCodes.ContainsKey('Ε')) _KeyCodes.Add('Ε', 917);
                if (!_KeyCodes.ContainsKey('Ζ')) _KeyCodes.Add('Ζ', 918);
                if (!_KeyCodes.ContainsKey('Η')) _KeyCodes.Add('Η', 919);
                if (!_KeyCodes.ContainsKey('Θ')) _KeyCodes.Add('Θ', 920);
                if (!_KeyCodes.ContainsKey('Ι')) _KeyCodes.Add('Ι', 921);
                if (!_KeyCodes.ContainsKey('Κ')) _KeyCodes.Add('Κ', 922);
                if (!_KeyCodes.ContainsKey('Λ')) _KeyCodes.Add('Λ', 923);
                if (!_KeyCodes.ContainsKey('Λ')) _KeyCodes.Add('Λ', 923);
                if (!_KeyCodes.ContainsKey('Μ')) _KeyCodes.Add('Μ', 924);
                if (!_KeyCodes.ContainsKey('Ν')) _KeyCodes.Add('Ν', 925);
                if (!_KeyCodes.ContainsKey('Ξ')) _KeyCodes.Add('Ξ', 926);
                if (!_KeyCodes.ContainsKey('Ο')) _KeyCodes.Add('Ο', 927);
                if (!_KeyCodes.ContainsKey('Π')) _KeyCodes.Add('Π', 928);
                if (!_KeyCodes.ContainsKey('Ρ')) _KeyCodes.Add('Ρ', 929);
                if (!_KeyCodes.ContainsKey('Σ')) _KeyCodes.Add('Σ', 931);
                if (!_KeyCodes.ContainsKey('Τ')) _KeyCodes.Add('Τ', 932);
                if (!_KeyCodes.ContainsKey('Υ')) _KeyCodes.Add('Υ', 933);
                if (!_KeyCodes.ContainsKey('Φ')) _KeyCodes.Add('Φ', 934);
                if (!_KeyCodes.ContainsKey('Χ')) _KeyCodes.Add('Χ', 935);
                if (!_KeyCodes.ContainsKey('Ψ')) _KeyCodes.Add('Ψ', 936);
                if (!_KeyCodes.ContainsKey('Ω')) _KeyCodes.Add('Ω', 937);
                if (!_KeyCodes.ContainsKey('Ϊ')) _KeyCodes.Add('Ϊ', 938);
                if (!_KeyCodes.ContainsKey('Ϋ')) _KeyCodes.Add('Ϋ', 939);
                if (!_KeyCodes.ContainsKey('ά')) _KeyCodes.Add('ά', 940);
                if (!_KeyCodes.ContainsKey('έ')) _KeyCodes.Add('έ', 941);
                if (!_KeyCodes.ContainsKey('ή')) _KeyCodes.Add('ή', 942);
                if (!_KeyCodes.ContainsKey('ί')) _KeyCodes.Add('ί', 943);
                if (!_KeyCodes.ContainsKey('ΰ')) _KeyCodes.Add('ΰ', 944);
                if (!_KeyCodes.ContainsKey('α')) _KeyCodes.Add('α', 945);
                if (!_KeyCodes.ContainsKey('β')) _KeyCodes.Add('β', 946);
                if (!_KeyCodes.ContainsKey('γ')) _KeyCodes.Add('γ', 947);
                if (!_KeyCodes.ContainsKey('δ')) _KeyCodes.Add('δ', 948);
                if (!_KeyCodes.ContainsKey('ε')) _KeyCodes.Add('ε', 949);
                if (!_KeyCodes.ContainsKey('ζ')) _KeyCodes.Add('ζ', 950);
                if (!_KeyCodes.ContainsKey('η')) _KeyCodes.Add('η', 951);
                if (!_KeyCodes.ContainsKey('θ')) _KeyCodes.Add('θ', 952);
                if (!_KeyCodes.ContainsKey('ι')) _KeyCodes.Add('ι', 953);
                if (!_KeyCodes.ContainsKey('κ')) _KeyCodes.Add('κ', 954);
                if (!_KeyCodes.ContainsKey('λ')) _KeyCodes.Add('λ', 955);
                if (!_KeyCodes.ContainsKey('λ')) _KeyCodes.Add('λ', 955);
                if (!_KeyCodes.ContainsKey('μ')) _KeyCodes.Add('μ', 956);
                if (!_KeyCodes.ContainsKey('ν')) _KeyCodes.Add('ν', 957);
                if (!_KeyCodes.ContainsKey('ξ')) _KeyCodes.Add('ξ', 958);
                if (!_KeyCodes.ContainsKey('ο')) _KeyCodes.Add('ο', 959);
                if (!_KeyCodes.ContainsKey('π')) _KeyCodes.Add('π', 960);
                if (!_KeyCodes.ContainsKey('ρ')) _KeyCodes.Add('ρ', 961);
                if (!_KeyCodes.ContainsKey('ς')) _KeyCodes.Add('ς', 962);
                if (!_KeyCodes.ContainsKey('σ')) _KeyCodes.Add('σ', 963);
                if (!_KeyCodes.ContainsKey('τ')) _KeyCodes.Add('τ', 964);
                if (!_KeyCodes.ContainsKey('υ')) _KeyCodes.Add('υ', 965);
                if (!_KeyCodes.ContainsKey('φ')) _KeyCodes.Add('φ', 966);
                if (!_KeyCodes.ContainsKey('χ')) _KeyCodes.Add('χ', 967);
                if (!_KeyCodes.ContainsKey('ψ')) _KeyCodes.Add('ψ', 968);
                if (!_KeyCodes.ContainsKey('ω')) _KeyCodes.Add('ω', 969);
                if (!_KeyCodes.ContainsKey('ϊ')) _KeyCodes.Add('ϊ', 970);
                if (!_KeyCodes.ContainsKey('ϋ')) _KeyCodes.Add('ϋ', 971);
                if (!_KeyCodes.ContainsKey('ό')) _KeyCodes.Add('ό', 972);
                if (!_KeyCodes.ContainsKey('ύ')) _KeyCodes.Add('ύ', 973);
                if (!_KeyCodes.ContainsKey('ώ')) _KeyCodes.Add('ώ', 974);
                if (!_KeyCodes.ContainsKey('Ё')) _KeyCodes.Add('Ё', 1025);
                if (!_KeyCodes.ContainsKey('Ђ')) _KeyCodes.Add('Ђ', 1026);
                if (!_KeyCodes.ContainsKey('Ѓ')) _KeyCodes.Add('Ѓ', 1027);
                if (!_KeyCodes.ContainsKey('Є')) _KeyCodes.Add('Є', 1028);
                if (!_KeyCodes.ContainsKey('Ѕ')) _KeyCodes.Add('Ѕ', 1029);
                if (!_KeyCodes.ContainsKey('І')) _KeyCodes.Add('І', 1030);
                if (!_KeyCodes.ContainsKey('Ї')) _KeyCodes.Add('Ї', 1031);
                if (!_KeyCodes.ContainsKey('Ј')) _KeyCodes.Add('Ј', 1032);
                if (!_KeyCodes.ContainsKey('Љ')) _KeyCodes.Add('Љ', 1033);
                if (!_KeyCodes.ContainsKey('Њ')) _KeyCodes.Add('Њ', 1034);
                if (!_KeyCodes.ContainsKey('Ћ')) _KeyCodes.Add('Ћ', 1035);
                if (!_KeyCodes.ContainsKey('Ќ')) _KeyCodes.Add('Ќ', 1036);
                if (!_KeyCodes.ContainsKey('Ў')) _KeyCodes.Add('Ў', 1038);
                if (!_KeyCodes.ContainsKey('Џ')) _KeyCodes.Add('Џ', 1039);
                if (!_KeyCodes.ContainsKey('А')) _KeyCodes.Add('А', 1040);
                if (!_KeyCodes.ContainsKey('Б')) _KeyCodes.Add('Б', 1041);
                if (!_KeyCodes.ContainsKey('В')) _KeyCodes.Add('В', 1042);
                if (!_KeyCodes.ContainsKey('Г')) _KeyCodes.Add('Г', 1043);
                if (!_KeyCodes.ContainsKey('Д')) _KeyCodes.Add('Д', 1044);
                if (!_KeyCodes.ContainsKey('Е')) _KeyCodes.Add('Е', 1045);
                if (!_KeyCodes.ContainsKey('Ж')) _KeyCodes.Add('Ж', 1046);
                if (!_KeyCodes.ContainsKey('З')) _KeyCodes.Add('З', 1047);
                if (!_KeyCodes.ContainsKey('И')) _KeyCodes.Add('И', 1048);
                if (!_KeyCodes.ContainsKey('Й')) _KeyCodes.Add('Й', 1049);
                if (!_KeyCodes.ContainsKey('К')) _KeyCodes.Add('К', 1050);
                if (!_KeyCodes.ContainsKey('Л')) _KeyCodes.Add('Л', 1051);
                if (!_KeyCodes.ContainsKey('М')) _KeyCodes.Add('М', 1052);
                if (!_KeyCodes.ContainsKey('Н')) _KeyCodes.Add('Н', 1053);
                if (!_KeyCodes.ContainsKey('О')) _KeyCodes.Add('О', 1054);
                if (!_KeyCodes.ContainsKey('П')) _KeyCodes.Add('П', 1055);
                if (!_KeyCodes.ContainsKey('Р')) _KeyCodes.Add('Р', 1056);
                if (!_KeyCodes.ContainsKey('С')) _KeyCodes.Add('С', 1057);
                if (!_KeyCodes.ContainsKey('Т')) _KeyCodes.Add('Т', 1058);
                if (!_KeyCodes.ContainsKey('У')) _KeyCodes.Add('У', 1059);
                if (!_KeyCodes.ContainsKey('Ф')) _KeyCodes.Add('Ф', 1060);
                if (!_KeyCodes.ContainsKey('Х')) _KeyCodes.Add('Х', 1061);
                if (!_KeyCodes.ContainsKey('Ц')) _KeyCodes.Add('Ц', 1062);
                if (!_KeyCodes.ContainsKey('Ч')) _KeyCodes.Add('Ч', 1063);
                if (!_KeyCodes.ContainsKey('Ш')) _KeyCodes.Add('Ш', 1064);
                if (!_KeyCodes.ContainsKey('Щ')) _KeyCodes.Add('Щ', 1065);
                if (!_KeyCodes.ContainsKey('Ъ')) _KeyCodes.Add('Ъ', 1066);
                if (!_KeyCodes.ContainsKey('Ы')) _KeyCodes.Add('Ы', 1067);
                if (!_KeyCodes.ContainsKey('Ь')) _KeyCodes.Add('Ь', 1068);
                if (!_KeyCodes.ContainsKey('Э')) _KeyCodes.Add('Э', 1069);
                if (!_KeyCodes.ContainsKey('Ю')) _KeyCodes.Add('Ю', 1070);
                if (!_KeyCodes.ContainsKey('Я')) _KeyCodes.Add('Я', 1071);
                if (!_KeyCodes.ContainsKey('а')) _KeyCodes.Add('а', 1072);
                if (!_KeyCodes.ContainsKey('б')) _KeyCodes.Add('б', 1073);
                if (!_KeyCodes.ContainsKey('в')) _KeyCodes.Add('в', 1074);
                if (!_KeyCodes.ContainsKey('г')) _KeyCodes.Add('г', 1075);
                if (!_KeyCodes.ContainsKey('д')) _KeyCodes.Add('д', 1076);
                if (!_KeyCodes.ContainsKey('е')) _KeyCodes.Add('е', 1077);
                if (!_KeyCodes.ContainsKey('ж')) _KeyCodes.Add('ж', 1078);
                if (!_KeyCodes.ContainsKey('з')) _KeyCodes.Add('з', 1079);
                if (!_KeyCodes.ContainsKey('и')) _KeyCodes.Add('и', 1080);
                if (!_KeyCodes.ContainsKey('й')) _KeyCodes.Add('й', 1081);
                if (!_KeyCodes.ContainsKey('к')) _KeyCodes.Add('к', 1082);
                if (!_KeyCodes.ContainsKey('л')) _KeyCodes.Add('л', 1083);
                if (!_KeyCodes.ContainsKey('м')) _KeyCodes.Add('м', 1084);
                if (!_KeyCodes.ContainsKey('н')) _KeyCodes.Add('н', 1085);
                if (!_KeyCodes.ContainsKey('о')) _KeyCodes.Add('о', 1086);
                if (!_KeyCodes.ContainsKey('п')) _KeyCodes.Add('п', 1087);
                if (!_KeyCodes.ContainsKey('р')) _KeyCodes.Add('р', 1088);
                if (!_KeyCodes.ContainsKey('с')) _KeyCodes.Add('с', 1089);
                if (!_KeyCodes.ContainsKey('т')) _KeyCodes.Add('т', 1090);
                if (!_KeyCodes.ContainsKey('у')) _KeyCodes.Add('у', 1091);
                if (!_KeyCodes.ContainsKey('ф')) _KeyCodes.Add('ф', 1092);
                if (!_KeyCodes.ContainsKey('х')) _KeyCodes.Add('х', 1093);
                if (!_KeyCodes.ContainsKey('ц')) _KeyCodes.Add('ц', 1094);
                if (!_KeyCodes.ContainsKey('ч')) _KeyCodes.Add('ч', 1095);
                if (!_KeyCodes.ContainsKey('ш')) _KeyCodes.Add('ш', 1096);
                if (!_KeyCodes.ContainsKey('щ')) _KeyCodes.Add('щ', 1097);
                if (!_KeyCodes.ContainsKey('ъ')) _KeyCodes.Add('ъ', 1098);
                if (!_KeyCodes.ContainsKey('ы')) _KeyCodes.Add('ы', 1099);
                if (!_KeyCodes.ContainsKey('ь')) _KeyCodes.Add('ь', 1100);
                if (!_KeyCodes.ContainsKey('э')) _KeyCodes.Add('э', 1101);
                if (!_KeyCodes.ContainsKey('ю')) _KeyCodes.Add('ю', 1102);
                if (!_KeyCodes.ContainsKey('я')) _KeyCodes.Add('я', 1103);
                if (!_KeyCodes.ContainsKey('ё')) _KeyCodes.Add('ё', 1105);
                if (!_KeyCodes.ContainsKey('ђ')) _KeyCodes.Add('ђ', 1106);
                if (!_KeyCodes.ContainsKey('ѓ')) _KeyCodes.Add('ѓ', 1107);
                if (!_KeyCodes.ContainsKey('є')) _KeyCodes.Add('є', 1108);
                if (!_KeyCodes.ContainsKey('ѕ')) _KeyCodes.Add('ѕ', 1109);
                if (!_KeyCodes.ContainsKey('і')) _KeyCodes.Add('і', 1110);
                if (!_KeyCodes.ContainsKey('ї')) _KeyCodes.Add('ї', 1111);
                if (!_KeyCodes.ContainsKey('ј')) _KeyCodes.Add('ј', 1112);
                if (!_KeyCodes.ContainsKey('љ')) _KeyCodes.Add('љ', 1113);
                if (!_KeyCodes.ContainsKey('њ')) _KeyCodes.Add('њ', 1114);
                if (!_KeyCodes.ContainsKey('ћ')) _KeyCodes.Add('ћ', 1115);
                if (!_KeyCodes.ContainsKey('ќ')) _KeyCodes.Add('ќ', 1116);
                if (!_KeyCodes.ContainsKey('ў')) _KeyCodes.Add('ў', 1118);
                if (!_KeyCodes.ContainsKey('џ')) _KeyCodes.Add('џ', 1119);
                if (!_KeyCodes.ContainsKey('Ґ')) _KeyCodes.Add('Ґ', 1168);
                if (!_KeyCodes.ContainsKey('ґ')) _KeyCodes.Add('ґ', 1169);
                if (!_KeyCodes.ContainsKey('Ғ')) _KeyCodes.Add('Ғ', 1170);
                if (!_KeyCodes.ContainsKey('ғ')) _KeyCodes.Add('ғ', 1171);
                if (!_KeyCodes.ContainsKey('Җ')) _KeyCodes.Add('Җ', 1174);
                if (!_KeyCodes.ContainsKey('җ')) _KeyCodes.Add('җ', 1175);
                if (!_KeyCodes.ContainsKey('Қ')) _KeyCodes.Add('Қ', 1178);
                if (!_KeyCodes.ContainsKey('қ')) _KeyCodes.Add('қ', 1179);
                if (!_KeyCodes.ContainsKey('Ҝ')) _KeyCodes.Add('Ҝ', 1180);
                if (!_KeyCodes.ContainsKey('ҝ')) _KeyCodes.Add('ҝ', 1181);
                if (!_KeyCodes.ContainsKey('Ң')) _KeyCodes.Add('Ң', 1186);
                if (!_KeyCodes.ContainsKey('ң')) _KeyCodes.Add('ң', 1187);
                if (!_KeyCodes.ContainsKey('Ү')) _KeyCodes.Add('Ү', 1198);
                if (!_KeyCodes.ContainsKey('ү')) _KeyCodes.Add('ү', 1199);
                if (!_KeyCodes.ContainsKey('Ұ')) _KeyCodes.Add('Ұ', 1200);
                if (!_KeyCodes.ContainsKey('ұ')) _KeyCodes.Add('ұ', 1201);
                if (!_KeyCodes.ContainsKey('Ҳ')) _KeyCodes.Add('Ҳ', 1202);
                if (!_KeyCodes.ContainsKey('ҳ')) _KeyCodes.Add('ҳ', 1203);
                if (!_KeyCodes.ContainsKey('Ҷ')) _KeyCodes.Add('Ҷ', 1206);
                if (!_KeyCodes.ContainsKey('ҷ')) _KeyCodes.Add('ҷ', 1207);
                if (!_KeyCodes.ContainsKey('Ҹ')) _KeyCodes.Add('Ҹ', 1208);
                if (!_KeyCodes.ContainsKey('ҹ')) _KeyCodes.Add('ҹ', 1209);
                if (!_KeyCodes.ContainsKey('Һ')) _KeyCodes.Add('Һ', 1210);
                if (!_KeyCodes.ContainsKey('һ')) _KeyCodes.Add('һ', 1211);
                if (!_KeyCodes.ContainsKey('Ә')) _KeyCodes.Add('Ә', 1240);
                if (!_KeyCodes.ContainsKey('ә')) _KeyCodes.Add('ә', 1241);
                if (!_KeyCodes.ContainsKey('Ӣ')) _KeyCodes.Add('Ӣ', 1250);
                if (!_KeyCodes.ContainsKey('ӣ')) _KeyCodes.Add('ӣ', 1251);
                if (!_KeyCodes.ContainsKey('Ө')) _KeyCodes.Add('Ө', 1256);
                if (!_KeyCodes.ContainsKey('ө')) _KeyCodes.Add('ө', 1257);
                if (!_KeyCodes.ContainsKey('Ӯ')) _KeyCodes.Add('Ӯ', 1262);
                if (!_KeyCodes.ContainsKey('ӯ')) _KeyCodes.Add('ӯ', 1263);
                if (!_KeyCodes.ContainsKey('Ա')) _KeyCodes.Add('Ա', 1329);
                if (!_KeyCodes.ContainsKey('Բ')) _KeyCodes.Add('Բ', 1330);
                if (!_KeyCodes.ContainsKey('Գ')) _KeyCodes.Add('Գ', 1331);
                if (!_KeyCodes.ContainsKey('Դ')) _KeyCodes.Add('Դ', 1332);
                if (!_KeyCodes.ContainsKey('Ե')) _KeyCodes.Add('Ե', 1333);
                if (!_KeyCodes.ContainsKey('Զ')) _KeyCodes.Add('Զ', 1334);
                if (!_KeyCodes.ContainsKey('Է')) _KeyCodes.Add('Է', 1335);
                if (!_KeyCodes.ContainsKey('Ը')) _KeyCodes.Add('Ը', 1336);
                if (!_KeyCodes.ContainsKey('Թ')) _KeyCodes.Add('Թ', 1337);
                if (!_KeyCodes.ContainsKey('Ժ')) _KeyCodes.Add('Ժ', 1338);
                if (!_KeyCodes.ContainsKey('Ի')) _KeyCodes.Add('Ի', 1339);
                if (!_KeyCodes.ContainsKey('Լ')) _KeyCodes.Add('Լ', 1340);
                if (!_KeyCodes.ContainsKey('Խ')) _KeyCodes.Add('Խ', 1341);
                if (!_KeyCodes.ContainsKey('Ծ')) _KeyCodes.Add('Ծ', 1342);
                if (!_KeyCodes.ContainsKey('Կ')) _KeyCodes.Add('Կ', 1343);
                if (!_KeyCodes.ContainsKey('Հ')) _KeyCodes.Add('Հ', 1344);
                if (!_KeyCodes.ContainsKey('Ձ')) _KeyCodes.Add('Ձ', 1345);
                if (!_KeyCodes.ContainsKey('Ղ')) _KeyCodes.Add('Ղ', 1346);
                if (!_KeyCodes.ContainsKey('Ճ')) _KeyCodes.Add('Ճ', 1347);
                if (!_KeyCodes.ContainsKey('Մ')) _KeyCodes.Add('Մ', 1348);
                if (!_KeyCodes.ContainsKey('Յ')) _KeyCodes.Add('Յ', 1349);
                if (!_KeyCodes.ContainsKey('Ն')) _KeyCodes.Add('Ն', 1350);
                if (!_KeyCodes.ContainsKey('Շ')) _KeyCodes.Add('Շ', 1351);
                if (!_KeyCodes.ContainsKey('Ո')) _KeyCodes.Add('Ո', 1352);
                if (!_KeyCodes.ContainsKey('Չ')) _KeyCodes.Add('Չ', 1353);
                if (!_KeyCodes.ContainsKey('Պ')) _KeyCodes.Add('Պ', 1354);
                if (!_KeyCodes.ContainsKey('Ջ')) _KeyCodes.Add('Ջ', 1355);
                if (!_KeyCodes.ContainsKey('Ռ')) _KeyCodes.Add('Ռ', 1356);
                if (!_KeyCodes.ContainsKey('Ս')) _KeyCodes.Add('Ս', 1357);
                if (!_KeyCodes.ContainsKey('Վ')) _KeyCodes.Add('Վ', 1358);
                if (!_KeyCodes.ContainsKey('Տ')) _KeyCodes.Add('Տ', 1359);
                if (!_KeyCodes.ContainsKey('Ր')) _KeyCodes.Add('Ր', 1360);
                if (!_KeyCodes.ContainsKey('Ց')) _KeyCodes.Add('Ց', 1361);
                if (!_KeyCodes.ContainsKey('Ւ')) _KeyCodes.Add('Ւ', 1362);
                if (!_KeyCodes.ContainsKey('Փ')) _KeyCodes.Add('Փ', 1363);
                if (!_KeyCodes.ContainsKey('Ք')) _KeyCodes.Add('Ք', 1364);
                if (!_KeyCodes.ContainsKey('Օ')) _KeyCodes.Add('Օ', 1365);
                if (!_KeyCodes.ContainsKey('Ֆ')) _KeyCodes.Add('Ֆ', 1366);
                if (!_KeyCodes.ContainsKey('՚')) _KeyCodes.Add('՚', 1370);
                if (!_KeyCodes.ContainsKey('՛')) _KeyCodes.Add('՛', 1371);
                if (!_KeyCodes.ContainsKey('՛')) _KeyCodes.Add('՛', 1371);
                if (!_KeyCodes.ContainsKey('՜')) _KeyCodes.Add('՜', 1372);
                if (!_KeyCodes.ContainsKey('՜')) _KeyCodes.Add('՜', 1372);
                if (!_KeyCodes.ContainsKey('՝')) _KeyCodes.Add('՝', 1373);
                if (!_KeyCodes.ContainsKey('՝')) _KeyCodes.Add('՝', 1373);
                if (!_KeyCodes.ContainsKey('՞')) _KeyCodes.Add('՞', 1374);
                if (!_KeyCodes.ContainsKey('՞')) _KeyCodes.Add('՞', 1374);
                if (!_KeyCodes.ContainsKey('ա')) _KeyCodes.Add('ա', 1377);
                if (!_KeyCodes.ContainsKey('բ')) _KeyCodes.Add('բ', 1378);
                if (!_KeyCodes.ContainsKey('գ')) _KeyCodes.Add('գ', 1379);
                if (!_KeyCodes.ContainsKey('դ')) _KeyCodes.Add('դ', 1380);
                if (!_KeyCodes.ContainsKey('ե')) _KeyCodes.Add('ե', 1381);
                if (!_KeyCodes.ContainsKey('զ')) _KeyCodes.Add('զ', 1382);
                if (!_KeyCodes.ContainsKey('է')) _KeyCodes.Add('է', 1383);
                if (!_KeyCodes.ContainsKey('ը')) _KeyCodes.Add('ը', 1384);
                if (!_KeyCodes.ContainsKey('թ')) _KeyCodes.Add('թ', 1385);
                if (!_KeyCodes.ContainsKey('ժ')) _KeyCodes.Add('ժ', 1386);
                if (!_KeyCodes.ContainsKey('ի')) _KeyCodes.Add('ի', 1387);
                if (!_KeyCodes.ContainsKey('լ')) _KeyCodes.Add('լ', 1388);
                if (!_KeyCodes.ContainsKey('խ')) _KeyCodes.Add('խ', 1389);
                if (!_KeyCodes.ContainsKey('ծ')) _KeyCodes.Add('ծ', 1390);
                if (!_KeyCodes.ContainsKey('կ')) _KeyCodes.Add('կ', 1391);
                if (!_KeyCodes.ContainsKey('հ')) _KeyCodes.Add('հ', 1392);
                if (!_KeyCodes.ContainsKey('ձ')) _KeyCodes.Add('ձ', 1393);
                if (!_KeyCodes.ContainsKey('ղ')) _KeyCodes.Add('ղ', 1394);
                if (!_KeyCodes.ContainsKey('ճ')) _KeyCodes.Add('ճ', 1395);
                if (!_KeyCodes.ContainsKey('մ')) _KeyCodes.Add('մ', 1396);
                if (!_KeyCodes.ContainsKey('յ')) _KeyCodes.Add('յ', 1397);
                if (!_KeyCodes.ContainsKey('ն')) _KeyCodes.Add('ն', 1398);
                if (!_KeyCodes.ContainsKey('շ')) _KeyCodes.Add('շ', 1399);
                if (!_KeyCodes.ContainsKey('ո')) _KeyCodes.Add('ո', 1400);
                if (!_KeyCodes.ContainsKey('չ')) _KeyCodes.Add('չ', 1401);
                if (!_KeyCodes.ContainsKey('պ')) _KeyCodes.Add('պ', 1402);
                if (!_KeyCodes.ContainsKey('ջ')) _KeyCodes.Add('ջ', 1403);
                if (!_KeyCodes.ContainsKey('ռ')) _KeyCodes.Add('ռ', 1404);
                if (!_KeyCodes.ContainsKey('ս')) _KeyCodes.Add('ս', 1405);
                if (!_KeyCodes.ContainsKey('վ')) _KeyCodes.Add('վ', 1406);
                if (!_KeyCodes.ContainsKey('տ')) _KeyCodes.Add('տ', 1407);
                if (!_KeyCodes.ContainsKey('ր')) _KeyCodes.Add('ր', 1408);
                if (!_KeyCodes.ContainsKey('ց')) _KeyCodes.Add('ց', 1409);
                if (!_KeyCodes.ContainsKey('ւ')) _KeyCodes.Add('ւ', 1410);
                if (!_KeyCodes.ContainsKey('փ')) _KeyCodes.Add('փ', 1411);
                if (!_KeyCodes.ContainsKey('ք')) _KeyCodes.Add('ք', 1412);
                if (!_KeyCodes.ContainsKey('օ')) _KeyCodes.Add('օ', 1413);
                if (!_KeyCodes.ContainsKey('ֆ')) _KeyCodes.Add('ֆ', 1414);
                if (!_KeyCodes.ContainsKey('և')) _KeyCodes.Add('և', 1415);
                if (!_KeyCodes.ContainsKey('։')) _KeyCodes.Add('։', 1417);
                if (!_KeyCodes.ContainsKey('։')) _KeyCodes.Add('։', 1417);
                if (!_KeyCodes.ContainsKey('֊')) _KeyCodes.Add('֊', 1418);
                if (!_KeyCodes.ContainsKey('֊')) _KeyCodes.Add('֊', 1418);
                if (!_KeyCodes.ContainsKey('א')) _KeyCodes.Add('א', 1488);
                if (!_KeyCodes.ContainsKey('ב')) _KeyCodes.Add('ב', 1489);
                if (!_KeyCodes.ContainsKey('ג')) _KeyCodes.Add('ג', 1490);
                if (!_KeyCodes.ContainsKey('ד')) _KeyCodes.Add('ד', 1491);
                if (!_KeyCodes.ContainsKey('ה')) _KeyCodes.Add('ה', 1492);
                if (!_KeyCodes.ContainsKey('ו')) _KeyCodes.Add('ו', 1493);
                if (!_KeyCodes.ContainsKey('ז')) _KeyCodes.Add('ז', 1494);
                if (!_KeyCodes.ContainsKey('ח')) _KeyCodes.Add('ח', 1495);
                if (!_KeyCodes.ContainsKey('ט')) _KeyCodes.Add('ט', 1496);
                if (!_KeyCodes.ContainsKey('י')) _KeyCodes.Add('י', 1497);
                if (!_KeyCodes.ContainsKey('ך')) _KeyCodes.Add('ך', 1498);
                if (!_KeyCodes.ContainsKey('כ')) _KeyCodes.Add('כ', 1499);
                if (!_KeyCodes.ContainsKey('ל')) _KeyCodes.Add('ל', 1500);
                if (!_KeyCodes.ContainsKey('ם')) _KeyCodes.Add('ם', 1501);
                if (!_KeyCodes.ContainsKey('מ')) _KeyCodes.Add('מ', 1502);
                if (!_KeyCodes.ContainsKey('ן')) _KeyCodes.Add('ן', 1503);
                if (!_KeyCodes.ContainsKey('נ')) _KeyCodes.Add('נ', 1504);
                if (!_KeyCodes.ContainsKey('ס')) _KeyCodes.Add('ס', 1505);
                if (!_KeyCodes.ContainsKey('ע')) _KeyCodes.Add('ע', 1506);
                if (!_KeyCodes.ContainsKey('ף')) _KeyCodes.Add('ף', 1507);
                if (!_KeyCodes.ContainsKey('פ')) _KeyCodes.Add('פ', 1508);
                if (!_KeyCodes.ContainsKey('ץ')) _KeyCodes.Add('ץ', 1509);
                if (!_KeyCodes.ContainsKey('צ')) _KeyCodes.Add('צ', 1510);
                if (!_KeyCodes.ContainsKey('ק')) _KeyCodes.Add('ק', 1511);
                if (!_KeyCodes.ContainsKey('ר')) _KeyCodes.Add('ר', 1512);
                if (!_KeyCodes.ContainsKey('ש')) _KeyCodes.Add('ש', 1513);
                if (!_KeyCodes.ContainsKey('ת')) _KeyCodes.Add('ת', 1514);
                if (!_KeyCodes.ContainsKey('،')) _KeyCodes.Add('،', 1548);
                if (!_KeyCodes.ContainsKey('؛')) _KeyCodes.Add('؛', 1563);
                if (!_KeyCodes.ContainsKey('؟')) _KeyCodes.Add('؟', 1567);
                if (!_KeyCodes.ContainsKey('ء')) _KeyCodes.Add('ء', 1569);
                if (!_KeyCodes.ContainsKey('آ')) _KeyCodes.Add('آ', 1570);
                if (!_KeyCodes.ContainsKey('أ')) _KeyCodes.Add('أ', 1571);
                if (!_KeyCodes.ContainsKey('ؤ')) _KeyCodes.Add('ؤ', 1572);
                if (!_KeyCodes.ContainsKey('إ')) _KeyCodes.Add('إ', 1573);
                if (!_KeyCodes.ContainsKey('ئ')) _KeyCodes.Add('ئ', 1574);
                if (!_KeyCodes.ContainsKey('ا')) _KeyCodes.Add('ا', 1575);
                if (!_KeyCodes.ContainsKey('ب')) _KeyCodes.Add('ب', 1576);
                if (!_KeyCodes.ContainsKey('ة')) _KeyCodes.Add('ة', 1577);
                if (!_KeyCodes.ContainsKey('ت')) _KeyCodes.Add('ت', 1578);
                if (!_KeyCodes.ContainsKey('ث')) _KeyCodes.Add('ث', 1579);
                if (!_KeyCodes.ContainsKey('ج')) _KeyCodes.Add('ج', 1580);
                if (!_KeyCodes.ContainsKey('ح')) _KeyCodes.Add('ح', 1581);
                if (!_KeyCodes.ContainsKey('خ')) _KeyCodes.Add('خ', 1582);
                if (!_KeyCodes.ContainsKey('د')) _KeyCodes.Add('د', 1583);
                if (!_KeyCodes.ContainsKey('ذ')) _KeyCodes.Add('ذ', 1584);
                if (!_KeyCodes.ContainsKey('ر')) _KeyCodes.Add('ر', 1585);
                if (!_KeyCodes.ContainsKey('ز')) _KeyCodes.Add('ز', 1586);
                if (!_KeyCodes.ContainsKey('س')) _KeyCodes.Add('س', 1587);
                if (!_KeyCodes.ContainsKey('ش')) _KeyCodes.Add('ش', 1588);
                if (!_KeyCodes.ContainsKey('ص')) _KeyCodes.Add('ص', 1589);
                if (!_KeyCodes.ContainsKey('ض')) _KeyCodes.Add('ض', 1590);
                if (!_KeyCodes.ContainsKey('ط')) _KeyCodes.Add('ط', 1591);
                if (!_KeyCodes.ContainsKey('ظ')) _KeyCodes.Add('ظ', 1592);
                if (!_KeyCodes.ContainsKey('ع')) _KeyCodes.Add('ع', 1593);
                if (!_KeyCodes.ContainsKey('غ')) _KeyCodes.Add('غ', 1594);
                if (!_KeyCodes.ContainsKey('ـ')) _KeyCodes.Add('ـ', 1600);
                if (!_KeyCodes.ContainsKey('ف')) _KeyCodes.Add('ف', 1601);
                if (!_KeyCodes.ContainsKey('ق')) _KeyCodes.Add('ق', 1602);
                if (!_KeyCodes.ContainsKey('ك')) _KeyCodes.Add('ك', 1603);
                if (!_KeyCodes.ContainsKey('ل')) _KeyCodes.Add('ل', 1604);
                if (!_KeyCodes.ContainsKey('م')) _KeyCodes.Add('م', 1605);
                if (!_KeyCodes.ContainsKey('ن')) _KeyCodes.Add('ن', 1606);
                if (!_KeyCodes.ContainsKey('ه')) _KeyCodes.Add('ه', 1607);
                if (!_KeyCodes.ContainsKey('و')) _KeyCodes.Add('و', 1608);
                if (!_KeyCodes.ContainsKey('ى')) _KeyCodes.Add('ى', 1609);
                if (!_KeyCodes.ContainsKey('ي')) _KeyCodes.Add('ي', 1610);
                if (!_KeyCodes.ContainsKey('ً')) _KeyCodes.Add('ً', 1611);
                if (!_KeyCodes.ContainsKey('ٌ')) _KeyCodes.Add('ٌ', 1612);
                if (!_KeyCodes.ContainsKey('ٍ')) _KeyCodes.Add('ٍ', 1613);
                if (!_KeyCodes.ContainsKey('َ')) _KeyCodes.Add('َ', 1614);
                if (!_KeyCodes.ContainsKey('ُ')) _KeyCodes.Add('ُ', 1615);
                if (!_KeyCodes.ContainsKey('ِ')) _KeyCodes.Add('ِ', 1616);
                if (!_KeyCodes.ContainsKey('ّ')) _KeyCodes.Add('ّ', 1617);
                if (!_KeyCodes.ContainsKey('ْ')) _KeyCodes.Add('ْ', 1618);
                if (!_KeyCodes.ContainsKey('ٓ')) _KeyCodes.Add('ٓ', 1619);
                if (!_KeyCodes.ContainsKey('ٔ')) _KeyCodes.Add('ٔ', 1620);
                if (!_KeyCodes.ContainsKey('ٕ')) _KeyCodes.Add('ٕ', 1621);
                if (!_KeyCodes.ContainsKey('0')) _KeyCodes.Add('0', 1632);
                if (!_KeyCodes.ContainsKey('1')) _KeyCodes.Add('1', 1633);
                if (!_KeyCodes.ContainsKey('2')) _KeyCodes.Add('2', 1634);
                if (!_KeyCodes.ContainsKey('3')) _KeyCodes.Add('3', 1635);
                if (!_KeyCodes.ContainsKey('4')) _KeyCodes.Add('4', 1636);
                if (!_KeyCodes.ContainsKey('5')) _KeyCodes.Add('5', 1637);
                if (!_KeyCodes.ContainsKey('6')) _KeyCodes.Add('6', 1638);
                if (!_KeyCodes.ContainsKey('7')) _KeyCodes.Add('7', 1639);
                if (!_KeyCodes.ContainsKey('8')) _KeyCodes.Add('8', 1640);
                if (!_KeyCodes.ContainsKey('9')) _KeyCodes.Add('9', 1641);
                if (!_KeyCodes.ContainsKey('٪')) _KeyCodes.Add('٪', 1642);
                if (!_KeyCodes.ContainsKey('ٰ')) _KeyCodes.Add('ٰ', 1648);
                if (!_KeyCodes.ContainsKey('ٹ')) _KeyCodes.Add('ٹ', 1657);
                if (!_KeyCodes.ContainsKey('پ')) _KeyCodes.Add('پ', 1662);
                if (!_KeyCodes.ContainsKey('چ')) _KeyCodes.Add('چ', 1670);
                if (!_KeyCodes.ContainsKey('ڈ')) _KeyCodes.Add('ڈ', 1672);
                if (!_KeyCodes.ContainsKey('ڑ')) _KeyCodes.Add('ڑ', 1681);
                if (!_KeyCodes.ContainsKey('ژ')) _KeyCodes.Add('ژ', 1688);
                if (!_KeyCodes.ContainsKey('ڤ')) _KeyCodes.Add('ڤ', 1700);
                if (!_KeyCodes.ContainsKey('ک')) _KeyCodes.Add('ک', 1705);
                if (!_KeyCodes.ContainsKey('گ')) _KeyCodes.Add('گ', 1711);
                if (!_KeyCodes.ContainsKey('ں')) _KeyCodes.Add('ں', 1722);
                if (!_KeyCodes.ContainsKey('ھ')) _KeyCodes.Add('ھ', 1726);
                if (!_KeyCodes.ContainsKey('ہ')) _KeyCodes.Add('ہ', 1729);
                if (!_KeyCodes.ContainsKey('ی')) _KeyCodes.Add('ی', 1740);
                if (!_KeyCodes.ContainsKey('ی')) _KeyCodes.Add('ی', 1740);
                if (!_KeyCodes.ContainsKey('ے')) _KeyCodes.Add('ے', 1746);
                if (!_KeyCodes.ContainsKey('۔')) _KeyCodes.Add('۔', 1748);
                if (!_KeyCodes.ContainsKey('0')) _KeyCodes.Add('0', 1776);
                if (!_KeyCodes.ContainsKey('1')) _KeyCodes.Add('1', 1777);
                if (!_KeyCodes.ContainsKey('2')) _KeyCodes.Add('2', 1778);
                if (!_KeyCodes.ContainsKey('3')) _KeyCodes.Add('3', 1779);
                if (!_KeyCodes.ContainsKey('4')) _KeyCodes.Add('4', 1780);
                if (!_KeyCodes.ContainsKey('5')) _KeyCodes.Add('5', 1781);
                if (!_KeyCodes.ContainsKey('6')) _KeyCodes.Add('6', 1782);
                if (!_KeyCodes.ContainsKey('7')) _KeyCodes.Add('7', 1783);
                if (!_KeyCodes.ContainsKey('8')) _KeyCodes.Add('8', 1784);
                if (!_KeyCodes.ContainsKey('9')) _KeyCodes.Add('9', 1785);
                if (!_KeyCodes.ContainsKey('ก')) _KeyCodes.Add('ก', 3585);
                if (!_KeyCodes.ContainsKey('ข')) _KeyCodes.Add('ข', 3586);
                if (!_KeyCodes.ContainsKey('ฃ')) _KeyCodes.Add('ฃ', 3587);
                if (!_KeyCodes.ContainsKey('ค')) _KeyCodes.Add('ค', 3588);
                if (!_KeyCodes.ContainsKey('ฅ')) _KeyCodes.Add('ฅ', 3589);
                if (!_KeyCodes.ContainsKey('ฆ')) _KeyCodes.Add('ฆ', 3590);
                if (!_KeyCodes.ContainsKey('ง')) _KeyCodes.Add('ง', 3591);
                if (!_KeyCodes.ContainsKey('จ')) _KeyCodes.Add('จ', 3592);
                if (!_KeyCodes.ContainsKey('ฉ')) _KeyCodes.Add('ฉ', 3593);
                if (!_KeyCodes.ContainsKey('ช')) _KeyCodes.Add('ช', 3594);
                if (!_KeyCodes.ContainsKey('ซ')) _KeyCodes.Add('ซ', 3595);
                if (!_KeyCodes.ContainsKey('ฌ')) _KeyCodes.Add('ฌ', 3596);
                if (!_KeyCodes.ContainsKey('ญ')) _KeyCodes.Add('ญ', 3597);
                if (!_KeyCodes.ContainsKey('ฎ')) _KeyCodes.Add('ฎ', 3598);
                if (!_KeyCodes.ContainsKey('ฏ')) _KeyCodes.Add('ฏ', 3599);
                if (!_KeyCodes.ContainsKey('ฐ')) _KeyCodes.Add('ฐ', 3600);
                if (!_KeyCodes.ContainsKey('ฑ')) _KeyCodes.Add('ฑ', 3601);
                if (!_KeyCodes.ContainsKey('ฒ')) _KeyCodes.Add('ฒ', 3602);
                if (!_KeyCodes.ContainsKey('ณ')) _KeyCodes.Add('ณ', 3603);
                if (!_KeyCodes.ContainsKey('ด')) _KeyCodes.Add('ด', 3604);
                if (!_KeyCodes.ContainsKey('ต')) _KeyCodes.Add('ต', 3605);
                if (!_KeyCodes.ContainsKey('ถ')) _KeyCodes.Add('ถ', 3606);
                if (!_KeyCodes.ContainsKey('ท')) _KeyCodes.Add('ท', 3607);
                if (!_KeyCodes.ContainsKey('ธ')) _KeyCodes.Add('ธ', 3608);
                if (!_KeyCodes.ContainsKey('น')) _KeyCodes.Add('น', 3609);
                if (!_KeyCodes.ContainsKey('บ')) _KeyCodes.Add('บ', 3610);
                if (!_KeyCodes.ContainsKey('ป')) _KeyCodes.Add('ป', 3611);
                if (!_KeyCodes.ContainsKey('ผ')) _KeyCodes.Add('ผ', 3612);
                if (!_KeyCodes.ContainsKey('ฝ')) _KeyCodes.Add('ฝ', 3613);
                if (!_KeyCodes.ContainsKey('พ')) _KeyCodes.Add('พ', 3614);
                if (!_KeyCodes.ContainsKey('ฟ')) _KeyCodes.Add('ฟ', 3615);
                if (!_KeyCodes.ContainsKey('ภ')) _KeyCodes.Add('ภ', 3616);
                if (!_KeyCodes.ContainsKey('ม')) _KeyCodes.Add('ม', 3617);
                if (!_KeyCodes.ContainsKey('ย')) _KeyCodes.Add('ย', 3618);
                if (!_KeyCodes.ContainsKey('ร')) _KeyCodes.Add('ร', 3619);
                if (!_KeyCodes.ContainsKey('ฤ')) _KeyCodes.Add('ฤ', 3620);
                if (!_KeyCodes.ContainsKey('ล')) _KeyCodes.Add('ล', 3621);
                if (!_KeyCodes.ContainsKey('ฦ')) _KeyCodes.Add('ฦ', 3622);
                if (!_KeyCodes.ContainsKey('ว')) _KeyCodes.Add('ว', 3623);
                if (!_KeyCodes.ContainsKey('ศ')) _KeyCodes.Add('ศ', 3624);
                if (!_KeyCodes.ContainsKey('ษ')) _KeyCodes.Add('ษ', 3625);
                if (!_KeyCodes.ContainsKey('ส')) _KeyCodes.Add('ส', 3626);
                if (!_KeyCodes.ContainsKey('ห')) _KeyCodes.Add('ห', 3627);
                if (!_KeyCodes.ContainsKey('ฬ')) _KeyCodes.Add('ฬ', 3628);
                if (!_KeyCodes.ContainsKey('อ')) _KeyCodes.Add('อ', 3629);
                if (!_KeyCodes.ContainsKey('ฮ')) _KeyCodes.Add('ฮ', 3630);
                if (!_KeyCodes.ContainsKey('ฯ')) _KeyCodes.Add('ฯ', 3631);
                if (!_KeyCodes.ContainsKey('ะ')) _KeyCodes.Add('ะ', 3632);
                if (!_KeyCodes.ContainsKey('ั')) _KeyCodes.Add('ั', 3633);
                if (!_KeyCodes.ContainsKey('า')) _KeyCodes.Add('า', 3634);
                if (!_KeyCodes.ContainsKey('ำ')) _KeyCodes.Add('ำ', 3635);
                if (!_KeyCodes.ContainsKey('ิ')) _KeyCodes.Add('ิ', 3636);
                if (!_KeyCodes.ContainsKey('ี')) _KeyCodes.Add('ี', 3637);
                if (!_KeyCodes.ContainsKey('ึ')) _KeyCodes.Add('ึ', 3638);
                if (!_KeyCodes.ContainsKey('ื')) _KeyCodes.Add('ื', 3639);
                if (!_KeyCodes.ContainsKey('ุ')) _KeyCodes.Add('ุ', 3640);
                if (!_KeyCodes.ContainsKey('ู')) _KeyCodes.Add('ู', 3641);
                if (!_KeyCodes.ContainsKey('ฺ')) _KeyCodes.Add('ฺ', 3642);
                if (!_KeyCodes.ContainsKey('฿')) _KeyCodes.Add('฿', 3647);
                if (!_KeyCodes.ContainsKey('เ')) _KeyCodes.Add('เ', 3648);
                if (!_KeyCodes.ContainsKey('แ')) _KeyCodes.Add('แ', 3649);
                if (!_KeyCodes.ContainsKey('โ')) _KeyCodes.Add('โ', 3650);
                if (!_KeyCodes.ContainsKey('ใ')) _KeyCodes.Add('ใ', 3651);
                if (!_KeyCodes.ContainsKey('ไ')) _KeyCodes.Add('ไ', 3652);
                if (!_KeyCodes.ContainsKey('ๅ')) _KeyCodes.Add('ๅ', 3653);
                if (!_KeyCodes.ContainsKey('ๆ')) _KeyCodes.Add('ๆ', 3654);
                if (!_KeyCodes.ContainsKey('็')) _KeyCodes.Add('็', 3655);
                if (!_KeyCodes.ContainsKey('่')) _KeyCodes.Add('่', 3656);
                if (!_KeyCodes.ContainsKey('้')) _KeyCodes.Add('้', 3657);
                if (!_KeyCodes.ContainsKey('๊')) _KeyCodes.Add('๊', 3658);
                if (!_KeyCodes.ContainsKey('๋')) _KeyCodes.Add('๋', 3659);
                if (!_KeyCodes.ContainsKey('์')) _KeyCodes.Add('์', 3660);
                if (!_KeyCodes.ContainsKey('ํ')) _KeyCodes.Add('ํ', 3661);
                if (!_KeyCodes.ContainsKey('0')) _KeyCodes.Add('0', 3664);
                if (!_KeyCodes.ContainsKey('1')) _KeyCodes.Add('1', 3665);
                if (!_KeyCodes.ContainsKey('2')) _KeyCodes.Add('2', 3666);
                if (!_KeyCodes.ContainsKey('3')) _KeyCodes.Add('3', 3667);
                if (!_KeyCodes.ContainsKey('4')) _KeyCodes.Add('4', 3668);
                if (!_KeyCodes.ContainsKey('5')) _KeyCodes.Add('5', 3669);
                if (!_KeyCodes.ContainsKey('6')) _KeyCodes.Add('6', 3670);
                if (!_KeyCodes.ContainsKey('7')) _KeyCodes.Add('7', 3671);
                if (!_KeyCodes.ContainsKey('8')) _KeyCodes.Add('8', 3672);
                if (!_KeyCodes.ContainsKey('9')) _KeyCodes.Add('9', 3673);
                if (!_KeyCodes.ContainsKey('ა')) _KeyCodes.Add('ა', 4304);
                if (!_KeyCodes.ContainsKey('ბ')) _KeyCodes.Add('ბ', 4305);
                if (!_KeyCodes.ContainsKey('გ')) _KeyCodes.Add('გ', 4306);
                if (!_KeyCodes.ContainsKey('დ')) _KeyCodes.Add('დ', 4307);
                if (!_KeyCodes.ContainsKey('ე')) _KeyCodes.Add('ე', 4308);
                if (!_KeyCodes.ContainsKey('ვ')) _KeyCodes.Add('ვ', 4309);
                if (!_KeyCodes.ContainsKey('ზ')) _KeyCodes.Add('ზ', 4310);
                if (!_KeyCodes.ContainsKey('თ')) _KeyCodes.Add('თ', 4311);
                if (!_KeyCodes.ContainsKey('ი')) _KeyCodes.Add('ი', 4312);
                if (!_KeyCodes.ContainsKey('კ')) _KeyCodes.Add('კ', 4313);
                if (!_KeyCodes.ContainsKey('ლ')) _KeyCodes.Add('ლ', 4314);
                if (!_KeyCodes.ContainsKey('მ')) _KeyCodes.Add('მ', 4315);
                if (!_KeyCodes.ContainsKey('ნ')) _KeyCodes.Add('ნ', 4316);
                if (!_KeyCodes.ContainsKey('ო')) _KeyCodes.Add('ო', 4317);
                if (!_KeyCodes.ContainsKey('პ')) _KeyCodes.Add('პ', 4318);
                if (!_KeyCodes.ContainsKey('ჟ')) _KeyCodes.Add('ჟ', 4319);
                if (!_KeyCodes.ContainsKey('რ')) _KeyCodes.Add('რ', 4320);
                if (!_KeyCodes.ContainsKey('ს')) _KeyCodes.Add('ს', 4321);
                if (!_KeyCodes.ContainsKey('ტ')) _KeyCodes.Add('ტ', 4322);
                if (!_KeyCodes.ContainsKey('უ')) _KeyCodes.Add('უ', 4323);
                if (!_KeyCodes.ContainsKey('ფ')) _KeyCodes.Add('ფ', 4324);
                if (!_KeyCodes.ContainsKey('ქ')) _KeyCodes.Add('ქ', 4325);
                if (!_KeyCodes.ContainsKey('ღ')) _KeyCodes.Add('ღ', 4326);
                if (!_KeyCodes.ContainsKey('ყ')) _KeyCodes.Add('ყ', 4327);
                if (!_KeyCodes.ContainsKey('შ')) _KeyCodes.Add('შ', 4328);
                if (!_KeyCodes.ContainsKey('ჩ')) _KeyCodes.Add('ჩ', 4329);
                if (!_KeyCodes.ContainsKey('ც')) _KeyCodes.Add('ც', 4330);
                if (!_KeyCodes.ContainsKey('ძ')) _KeyCodes.Add('ძ', 4331);
                if (!_KeyCodes.ContainsKey('წ')) _KeyCodes.Add('წ', 4332);
                if (!_KeyCodes.ContainsKey('ჭ')) _KeyCodes.Add('ჭ', 4333);
                if (!_KeyCodes.ContainsKey('ხ')) _KeyCodes.Add('ხ', 4334);
                if (!_KeyCodes.ContainsKey('ჯ')) _KeyCodes.Add('ჯ', 4335);
                if (!_KeyCodes.ContainsKey('ჰ')) _KeyCodes.Add('ჰ', 4336);
                if (!_KeyCodes.ContainsKey('ჱ')) _KeyCodes.Add('ჱ', 4337);
                if (!_KeyCodes.ContainsKey('ჲ')) _KeyCodes.Add('ჲ', 4338);
                if (!_KeyCodes.ContainsKey('ჳ')) _KeyCodes.Add('ჳ', 4339);
                if (!_KeyCodes.ContainsKey('ჴ')) _KeyCodes.Add('ჴ', 4340);
                if (!_KeyCodes.ContainsKey('ჵ')) _KeyCodes.Add('ჵ', 4341);
                if (!_KeyCodes.ContainsKey('ჶ')) _KeyCodes.Add('ჶ', 4342);
                if (!_KeyCodes.ContainsKey('Ḃ')) _KeyCodes.Add('Ḃ', 7682);
                if (!_KeyCodes.ContainsKey('ḃ')) _KeyCodes.Add('ḃ', 7683);
                if (!_KeyCodes.ContainsKey('Ḋ')) _KeyCodes.Add('Ḋ', 7690);
                if (!_KeyCodes.ContainsKey('ḋ')) _KeyCodes.Add('ḋ', 7691);
                if (!_KeyCodes.ContainsKey('Ḟ')) _KeyCodes.Add('Ḟ', 7710);
                if (!_KeyCodes.ContainsKey('ḟ')) _KeyCodes.Add('ḟ', 7711);
                if (!_KeyCodes.ContainsKey('Ḷ')) _KeyCodes.Add('Ḷ', 7734);
                if (!_KeyCodes.ContainsKey('ḷ')) _KeyCodes.Add('ḷ', 7735);
                if (!_KeyCodes.ContainsKey('Ṁ')) _KeyCodes.Add('Ṁ', 7744);
                if (!_KeyCodes.ContainsKey('ṁ')) _KeyCodes.Add('ṁ', 7745);
                if (!_KeyCodes.ContainsKey('Ṗ')) _KeyCodes.Add('Ṗ', 7766);
                if (!_KeyCodes.ContainsKey('ṗ')) _KeyCodes.Add('ṗ', 7767);
                if (!_KeyCodes.ContainsKey('Ṡ')) _KeyCodes.Add('Ṡ', 7776);
                if (!_KeyCodes.ContainsKey('ṡ')) _KeyCodes.Add('ṡ', 7777);
                if (!_KeyCodes.ContainsKey('Ṫ')) _KeyCodes.Add('Ṫ', 7786);
                if (!_KeyCodes.ContainsKey('ṫ')) _KeyCodes.Add('ṫ', 7787);
                if (!_KeyCodes.ContainsKey('Ẁ')) _KeyCodes.Add('Ẁ', 7808);
                if (!_KeyCodes.ContainsKey('ẁ')) _KeyCodes.Add('ẁ', 7809);
                if (!_KeyCodes.ContainsKey('Ẃ')) _KeyCodes.Add('Ẃ', 7810);
                if (!_KeyCodes.ContainsKey('ẃ')) _KeyCodes.Add('ẃ', 7811);
                if (!_KeyCodes.ContainsKey('Ẅ')) _KeyCodes.Add('Ẅ', 7812);
                if (!_KeyCodes.ContainsKey('ẅ')) _KeyCodes.Add('ẅ', 7813);
                if (!_KeyCodes.ContainsKey('Ẋ')) _KeyCodes.Add('Ẋ', 7818);
                if (!_KeyCodes.ContainsKey('ẋ')) _KeyCodes.Add('ẋ', 7819);
                if (!_KeyCodes.ContainsKey('Ạ')) _KeyCodes.Add('Ạ', 7840);
                if (!_KeyCodes.ContainsKey('ạ')) _KeyCodes.Add('ạ', 7841);
                if (!_KeyCodes.ContainsKey('Ả')) _KeyCodes.Add('Ả', 7842);
                if (!_KeyCodes.ContainsKey('ả')) _KeyCodes.Add('ả', 7843);
                if (!_KeyCodes.ContainsKey('Ấ')) _KeyCodes.Add('Ấ', 7844);
                if (!_KeyCodes.ContainsKey('ấ')) _KeyCodes.Add('ấ', 7845);
                if (!_KeyCodes.ContainsKey('Ầ')) _KeyCodes.Add('Ầ', 7846);
                if (!_KeyCodes.ContainsKey('ầ')) _KeyCodes.Add('ầ', 7847);
                if (!_KeyCodes.ContainsKey('Ẩ')) _KeyCodes.Add('Ẩ', 7848);
                if (!_KeyCodes.ContainsKey('ẩ')) _KeyCodes.Add('ẩ', 7849);
                if (!_KeyCodes.ContainsKey('Ẫ')) _KeyCodes.Add('Ẫ', 7850);
                if (!_KeyCodes.ContainsKey('ẫ')) _KeyCodes.Add('ẫ', 7851);
                if (!_KeyCodes.ContainsKey('Ậ')) _KeyCodes.Add('Ậ', 7852);
                if (!_KeyCodes.ContainsKey('ậ')) _KeyCodes.Add('ậ', 7853);
                if (!_KeyCodes.ContainsKey('Ắ')) _KeyCodes.Add('Ắ', 7854);
                if (!_KeyCodes.ContainsKey('ắ')) _KeyCodes.Add('ắ', 7855);
                if (!_KeyCodes.ContainsKey('Ằ')) _KeyCodes.Add('Ằ', 7856);
                if (!_KeyCodes.ContainsKey('ằ')) _KeyCodes.Add('ằ', 7857);
                if (!_KeyCodes.ContainsKey('Ẳ')) _KeyCodes.Add('Ẳ', 7858);
                if (!_KeyCodes.ContainsKey('ẳ')) _KeyCodes.Add('ẳ', 7859);
                if (!_KeyCodes.ContainsKey('Ẵ')) _KeyCodes.Add('Ẵ', 7860);
                if (!_KeyCodes.ContainsKey('ẵ')) _KeyCodes.Add('ẵ', 7861);
                if (!_KeyCodes.ContainsKey('Ặ')) _KeyCodes.Add('Ặ', 7862);
                if (!_KeyCodes.ContainsKey('ặ')) _KeyCodes.Add('ặ', 7863);
                if (!_KeyCodes.ContainsKey('Ẹ')) _KeyCodes.Add('Ẹ', 7864);
                if (!_KeyCodes.ContainsKey('ẹ')) _KeyCodes.Add('ẹ', 7865);
                if (!_KeyCodes.ContainsKey('Ẻ')) _KeyCodes.Add('Ẻ', 7866);
                if (!_KeyCodes.ContainsKey('ẻ')) _KeyCodes.Add('ẻ', 7867);
                if (!_KeyCodes.ContainsKey('Ẽ')) _KeyCodes.Add('Ẽ', 7868);
                if (!_KeyCodes.ContainsKey('ẽ')) _KeyCodes.Add('ẽ', 7869);
                if (!_KeyCodes.ContainsKey('Ế')) _KeyCodes.Add('Ế', 7870);
                if (!_KeyCodes.ContainsKey('ế')) _KeyCodes.Add('ế', 7871);
                if (!_KeyCodes.ContainsKey('Ề')) _KeyCodes.Add('Ề', 7872);
                if (!_KeyCodes.ContainsKey('ề')) _KeyCodes.Add('ề', 7873);
                if (!_KeyCodes.ContainsKey('Ể')) _KeyCodes.Add('Ể', 7874);
                if (!_KeyCodes.ContainsKey('ể')) _KeyCodes.Add('ể', 7875);
                if (!_KeyCodes.ContainsKey('Ễ')) _KeyCodes.Add('Ễ', 7876);
                if (!_KeyCodes.ContainsKey('ễ')) _KeyCodes.Add('ễ', 7877);
                if (!_KeyCodes.ContainsKey('Ệ')) _KeyCodes.Add('Ệ', 7878);
                if (!_KeyCodes.ContainsKey('ệ')) _KeyCodes.Add('ệ', 7879);
                if (!_KeyCodes.ContainsKey('Ỉ')) _KeyCodes.Add('Ỉ', 7880);
                if (!_KeyCodes.ContainsKey('ỉ')) _KeyCodes.Add('ỉ', 7881);
                if (!_KeyCodes.ContainsKey('Ị')) _KeyCodes.Add('Ị', 7882);
                if (!_KeyCodes.ContainsKey('ị')) _KeyCodes.Add('ị', 7883);
                if (!_KeyCodes.ContainsKey('Ọ')) _KeyCodes.Add('Ọ', 7884);
                if (!_KeyCodes.ContainsKey('ọ')) _KeyCodes.Add('ọ', 7885);
                if (!_KeyCodes.ContainsKey('Ỏ')) _KeyCodes.Add('Ỏ', 7886);
                if (!_KeyCodes.ContainsKey('ỏ')) _KeyCodes.Add('ỏ', 7887);
                if (!_KeyCodes.ContainsKey('Ố')) _KeyCodes.Add('Ố', 7888);
                if (!_KeyCodes.ContainsKey('ố')) _KeyCodes.Add('ố', 7889);
                if (!_KeyCodes.ContainsKey('Ồ')) _KeyCodes.Add('Ồ', 7890);
                if (!_KeyCodes.ContainsKey('ồ')) _KeyCodes.Add('ồ', 7891);
                if (!_KeyCodes.ContainsKey('Ổ')) _KeyCodes.Add('Ổ', 7892);
                if (!_KeyCodes.ContainsKey('ổ')) _KeyCodes.Add('ổ', 7893);
                if (!_KeyCodes.ContainsKey('Ỗ')) _KeyCodes.Add('Ỗ', 7894);
                if (!_KeyCodes.ContainsKey('ỗ')) _KeyCodes.Add('ỗ', 7895);
                if (!_KeyCodes.ContainsKey('Ộ')) _KeyCodes.Add('Ộ', 7896);
                if (!_KeyCodes.ContainsKey('ộ')) _KeyCodes.Add('ộ', 7897);
                if (!_KeyCodes.ContainsKey('Ớ')) _KeyCodes.Add('Ớ', 7898);
                if (!_KeyCodes.ContainsKey('ớ')) _KeyCodes.Add('ớ', 7899);
                if (!_KeyCodes.ContainsKey('Ờ')) _KeyCodes.Add('Ờ', 7900);
                if (!_KeyCodes.ContainsKey('ờ')) _KeyCodes.Add('ờ', 7901);
                if (!_KeyCodes.ContainsKey('Ở')) _KeyCodes.Add('Ở', 7902);
                if (!_KeyCodes.ContainsKey('ở')) _KeyCodes.Add('ở', 7903);
                if (!_KeyCodes.ContainsKey('Ỡ')) _KeyCodes.Add('Ỡ', 7904);
                if (!_KeyCodes.ContainsKey('ỡ')) _KeyCodes.Add('ỡ', 7905);
                if (!_KeyCodes.ContainsKey('Ợ')) _KeyCodes.Add('Ợ', 7906);
                if (!_KeyCodes.ContainsKey('ợ')) _KeyCodes.Add('ợ', 7907);
                if (!_KeyCodes.ContainsKey('Ụ')) _KeyCodes.Add('Ụ', 7908);
                if (!_KeyCodes.ContainsKey('ụ')) _KeyCodes.Add('ụ', 7909);
                if (!_KeyCodes.ContainsKey('Ủ')) _KeyCodes.Add('Ủ', 7910);
                if (!_KeyCodes.ContainsKey('ủ')) _KeyCodes.Add('ủ', 7911);
                if (!_KeyCodes.ContainsKey('Ứ')) _KeyCodes.Add('Ứ', 7912);
                if (!_KeyCodes.ContainsKey('ứ')) _KeyCodes.Add('ứ', 7913);
                if (!_KeyCodes.ContainsKey('Ừ')) _KeyCodes.Add('Ừ', 7914);
                if (!_KeyCodes.ContainsKey('ừ')) _KeyCodes.Add('ừ', 7915);
                if (!_KeyCodes.ContainsKey('Ử')) _KeyCodes.Add('Ử', 7916);
                if (!_KeyCodes.ContainsKey('ử')) _KeyCodes.Add('ử', 7917);
                if (!_KeyCodes.ContainsKey('Ữ')) _KeyCodes.Add('Ữ', 7918);
                if (!_KeyCodes.ContainsKey('ữ')) _KeyCodes.Add('ữ', 7919);
                if (!_KeyCodes.ContainsKey('Ự')) _KeyCodes.Add('Ự', 7920);
                if (!_KeyCodes.ContainsKey('ự')) _KeyCodes.Add('ự', 7921);
                if (!_KeyCodes.ContainsKey('Ỳ')) _KeyCodes.Add('Ỳ', 7922);
                if (!_KeyCodes.ContainsKey('ỳ')) _KeyCodes.Add('ỳ', 7923);
                if (!_KeyCodes.ContainsKey('Ỵ')) _KeyCodes.Add('Ỵ', 7924);
                if (!_KeyCodes.ContainsKey('ỵ')) _KeyCodes.Add('ỵ', 7925);
                if (!_KeyCodes.ContainsKey('Ỷ')) _KeyCodes.Add('Ỷ', 7926);
                if (!_KeyCodes.ContainsKey('ỷ')) _KeyCodes.Add('ỷ', 7927);
                if (!_KeyCodes.ContainsKey('Ỹ')) _KeyCodes.Add('Ỹ', 7928);
                if (!_KeyCodes.ContainsKey('ỹ')) _KeyCodes.Add('ỹ', 7929);
                if (!_KeyCodes.ContainsKey(' ')) _KeyCodes.Add(' ', 8194);
                if (!_KeyCodes.ContainsKey(' ')) _KeyCodes.Add(' ', 8195);
                if (!_KeyCodes.ContainsKey(' ')) _KeyCodes.Add(' ', 8196);
                if (!_KeyCodes.ContainsKey(' ')) _KeyCodes.Add(' ', 8197);
                if (!_KeyCodes.ContainsKey(' ')) _KeyCodes.Add(' ', 8199);
                if (!_KeyCodes.ContainsKey(' ')) _KeyCodes.Add(' ', 8200);
                if (!_KeyCodes.ContainsKey(' ')) _KeyCodes.Add(' ', 8201);
                if (!_KeyCodes.ContainsKey(' ')) _KeyCodes.Add(' ', 8202);
                if (!_KeyCodes.ContainsKey('‒')) _KeyCodes.Add('‒', 8210);
                if (!_KeyCodes.ContainsKey('–')) _KeyCodes.Add('–', 8211);
                if (!_KeyCodes.ContainsKey('—')) _KeyCodes.Add('—', 8212);
                if (!_KeyCodes.ContainsKey('―')) _KeyCodes.Add('―', 8213);
                if (!_KeyCodes.ContainsKey('‗')) _KeyCodes.Add('‗', 8215);
                if (!_KeyCodes.ContainsKey('‘')) _KeyCodes.Add('‘', 8216);
                if (!_KeyCodes.ContainsKey('’')) _KeyCodes.Add('’', 8217);
                if (!_KeyCodes.ContainsKey('‚')) _KeyCodes.Add('‚', 8218);
                if (!_KeyCodes.ContainsKey('“')) _KeyCodes.Add('“', 8220);
                if (!_KeyCodes.ContainsKey('”')) _KeyCodes.Add('”', 8221);
                if (!_KeyCodes.ContainsKey('„')) _KeyCodes.Add('„', 8222);
                if (!_KeyCodes.ContainsKey('†')) _KeyCodes.Add('†', 8224);
                if (!_KeyCodes.ContainsKey('‡')) _KeyCodes.Add('‡', 8225);
                if (!_KeyCodes.ContainsKey('•')) _KeyCodes.Add('•', 8226);
                if (!_KeyCodes.ContainsKey('‥')) _KeyCodes.Add('‥', 8229);
                if (!_KeyCodes.ContainsKey('…')) _KeyCodes.Add('…', 8230);
                if (!_KeyCodes.ContainsKey('′')) _KeyCodes.Add('′', 8242);
                if (!_KeyCodes.ContainsKey('″')) _KeyCodes.Add('″', 8243);
                if (!_KeyCodes.ContainsKey('‸')) _KeyCodes.Add('‸', 8248);
                if (!_KeyCodes.ContainsKey('‾')) _KeyCodes.Add('‾', 8254);
                if (!_KeyCodes.ContainsKey('₠')) _KeyCodes.Add('₠', 8352);
                if (!_KeyCodes.ContainsKey('₡')) _KeyCodes.Add('₡', 8353);
                if (!_KeyCodes.ContainsKey('₢')) _KeyCodes.Add('₢', 8354);
                if (!_KeyCodes.ContainsKey('₣')) _KeyCodes.Add('₣', 8355);
                if (!_KeyCodes.ContainsKey('₤')) _KeyCodes.Add('₤', 8356);
                if (!_KeyCodes.ContainsKey('₥')) _KeyCodes.Add('₥', 8357);
                if (!_KeyCodes.ContainsKey('₦')) _KeyCodes.Add('₦', 8358);
                if (!_KeyCodes.ContainsKey('₧')) _KeyCodes.Add('₧', 8359);
                if (!_KeyCodes.ContainsKey('₨')) _KeyCodes.Add('₨', 8360);
                if (!_KeyCodes.ContainsKey('₩')) _KeyCodes.Add('₩', 8361);
                if (!_KeyCodes.ContainsKey('₩')) _KeyCodes.Add('₩', 8361);
                if (!_KeyCodes.ContainsKey('₪')) _KeyCodes.Add('₪', 8362);
                if (!_KeyCodes.ContainsKey('₫')) _KeyCodes.Add('₫', 8363);
                if (!_KeyCodes.ContainsKey('€')) _KeyCodes.Add('€', 8364);
                if (!_KeyCodes.ContainsKey('℅')) _KeyCodes.Add('℅', 8453);
                if (!_KeyCodes.ContainsKey('№')) _KeyCodes.Add('№', 8470);
                if (!_KeyCodes.ContainsKey('℗')) _KeyCodes.Add('℗', 8471);
                if (!_KeyCodes.ContainsKey('℞')) _KeyCodes.Add('℞', 8478);
                if (!_KeyCodes.ContainsKey('™')) _KeyCodes.Add('™', 8482);
                if (!_KeyCodes.ContainsKey('⅓')) _KeyCodes.Add('⅓', 8531);
                if (!_KeyCodes.ContainsKey('⅔')) _KeyCodes.Add('⅔', 8532);
                if (!_KeyCodes.ContainsKey('⅕')) _KeyCodes.Add('⅕', 8533);
                if (!_KeyCodes.ContainsKey('⅖')) _KeyCodes.Add('⅖', 8534);
                if (!_KeyCodes.ContainsKey('⅗')) _KeyCodes.Add('⅗', 8535);
                if (!_KeyCodes.ContainsKey('⅘')) _KeyCodes.Add('⅘', 8536);
                if (!_KeyCodes.ContainsKey('⅙')) _KeyCodes.Add('⅙', 8537);
                if (!_KeyCodes.ContainsKey('⅚')) _KeyCodes.Add('⅚', 8538);
                if (!_KeyCodes.ContainsKey('⅛')) _KeyCodes.Add('⅛', 8539);
                if (!_KeyCodes.ContainsKey('⅜')) _KeyCodes.Add('⅜', 8540);
                if (!_KeyCodes.ContainsKey('⅝')) _KeyCodes.Add('⅝', 8541);
                if (!_KeyCodes.ContainsKey('⅞')) _KeyCodes.Add('⅞', 8542);
                if (!_KeyCodes.ContainsKey('←')) _KeyCodes.Add('←', 8592);
                if (!_KeyCodes.ContainsKey('↑')) _KeyCodes.Add('↑', 8593);
                if (!_KeyCodes.ContainsKey('→')) _KeyCodes.Add('→', 8594);
                if (!_KeyCodes.ContainsKey('↓')) _KeyCodes.Add('↓', 8595);
                if (!_KeyCodes.ContainsKey('⇒')) _KeyCodes.Add('⇒', 8658);
                if (!_KeyCodes.ContainsKey('⇔')) _KeyCodes.Add('⇔', 8660);
                if (!_KeyCodes.ContainsKey('∂')) _KeyCodes.Add('∂', 8706);
                if (!_KeyCodes.ContainsKey('∇')) _KeyCodes.Add('∇', 8711);
                if (!_KeyCodes.ContainsKey('∘')) _KeyCodes.Add('∘', 8728);
                if (!_KeyCodes.ContainsKey('√')) _KeyCodes.Add('√', 8730);
                if (!_KeyCodes.ContainsKey('∝')) _KeyCodes.Add('∝', 8733);
                if (!_KeyCodes.ContainsKey('∞')) _KeyCodes.Add('∞', 8734);
                if (!_KeyCodes.ContainsKey('∧')) _KeyCodes.Add('∧', 8743);
                if (!_KeyCodes.ContainsKey('∧')) _KeyCodes.Add('∧', 8743);
                if (!_KeyCodes.ContainsKey('∨')) _KeyCodes.Add('∨', 8744);
                if (!_KeyCodes.ContainsKey('∨')) _KeyCodes.Add('∨', 8744);
                if (!_KeyCodes.ContainsKey('∩')) _KeyCodes.Add('∩', 8745);
                if (!_KeyCodes.ContainsKey('∩')) _KeyCodes.Add('∩', 8745);
                if (!_KeyCodes.ContainsKey('∪')) _KeyCodes.Add('∪', 8746);
                if (!_KeyCodes.ContainsKey('∪')) _KeyCodes.Add('∪', 8746);
                if (!_KeyCodes.ContainsKey('∫')) _KeyCodes.Add('∫', 8747);
                if (!_KeyCodes.ContainsKey('∴')) _KeyCodes.Add('∴', 8756);
                if (!_KeyCodes.ContainsKey('∼')) _KeyCodes.Add('∼', 8764);
                if (!_KeyCodes.ContainsKey('≃')) _KeyCodes.Add('≃', 8771);
                if (!_KeyCodes.ContainsKey('≠')) _KeyCodes.Add('≠', 8800);
                if (!_KeyCodes.ContainsKey('≡')) _KeyCodes.Add('≡', 8801);
                if (!_KeyCodes.ContainsKey('≤')) _KeyCodes.Add('≤', 8804);
                if (!_KeyCodes.ContainsKey('≥')) _KeyCodes.Add('≥', 8805);
                if (!_KeyCodes.ContainsKey('⊂')) _KeyCodes.Add('⊂', 8834);
                if (!_KeyCodes.ContainsKey('⊂')) _KeyCodes.Add('⊂', 8834);
                if (!_KeyCodes.ContainsKey('⊃')) _KeyCodes.Add('⊃', 8835);
                if (!_KeyCodes.ContainsKey('⊃')) _KeyCodes.Add('⊃', 8835);
                if (!_KeyCodes.ContainsKey('⊢')) _KeyCodes.Add('⊢', 8866);
                if (!_KeyCodes.ContainsKey('⊣')) _KeyCodes.Add('⊣', 8867);
                if (!_KeyCodes.ContainsKey('⊤')) _KeyCodes.Add('⊤', 8868);
                if (!_KeyCodes.ContainsKey('⊥')) _KeyCodes.Add('⊥', 8869);
                if (!_KeyCodes.ContainsKey('⌈')) _KeyCodes.Add('⌈', 8968);
                if (!_KeyCodes.ContainsKey('⌊')) _KeyCodes.Add('⌊', 8970);
                if (!_KeyCodes.ContainsKey('⌕')) _KeyCodes.Add('⌕', 8981);
                if (!_KeyCodes.ContainsKey('⌠')) _KeyCodes.Add('⌠', 8992);
                if (!_KeyCodes.ContainsKey('⌡')) _KeyCodes.Add('⌡', 8993);
                if (!_KeyCodes.ContainsKey('⎕')) _KeyCodes.Add('⎕', 9109);
                if (!_KeyCodes.ContainsKey('⎛')) _KeyCodes.Add('⎛', 9115);
                if (!_KeyCodes.ContainsKey('⎝')) _KeyCodes.Add('⎝', 9117);
                if (!_KeyCodes.ContainsKey('⎞')) _KeyCodes.Add('⎞', 9118);
                if (!_KeyCodes.ContainsKey('⎠')) _KeyCodes.Add('⎠', 9120);
                if (!_KeyCodes.ContainsKey('⎡')) _KeyCodes.Add('⎡', 9121);
                if (!_KeyCodes.ContainsKey('⎣')) _KeyCodes.Add('⎣', 9123);
                if (!_KeyCodes.ContainsKey('⎤')) _KeyCodes.Add('⎤', 9124);
                if (!_KeyCodes.ContainsKey('⎦')) _KeyCodes.Add('⎦', 9126);
                if (!_KeyCodes.ContainsKey('⎨')) _KeyCodes.Add('⎨', 9128);
                if (!_KeyCodes.ContainsKey('⎬')) _KeyCodes.Add('⎬', 9132);
                if (!_KeyCodes.ContainsKey('⎷')) _KeyCodes.Add('⎷', 9143);
                if (!_KeyCodes.ContainsKey('⎺')) _KeyCodes.Add('⎺', 9146);
                if (!_KeyCodes.ContainsKey('⎻')) _KeyCodes.Add('⎻', 9147);
                if (!_KeyCodes.ContainsKey('⎼')) _KeyCodes.Add('⎼', 9148);
                if (!_KeyCodes.ContainsKey('⎽')) _KeyCodes.Add('⎽', 9149);
                if (!_KeyCodes.ContainsKey('␉')) _KeyCodes.Add('␉', 9225);
                if (!_KeyCodes.ContainsKey('␊')) _KeyCodes.Add('␊', 9226);
                if (!_KeyCodes.ContainsKey('␋')) _KeyCodes.Add('␋', 9227);
                if (!_KeyCodes.ContainsKey('␌')) _KeyCodes.Add('␌', 9228);
                if (!_KeyCodes.ContainsKey('␍')) _KeyCodes.Add('␍', 9229);
                if (!_KeyCodes.ContainsKey('␣')) _KeyCodes.Add('␣', 9251);
                if (!_KeyCodes.ContainsKey('␤')) _KeyCodes.Add('␤', 9252);
                if (!_KeyCodes.ContainsKey('─')) _KeyCodes.Add('─', 9472);
                if (!_KeyCodes.ContainsKey('─')) _KeyCodes.Add('─', 9472);
                if (!_KeyCodes.ContainsKey('│')) _KeyCodes.Add('│', 9474);
                if (!_KeyCodes.ContainsKey('│')) _KeyCodes.Add('│', 9474);
                if (!_KeyCodes.ContainsKey('┌')) _KeyCodes.Add('┌', 9484);
                if (!_KeyCodes.ContainsKey('┌')) _KeyCodes.Add('┌', 9484);
                if (!_KeyCodes.ContainsKey('┐')) _KeyCodes.Add('┐', 9488);
                if (!_KeyCodes.ContainsKey('└')) _KeyCodes.Add('└', 9492);
                if (!_KeyCodes.ContainsKey('┘')) _KeyCodes.Add('┘', 9496);
                if (!_KeyCodes.ContainsKey('├')) _KeyCodes.Add('├', 9500);
                if (!_KeyCodes.ContainsKey('┤')) _KeyCodes.Add('┤', 9508);
                if (!_KeyCodes.ContainsKey('┬')) _KeyCodes.Add('┬', 9516);
                if (!_KeyCodes.ContainsKey('┴')) _KeyCodes.Add('┴', 9524);
                if (!_KeyCodes.ContainsKey('┼')) _KeyCodes.Add('┼', 9532);
                if (!_KeyCodes.ContainsKey('▒')) _KeyCodes.Add('▒', 9618);
                if (!_KeyCodes.ContainsKey('▪')) _KeyCodes.Add('▪', 9642);
                if (!_KeyCodes.ContainsKey('▫')) _KeyCodes.Add('▫', 9643);
                if (!_KeyCodes.ContainsKey('▬')) _KeyCodes.Add('▬', 9644);
                if (!_KeyCodes.ContainsKey('▭')) _KeyCodes.Add('▭', 9645);
                if (!_KeyCodes.ContainsKey('▮')) _KeyCodes.Add('▮', 9646);
                if (!_KeyCodes.ContainsKey('▯')) _KeyCodes.Add('▯', 9647);
                if (!_KeyCodes.ContainsKey('▲')) _KeyCodes.Add('▲', 9650);
                if (!_KeyCodes.ContainsKey('△')) _KeyCodes.Add('△', 9651);
                if (!_KeyCodes.ContainsKey('▶')) _KeyCodes.Add('▶', 9654);
                if (!_KeyCodes.ContainsKey('▷')) _KeyCodes.Add('▷', 9655);
                if (!_KeyCodes.ContainsKey('▼')) _KeyCodes.Add('▼', 9660);
                if (!_KeyCodes.ContainsKey('▽')) _KeyCodes.Add('▽', 9661);
                if (!_KeyCodes.ContainsKey('◀')) _KeyCodes.Add('◀', 9664);
                if (!_KeyCodes.ContainsKey('◁')) _KeyCodes.Add('◁', 9665);
                if (!_KeyCodes.ContainsKey('◆')) _KeyCodes.Add('◆', 9670);
                if (!_KeyCodes.ContainsKey('○')) _KeyCodes.Add('○', 9675);
                if (!_KeyCodes.ContainsKey('○')) _KeyCodes.Add('○', 9675);
                if (!_KeyCodes.ContainsKey('●')) _KeyCodes.Add('●', 9679);
                if (!_KeyCodes.ContainsKey('◦')) _KeyCodes.Add('◦', 9702);
                if (!_KeyCodes.ContainsKey('☆')) _KeyCodes.Add('☆', 9734);
                if (!_KeyCodes.ContainsKey('☎')) _KeyCodes.Add('☎', 9742);
                if (!_KeyCodes.ContainsKey('☓')) _KeyCodes.Add('☓', 9747);
                if (!_KeyCodes.ContainsKey('☜')) _KeyCodes.Add('☜', 9756);
                if (!_KeyCodes.ContainsKey('☞')) _KeyCodes.Add('☞', 9758);
                if (!_KeyCodes.ContainsKey('♀')) _KeyCodes.Add('♀', 9792);
                if (!_KeyCodes.ContainsKey('♂')) _KeyCodes.Add('♂', 9794);
                if (!_KeyCodes.ContainsKey('♣')) _KeyCodes.Add('♣', 9827);
                if (!_KeyCodes.ContainsKey('♥')) _KeyCodes.Add('♥', 9829);
                if (!_KeyCodes.ContainsKey('♦')) _KeyCodes.Add('♦', 9830);
                if (!_KeyCodes.ContainsKey('♭')) _KeyCodes.Add('♭', 9837);
                if (!_KeyCodes.ContainsKey('♯')) _KeyCodes.Add('♯', 9839);
                if (!_KeyCodes.ContainsKey('✓')) _KeyCodes.Add('✓', 10003);
                if (!_KeyCodes.ContainsKey('✗')) _KeyCodes.Add('✗', 10007);
                if (!_KeyCodes.ContainsKey('✝')) _KeyCodes.Add('✝', 10013);
                if (!_KeyCodes.ContainsKey('✠')) _KeyCodes.Add('✠', 10016);
                if (!_KeyCodes.ContainsKey('⟨')) _KeyCodes.Add('⟨', 10216);
                if (!_KeyCodes.ContainsKey('⟩')) _KeyCodes.Add('⟩', 10217);
                if (!_KeyCodes.ContainsKey('、')) _KeyCodes.Add('、', 12289);
                if (!_KeyCodes.ContainsKey('。')) _KeyCodes.Add('。', 12290);
                if (!_KeyCodes.ContainsKey('「')) _KeyCodes.Add('「', 12300);
                if (!_KeyCodes.ContainsKey('」')) _KeyCodes.Add('」', 12301);
                if (!_KeyCodes.ContainsKey('゛')) _KeyCodes.Add('゛', 12443);
                if (!_KeyCodes.ContainsKey('゜')) _KeyCodes.Add('゜', 12444);
                if (!_KeyCodes.ContainsKey('ァ')) _KeyCodes.Add('ァ', 12449);
                if (!_KeyCodes.ContainsKey('ア')) _KeyCodes.Add('ア', 12450);
                if (!_KeyCodes.ContainsKey('ィ')) _KeyCodes.Add('ィ', 12451);
                if (!_KeyCodes.ContainsKey('イ')) _KeyCodes.Add('イ', 12452);
                if (!_KeyCodes.ContainsKey('ゥ')) _KeyCodes.Add('ゥ', 12453);
                if (!_KeyCodes.ContainsKey('ウ')) _KeyCodes.Add('ウ', 12454);
                if (!_KeyCodes.ContainsKey('ェ')) _KeyCodes.Add('ェ', 12455);
                if (!_KeyCodes.ContainsKey('エ')) _KeyCodes.Add('エ', 12456);
                if (!_KeyCodes.ContainsKey('ォ')) _KeyCodes.Add('ォ', 12457);
                if (!_KeyCodes.ContainsKey('オ')) _KeyCodes.Add('オ', 12458);
                if (!_KeyCodes.ContainsKey('カ')) _KeyCodes.Add('カ', 12459);
                if (!_KeyCodes.ContainsKey('キ')) _KeyCodes.Add('キ', 12461);
                if (!_KeyCodes.ContainsKey('ク')) _KeyCodes.Add('ク', 12463);
                if (!_KeyCodes.ContainsKey('ケ')) _KeyCodes.Add('ケ', 12465);
                if (!_KeyCodes.ContainsKey('コ')) _KeyCodes.Add('コ', 12467);
                if (!_KeyCodes.ContainsKey('サ')) _KeyCodes.Add('サ', 12469);
                if (!_KeyCodes.ContainsKey('シ')) _KeyCodes.Add('シ', 12471);
                if (!_KeyCodes.ContainsKey('ス')) _KeyCodes.Add('ス', 12473);
                if (!_KeyCodes.ContainsKey('セ')) _KeyCodes.Add('セ', 12475);
                if (!_KeyCodes.ContainsKey('ソ')) _KeyCodes.Add('ソ', 12477);
                if (!_KeyCodes.ContainsKey('タ')) _KeyCodes.Add('タ', 12479);
                if (!_KeyCodes.ContainsKey('チ')) _KeyCodes.Add('チ', 12481);
                if (!_KeyCodes.ContainsKey('ッ')) _KeyCodes.Add('ッ', 12483);
                if (!_KeyCodes.ContainsKey('ツ')) _KeyCodes.Add('ツ', 12484);
                if (!_KeyCodes.ContainsKey('テ')) _KeyCodes.Add('テ', 12486);
                if (!_KeyCodes.ContainsKey('ト')) _KeyCodes.Add('ト', 12488);
                if (!_KeyCodes.ContainsKey('ナ')) _KeyCodes.Add('ナ', 12490);
                if (!_KeyCodes.ContainsKey('ニ')) _KeyCodes.Add('ニ', 12491);
                if (!_KeyCodes.ContainsKey('ヌ')) _KeyCodes.Add('ヌ', 12492);
                if (!_KeyCodes.ContainsKey('ネ')) _KeyCodes.Add('ネ', 12493);
                if (!_KeyCodes.ContainsKey('ノ')) _KeyCodes.Add('ノ', 12494);
                if (!_KeyCodes.ContainsKey('ハ')) _KeyCodes.Add('ハ', 12495);
                if (!_KeyCodes.ContainsKey('ヒ')) _KeyCodes.Add('ヒ', 12498);
                if (!_KeyCodes.ContainsKey('フ')) _KeyCodes.Add('フ', 12501);
                if (!_KeyCodes.ContainsKey('ヘ')) _KeyCodes.Add('ヘ', 12504);
                if (!_KeyCodes.ContainsKey('ホ')) _KeyCodes.Add('ホ', 12507);
                if (!_KeyCodes.ContainsKey('マ')) _KeyCodes.Add('マ', 12510);
                if (!_KeyCodes.ContainsKey('ミ')) _KeyCodes.Add('ミ', 12511);
                if (!_KeyCodes.ContainsKey('ム')) _KeyCodes.Add('ム', 12512);
                if (!_KeyCodes.ContainsKey('メ')) _KeyCodes.Add('メ', 12513);
                if (!_KeyCodes.ContainsKey('モ')) _KeyCodes.Add('モ', 12514);
                if (!_KeyCodes.ContainsKey('ャ')) _KeyCodes.Add('ャ', 12515);
                if (!_KeyCodes.ContainsKey('ヤ')) _KeyCodes.Add('ヤ', 12516);
                if (!_KeyCodes.ContainsKey('ュ')) _KeyCodes.Add('ュ', 12517);
                if (!_KeyCodes.ContainsKey('ユ')) _KeyCodes.Add('ユ', 12518);
                if (!_KeyCodes.ContainsKey('ョ')) _KeyCodes.Add('ョ', 12519);
                if (!_KeyCodes.ContainsKey('ヨ')) _KeyCodes.Add('ヨ', 12520);
                if (!_KeyCodes.ContainsKey('ラ')) _KeyCodes.Add('ラ', 12521);
                if (!_KeyCodes.ContainsKey('リ')) _KeyCodes.Add('リ', 12522);
                if (!_KeyCodes.ContainsKey('ル')) _KeyCodes.Add('ル', 12523);
                if (!_KeyCodes.ContainsKey('レ')) _KeyCodes.Add('レ', 12524);
                if (!_KeyCodes.ContainsKey('ロ')) _KeyCodes.Add('ロ', 12525);
                if (!_KeyCodes.ContainsKey('ワ')) _KeyCodes.Add('ワ', 12527);
                if (!_KeyCodes.ContainsKey('ヲ')) _KeyCodes.Add('ヲ', 12530);
                if (!_KeyCodes.ContainsKey('ン')) _KeyCodes.Add('ン', 12531);
                if (!_KeyCodes.ContainsKey('・')) _KeyCodes.Add('・', 12539);
                if (!_KeyCodes.ContainsKey('ー')) _KeyCodes.Add('ー', 12540);

                return (true);
            }
            catch (Exception ea)
            {
                throw new Exception("Cannot read or execute keys.csv. Be sure it is availabe! Error is " + ea.ToString());
            }
        }

        private bool prepareConnection(string server, int port, string password)
        {
            loadKeyDictionary();
            LoadEncodings();

#if logging
				tmrLog.Tick += new EventHandler(tmrLog_Tick);
				tmrLog.Interval = new TimeSpan(0, 0, 1);
				tmrLog.Start();
				
#endif
            //Set the Server, Port, Password and RFB-Client-Version Properties
            Properties = new ConnectionProperties(server, password, port);
                        
            Log(Logtype.Information, "+++++++++++++++++++++ Initialize new VNC connection +++++++++++++++++++++");

            return (true);
        }

        private bool initialize()
        {
            Log(Logtype.Information, "Connecting to " + Properties.Server + ":" + Properties.Port);

            //Start the Connection to the Server
            if (Connect(Properties.Server, Properties.Port) == false) { return (false); }  //Connect failed
				Thread.Sleep(23);
            HandleRfbVersion(); //Check out the RFB-Version of the Server
				Thread.Sleep(23);
            HandleSecurityType(Properties.Password.Length > 0 ? true : false); //Handle the SecurityType, based on the RfbVersion
				Thread.Sleep(23);

            if (Authenticate() == true) //Authenticate at the Server with handled SecurityType
            {
                if (SendClientSharedFlag() == false) return (false); //Do ClientInit
				Thread.Sleep(23);
                if (ReceiveServerInit() == false) return (false); //Receive ServerInit
				Thread.Sleep(23);
                StartServerListener(); //Start the Listener for ServerToClient-Messages
				Thread.Sleep(23);

                //Set the PixelFormat; Currently the only working Format
                SendSetPixelFormat(new PixelFormat(32, 24, false, true, 255, 255, 255, 16, 8, 0));
				Thread.Sleep(23);
                SendSetEncodings(); //Currently only RAW is supported
				Thread.Sleep(23);

                _IsConnected = true;
				Thread.Sleep(23);

                sendMouseClick((UInt16)(Properties.FramebufferWidth/2), (UInt16)(Properties.FramebufferHeight/2), (byte)0);
				Thread.Sleep(23);

                //Enable the Frame-Update-Timer
					 //_LastReceiveTimer.Tick += new EventHandler(_LastReceiveTimer_Tick);
					 //_LastReceiveTimer.Interval = new TimeSpan(0,0,0,0,_LastReceiveTimeout);
					 //_LastReceiveTimer.Start();
            }
            else
            {
                Disconnect();
                Log(Logtype.User, "Authentication failed");
                return (false);
            }
            return (true);
        }

        /// <summary>
        /// Fills the Encoding-Dictionary with the Content
        /// </summary>
        private void LoadEncodings()
        {
            //Unsupported Encodings are in Comment
            //_EncodingDetails.Add(RfbEncoding.ZRLE_ENCODING, new RfbEncodingDetails(16, "ZRLE", 1));
            //_EncodingDetails.Add(RfbEncoding.Hextile_ENCODING, new RfbEncodingDetails(5, "Hextile", 2));
            //_EncodingDetails.Add(RfbEncoding.RRE_ENCODING, new RfbEncodingDetails(2, "RRE", 3));
            //_EncodingDetails.Add(RfbEncoding.CopyRect_ENCODING, new RfbEncodingDetails(1, "CopyRect", 4));
            _EncodingDetails.Add(RfbEncoding.Raw_ENCODING, new RfbEncodingDetails(0, "RAW", 255));
            //_EncodingDetails.Add(RfbEncoding.CoRRE_ENCODING, new RfbEncodingDetails(4, "CoRRE", 5));
            //_EncodingDetails.Add(RfbEncoding.zlib_ENCODING, new RfbEncodingDetails(6, "zlib", 6));
            //_EncodingDetails.Add(RfbEncoding.tight_ENCODING, new RfbEncodingDetails(7, "tight", 7));
            //_EncodingDetails.Add(RfbEncoding.zlibhex_ENCODING, new RfbEncodingDetails(8, "zlibhex", 8));
            //_EncodingDetails.Add(RfbEncoding.TRLE_ENCODING, new RfbEncodingDetails(15, "TRLE", 9));
            //_EncodingDetails.Add(RfbEncoding.Hitachi_ZYWRLE_ENCODING, new RfbEncodingDetails(17, "Hitachi ZYWRLE", 10));
            //_EncodingDetails.Add(RfbEncoding.Adam_Walling_XZ_ENCODING, new RfbEncodingDetails(18, "Adam Walling XZ", 11));
            //_EncodingDetails.Add(RfbEncoding.Adam_Walling_XZYW_ENCODING, new RfbEncodingDetails(19, "Adam Walling XZYW", 12));

            _EncodingDetails.Add(RfbEncoding.Pseudo_DesktopSize_ENCODING, new RfbEncodingDetails(-223, "Pseudo DesktopSize", 0)); //FFFFFF21
            //_EncodingDetails.Add(RfbEncoding.Pseudo_Cursor_ENCODING, new RfbEncodingDetails(-239, "Pseudo Cursor", 0)); //FFFFFF11

            //_EncodingDetails.Add(RfbEncoding.Pseudo_Cursor_ENCODING, new RfbEncodingDetails(-250, "Pseudo 250", 0)); //FFFFFF06; UltraVNC
            //_EncodingDetails.Add(RfbEncoding.Pseudo_Cursor_ENCODING, new RfbEncodingDetails(-24, "Pseudo 24", 0)); //FFFFFFE6; UltraVNC
            //_EncodingDetails.Add(RfbEncoding.Pseudo_Cursor_ENCODING, new RfbEncodingDetails(-65530, "Pseudo 65530", 0)); //FFFF0006; UltraVNC
            //_EncodingDetails.Add(RfbEncoding.Pseudo_Cursor_ENCODING, new RfbEncodingDetails(-224, "Pseudo 224", 0)); //FFFFFF20; UltraVNC

        }

        /// <summary>
        /// Starts the Receiverthread to wait for new Data from the Server
        /// </summary>
        private void StartServerListener()
        {
            _Receiver = new BackgroundWorker();
            _Receiver.ProgressChanged += new ProgressChangedEventHandler(_Receiver_ProgressChanged);
            _Receiver.DoWork += new DoWorkEventHandler(Receiver);
            _Receiver.WorkerReportsProgress = true;
            _Receiver.RunWorkerAsync();
        }
        
        /// <summary>
        /// Triggers the Thread for Backbufferchanges
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The Color/Pixel-Information</param>
        void _Receiver_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {   
            //Create Thread for running Backbuffer-Update
            Thread th = new Thread(_Receiver_ProgressThread);
            th.Priority = ThreadPriority.BelowNormal;
            th.Start(e.UserState);
        }

        /// <summary>
        /// The Thread to update the Backbuffer
        /// </summary>
        /// <param name="objChangeData"></param>
        void _Receiver_ProgressThread(object objChangeData)
        {
            try
            {
                var changeDatas = (List<RfbRectangle>)objChangeData; //Parse Update-Data

					 if (ScreenUpdate != null)
					 {
						 ScreenUpdate(this, new ScreenUpdateEventArgs() {
							 Rects = changeDatas,
							//PixelData = changeData.PixelData,
							//X = changeData.PosX,
							//Y = changeData.PosY,
							//Width = changeData.Width,
							//Height = changeData.Height,
						 });
					 }

					 ////How many bytes a Pixel use?
					 //int bytePixelCount = _BackBuffer2PixelFormat.BitsPerPixel / 8;

					 ////For every Y-Pixel
					 //for (int y = 0; y < changeData.Height - 1; y++)
					 //{
					 //	 int yIndex = y * _BackBuffer2RawStride; //Get current "row"-position in _BackBuffer2PixelData

					 //	 //For every X-Pixel
					 //	 for (int x = 0; x < changeData.Width - 1; x++)
					 //	 {
					 //		  int byteX = x * bytePixelCount; //Get the "column"-position

					 //		  //Calculate the postionShifting, caused by the reason, that not every Update is on 0x0
					 //		  int positionShifting = changeData.PosY * _BackBuffer2RawStride + changeData.PosX * bytePixelCount;

					 //		  //Update the Backbuffer
					 //		  _BackBuffer2PixelData[byteX + yIndex + positionShifting] = changeData.PixelData[x, y, 0];
					 //		  _BackBuffer2PixelData[byteX + yIndex + 1 + positionShifting] = changeData.PixelData[x, y, 1];
					 //		  _BackBuffer2PixelData[byteX + yIndex + 2 + positionShifting] = changeData.PixelData[x, y, 2];
					 //	 }
					 //}

					 //UpdateScreen(null, null);
            }
            catch (Exception ea)
            {
                Log(Logtype.Error, ea.ToString());
            }
        }

		  ///// <summary>
		  ///// Send Update-Command to the Frontend
		  ///// </summary>
		  ///// <param name="o"></param>
		  ///// <param name="e"></param>
		  //void UpdateScreen(object o, EventArgs e)
		  //{
		  //	 //Update the Screen
		  //	 if (ScreenUpdate != null)
		  //	 {
		  //		  ScreenUpdate(null, new ScreenUpdateEventArgs(_BackBuffer2PixelData, Properties.FramebufferWidth, Properties.FramebufferHeight));
		  //	 }
		  //}

        #endregion

        #region Additions for Server-Compatibilities

        private void _LastReceiveTimer_Tick(object sender, EventArgs e)
        {
            Log(Logtype.Debug, "Query a new Frame automatically");

            //Request a new Frame
            SendFramebufferUpdateRequest(true, 0, 0, Properties.FramebufferWidth, Properties.FramebufferHeight);

            _LastReceive = DateTime.Now;
        }

        #endregion

        #region Events

        public delegate void ConnectionFailedEventHandler(object sender, ConnectionFailedEventArgs e);
        public event ConnectionFailedEventHandler ConnectionFailed;
        
        public delegate void NotSupportedServerMessageEventHandler(object sender, NotSupportedServerMessageEventArgs e);
        public event NotSupportedServerMessageEventHandler NotSupportedServerMessage;

        public delegate void LogMessageEventHandler(object sender, LogMessageEventArgs e);
        public event LogMessageEventHandler LogMessage;

        public delegate void ScreenUpdateEventHandler(object sender, ScreenUpdateEventArgs e);
        public event ScreenUpdateEventHandler ScreenUpdate;

        public delegate void ServerCutTextEventHandler(object sender, ServerCutTextEventArgs e);
        public event ServerCutTextEventHandler ServerCutText;

        #endregion

        #region Methods

        #region Connection Handling
        /// <summary>
        /// Start the Connection to the VNC-Server
        /// </summary>
        /// <param name="server">The IP or Hostname of the Server</param>
        /// <param name="port">The Port of the Server</param>
        /// <returns></returns>
        private bool Connect(String server, int port)
        {
            try
            {
                //Create a TcpClient
					_Client = new TcpClient();
					_Client.Connect(server, port);

                //Get a client stream for reading and writing.
                _DataStream = _Client.GetStream();
                return(true);
            }
            catch (SocketException ea)
            {
                Log(Logtype.Warning, "SocketException: {0}" + ea.ToString());
                return (false);
            }
            catch (Exception ea)
            {
                Log(Logtype.Error, "Exception: {0}" + ea.ToString());
                return (false);
            }
        }

        public void Disconnect ()
        {
            try
            {
                // Close everything.
                _DataStream.Close();
                _Client.Close();
            }
            catch (SocketException ea)
            {
                Console.WriteLine("SocketException: {0}", ea);
                throw new SocketException(ea.ErrorCode);
            }
            catch (Exception ea)
            {
                Console.WriteLine("Exception: {0}", ea);
                throw new Exception(ea.Message, ea);
            }
        }

        public void StartConnection()
        {
            initialize();
        }

        #endregion

        #region Initialisation Communication
        /// <summary>
        /// Get servers VNC-Version and Client-VNC-Version; see 6.1.1
        /// </summary>
        private void HandleRfbVersion()
        {
            //Read the of the TcpServer response bytes.
            Byte[] recData = new Byte[12]; //estimate a 12 Byte long message with VNC-Version
            Int32 bytes = _DataStream.Read(recData, 0, recData.Length);
            Properties.RfbServerVersion = System.Text.Encoding.ASCII.GetString(recData, 0, 12);

            Log(Logtype.Information, "Servers RFB-Version: " + Properties.RfbServerVersion);

            //Write the Clientversion
            Byte[] sendData = System.Text.Encoding.ASCII.GetBytes(Properties.RfbClientVersion);
            _DataStream.Write(sendData, 0, sendData.Length);
        }

        /// <summary>
        /// Handles the SecurtiyType that is used by this connection; see 6.1.2 + 6.1.3
        /// </summary>
        private void HandleSecurityType(bool hasPassword)
        {
            Byte[] recData;
            int bytes;
            Byte[] sendData;

            if (Properties.RfbServerVersion2.Minor < 7) //For Version 3.3 and other Versions
            {
                // Buffer to store the response bytes (4 Bytes with Securitytype; SecurityType will be 0 = invalid; 1 = None or 2 = VNC)
                recData = new Byte[4];
                bytes = _DataStream.Read(recData, 0, recData.Length);
                SetSecurityType(recData);
            }
            else //Versions higher or equal to 3.7
            {
                //Get Count of Securitytypes
                byte secTypeCount = (byte)_DataStream.ReadByte();

                Log(Logtype.Debug, "Supported Server authorisationtypes: " + secTypeCount);

                if (secTypeCount == 0) //Failed/Invalid
                {
                    //Set SecurityType to Failure
                    SetSecurityType(new Byte[4] { 0, 0, 0, 0 }); 

                    //Get the Failure Reason Lenght
                    UInt32 failLenght = ReadUInt32();

                    //Get the Failure Reason Text
                    recData = new Byte[failLenght];
                    bytes = _DataStream.Read(recData, 0, recData.Length);

                    Log(Logtype.User, "Authorisation failed because no Securitytypes are supported. Reason: " + System.Text.Encoding.ASCII.GetString(recData, 0, bytes));

                    //Trigger ConnectionFailed-Event
                    if (ConnectionFailed != null)
                    {
                        ConnectionFailed(null,
                                         new ConnectionFailedEventArgs("Connection failed",
                                                                       System.Text.Encoding.ASCII.GetString(recData, 0, bytes), 0));
                    }
                    return;
                }
                else //Successful
                {
                    //Buffer to store the response bytes; each Type 1 Byte (secTypeCount Bytes with Securitytypenumbers)
                    recData = new Byte[secTypeCount];
                    bytes = _DataStream.Read(recData, 0, recData.Length);

                    //TODO: Choose a SecurtyType dynamically when more note None & VNC are implemented

                    //Send the chosen SecurityType to the Server
                    if (hasPassword == true)
                    {
                        sendData = new Byte[1] { 2 }; //Choose VNC-Authentication
                        Log(Logtype.Debug, "VNC authorisation set by Client");
                    }
                    else
                    {
                        sendData = new Byte[1] { 1 }; //Choose No Authentication
                        Log(Logtype.Debug, "No authorisation set by Client");
                    }

                    _DataStream.Write(sendData, 0, sendData.Length); //Send used SecurityType to Server

                    SetSecurityType(new Byte[4] { 0, 0, 0, sendData[0] }); //Set SecurityType locally to VNC
                }
            }
        }

        /// <summary>
        /// Authenticates with using the handled SecurityType (see 6.2)
        /// </summary>
        /// <returns>True, if authentication was successful</returns>
        private bool Authenticate()
        {
            Byte[] recData;
            int bytes;

            switch (Properties.RfbSecurityType) //Related on the SecurityType authenticate
            {
                    #region default

                default: //A not supported Securitytype
                    if (ConnectionFailed != null)
                    {
                        Log(Logtype.Warning, "The SecurityType " + Properties.RfbSecurityType.ToString() + " is not supported.");
                        ConnectionFailed(null, new ConnectionFailedEventArgs("SecurityType not supported",
                                                                             "The SecurityType " + Properties.RfbSecurityType.ToString() +
                                                                             " is not supported.", 0));
                    }
                    return (false);

                    #endregion

                    #region Invalid

                case SecurityType.Invalid:
                    throw new Exception("This should not happen :(");

                    #endregion

                    #region None

                case SecurityType.None:
                    if (Properties.RfbServerVersion2.Minor > 7) //on 3.8 and above - Read SecurityResult
                    {
                        if (ReadUInt32() == 0)
                        {
                            Log(Logtype.Information, "Authentication Successful, because authentication set to none");
                            return (true);
                        }
                        else //== 1 => Failed
                        {
                            authenticationFailed(); //Handle a failed authentication
                            return (false);
                        }
                    }
                    else //on 3.7 and below  - do nothing
                    {
                        Log(Logtype.Information, "Authentication Successful, because authentication set to none");
                        return(true);
                    }

                    #endregion

                    #region VNC Authentication

                case SecurityType.VNCAuthentication:
                    Log(Logtype.Information, "Authenticate using VNC-Authentication");

                    //Get the 16 Byte-Challenge from the Server:
                    recData = new Byte[16];
                    bytes = _DataStream.Read(recData, 0, recData.Length);


                    //Passwordbyte-Array
                    Byte[] pwVNC = new Byte[8];

                    //Password maximum lenght is 8 Bytes
                    if (Properties.Password.Length < 8)
                    {
                        System.Text.Encoding.ASCII.GetBytes(Properties.Password, 0, Properties.Password.Length, pwVNC, 0);
                    }
                    else //If the Length is longer than 8 Bytes (=Signs), only use the first 8 characters
                    {
                        System.Text.Encoding.ASCII.GetBytes(Properties.Password, 0, 8, pwVNC, 0);
                    }

                    //Change order of bytes by Bitshifting
                    for (int i = 0; i < 8; i++)
                    {
                        pwVNC[i] = (byte) (((pwVNC[i] & 1) << 7) |
                                           ((pwVNC[i] & 2) << 5) |
                                           ((pwVNC[i] & 4) << 3) |
                                           ((pwVNC[i] & 8) << 1) |
                                           ((pwVNC[i] & 16) >> 1) |
                                           ((pwVNC[i] & 32) >> 3) |
                                           ((pwVNC[i] & 64) >> 5) |
                                           ((pwVNC[i] & 128) >> 7));
                    }

                    //DES Encryption
                    DES desEncryption = new DESCryptoServiceProvider();
                    desEncryption.Mode = CipherMode.ECB;
                    desEncryption.Padding = PaddingMode.None;

                    ICryptoTransform encryptor = desEncryption.CreateEncryptor(pwVNC, null);

                    //Generate the Responsekey for the Challenge
                    byte[] challengeResponse = new Byte[16];
                    encryptor.TransformBlock(recData, 0, recData.Length, challengeResponse, 0);

                    //Send the Challengeresponse
                    _DataStream.Write(challengeResponse, 0, challengeResponse.Length);

                    //Get the SecurityResult
                    if (ReadUInt32() == 0) //OK
                    {
                        Log(Logtype.Information, "VNC Authentication successful");
                        return (true);
                    }
                    else //== 1 => Failed
                    {
                        authenticationFailed(); //Handle a failed authentication
                        return (false);
                    }

                    #endregion
            }
        }

        /// <summary>
        /// Handles a failed authentication
        /// </summary>
        private void authenticationFailed()
        {
            if (Properties.RfbClientVersion2.Minor > 7) //In Version 3,8 and higher, get the Reason
            {
                //Get the Failure Reason Lenght
                UInt32 failLenght = ReadUInt32();

                //Get the Failure Reason Text
                Byte[] recData = new Byte[failLenght];
                int bytes = _DataStream.Read(recData, 0, recData.Length);

                Log(Logtype.User, "Authentication failed: " + System.Text.Encoding.ASCII.GetString(recData, 0, bytes));
                if (ConnectionFailed != null)
                {
                    ConnectionFailed(null,
                                     new ConnectionFailedEventArgs("Authentication failed",
                                                                   System.Text.Encoding.ASCII.GetString(recData, 0, bytes),
                                                                   0));
                }
            }
            else //Version 3.7 and below
            {
                Log(Logtype.Warning, "Authentication failed: Password wrong?");
                if (ConnectionFailed != null)
                {
                    ConnectionFailed(null,
                                     new ConnectionFailedEventArgs("Authentication failed",
                                                                   "Authentication failed; using wrong password?",
                                                                   0));
                }
            }
        }

        /// <summary>
        /// Sends the SharedFlag to the Server (sse. 6.3.1)
        /// </summary>
        private bool SendClientSharedFlag()
        {
            try
            {
                Log(Logtype.Information, "Send ClientSharedFlag (aka ClientInit)");

                //Send the Shared-Flag (ClientInit)
                Byte[] sharedFlag = new Byte[1] { Properties.SharedFlag ? (byte)1 : (byte)0 };
                _DataStream.Write(sharedFlag, 0, sharedFlag.Length); //Send the SharedFlag

                return (true);
            }
            catch (Exception ea)
            {
                Log(Logtype.Error, "SendClientSharedFlag failed: " + ea);
                return(false);
            }
        }

        /// <summary>
        /// Receive the ServerInit (see 6.3.2)
        /// </summary>
        private bool ReceiveServerInit()
        {
            try
            {
                Log(Logtype.Information, "Receive Server Initialisation Parameter");

                //Receive the ServerInit
                //Framebuffer Width
                Properties.FramebufferWidth = ReadUInt16();

                //Framebuffer Height
                Properties.FramebufferHeight = ReadUInt16();

                //Set how many byte a Row have
                _BackBuffer2RawStride = (Properties.FramebufferWidth * _BackBuffer2PixelFormat.BitsPerPixel + 7) / 8;

                //Initialize the Backend-Backbuffer with the correct size
                //_BackBuffer2PixelData = new byte[_BackBuffer2RawStride * Properties.FramebufferHeight];

                //Server Pixel Format
                PixelFormat newPxf = new PixelFormat(ReadByte(),
                                                     ReadByte(),
                                                     ReadBool(),
                                                     ReadBool(),
                                                     ReadUInt16(),
                                                     ReadUInt16(),
                                                     ReadUInt16(),
                                                     ReadByte(),
                                                     ReadByte(),
                                                     ReadByte());
                Properties.PxFormat = newPxf;

                ReadBytePadding(3);

                //Name Lenght
                UInt32 nameLenght = ReadUInt32();

                //Name String
                Byte[] recData = new Byte[nameLenght];
                int bytes = _DataStream.Read(recData, 0, recData.Length);
                Properties.ConnectionName = System.Text.Encoding.ASCII.GetString(recData, 0, recData.Length);

                Log(Logtype.Debug, "Server Initialisationparameter Received. w:" + Properties.FramebufferWidth + " h:" + Properties.FramebufferHeight +
                                   " bbp:" + Properties.PxFormat.BitsPerPixel + " dep:" + Properties.PxFormat.Depth + " big:" + Properties.PxFormat.BigEndianFlag +
                                   " true:" + Properties.PxFormat.TrueColourFlag + " rmax:" + Properties.PxFormat.RedMax + " gmax:" + Properties.PxFormat.GreenMax +
                                   " bmax:" + Properties.PxFormat.BlueMax + " rsh:" + Properties.PxFormat.RedShift + " gsh:" + Properties.PxFormat.GreenShift +
                                   " bsh:" + Properties.PxFormat.BlueShift + " Remotename: " + Properties.ConnectionName);

                return(true);
            }
            catch (Exception ea)
            {
                Log(Logtype.Error, "Error during receiving ServerInit:" + ea.ToString());
                return (false);
            }
        }
        
        #endregion

		  //#region Screenhandling
		  //public byte[] GetScreen()
		  //{
		  //	 return (_BackBuffer2PixelData);
		  //}
		  //#endregion

        #region Server to Client Messages

        /// <summary>
        /// Waits for Data from the Server
        /// </summary>
        private void Receiver(object sender, DoWorkEventArgs e)
        {
            while (!_DisconnectionInProgress) //Wait for ServerMessages until the Client (or Server) wants to disconnect
            {
                ServerMessageType srvMsg = GetServerMessageType();
                switch (srvMsg)
                {
                    default:
                        Log(Logtype.Warning, "Receiving unsupported Messagetype");
                        if (NotSupportedServerMessage != null)
                        {
                            NotSupportedServerMessage(null, new NotSupportedServerMessageEventArgs(srvMsg.ToString()));
                        }
                        break;
                    case ServerMessageType.Unknown:
                        Log(Logtype.Warning, "Receiving unknown Messagetype");
                        if (NotSupportedServerMessage != null)
                        {
                            NotSupportedServerMessage(null, new NotSupportedServerMessageEventArgs("Unknown Servermessagetype"));
                        }
                        break;
                    case ServerMessageType.Bell:
                        Log(Logtype.Information, "Receiving Bell");
                        ReceiveBell();
                        break;
                    case ServerMessageType.SetColourMapEntries:
                        Log(Logtype.Debug, "Receiving SetColourMapEntries");
                        ReceiveSetColourMapEntries();
                        break;
                    case ServerMessageType.FramebufferUpdate:
                        Log(Logtype.Debug, "Receiving FramebufferUpdate");
                        ReceiveFramebufferUpdate();
                        break;
                    case ServerMessageType.ServerCutText:
                        Log(Logtype.Debug, "Receiving ServerCutText");
                        ReceiveServerCutText();
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the ServerMessageType by the Next Byte
        /// </summary>
        /// <returns></returns>
        private ServerMessageType GetServerMessageType()
        {
            var recData = new Byte[1];
            _DataStream.Read(recData, 0, 1);
            return (GetServerMessageType(recData[0]));
        }

        /// <summary>
        /// Gets the ServerMessageType by the Next Byte
        /// </summary>
        /// <returns></returns>
        private ServerMessageType GetServerMessageType(Byte ServerMessageId)
        {
            switch (ServerMessageId)
            {
                default:
                    return (ServerMessageType.Unknown);
                case 0:
                    return (ServerMessageType.FramebufferUpdate);
                case 1:
                    return (ServerMessageType.SetColourMapEntries);
                case 2:
                    return (ServerMessageType.Bell);
                case 3:
                    return (ServerMessageType.ServerCutText);
                case 249:
                    return (ServerMessageType.OLIVE_Call_Control);
                case 250:
                    return (ServerMessageType.Colin_dean_xvp);
                case 252:
                    return (ServerMessageType.tight);
                case 253:
                    return (ServerMessageType.gii);
                case 127:
                case 254:
                    return (ServerMessageType.VMWare);
                case 255:
                    return (ServerMessageType.Anthony_Liguori);
            }
        }
        
        /// <summary>
        /// Receives ne Frames after a Framebufferupdaterquest (see 6.5.1)
        /// </summary>
        private void ReceiveFramebufferUpdate()
        {
				//_LastReceiveTimer.Stop(); //Stop the Timer for Frameupdates

            ReadBytePadding(1);
            
            UInt16 rectCount = ReadUInt16();

            Log(Logtype.Debug, "FramebufferUpdate received. Frames: " + rectCount);
				var rects = new List<RfbRectangle>(rectCount);

            for (int i = 0; i < rectCount; i++)
            {
                RfbRectangle rRec = new RfbRectangle(ReadUInt16(), ReadUInt16(), ReadUInt16(), ReadUInt16(), ReadByte(4));
                rRec.PixelData = ReceiveRectangleData(rRec.EncodingType, rRec.Width, rRec.Height);

                //Read the Pixelinformation, depending on EncryptionType and report it to the UI-Mainthread
					 if (rRec.PixelData != null)
						 rects.Add(rRec);
            }

				 _Receiver.ReportProgress(0, rects);

            _DataStream.Flush();

				//_LastReceiveTimer.Start(); //Start the Timer for Frameupdates
        }


        private void ReceiveSetColourMapEntries()
        {
            DateTime start = DateTime.Now;
            Log(Logtype.Debug, "Receive ColorMapEntries started");

            UInt16 firstColor = ReadUInt16();
            UInt16 colorCount = ReadUInt16(); 

            for (int i = 0; i < colorCount; i++)
            {
                _ColorMap[firstColor + i, 0] = ReadUInt16();
                _ColorMap[firstColor + i, 1] = ReadUInt16();
                _ColorMap[firstColor + i, 2] = ReadUInt16(); 
            }

            _DataStream.Flush();

            Log(Logtype.Debug, "Receiving ColormapEntries finished. Duration: " + DateTime.Now.Subtract(start).TotalMilliseconds + "ms");
        }

        /// <summary>
        /// Beeps the Client (see 6.5.3)
        /// </summary>
        private void ReceiveBell()
        {
            Log(Logtype.Information, "DingDong");
            Console.Beep(1000,500);
            Console.Beep(800, 500);
        }

        /// <summary>
        /// Copys a text to the local cache (see 6.5.4)
        /// </summary>
        private void ReceiveServerCutText()
        {
            var recData = new Byte[3];
            _DataStream.Read(recData, 0, recData.Length);

            recData = new Byte[4];
            _DataStream.Read(recData, 0, recData.Length);

            UInt32 textLength = Helper.ConvertToUInt32(recData, _Properties.PxFormat.BigEndianFlag);

            recData = new Byte[textLength];
            _DataStream.Read(recData, 0, recData.Length);

            string cacheText = System.Text.Encoding.ASCII.GetString(recData);

            if (ServerCutText != null)
            {
                ServerCutTextEventArgs sct = new ServerCutTextEventArgs(cacheText);
            }

            Log(Logtype.Debug, "New ServerCutText received. Text: " + cacheText);

            //Call Helperthread with STA to set the Clipboard-Text
            System.Threading.Thread cacheSetterThread = new Thread(CacheSetter);
            cacheSetterThread.SetApartmentState(ApartmentState.STA);
            cacheSetterThread.Start((object) cacheText);

            //Clipboard.SetText(cacheText);
        }

        /// <summary>
        /// Helper Thread with ApartmentState.STA for setting the local clipboard
        /// </summary>
        /// <param name="cacheText"></param>
        public void CacheSetter(object cacheText)
        {
            try
            {
                //Always fails... Some people say it is a bug in the WPF Clipboard handler. This works anyway but always results in an exception too. This should be changed somewhen.
                Clipboard.SetText(cacheText.ToString()); 
            }
            catch (Exception)
            {
                //Happens every time
            }
        }

        #endregion

        #region Client to Server Messages
        /// <summary>
        /// (see 6.4.1)
        /// </summary>
        private void SendSetPixelFormat(PixelFormat pxFormat)
        {
            Log(Logtype.Debug, "Send SetPixelFormat");

            Byte[] data = new Byte[20];
            data[0] = 0; //SetPixel

            pxFormat.getPixelFormat(_Properties.PxFormat.BigEndianFlag).CopyTo(data, 4);
            _DataStream.Write(data, 0, data.Length);

            _DataStream.Flush();
        }

        /// <summary>
        /// SetEncodings (see 6.4.2)
        /// </summary>
        private void SendSetEncodings()
        {
            Log(Logtype.Debug, "Send SetEncoding (" + _EncodingDetails.Count + ")");

            Byte[] data = new Byte[4];
            data[0] = 2;
            
            //Send Count of Supported Encodings by this client
            Helper.ConvertToByteArray((UInt16)_EncodingDetails.Count, _Properties.PxFormat.BigEndianFlag).CopyTo(data, 2);
            _DataStream.Write(data, 0, data.Length);

            //Send Encoding Details for each supported Encoding
            foreach(KeyValuePair<RfbEncoding, RfbEncodingDetails> kvp in _EncodingDetails)
            {
                Byte[] sendByte = Helper.ConvertToByteArray(kvp.Value.Id, _Properties.PxFormat.BigEndianFlag);
                _DataStream.Write(new Byte[4] { sendByte[0],sendByte[1], sendByte[2], sendByte[3] }, 0, 4);
            }

            _DataStream.Flush();
        }

        /// <summary>
        /// Sends a Request for Screenupdate (see 6.4.3)
        /// </summary>
        /// <param name="isIncremental"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void SendFramebufferUpdateRequest(bool isIncremental, UInt16 posX, UInt16 posY, UInt16 width, UInt16 height)
        {
            Log(Logtype.Debug, "Send SetFramebufferUpdateRequest with the following parameters: Incr:" + isIncremental + ", PosX:" + posX + ", PosY:" + posY + ", Width:" + width + ", Height:" + height);

            Byte[] data = new Byte[10];
            data[0] = 3;
            data[1] = (Byte)(isIncremental ? 1 : 0);
            Helper.ConvertToByteArray(posX, _Properties.PxFormat.BigEndianFlag).CopyTo(data, 2);
            Helper.ConvertToByteArray(posY, _Properties.PxFormat.BigEndianFlag).CopyTo(data, 4);
            Helper.ConvertToByteArray(width, _Properties.PxFormat.BigEndianFlag).CopyTo(data, 6);
            Helper.ConvertToByteArray(height, _Properties.PxFormat.BigEndianFlag).CopyTo(data, 8);

            _DataStream.Write(data, 0, data.Length);

            _DataStream.Flush();
        }
        
        /// <summary>
        /// Sends a KeyEvent to the Server (see 6.4.4)
        /// </summary>
        /// <param name="pressedKey">The pressed Character</param>
        /// <param name="isKeyDown">Is the Key currently pressed</param>
        private void SendKeyEvent(System.Windows.Input.KeyEventArgs e)
        {
            SendKeyEvent(e.Key, e.IsDown);
        }

        /// <summary>
        /// Send a Key in pressed or released state (see http://www.cl.cam.ac.uk/~mgk25/ucs/keysymdef.h)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="isDown"></param>
        private void SendKeyEvent(Key e, bool isDown)
        {
            if (_IsConnected == false) return;

            Log(Logtype.Debug, "Sending Key: " + e.ToString());

            UInt32 keyCode = 0;

            Log(Logtype.Information, "Sending " + e.ToString());

            switch (e)
            {
                #region Specialsigns

                case Key.LeftShift:
                    keyCode = 0x0000ffe1;
                    break;

                case Key.Space:
                    keyCode = 0x00000020;
                    break;
                case Key.Tab:
                    keyCode = 0x0000FF09;
                    break;
                case Key.Enter:
                    keyCode = 0x0000FF0D;
                    break;
                case Key.Escape:
                    keyCode = 0x0000FF1B;
                    break;
                case Key.Home:
                    keyCode = 0x0000FF50;
                    break;
                case Key.Left:
                    keyCode = 0x0000FF51;
                    break;
                case Key.Up:
                    keyCode = 0x0000FF52;
                    break;
                case Key.Right:
                    keyCode = 0x0000FF53;
                    break;
                case Key.Down:
                    keyCode = 0x0000FF54;
                    break;
                case Key.PageUp:
                    keyCode = 0x0000FF55;
                    break;
                case Key.PageDown:
                    //case Key.Next:
                    keyCode = 0x0000FF56;
                    break;
                case Key.End:
                    keyCode = 0x0000FF57;
                    break;
                case Key.Insert:
                    keyCode = 0x0000FF63;
                    break;
                case Key.Delete:
                    keyCode = 0x0000FFFF;
                    break;

                case Key.CapsLock:
                    keyCode = 0x0000FFE5;
                    break;
                case Key.LeftAlt:
                    keyCode = 0x0000FFE9;
                    break;
                case Key.RightAlt:
                    keyCode = 0x0000FFEA;
                    break;
                case Key.LeftCtrl:
                    keyCode = 0x0000FFE3;
                    break;
                case Key.RightCtrl:
                    keyCode = 0x0000FFE4;
                    break;
                case Key.LWin:
                    keyCode = 0x0000FFEB;
                    break;
                case Key.RWin:
                    keyCode = 0x0000FFEC;
                    break;
                case Key.Apps:
                    keyCode = 0x0000FFEE;
                    break;

                case Key.F1:
                    keyCode = 0x0000FFBE;
                    break;
                case Key.F2:
                    keyCode = 0x0000FFBF;
                    break;
                case Key.F3:
                    keyCode = 0x0000FFC0;
                    break;
                case Key.F4:
                    keyCode = 0x0000FFC1;
                    break;
                case Key.F5:
                    keyCode = 0x0000FFC2;
                    break;
                case Key.F6:
                    keyCode = 0x0000FFC3;
                    break;
                case Key.F7:
                    keyCode = 0x0000FFC4;
                    break;
                case Key.F8:
                    keyCode = 0x0000FFC5;
                    break;
                case Key.F9:
                    keyCode = 0x0000FFC6;
                    break;
                case Key.F10:
                    keyCode = 0x0000FFC7;
                    break;
                case Key.F11:
                    keyCode = 0x0000FFC8;
                    break;
                case Key.F12:
                    keyCode = 0x0000FFC9;
                    break;

                case Key.NumLock:
                    keyCode = 0x0000FF7F;
                    break;
                case Key.NumPad0:
                    keyCode = 0x0000FFB0;
                    break;
                case Key.NumPad1:
                    keyCode = 0x0000FFB1;
                    break;
                case Key.NumPad2:
                    keyCode = 0x0000FFB2;
                    break;
                case Key.NumPad3:
                    keyCode = 0x0000FFB3;
                    break;
                case Key.NumPad4:
                    keyCode = 0x0000FFB4;
                    break;
                case Key.NumPad5:
                    keyCode = 0x0000FFB5;
                    break;
                case Key.NumPad6:
                    keyCode = 0x0000FFB6;
                    break;
                case Key.NumPad7:
                    keyCode = 0x0000FFB7;
                    break;
                case Key.NumPad8:
                    keyCode = 0x0000FFB8;
                    break;
                case Key.NumPad9:
                    keyCode = 0x0000FFB9;
                    break;

                #endregion
            }

            if (keyCode == 0) //Was not a Special Sign
            {
                //Get the Keycode
                int key = KeyInterop.VirtualKeyFromKey(e);

                //Get the related Char
                char enteredChar = System.Text.Encoding.ASCII.GetChars(Helper.ConvertToByteArray(key, true))[0];

                //Offset for Keycode of pressed Sign
                UInt32 offset = 0;

                Regex rgExAZ = new Regex("[A-Z]"); //for A-Z and a-z
                Regex rgEx09 = new Regex("[0-9]"); //For 0-9

                #region Shift-Modifiers A-Z
                if (rgExAZ.IsMatch(enteredChar.ToString())) //If it should be a small letter
                {
                    if (Keyboard.Modifiers != ModifierKeys.Shift)
                    {
                        key += 32;
                        enteredChar = System.Text.Encoding.ASCII.GetChars(Helper.ConvertToByteArray(key, true))[0];
                    }
                }
                #endregion

                #region Shift-Modifiers 0-9
                else if (rgEx09.IsMatch(enteredChar.ToString()) && Keyboard.Modifiers != ModifierKeys.Shift) //It is a number
                {
                    //Do nothing
                }
                #endregion

                #region Offset for unknown characters
                else
                {
                    offset = 0xFF00; //65280
                }
                #endregion
                keyCode = (UInt32)key + offset;
            }

            Byte[] data = new Byte[8];
            data[0] = 4;

            //Press or Release the Key
            if (isDown == true)
                data[1] = 1;
            else
                data[1] = 0;

            Helper.ConvertToByteArray(keyCode, _Properties.PxFormat.BigEndianFlag).CopyTo(data, 4);
            _DataStream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Send a KeyEvent to the Server based on a sign (see 6.4.4)
        /// </summary>
        /// <param name="sign">the Sign to send like typing a key on a keyboard</param>
        private void SendSignEvent(char sign)
        {
            if (_IsConnected == false) return;

            Log(Logtype.Information, "Sending " + sign);

            Byte[] data = new Byte[8];
            data[0] = 4;

            //Press or Release the Key (currently defiend as released)
            data[1] = 1;

            Helper.ConvertToByteArray(_KeyCodes[sign], _Properties.PxFormat.BigEndianFlag).CopyTo(data, 4);
            _DataStream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ButtonMask">bit1=left; bit2=middle; bit3=right; bit4=wheelup; bit5=wheeldown</param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        private void SendPointerEvent(UInt16 posX, UInt16 posY, Byte ButtonMask)
        {
            if (_IsConnected == false) return;

            Log(Logtype.Debug, "Send Pointer: Button:" + ButtonMask + ", PosX:" + posX + ", PosY:" + posY);

            Byte[] data = new Byte[6];
            data[0] = 5;
            data[1] = ButtonMask;
            Helper.ConvertToByteArray(posX, _Properties.PxFormat.BigEndianFlag).CopyTo(data, 2);
            Helper.ConvertToByteArray(posY, _Properties.PxFormat.BigEndianFlag).CopyTo(data, 4);
            
            _DataStream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Sends a Text to the Cache of the Server (see 6.4.6)
        /// </summary>
        /// <param name="text"></param>
        private void SendClientCutText(string text)
        {
            return;

            if (_IsConnected == false) return;

            Log(Logtype.Debug, "Send ClientCutText. Text: " + text);

            Byte[] textData = System.Text.Encoding.ASCII.GetBytes(text);

            Byte[] data = new Byte[8 + textData.Length];
            data[0] = 6;
            Helper.ConvertToByteArray((UInt32)textData.Length, _Properties.PxFormat.BigEndianFlag).CopyTo(data, 4);
            textData.CopyTo(data, 8);

            _DataStream.Write(data, 0, data.Length);
        }
        #endregion

        #endregion

        #region Communicationhelper
        private void SetSecurityType(byte[] responseData)
        {
            int secTypeNo = responseData[3] + responseData[2] * 2 + responseData[1] * 4 + responseData[0] * 8;
            switch (secTypeNo)
            {
                default:
                    Properties.RfbSecurityType = SecurityType.Unknown;
                    break;
                case 0:
                    Properties.RfbSecurityType = SecurityType.Invalid;
                    break;
                case 1:
                    Properties.RfbSecurityType = SecurityType.None;
                    break;
                case 2:
                    Properties.RfbSecurityType = SecurityType.VNCAuthentication;
                    break;
                case 5:
                    Properties.RfbSecurityType = SecurityType.RA2;
                    break;
                case 6:
                    Properties.RfbSecurityType = SecurityType.RA2ne;
                    break;
                case 16:
                    Properties.RfbSecurityType = SecurityType.Tight;
                    break;
                case 17:
                    Properties.RfbSecurityType = SecurityType.UltraVNC;
                    break;
                case 18:
                    Properties.RfbSecurityType = SecurityType.TLS;
                    break;
                case 19:
                    Properties.RfbSecurityType = SecurityType.VeNCrypt;
                    break;
                case 20:
                    Properties.RfbSecurityType = SecurityType.GTK_VNC_SASL;
                    break;
                case 21:
                    Properties.RfbSecurityType = SecurityType.MD5_hash_authentication;
                    break;
                case 22:
                    Properties.RfbSecurityType = SecurityType.Colin_Dean_xvp;
                    break;
            }

            Log(Logtype.Information, "Securitytype is " + Properties.RfbSecurityType.ToString());
        }

        private Byte[] ReceiveRectangleData(Byte[] encryptionType, UInt16 width, UInt16 height)
        {
				//Byte[, ,] retValue = new Byte[width, height, 4]; //3rd Dimension is the Color (RBGA)
            DateTime start = DateTime.Now;

            try
            {
                List<Byte> test = new List<byte>();

                Log(Logtype.Information, "Receiving RectangleData. Encodingtype is " + encryptionType[0] + "; Framesize: " + width + "x" + height);

                UInt32 encrNr = Helper.ConvertToUInt32(encryptionType, _Properties.PxFormat.BigEndianFlag);
                switch (encrNr)
                {
                    #region X Not Supported

                    default:
                    case 1: //CopyRect
                    case 2: //RRE
                    case 5: //Hextile
                    case 16: //ZRLE
                        Log(Logtype.Information, "Encodingtype " + encrNr + " currently not supported");
                        break;

                    #endregion

                    #region 0 RAW

                    case 0: //Raw

                        _Properties.EncodingType = RfbEncoding.Raw_ENCODING;

                        Byte[] myData = new Byte[width * height * 4];
								Console.WriteLine("!!!!! W: {0}", width);

                        int readCount = 0;

								while (readCount != myData.Length) //Get all Bytes
								{
									readCount += _DataStream.Read(myData, readCount, myData.Length - readCount);
									//Debug.WriteLine("{0}/{1}", readCount, myData.Length);
								}

								return myData;

                        //Encode the received data
								//retValue = RawRectangle.EncodeRawRectangle(height, width, myData);
								//break;
                    #endregion

                    #region Pseudo DesktopSize
                    case 4294967073: //See 6.7.2
                        //Set a new Backbuffersize
								//_BackBuffer2PixelData = new byte[width*height*4];

								Console.WriteLine("!!!!! INIT W: {0}", width);
                        //Request new Screen
                        SendFramebufferUpdateRequest(true, 0, 0, width, height);
                        break;
                    #endregion
                }

                
            }
            catch (Exception ea)
            {
                Log(Logtype.Error, "Error on receiving a RawRectangle: " + ea.ToString());
            }

            Log(Logtype.Debug, "Finished receiving RectangleData. Duration: " + DateTime.Now.Subtract(start).TotalMilliseconds + "ms");
				//return (retValue);
				return null;
        }

        private Boolean ReadBool()
        {
            try
            {
                if (ReadByte(1)[0] == 0)
                    return (false);
                else
                    return(true);
            }
            catch(Exception)
            {
                Log(Logtype.Warning, "Remoteconnection closed by Server");
                return (false);
            }
        }

        private Byte ReadByte()
        {
            return (ReadByte(1)[0]);
        }

        private Byte[] ReadByte(UInt64 count)
        {
            try
            {
                Byte[] recData = new Byte[count];
                _DataStream.Read(recData, 0, recData.Length);

					 //foreach (var b in recData)
					 //	Debug.Write(string.Format("{0:X2} ", b));

					 //Debug.Write("-> ");

					 //foreach (var b in recData)
					 //	Debug.Write(string.Format("{0} ", b));

					 //Debug.Write("-> ");

					 //foreach (var b in recData)
					 //	Debug.Write(string.Format("{0} ", (char)b));

					 //Debug.WriteLine("");

                return (recData);
            }
            catch(Exception)
            {
                Log(Logtype.Warning, "Remoteconnection closed by Server");
                return (new Byte[0]);
            }
        }

        private UInt16 ReadUInt16()
        {
            return (ReadUInt16(false));
        }

        private UInt16 ReadUInt16(bool isBigEndian)
        {
            try
            {
                Byte[] recData = ReadByte(2);
                return (Helper.ConvertToUInt16(recData, isBigEndian));
            }
            catch (Exception)
            {
                Log(Logtype.Warning, "Remoteconnection closed by Server");
                return (0);
            }
        }

        private UInt32 ReadUInt32()
        {
            return (ReadUInt32(false));
        }

        private UInt32 ReadUInt32(bool isBigEndian)
        {
            try
            {
                Byte[] recData = ReadByte(4);
                return (Helper.ConvertToUInt32(recData, isBigEndian));
            }
            catch (Exception)
            {
                Log(Logtype.Warning, "Remoteconnection closed by Server");
                return (0);
            }

        }

        private UInt64 ReadUInt64()
        {
            throw new NotImplementedException("Not done");
        }

        private void ReadBytePadding(UInt64 count)
        {
            try
            {
                ReadByte(count);
            }
            catch(Exception)
            {
                Log(Logtype.Warning, "Remoteconnection closed by Server");
            }
        }

        #endregion

        #region Logging


#if logging
		  private Queue<string> LogQueue = new Queue<string>();

		  private void Log(Logtype lt, string lm)
		  {
			  if (LogQueue.Count < Int32.MaxValue)
			  {
				  if (_LoggingLevel <= lt)
					  LogQueue.Enqueue(DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString() + " " + lt.ToString() + "\t" + lm);

				  if (LogMessage != null)
				  {
					  LogMessage(null, new LogMessageEventArgs(lm, DateTime.Now, lt));
				  }
			  }
		  }

		  void tmrLog_Tick(object sender, EventArgs e)
		  {
			  if (_LoggingLevel == Logtype.None)
				  return;

			  System.IO.StreamWriter sW = new System.IO.StreamWriter(_LoggingPath, true);

			  while (LogQueue.Count > 0)
			  {
				  sW.WriteLine(LogQueue.Dequeue());
			  }
			  sW.Close();
		  }

		  private DispatcherTimer tmrLog = new DispatcherTimer();
#else
		  private void Log(Logtype lt, string lm) { }
#endif
        #endregion

        #region Public Methods
        public void refreshScreen()
        {
            SendFramebufferUpdateRequest(true, 0, 0, _Properties.FramebufferWidth, _Properties.FramebufferHeight);
        }

        /// <summary>
        /// Send a pressed Key to the Server (i.e. Enter, Tab, Cntr, Alt etc.)
        /// </summary>
        /// <param name="e"></param>
        public void sendKey(System.Windows.Input.KeyEventArgs e)
        {
            SendKeyEvent(e);
        }

        /// <summary>
        /// Send a Sign to the Server
        /// </summary>
        /// <param name="sign"></param>
        public void sendSign(char sign)
        {
            SendSignEvent(sign);
        }

        /// <summary>
        /// Sends a Mousemove to the Server
        /// </summary>
        /// <param name="posX">X-Position of the Mouse</param>
        /// <param name="posY">Y-Position of the Mouse</param>
        public void sendMousePosition(UInt16 posX, UInt16 posY)
        {
            SendPointerEvent(posX, posY, 0);
        }

        /// <summary>
        /// Sends a Click to the Server
        /// </summary>
        /// <param name="posX">X-Position of the Mouse</param>
        /// <param name="posY">Y-Position of the Mouse</param>
        /// <param name="button">1=left 4=right 2=middle 8=wheelup 16=wheeldown</param>
        public void sendMouseClick(UInt16 posX, UInt16 posY, byte button)
        {
            SendPointerEvent(posX, posY, button);
        }
        
        /// <summary>
        /// Sends a Saved Clipboardtext to the Server
        /// </summary>
        /// <param name="text"></param>
        public void sendClientCutText(string text)
        {
            SendClientCutText(text);
        }

        /// <summary>
        /// Send a special Key Combination to the Server
        /// </summary>
        /// <param name="keyComb"></param>
        public void sendKeyCombination(KeyCombination keyComb)
        {
            Key aKey1 = default(Key);
            Key aKey2 = default(Key);
            Key aKey3 = default(Key);

            switch (keyComb)
            {
                case KeyCombination.AltF4:
                    aKey1 = Key.LeftAlt;
                    aKey2 = Key.F4;
                    break;
                case KeyCombination.AltTab:
                    aKey1 = Key.LeftAlt;
                    aKey2 = Key.Tab;
                    break;
                case KeyCombination.CapsLock:
                    aKey1 = Key.CapsLock;
                    break;
                case KeyCombination.CtrlAltDel:
                    aKey1 = Key.LeftCtrl;
                    aKey2 = Key.LeftAlt;
                    aKey3 = Key.Delete;
                    break;
                case KeyCombination.CtrlAltEnd:
                    aKey1 = Key.LeftCtrl;
                    aKey2 = Key.LeftAlt;
                    aKey3 = Key.End;
                    break;
                case KeyCombination.CtrlEsc:
                    aKey1 = Key.LeftCtrl;
                    aKey2 = Key.Escape;
                    break;
                case KeyCombination.NumLock:
                    aKey1 = Key.NumLock;
                    break;
                case KeyCombination.Print:
                    aKey1 = Key.PrintScreen;
                    break;
                case KeyCombination.Scroll:
                    aKey1 = Key.Scroll;
                    break;
            }

            SendKeyEvent(aKey1, true);
            if (aKey2 != default(Key)) SendKeyEvent(aKey2, true);
            if (aKey3 != default(Key)) SendKeyEvent(aKey3, true);

            System.Threading.Thread.Sleep(100);

            if (aKey3 != default(Key)) SendKeyEvent(aKey3, false);
            if (aKey2 != default(Key)) SendKeyEvent(aKey2, false);
            SendKeyEvent(aKey1, false);
        }

        #endregion

        #region Properties
        public ConnectionProperties Properties
        {
            get { return (_Properties); }
            set { _Properties = value; }
        }

#if logging
		  /// <summary>
		  /// How much logging should be done? None=Disable
		  /// </summary>
		  public Logtype LoggingLevel
		  {
			  get { return _LoggingLevel; }
			  set { _LoggingLevel = value; }
		  }

		  /// <summary>
		  /// Where should the log be saved?
		  /// </summary>
		  public string LoggingPath
		  {
			  get { return _LoggingPath; }
			  set { _LoggingPath = value; }
		  } 
#endif
        
        public bool IsConnected 
        { 
            get { return (_IsConnected); }
        }

        #endregion
    }
}