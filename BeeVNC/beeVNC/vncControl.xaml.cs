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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace beeVNC
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class vncControl : UserControl
	{
#if logging
		  private Logtype _LoggingLevel = Logtype.None;
        private string _LoggingPath = "C:\\temp\\beevnc.log";
#endif
		private RfbClient _Connection;
		private WriteableBitmap _RemoteScreen;
		private DateTime _LastMouseMove = DateTime.Now;
		private bool _LimitMouseMove = true;
		private WriteableBitmap Bitmap;
		private List<Key> _NonSpecialKeys = new List<Key>(); //A List of all Keys, that are handled by the transparent Textbox

		private string _ServerAddress = ""; //The Remoteserveraddress
		private int _ServerPort = 5900; //The Remoteserverport
		private string _ServerPassword = ""; //The Remoteserverpassword

		public delegate void UpdateScreenCallback(ScreenUpdateEventArgs newScreen);

		public DispatcherTimer tmrEllipse;
		public DispatcherTimer tmrClipboardCheck;
		public DispatcherTimer tmrScreen;

		public vncControl()
		{
			InitializeComponent();

			#region Define NonSpecialKeys

			_NonSpecialKeys.Add(Key.A);
			_NonSpecialKeys.Add(Key.B);
			_NonSpecialKeys.Add(Key.C);
			_NonSpecialKeys.Add(Key.D);
			_NonSpecialKeys.Add(Key.E);
			_NonSpecialKeys.Add(Key.F);
			_NonSpecialKeys.Add(Key.G);
			_NonSpecialKeys.Add(Key.H);
			_NonSpecialKeys.Add(Key.I);
			_NonSpecialKeys.Add(Key.J);
			_NonSpecialKeys.Add(Key.K);
			_NonSpecialKeys.Add(Key.L);
			_NonSpecialKeys.Add(Key.M);
			_NonSpecialKeys.Add(Key.N);
			_NonSpecialKeys.Add(Key.O);
			_NonSpecialKeys.Add(Key.P);
			_NonSpecialKeys.Add(Key.Q);
			_NonSpecialKeys.Add(Key.R);
			_NonSpecialKeys.Add(Key.S);
			_NonSpecialKeys.Add(Key.T);
			_NonSpecialKeys.Add(Key.U);
			_NonSpecialKeys.Add(Key.V);
			_NonSpecialKeys.Add(Key.W);
			_NonSpecialKeys.Add(Key.X);
			_NonSpecialKeys.Add(Key.Y);
			_NonSpecialKeys.Add(Key.Z);

			_NonSpecialKeys.Add(Key.D0);
			_NonSpecialKeys.Add(Key.D1);
			_NonSpecialKeys.Add(Key.D2);
			_NonSpecialKeys.Add(Key.D3);
			_NonSpecialKeys.Add(Key.D4);
			_NonSpecialKeys.Add(Key.D5);
			_NonSpecialKeys.Add(Key.D6);
			_NonSpecialKeys.Add(Key.D7);
			_NonSpecialKeys.Add(Key.D8);
			_NonSpecialKeys.Add(Key.D9);

			_NonSpecialKeys.Add(Key.Add);
			_NonSpecialKeys.Add(Key.Decimal);
			_NonSpecialKeys.Add(Key.Divide);
			_NonSpecialKeys.Add(Key.Multiply);
			_NonSpecialKeys.Add(Key.OemBackslash);
			_NonSpecialKeys.Add(Key.OemCloseBrackets);
			_NonSpecialKeys.Add(Key.OemComma);
			_NonSpecialKeys.Add(Key.OemMinus);
			_NonSpecialKeys.Add(Key.OemOpenBrackets);
			_NonSpecialKeys.Add(Key.OemPeriod);
			_NonSpecialKeys.Add(Key.OemPipe);
			_NonSpecialKeys.Add(Key.OemPlus);
			_NonSpecialKeys.Add(Key.OemQuestion);
			_NonSpecialKeys.Add(Key.OemQuotes);
			_NonSpecialKeys.Add(Key.OemSemicolon);
			_NonSpecialKeys.Add(Key.OemTilde);

			_NonSpecialKeys.Add(Key.NumPad0);
			_NonSpecialKeys.Add(Key.NumPad1);
			_NonSpecialKeys.Add(Key.NumPad2);
			_NonSpecialKeys.Add(Key.NumPad3);
			_NonSpecialKeys.Add(Key.NumPad4);
			_NonSpecialKeys.Add(Key.NumPad5);
			_NonSpecialKeys.Add(Key.NumPad6);
			_NonSpecialKeys.Add(Key.NumPad7);
			_NonSpecialKeys.Add(Key.NumPad8);
			_NonSpecialKeys.Add(Key.NumPad9);
			#endregion

			tmrEllipse = new DispatcherTimer();
			tmrEllipse.Interval = new TimeSpan(0, 0, 0, 0, 500);
			tmrEllipse.Tick += new EventHandler(tmrEllipse_Tick);

			tmrClipboardCheck = new DispatcherTimer();
			tmrClipboardCheck.Interval = new TimeSpan(0, 0, 0, 1);
			tmrClipboardCheck.Tick += new EventHandler(tmrClipboard_Tick);
			tmrClipboardCheck.IsEnabled = true;

			tmrScreen = new DispatcherTimer();
			tmrScreen.Interval = new TimeSpan(0, 0, 0, 0, 23);
			tmrScreen.Tick += tmrScreen_Tick;
			tmrScreen.Start();

			this.Focus();
		}

		DateTime lastUpdate = DateTime.MinValue;
		void tmrScreen_Tick(object sender, EventArgs e)
		{
			if (_Connection != null)
				if (DateTime.Now.Subtract(lastUpdate).TotalMilliseconds > 42)
					_Connection.refreshScreen();
		}

		void tmrEllipse_Tick(object sender, EventArgs e)
		{
			ellipse1.Visibility = System.Windows.Visibility.Collapsed;
			tmrEllipse.Stop();
		}

		private void connect(string ServerAddress, int ServerPort, string ServerPassword)
		{
			//Create a connection
			_Connection = new RfbClient(ServerAddress, ServerPort, ServerPassword);
#if logging
			_Connection.LoggingLevel = _LoggingLevel;
			_Connection.LoggingPath = _LoggingPath;
#endif
			//Is Triggered when the Screen is beeing updated
			_Connection.ScreenUpdate += new RfbClient.ScreenUpdateEventHandler(_Connection_ScreenUpdate);
			//Is Triggered, when the RfbClient sends a Log-Event
			_Connection.LogMessage += new RfbClient.LogMessageEventHandler(_Connection_LogMessage);

			_Connection.StartConnection();
		}

		private void _Connection_LogMessage(object sender, LogMessageEventArgs e)
		{
			if (e.LogType == Logtype.User)
				MessageBox.Show(e.LogMessage);
		}

		private void _Connection_ScreenResolutionChange(object sender, ScreenResolutionChangeEventArgs e)
		{
			if (_RemoteScreen == null)
				_RemoteScreen = new WriteableBitmap(e.ResX, e.ResY, 96, 96, PixelFormats.Rgb24, null);
		}

		private void _Connection_ScreenUpdate(object sender, ScreenUpdateEventArgs e)
		{
			if (_Connection != null && _Connection.IsConnected == true)
			{
				image1.Dispatcher.Invoke(new UpdateScreenCallback(UpdateImage), new object[] { e });
			}
		}

		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			_Connection.Disconnect();
		}

		/// <summary>
		/// Update the RemoteImage
		/// </summary>
		/// <param name="newScreen"></param>
		private void UpdateImage(ScreenUpdateEventArgs newScreens)
		{
			try
			{
				if (newScreens.Rects.Count == 0)
					return;
				lastUpdate = DateTime.Now;

				if (Bitmap == null)
				{
					//NET function to create and empty bitmap
					Bitmap = new WriteableBitmap(newScreens.Rects.First().Width, newScreens.Rects.First().Height, 96, 96, PixelFormats.Bgr32, null);
					image1.Source = Bitmap;
				}
				
				//Update each rectangle to Bitmap
				foreach (var newScreen in newScreens.Rects)
				{
					//Write rectangle to bitmap (faster method)
					//: WritePixels Method (Int32Rect, Array, Int32, Int32, Int32)
					//sourceRect - The rectangle in sourceBuffer to copy
					//sourceBuffer - The input buffer used to update the Bitmap
					//sourceBufferStride - Stride in byte (see 4 as 32bpp - no pellete image)
					//destinationX, destinationY - position in bitmap (top, left)
					Bitmap.WritePixels(new Int32Rect(0, 0, newScreen.Width, newScreen.Height), newScreen.PixelData, newScreen.Width * 4, newScreen.PosX, newScreen.PosY);
				}
				
				//Ivalidate image so it'll be refreshed next
				image1.InvalidateVisual();

			}
			catch (Exception)
			{
				Console.WriteLine();
				throw;
			}
		}

		#region Keyboard/Mouse handling
		bool _IgnoreNextKey = false;

		/// <summary>
		/// Send Special Keys
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (_Connection == null) return;

			if (!_NonSpecialKeys.Contains(e.Key) || Keyboard.IsKeyDown(Key.LeftCtrl) == true) //If Control-Key is pressed, don't Send NonSpecialKey as a Sign
			{
				if (Keyboard.IsKeyDown(Key.LeftCtrl) == true)
					_IgnoreNextKey = true;

				_Connection.sendKey(e);
			}
		}

		/// <summary>
		/// Send Special Keys
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (_Connection == null) return;

			if (!_NonSpecialKeys.Contains(e.Key) || Keyboard.IsKeyDown(Key.LeftCtrl) == true)
			{
				if (Keyboard.IsKeyDown(Key.LeftCtrl) == true)
					_IgnoreNextKey = true;

				_Connection.sendKey(e);
			}
		}

		/// <summary>
		/// Captures the Mouseclicks
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			handleMouseState(e);
		}

		/// <summary>
		/// Captures the Mouseclicks
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			handleMouseState(e);
		}

		/// <summary>
		/// Send Mouse Movements to the Server
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			handleMouseState(e);
		}

		/// <summary>
		/// Triggers the MouseWheelActions
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			byte buttonValue = 0;
			if (e.Delta > 0) //Wheel moved Up
				buttonValue = 8;
			else if (e.Delta < 0) //Wheel moved down
				buttonValue = 16;

			_Connection.sendMouseClick((UInt16)e.GetPosition(this).X, (UInt16)e.GetPosition(this).Y, buttonValue);
		}

		/// <summary>
		/// Send all typed Signes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tbInsert_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (tbInput.Text.Length > 0)
			{
				string sign = tbInput.Text;
				tbInput.Clear();

				//if (_IgnoreNextKey == false)
				//{
				_Connection.sendSign(sign.ToCharArray()[0]);
				_IgnoreNextKey = true;
				//}
			}
		}

		/// <summary>
		/// Thats what to do, when a Mousebutton was clicked
		/// </summary>
		/// <param name="e"></param>
		private void handleMouseState(MouseEventArgs e)
		{
			byte buttonValue = 0;
			if (e.LeftButton == MouseButtonState.Pressed)
				buttonValue = 1;
			if (e.RightButton == MouseButtonState.Pressed)
				buttonValue += 4;
			if (e.MiddleButton == MouseButtonState.Pressed)
				buttonValue += 2;

			//Don't send, if there is no MouseClick and a Event was triggered less then 1/5 second before.
			if (_LimitMouseMove == true && buttonValue == 0 && DateTime.Now.Subtract(_LastMouseMove).TotalMilliseconds < 200)
				return;

			if (_Connection != null && _Connection.IsConnected == true)
			{
				_Connection.sendMouseClick(
					 (UInt16)(e.GetPosition(tbInput).X / image1.ActualWidth * _Connection.Properties.FramebufferWidth + 1),
					 (UInt16)(e.GetPosition(tbInput).Y / image1.ActualHeight * _Connection.Properties.FramebufferHeight + 1)
					 , buttonValue);
			}

			if (_LimitMouseMove == true)
				_LastMouseMove = DateTime.Now;
		}

		#endregion

		#region Clipboard Handling

		private int _LastClipboardHash = 0; //To check, if the text has changed

		/// <summary>
		/// Checkinterval to check, if the Clipboard changed. Not a stylish way, but it works
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void tmrClipboard_Tick(object sender, EventArgs e)
		{
			if (_Connection == null)
				return;

			if (Clipboard.ContainsText() == true) //Check if Clipboard contains Text
			{
				try
				{
					if (Clipboard.GetText().Length < Int32.MaxValue) //normally it should be UInt32.MaxValue, but this is larger then .Length ever can be. So 2.1Million signs are maximum
					{
						if (_LastClipboardHash != Clipboard.GetText().GetHashCode()) //If the Text has changed since last check
						{
							_Connection.sendClientCutText(Clipboard.GetText());
							_LastClipboardHash = Clipboard.GetText().GetHashCode(); //Set the new Hash
						}
					}
				}
				catch (Exception) //
				{
				}
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// The IP or Hostname of the Remoteserver
		/// </summary>
		public string ServerAddress
		{
			get { return (_ServerAddress); }
			set
			{
				//Check if it is a possible correct server address
				string ipAddressRegex = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
				string hostnameRegex = @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$";

				System.Text.RegularExpressions.Regex validateIp = new System.Text.RegularExpressions.Regex(ipAddressRegex);
				System.Text.RegularExpressions.Regex validateHost = new System.Text.RegularExpressions.Regex(hostnameRegex);

				if (validateIp.IsMatch(value) && validateHost.IsMatch(value))
					_ServerAddress = value;
			}
		}

		/// <summary>
		/// The Port of the Remoteserver (Default: 5900)
		/// </summary>
		public int ServerPort
		{
			get { return (_ServerPort); }
			set
			{
				//Validate of port is valid
				if (value > 0 && value < 65536)
				{
					_ServerPort = value;
				}
			}
		}

		/// <summary>
		/// The Password to join the Server
		/// </summary>
		public string ServerPassword
		{
			get { return (_ServerPassword); }
			set { _ServerPassword = value; }
		}

		/// <summary>
		/// Should the interval of sending MouseMoveCommands to the VNC-Server be limited? Default=true
		/// </summary>
		public bool LimitMouseEvents
		{
			get { return _LimitMouseMove; }
			set { _LimitMouseMove = value; }
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
		#endregion

		#region Public Methods

		public void Connect()
		{
			connect(_ServerAddress, _ServerPort, _ServerPassword);
		}

		public void Connect(string ServerAddress)
		{
			connect(ServerAddress, _ServerPort, "");
		}

		public void Connect(string ServerAddress, string ServerPassword)
		{
			connect(ServerAddress, _ServerPort, ServerPassword);
		}

		public void Connect(string ServerAddress, int ServerPort, string ServerPassword)
		{
			connect(ServerAddress, ServerPort, ServerPassword);
		}

		public void SendKeyCombination(KeyCombination keyComb)
		{
			_Connection.sendKeyCombination(keyComb);
		}

		public void UpdateScreen()
		{
			_Connection.refreshScreen();
		}

		#endregion

	}
}
