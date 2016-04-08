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
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace beeVNC
{
	internal class ConnectionFailedEventArgs : EventArgs
	{
		private string _ShortMessage = "";
		private string _FullMessage = "";
		private int _ErrorNumber = 0;

		public ConnectionFailedEventArgs(string sMsg, string fMsg, int eNo)
		{
			ShortMessage = sMsg;
			FullMessage = fMsg;
			ErrorNumber = eNo;
		}

		public string ShortMessage
		{
			get { return (_ShortMessage); }
			set { _ShortMessage = value; }
		}

		public string FullMessage
		{
			get { return (_FullMessage); }
			set { _FullMessage = value; }
		}

		public int ErrorNumber
		{
			get { return (_ErrorNumber); }
			set { _ErrorNumber = value; }
		}
	}

	internal class NotSupportedServerMessageEventArgs : EventArgs
	{
		private string _MessageTypeName = "";
		private byte _MessageId = 0;

		public NotSupportedServerMessageEventArgs(string msgType)
		{
			MessageTypeName = msgType;
		}

		public NotSupportedServerMessageEventArgs(string msgType, byte msgId)
		{
			MessageTypeName = msgType;
			MessageId = msgId;
		}

		public string MessageTypeName
		{
			get { return (_MessageTypeName); }
			set { _MessageTypeName = value; }
		}

		public byte MessageId
		{
			get { return (_MessageId); }
			set { _MessageId = value; }
		}
	}

	public class LogMessageEventArgs : EventArgs
	{
		private string _LogMessage = "";
		private DateTime _LogTime;
		private Logtype _LogType;

		public LogMessageEventArgs()
		{
		}

		public LogMessageEventArgs(string logMessage, DateTime logTime, Logtype logType)
		{
			_LogMessage = logMessage;
			_LogTime = logTime;
			_LogType = logType;
		}

		public string LogMessage
		{
			get { return (_LogMessage); }
			set { _LogMessage = value; }
		}

		public DateTime LogTime
		{
			get { return (_LogTime); }
			set { _LogTime = value; }
		}

		public Logtype LogType
		{
			get { return (_LogType); }
			set { _LogType = value; }
		}
	}

	public class ScreenUpdateEventArgs : EventArgs
	{
		public ScreenUpdateEventArgs() { }

		public List<RfbRectangle> Rects { get; set; }

		//public byte[] PixelData { get; set; }

		//public UInt16 X { get; set; }

		//public UInt16 Y { get; set; }

		//public UInt16 Width { get; set; }

		//public UInt16 Height { get; set; }
	}

	internal class ScreenResolutionChangeEventArgs : EventArgs
	{
		private int _ResX = 0;
		private int _ResY = 0;

		public ScreenResolutionChangeEventArgs(int resX, int resY)
		{
			ResX = resX;
			ResY = resY;
		}

		public int ResX
		{
			get { return (_ResX); }
			set { _ResX = value; }
		}

		public int ResY
		{
			get { return (_ResY); }
			set { _ResY = value; }
		}
	}

	public class ServerCutTextEventArgs : EventArgs
	{
		private string _Text = "";

		public ServerCutTextEventArgs(string text)
		{
			_Text = text;
		}

		public string Text
		{
			get { return (_Text); }
			set { _Text = value; }
		}
	}

}
