/************************************
 * Developed by Kristian Reukauff
 * License and Project:
 * https://beevnc.codeplex.com/
 * Published under NewBSD-License
 * without any warrenties
 * provided "as is"
 ************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace beeVNC
{
	internal class ConnectionProperties
	{
		private string _RfbServerVersion = "";
		private Version _RfbServerVersion2 = new Version();
		private string _RfbClientVersion = "";
		private Version _RfbClientVersion2 = new Version();
		private SecurityType _RfbSecurityType = SecurityType.Invalid;

		private UInt16 _FramebufferWidth = 0;
		private UInt16 _FramebufferHeight = 0;
		private PixelFormat _PxFormat = new PixelFormat();
		private string _ConnectionName = "";
		private RfbEncoding _EncodingType;

		private string _Server = "";
		private int _Port = 5900;
		private string _Password = "";
		private bool _SharedFlag = true;

		public ConnectionProperties()
		{
		}

		public ConnectionProperties(string server, string password, int port)
		{
			RfbClientVersion = "RFB 003.008\n";
			Server = server;
			Password = password;
			Port = port;
		}

		public string RfbServerVersion
		{
			get { return (_RfbServerVersion); }
			set
			{
				_RfbServerVersion = value;

				string strVer = value.Substring(4).Replace("\n", "");
				string[] strVer2 = strVer.Split('.');

				_RfbServerVersion2 = new Version(Convert.ToInt16(strVer2[0]), Convert.ToInt16(strVer2[1]));
			}
		}

		public Version RfbServerVersion2
		{
			get { return (_RfbServerVersion2); }
		}

		public string RfbClientVersion
		{
			get { return (_RfbClientVersion); }
			set
			{
				_RfbClientVersion = value;

				string strVer = value.Substring(4).Replace("\n", "");
				string[] strVer2 = strVer.Split('.');

				_RfbClientVersion2 = new Version(Convert.ToInt16(strVer2[0]), Convert.ToInt16(strVer2[1]));
			}
		}

		public Version RfbClientVersion2
		{
			get { return (_RfbClientVersion2); }
		}

		public SecurityType RfbSecurityType
		{
			get { return (_RfbSecurityType); }
			set { _RfbSecurityType = value; }
		}

		public UInt16 FramebufferWidth
		{
			get { return (_FramebufferWidth); }
			set { _FramebufferWidth = value; }
		}

		public UInt16 FramebufferHeight
		{
			get { return (_FramebufferHeight); }
			set { _FramebufferHeight = value; }
		}

		public PixelFormat PxFormat
		{
			get { return (_PxFormat); }
			set { _PxFormat = value; }
		}

		public string ConnectionName
		{
			get { return (_ConnectionName); }
			set { _ConnectionName = value; }
		}

		public RfbEncoding EncodingType
		{
			get { return (_EncodingType); }
			set { _EncodingType = value; }
		}

		public string Server
		{
			get { return (_Server); }
			set { _Server = value; }
		}

		public int Port
		{
			get { return (_Port); }
			set { _Port = value; }
		}

		public string Password
		{
			get { return (_Password); }
			set { _Password = value; }
		}

		public bool SharedFlag
		{
			get { return (_SharedFlag); }
			set { _SharedFlag = value; }
		}
	}

	public class PixelFormat
	{
		private Byte _BitsPerPixel = 0;
		private Byte _Depth = 0;
		private Boolean _BigEndianFlag = false;
		private Boolean _TrueColourFlag = false;
		private UInt16 _RedMax = 0;
		private UInt16 _GreenMax = 0;
		private UInt16 _BlueMax = 0;
		private Byte _RedShift = 0;
		private Byte _GreenShift = 0;
		private Byte _BlueShift = 0;
		private Byte[] _Padding = new Byte[3];

		public PixelFormat()
		{
		}

		public PixelFormat(Byte bpp, Byte dp, Boolean bef, Boolean tcf, UInt16 rm, UInt16 gm,
		UInt16 bm, Byte rs, Byte gs, Byte bs)
		{
			BitsPerPixel = bpp;
			Depth = dp;
			BigEndianFlag = bef;
			TrueColourFlag = tcf;
			RedMax = rm;
			GreenMax = gm;
			BlueMax = bm;
			RedShift = rs;
			GreenShift = gs;
			BlueShift = bs;
		}

		public byte[] getPixelFormat(bool bigEndianFlag)
		{
			Byte[] ret = new Byte[16];
			ret[0] = BitsPerPixel;
			ret[1] = Depth;
			ret[2] = Helper.ConvertToByteArray(BigEndianFlag)[0];
			ret[3] = Helper.ConvertToByteArray(TrueColourFlag)[0];
			ret[4] = Helper.ConvertToByteArray(RedMax, bigEndianFlag)[0];
			ret[5] = Helper.ConvertToByteArray(RedMax, bigEndianFlag)[1];
			ret[6] = Helper.ConvertToByteArray(GreenMax, bigEndianFlag)[0];
			ret[7] = Helper.ConvertToByteArray(GreenMax, bigEndianFlag)[1];
			ret[8] = Helper.ConvertToByteArray(BlueMax, bigEndianFlag)[0];
			ret[9] = Helper.ConvertToByteArray(BlueMax, bigEndianFlag)[1];
			ret[10] = RedShift;
			ret[11] = GreenShift;
			ret[12] = BlueShift;
			ret[13] = Padding[0];
			ret[14] = Padding[1];
			ret[15] = Padding[2];

			return (ret);
		}

		public Byte BitsPerPixel
		{
			get { return (_BitsPerPixel); }
			set { _BitsPerPixel = value; }
		}

		public Byte Depth
		{
			get { return (_Depth); }
			set { _Depth = value; }
		}

		public Boolean BigEndianFlag
		{
			get { return (_BigEndianFlag); }
			set { _BigEndianFlag = value; }
		}

		public Boolean TrueColourFlag
		{
			get { return (_TrueColourFlag); }
			set { _TrueColourFlag = value; }
		}

		public UInt16 RedMax
		{
			get { return (_RedMax); }
			set { _RedMax = value; }
		}

		public UInt16 GreenMax
		{
			get { return (_GreenMax); }
			set { _GreenMax = value; }
		}

		public UInt16 BlueMax
		{
			get { return (_BlueMax); }
			set { _BlueMax = value; }
		}

		public Byte RedShift
		{
			get { return (_RedShift); }
			set { _RedShift = value; }
		}

		public Byte GreenShift
		{
			get { return (_GreenShift); }
			set { _GreenShift = value; }
		}

		public Byte BlueShift
		{
			get { return (_BlueShift); }
			set { _BlueShift = value; }
		}

		public Byte[] Padding
		{
			get { return (_Padding); }
			set
			{
				if (value.Length == 3)
					_Padding = value;
			}
		}
	}

	public class RfbRectangle
	{
		public RfbRectangle()
		{
		}

		public RfbRectangle(UInt16 posX, UInt16 posY, UInt16 width, UInt16 height, Byte[] encodingType)
		{
			PosX = posX;
			PosY = posY;
			Width = width;
			Height = height;
			EncodingType = encodingType;
		}

		public UInt16 PosX { get; set; }

		public UInt16 PosY { get; set; }

		public UInt16 Width { get; set; }

		public UInt16 Height { get; set; }

		public Byte[] EncodingType { get; set; }

		public Byte[] PixelData { get; set; }
	}

	public class RfbEncodingDetails
	{
		private Int32 _Id = 0;
		private string _Name = "";
		private UInt16 _Priority = 0;

		public RfbEncodingDetails(Int32 id, string name, UInt16 prio)
		{
			Id = id;
			Name = name;
			Priority = prio;
		}

		public Int32 Id
		{
			get { return (_Id); }
			set { _Id = value; }
		}

		public string Name
		{
			get { return (_Name); }
			set { _Name = value; }
		}

		public UInt16 Priority
		{
			get { return (_Priority); }
			set { _Priority = value; }
		}
	}
}
