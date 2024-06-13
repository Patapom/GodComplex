using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.IO.Ports;

namespace WaterTankMonitor {
	public partial class WaterTankMonitorForm : Form {

		#region CONSTANTS

		public const int	DEFAULT_BAUD_RATE = 19200;				// Default baud rate to use to communicate with the Arduino

		public const int	SAVE_LOG_INTERVAL_MINUTES = 15;			// Save log every 15 minutes

		public const float	MEASURE_INTERVAL_MINUTES_MAX = 5.0f;	// When debit is slow, measure level every 5 minutes
		public const float	MEASURE_INTERVAL_MINUTES_MIN = 0.5f;	// When debit is fast, measure level every 30 seconds

		public const float	DEBIT_SLOW_LITRE_PER_MINUTE = 0.1f;		// Debit is considered slow when at 0.1 litre / minute
		public const float	DEBIT_FAST_LITRE_PER_MINUTE = 1.0f;		// Debit is considered fast when at 1.0 litre / minute

		public const float	DEFAULT_SPEED_OF_SOUND = 343.4f;	// Default speed of sound at 1 bar and 20°C is 343.4 m/s

		public const float	TANK_CAPACITY_LITRES = 4000;		// Tank capacity in litres
		public const float	TANK_HEIGHT_FULL = 1.73f;			// Tank height when full

		// Reference tank measurements at time of installation
		public const float	TANK_HEIGHT_REFERENCE = 139.5f;		// Tank height reference (measured for 3225L)
		public const uint	MEASURED_TIME_REFERENCE = 0x3724;	// Raw time reference value (measured for 3225L)

		#endregion

		#region TYPES

		class CommandTimeOutException : Exception {

		}

		[System.Diagnostics.DebuggerDisplay( "{Time} {Volume}" )]
		class LogEntry {

			public DateTime	m_timeStamp;
			public uint		m_rawTime_microSeconds;	// The raw sensor time of flight, in µs

			/// <summary>
			/// Gets the tank volume in litres
			/// </summary>
			public float	Volume => Distance2Volume( RawTime2Distance( m_rawTime_microSeconds ) );

			public void	Read( TextReader _R ) {
				string		line = _R.ReadLine();
				string[]	parts = line.Split( ';' );
				m_timeStamp = DateTime.FromFileTime( long.Parse( parts[0].Trim(), System.Globalization.NumberStyles.HexNumber ) );
//					throw new Exception( "Failed to read time stamp!" );
				if ( !uint.TryParse( parts[1].Trim(), out m_rawTime_microSeconds) )
					throw new Exception( "Failed to read value!" );
			}

			public void	Write( StreamWriter _W ) {
				_W.WriteLine( m_timeStamp.ToFileTime().ToString( "X08" ) + "; " + m_rawTime_microSeconds );
			}

			/// <summary>
			/// Maps a sensor distance to a volume
			/// </summary>
			/// <param name="_distance_meters"></param>
			/// <returns></returns>
			public static float	Distance2Volume( float _distance_meters ) {
				float	distance_Ref = RawTime2Distance( MEASURED_TIME_REFERENCE );
				float	distance_4000L = distance_Ref - (TANK_HEIGHT_FULL - TANK_HEIGHT_REFERENCE);	// Measured distance when the tank is full
				float	distance_0L = distance_4000L + TANK_HEIGHT_FULL;							// Measure distance when the tank is empty
				return (_distance_meters - distance_0L) * TANK_CAPACITY_LITRES / (distance_4000L - distance_0L);
			}

			/// <summary>
			/// Converts raw time of flight (in µs) into a measure of distance (in meters)
			/// </summary>
			/// <param name="_rawTime_microSeconds"></param>
			/// <param name="_speedOfSound_metersPerSecond"></param>
			/// <returns></returns>
			public static float	RawTime2Distance( uint _rawTime_microSeconds, float _speedOfSound_metersPerSecond = DEFAULT_SPEED_OF_SOUND ) {
				return _rawTime_microSeconds * _speedOfSound_metersPerSecond / (2.0f * 1e6f);
			}
		}

		#endregion

		#region FIELDS

		RegistryKey		m_appKey;
		string			m_applicationPath;
		FileInfo		m_fileNameLog;

		List<LogEntry>	m_entries = new List<LogEntry>();

		string			m_COMPortName = "COM11";		// Default COM port name
		SerialPort		m_COMPort;

		bool			m_followRunTime = true;			// Default is realtime following of measurements
		float			m_windowSize_Hours = 1.0f;		// Default window size is 1 hour
		DateTime		m_windowEndTime = DateTime.Now;	// Default window time is now
//		DateTime		m_windowStartTime;

		Pen				m_axesPen = new Pen( Color.Black );
		Brush			m_axesBrush = new SolidBrush( Color.Black );
		string			m_labelAxisX = "Time";
		string			m_labelAxisY = "Volume";
		int				m_margin = 16;
		float			m_arrowWidth = 8.0f;
		float			m_arrowLength = 10.0f;

		public bool		FollowRuntime {
			get => m_followRunTime;
			set {
				if ( value == m_followRunTime )
					return;

				m_followRunTime = value;
				SetRegKey( "m_followRunTime", m_followRunTime.ToString() );

				// Go to now
				WindowEndTime = DateTime.Now;
			}
		}

		public float	WindowSize_Hours {
			get => m_windowSize_Hours;
			set {
				m_windowSize_Hours = value;
//				m_windowStartTime = m_windowEndTime - TimeSpan.FromHours( m_windowSize_Hours );
				SetRegKey( "WindowSize_Hours", m_windowSize_Hours.ToString() );
				UpdateGraph();
			}
		}

		public DateTime		WindowEndTime {
			get => m_windowEndTime;
			set {
				m_windowEndTime = value;
//				m_windowStartTime = m_windowEndTime - TimeSpan.FromHours( m_windowSize_Hours );
				SetRegKey( "WindowEndTime", m_windowEndTime.ToFileTime().ToString( "X08" ) );
				UpdateGraph();
			}
		}

		#endregion

		public WaterTankMonitorForm() {
 			m_appKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\WaterTankMonitor" );
			m_applicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );

			InitializeComponent();

			panelOutput.MouseDown += PanelOutput_MouseDown;
			panelOutput.MouseUp += PanelOutput_MouseUp;
			panelOutput.MouseMove += PanelOutput_MouseMove;
			panelOutput.MouseWheel += PanelOutput_MouseWheel;
			panelOutput.BitmapUpdating += PanelOutput_BitmapUpdating;

			try {
				// Retrieve parameters
				m_COMPortName = GetRegKey( "m_COMPortName", m_COMPortName );
				m_followRunTime = bool.Parse( GetRegKey( "m_followRunTime", "true" ) );
				m_windowSize_Hours = float.Parse( GetRegKey( "WindowSize_Hours", m_windowSize_Hours.ToString() ) );
				m_windowEndTime = DateTime.FromFileTime( long.Parse( GetRegKey( "WindowEndTime", m_windowEndTime.ToFileTime().ToString( "X08" ) ), System.Globalization.NumberStyles.HexNumber ) ) ;

				// Open log file
				m_fileNameLog = new FileInfo( GetRegKey( "LogFileName", Path.Combine( m_applicationPath, "Tank3.log" ) ) );
				if ( m_fileNameLog.Exists ) {
					ReadLog( m_fileNameLog );
				}

				// List COM ports
				ListCOMPorts();

				// Render graph
				UpdateGraph();

			} catch ( Exception _e ) {
				MessageBoxError( "An exception occurred while opening the application!", _e );
			}

			// Start timer
			m_startTime = DateTime.Now;
			Application.Idle += Application_Idle;
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );
			RestoreLocationAndSize( this );
		}

		protected override void OnSizeChanged( EventArgs e ) {
			base.OnSizeChanged( e );
			SaveLocationAndSize( this );
		}

		protected override void OnLocationChanged( EventArgs e ) {
			base.OnLocationChanged( e );
			SaveLocationAndSize( this );
		}

		DateTime	m_startTime;
		DateTime	m_lastLogSaveTime = DateTime.Now;
		DateTime	m_lastMeasurementTime = DateTime.FromFileTime( 0 );	// A long time ago!

		private void Application_Idle( object sender, EventArgs e ) {
			DateTime	now = DateTime.Now;

			if ( m_COMPort == null || !m_COMPort.IsOpen ) {
				UpdateGraph();
				return;
			}

// if ( m_COMPort.BytesToRead >= 0 ) {
// 	string	line = m_COMPort.ReadLine();
// 	System.Diagnostics.Debug.WriteLine( line );
// }
return;
			// Check if we should save the log
			if ( (now - m_lastLogSaveTime).TotalMinutes > SAVE_LOG_INTERVAL_MINUTES ) {
				try {
					WriteLog( m_fileNameLog );
				} catch ( Exception _e ) {
					ShowError( "Failed to save log!" );
				}
				m_lastLogSaveTime = DateTime.Now;
			}

			// Check if we should perform a new measurement
			LogEntry	currentEntry = m_entries.Count > 0 ? m_entries[m_entries.Count-1] : null;
			LogEntry	lastEntry = m_entries.Count > 1 ? m_entries[m_entries.Count-2] : currentEntry;

				// Estimate the debit
			float		debitEstimate_LitresPerMinute = 0;	// Assume steady state
			if ( currentEntry != null && lastEntry != null ) {
				float	deltaTime = (float) (currentEntry.m_timeStamp - lastEntry.m_timeStamp).TotalMinutes;
				float	deltaVolume = currentEntry.Volume - lastEntry.Volume;
				debitEstimate_LitresPerMinute = Math.Abs( deltaVolume / Math.Max( 1e-3f, deltaTime ) );
			}

				// Estimate the measurement interval based on the debit
			float	debitFactor = (debitEstimate_LitresPerMinute - DEBIT_SLOW_LITRE_PER_MINUTE) / (DEBIT_FAST_LITRE_PER_MINUTE - DEBIT_SLOW_LITRE_PER_MINUTE);
			float	measurementInterval = MEASURE_INTERVAL_MINUTES_MIN + debitFactor * (MEASURE_INTERVAL_MINUTES_MAX - MEASURE_INTERVAL_MINUTES_MIN);
					measurementInterval = Math.Max( MEASURE_INTERVAL_MINUTES_MIN, Math.Min( MEASURE_INTERVAL_MINUTES_MAX, measurementInterval ) );

			if ( (now - m_lastMeasurementTime).TotalSeconds > measurementInterval ) {
				PerformMeasurement();
			}
		}

		protected override void OnFormClosing( FormClosingEventArgs e ) {
			if ( e.CloseReason == CloseReason.UserClosing ) {
				// Just hide the form...
				e.Cancel = true;
				Visible = false;
			}
			base.OnFormClosing( e );
		}

		void		UpdateGraph() {
			panelOutput.UpdateBitmap();
		}

		#region COM Ports / Serial Interface

		void	ListCOMPorts() {
			comboBoxCOMPort.Items.Clear();
			foreach ( string portName in SerialPort.GetPortNames() ) {
				comboBoxCOMPort.Items.Add( portName );
			}
			comboBoxCOMPort.SelectedItem = m_COMPortName;
		}

		void	SendString( string _string ) {
			m_COMPort.WriteLine( _string );
		}

		/// <summary>
		/// Sends a command and waits for a response
		/// </summary>
		/// <param name="_command"></param>
		/// <param name="_timeOut_ms"></param>
		/// <returns>The response to the command</returns>
		/// <exception cref="">Command time out</exception>
		string	SendCommand( string _command, uint _timeOut_ms ) {
			SendString( _command );

			// Wait for a response or a timeout
			DateTime	sendTime = DateTime.Now;
			while ( (DateTime.Now - sendTime).TotalMilliseconds < _timeOut_ms ) {
				System.Threading.Thread.Sleep( 10 );

				if ( m_COMPort.BytesToRead > 0 ) {
					return m_COMPort.ReadLine();
				}
			}

			throw new CommandTimeOutException();
		}

		void	PerformMeasurement() {
			try {
				string	reply = SendCommand( "DST0", 10 * 1000 );

				// Handle reply!

			} catch ( CommandTimeOutException ) {
//				ShowError()
			} catch ( Exception _e ) {
				ShowError( "An error occurred while sending a measurement command!\r\n" + _e.Message );
			}
		}

		#endregion

		#region Log File

		void	ReadLog( FileInfo _fileNameLog ) {
			if ( !_fileNameLog.Exists )
				throw new FileNotFoundException();

			using ( StreamReader R = _fileNameLog.OpenText() ) {
				while ( !R.EndOfStream ) {
					string	line = R.ReadLine().Trim();
					if ( line.Length == 0 )
						break;	// End of stream...

					LogEntry	entry = new LogEntry();
								entry.Read( R );
					m_entries.Add( entry );
				}
			}
		}

		void	WriteLog( FileInfo _fileNameLog ) {
			if ( _fileNameLog.Exists ) {
				// Backup any existing file first...
				string	backupFileName = _fileNameLog.FullName + ".bak";
				if ( File.Exists( backupFileName ) ) {
					File.Delete( backupFileName );
				}
				File.Copy( _fileNameLog.FullName, backupFileName );
			}

			using ( StreamWriter W = _fileNameLog.CreateText() ) {
				foreach ( LogEntry entry in m_entries ) {
					entry.Write( W );
				}
			}
		}

		#endregion

		#region Info / Warning / Error Notification

		void	ShowWarning( string _message ) {
			notifyIcon.ShowBalloonTip( 5000, "Water Tank Monitor", _message, ToolTipIcon.Warning );
		}

		void	ShowError( string _message ) {
			notifyIcon.ShowBalloonTip( 5000, "Water Tank Monitor", _message, ToolTipIcon.Error );
		}

		void	MessageBoxError( string _message, Exception _e ) {
			_message += "\r\n";
			_message += "Exception:\r\n";
			_message += _e.Message + "\r\n";
			_message += _e.StackTrace;
			MessageBox( _message, MessageBoxIcon.Error );
		}

		void	MessageBox( string _message, MessageBoxIcon _icon ) {
			System.Windows.Forms.MessageBox.Show( this, _message, "Water Tank Monitor", MessageBoxButtons.OK, _icon );
		}

		#endregion

		#region Helpers

		private string	GetRegKey( string _key, string _default ) {
			string	result = m_appKey.GetValue( _key ) as string;
			return result != null ? result : _default;
		}
		private void	SetRegKey( string _key, string _value ) {
			m_appKey.SetValue( _key, _value );
		}

		/// <summary>
		/// Saves the location and size of the form
		/// </summary>
		/// <param name="_form"></param>
		public void	SaveLocationAndSize( Form _form ) {
			Point	formRelativeLocation = _form.Location;

//			// Make sure it's relative to the parent monitor
//			Screen	containingScreen;
//			IntPtr	hMonitor;
//			Interop.GetMonitorFromPosition( formRelativeLocation, out containingScreen, out hMonitor );
//			formRelativeLocation.X -= containingScreen.WorkingArea.Left;
//			formRelativeLocation.Y -= containingScreen.WorkingArea.Top;

			SetRegKey( "Form." + _form.GetType().Name + ".Location", formRelativeLocation.ToString() );
			SetRegKey( "Form." + _form.GetType().Name + ".Size", _form.Size.ToString() );
		}

		/// <summary>
		/// Restores the location and size of the form
		/// </summary>
		/// <param name="_form"></param>
		public void	RestoreLocationAndSize( Form _form ) {
			string	strLocation = GetRegKey( "Form." + _form.GetType().Name + ".Location", _form.Location.ToString() );
			string	strSize = GetRegKey( "Form." + _form.GetType().Name + ".Size", _form.Size.ToString() );

			Point	formRelativeLocation = _form.Location;
			Size	formSize = _form.Size;
			TryParse( strLocation, out formRelativeLocation );
			TryParse( strSize, out formSize );

//			// Offset relative to parent screen
//			Form	ownerForm = _form.Owner != null ? _form.Owner : MainForm.Singleton;
//			if ( ownerForm != null ) {
//				Screen	containingScreen;
//				IntPtr	hMonitor;
//				Interop.GetMonitorFromPosition( ownerForm.Location, out containingScreen, out hMonitor );
//
//				formRelativeLocation.Offset( containingScreen.WorkingArea.Location );
//			}

			_form.Location = formRelativeLocation;
			switch ( _form.FormBorderStyle ) {
				case FormBorderStyle.SizableToolWindow:
				case FormBorderStyle.Sizable:
					_form.Size = formSize;
					break;
			}

			// Make sure the form's rectangle is visible
			EnsureVisible( _form );
		}

		/// <summary>
		/// Make sure the form is visible
		/// </summary>
		/// <param name="_form"></param>
		public static void	EnsureVisible( Form _form ) {
			Rectangle	formRectangle = new Rectangle( _form.Location, _form.Size );
			int			minDistance = int.MaxValue;
			Screen		minDistanceScreen = null;
			foreach ( Screen screen in Screen.AllScreens ) {
				if ( screen.WorkingArea.Contains( formRectangle ) ) {
					return;	// Already visible...
				}

				int	screenDistance = Math.Abs( formRectangle.Left - screen.WorkingArea.Right );
					screenDistance = Math.Min( screenDistance, Math.Abs( formRectangle.Right - screen.WorkingArea.Left ) );
					screenDistance = Math.Min( screenDistance, Math.Abs( formRectangle.Top - screen.WorkingArea.Top ) );
					screenDistance = Math.Min( screenDistance, Math.Abs( formRectangle.Bottom - screen.WorkingArea.Bottom ) );
				if ( screenDistance < minDistance ) {
					minDistance = screenDistance;
					minDistanceScreen = screen;
				}
			}

			// Adjust location horizontally
			if ( formRectangle.Left < minDistanceScreen.WorkingArea.Left )
				formRectangle.X = minDistanceScreen.WorkingArea.Left;
			else if ( formRectangle.Right > minDistanceScreen.WorkingArea.Right )
				formRectangle.X = minDistanceScreen.WorkingArea.Right - formRectangle.Width;

			// Adjust location vertically
			if ( formRectangle.Top < minDistanceScreen.WorkingArea.Top )
				formRectangle.Y = minDistanceScreen.WorkingArea.Top;
			else if ( formRectangle.Bottom > minDistanceScreen.WorkingArea.Bottom )
				formRectangle.Y = minDistanceScreen.WorkingArea.Bottom - formRectangle.Height;

			_form.Location = formRectangle.Location;
		}

		public static bool	TryParse( string _strPoint, out Point _point ) {
			string[]	XY = _strPoint.Split( ',' );
			if ( XY == null || XY.Length != 2 ) {
				_point = Point.Empty;
				return false;
			}

			if ( !XY[0].StartsWith( "{X=" ) || !XY[1].StartsWith( "Y=" ) || !XY[1].EndsWith( "}" ) ) {
				_point = Point.Empty;
				return false;
			}

			XY[0] = XY[0].Substring( "{X=".Length );
			XY[1] = XY[1].Substring( "Y=".Length );
			XY[1] = XY[1].Remove( XY[1].Length-1 );

			int	X, Y;
			if ( !int.TryParse( XY[0], out X ) || !int.TryParse( XY[1], out Y ) ) {
				_point = Point.Empty;
				return false;
			}

			_point = new Point( X, Y );
			return true;
		}

		public static bool	TryParse( string _strPoint, out Size _size ) {
			string[]	XY = _strPoint.Split( ',' );
			if ( XY == null || XY.Length != 2 ) {
				_size = Size.Empty;
				return false;
			}

			if ( !XY[0].StartsWith( "{Width=" ) || !XY[1].StartsWith( " Height=" ) || !XY[1].EndsWith( "}" ) ) {
				_size = Size.Empty;
				return false;
			}

			XY[0] = XY[0].Substring( "{Width=".Length );
			XY[1] = XY[1].Substring( " Height=".Length );
			XY[1] = XY[1].Remove( XY[1].Length-1 );

			int	X, Y;
			if ( !int.TryParse( XY[0], out X ) || !int.TryParse( XY[1], out Y ) ) {
				_size = Size.Empty;
				return false;
			}

			_size = new Size( X, Y );
			return true;
		}

		#endregion

		#region EVENTS

		#region Panel Handling

		private void PanelOutput_MouseMove( object sender, MouseEventArgs e ) {
		}

		private void PanelOutput_MouseDown( object sender, MouseEventArgs e ) {
		}

		private void PanelOutput_MouseUp( object sender, MouseEventArgs e ) {
		}

		private void PanelOutput_MouseWheel( object sender, MouseEventArgs e ) {
			float	factor = 2.0f;
			WindowSize_Hours *= e.Delta > 0.0f ? factor : 1.0f / factor;
		}

		private void PanelOutput_BitmapUpdating( int W, int H, Graphics G ) {
//			G.FillRectangle( Brushes.LemonChiffon, 0, 0, W, H );
			G.FillRectangle( Brushes.LightYellow, 0, 0, W, H );

			if ( m_COMPort == null || !m_COMPort.IsOpen ) {
				// Draw an invalid graph
				G.DrawLine( Pens.Red, 0, 0, W, H );
				G.DrawLine( Pens.Red, 0, H, W, 0 );

				string	text = m_COMPort == null ? "Invalid COM Port..." : "Port is closed";
				SizeF	textSize = G.MeasureString( text, this.Font );
				G.DrawString( text, this.Font, Brushes.Red, (W - textSize.Width) / 2.0f, H / 4.0f );
				return;
			}

			// Render the graph
			float	Width = W;
			float	Height = H - 8;

			// Paint level lines
			float	Y0 = LevelY( 1.00f * TANK_CAPACITY_LITRES );	// 4000L line
			float	Y1 = LevelY( 0.75f * TANK_CAPACITY_LITRES );	// 3000L line
			float	Y2 = LevelY( 0.50f * TANK_CAPACITY_LITRES );	// 2000L line
			float	Y3 = LevelY( 0.25f * TANK_CAPACITY_LITRES );	// 1000L line
			Pen		penLevelLine = Pens.Goldenrod;
			G.DrawLine( penLevelLine, m_margin, Y0, W - m_margin, Y0 );
			G.DrawLine( penLevelLine, m_margin, Y1, W - m_margin, Y1 );
			G.DrawLine( penLevelLine, m_margin, Y2, W - m_margin, Y2 );
			G.DrawLine( penLevelLine, m_margin, Y3, W - m_margin, Y3 );

			// Paint measurements
			if ( m_entries.Count > 1 ) {
				LogEntry	previous = null;
				float		previousX = 0;
				float		previousY = 0;
				for ( int entryIndex=m_entries.Count-1; entryIndex >= 0; entryIndex-- ) {
					LogEntry	current = m_entries[entryIndex];
					float		X = TimeStampX( current );
					float		Y = LevelY( current );
					if ( previous != null ) {
						G.DrawLine( Pens.Black, X, Y, previousX, previousY );
					}
					previous = current;
					previousX = X;
					previousY = Y;
					if ( X < 0 )
						break;	// We've escaped through left side, no need to paint any more measurements...
				}
			}

			// Paint X axis
			G.DrawLine( m_axesPen, m_margin, Height - m_margin, Width - m_margin, Height - m_margin );
			PaintArrow( G, m_axesBrush, new PointF( Width - m_margin - m_arrowLength, Height - m_margin ), new PointF( 1, 0 ), new PointF( 0, -1 ), m_arrowLength, m_arrowWidth );
			SizeF	sizeLabelX = G.MeasureString( m_labelAxisX, panelOutput.Font );
			G.DrawString( m_labelAxisX, panelOutput.Font, m_axesBrush, Width - 0.5f * m_margin - sizeLabelX.Width, Height - sizeLabelX.Height );

			// Paint Y axis
			G.DrawLine( m_axesPen, m_margin, Height - m_margin, m_margin, m_margin );
			PaintArrow( G, m_axesBrush, new PointF( m_margin, m_margin + m_arrowLength ), new PointF( 0, -1 ), new PointF( -1, 0 ), m_arrowLength, m_arrowWidth );
			SizeF	sizeLabelY = G.MeasureString( m_labelAxisY, panelOutput.Font );
			G.TranslateTransform( sizeLabelY.Height, 0.5f * m_margin );
			G.RotateTransform( 90 );
			G.DrawString( m_labelAxisY, panelOutput.Font, m_axesBrush, 0, 0 );
		}

		float	TimeStampX( LogEntry _entry ) {
			return TimeStampX( _entry.m_timeStamp );
		}
		float	TimeStampX( DateTime _timeStamp ) {
			float	W = panelOutput.Width - 2 * m_margin;
			float	deltaTime_Hours = (float) (m_windowEndTime - _timeStamp).TotalHours;
			return m_margin + W * (1.0f - deltaTime_Hours / m_windowSize_Hours);
		}

		// Transforms a volume in litres into a Y coordinate
		float	LevelY( LogEntry _entry ) {
			return LevelY( _entry.Volume );
		}
		float	LevelY( float _volume_litres ) {
			float	H = panelOutput.Height - 3 * m_margin;
			return panelOutput.Height - m_margin - H * _volume_litres / TANK_CAPACITY_LITRES;
		}

		PointF[]	m_arrowPoints = new PointF[3];
		void	PaintArrow( Graphics _G, Brush _brush, PointF _positionBase, PointF _axisX, PointF _axisY, float _length, float _width ) {
			PointF	P0 = _positionBase;
					P0.X += (0.5f * _width) * _axisY.X;
					P0.Y += (0.5f * _width) * _axisY.Y;
			PointF	P1 = _positionBase;
					P1.X -= (0.5f * _width) * _axisY.X;
					P1.Y -= (0.5f * _width) * _axisY.Y;
			PointF	P2 = _positionBase;
					P2.X += _length * _axisX.X;
					P2.Y += _length * _axisX.Y;

			m_arrowPoints[0].X = P0.X;
			m_arrowPoints[0].Y = P0.Y;
			m_arrowPoints[1].X = P1.X;
			m_arrowPoints[1].Y = P1.Y;
			m_arrowPoints[2].X = P2.X;
			m_arrowPoints[2].Y = P2.Y;

			_G.FillPolygon( _brush, m_arrowPoints,System.Drawing.Drawing2D.FillMode.Winding );
		}

		#endregion

		private void buttonRefreshCOMPorts_Click( object sender, EventArgs e ) {
			ListCOMPorts();
		}

		private void comboBoxCOMPort_SelectedIndexChanged( object sender, EventArgs e ) {
			if ( comboBoxCOMPort.SelectedIndex < 0 )
				return;

			m_COMPortName = comboBoxCOMPort.SelectedItem as string;
			SetRegKey( "m_COMPortName", m_COMPortName );

			if ( m_COMPort != null ) {
				// Dispose of existing port first...
				m_COMPort.Close();
				m_COMPort.Dispose();
			}

			// Build the serial port object
//			m_COMPort = new SerialPort( m_COMPortName, DEFAULT_BAUD_RATE, Parity.Even, 8, StopBits.Two );
			m_COMPort = new SerialPort( m_COMPortName, DEFAULT_BAUD_RATE, Parity.None, 8, StopBits.One );
// m_COMPort = new SerialPort( m_COMPortName );
// m_COMPort.BaudRate = DEFAULT_BAUD_RATE;
			m_COMPort.DataReceived += COMPort_DataReceived;
//			m_COMPort.NewLine = "\r\n";
			m_COMPort.Open();
		}

		byte[]	temp = new byte[4096];
		private void COMPort_DataReceived( object sender, SerialDataReceivedEventArgs e ) {
//			m_COMPort.Read( temp, 0, m_COMPort.BytesToRead );
// string	line = m_COMPort.ReadLine();
// System.Diagnostics.Debug.WriteLine( line );
		}

		private void buttonMonth_Click( object sender, EventArgs e ) {
			WindowSize_Hours = 24 * 7 * 31;
		}

		private void buttonWeek_Click( object sender, EventArgs e ) {
			WindowSize_Hours = 24 * 7;
		}

		private void buttonDay_Click( object sender, EventArgs e ) {
			WindowSize_Hours = 24;
		}

		private void buttonNow_Click( object sender, EventArgs e ) {
			FollowRuntime = true;
		}

		#endregion

		private void button1_Click( object sender, EventArgs e ) {
			string	reply = SendCommand( "DST0,", 10 * 1000 );
			System.Diagnostics.Debug.WriteLine( reply );
		}
	}
}
