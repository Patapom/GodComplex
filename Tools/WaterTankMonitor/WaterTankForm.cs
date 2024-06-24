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


// @TODO: filtrer/smoother le volume sur plein d'entries! C'est trop rough!

namespace WaterTankMonitor {
	public partial class WaterTankMonitorForm : Form {

		#region CONSTANTS

		public const int	DEFAULT_BAUD_RATE = 19200;				// Default baud rate to use to communicate with the Arduino

		public const int	SAVE_LOG_INTERVAL_MINUTES = 15;			// Save log every 15 minutes
		public const int	SAVE_LOG_AFTER_UNSAVED_ENTRIES_COUNT = 10;	// Save log if we have more than 10 unsaved entries

		public const float	MEASURE_INTERVAL_MINUTES_MAX = 5.0f;	// When debit is slow, measure level every 5 minutes
		public const float	MEASURE_INTERVAL_MINUTES_MIN = 0.5f;	// When debit is fast, measure level every 30 seconds

		public const float	DEBIT_SLOW_LITRE_PER_MINUTE = 0.1f;		// Debit is considered slow when at 0.1 litre / minute
		public const float	DEBIT_FAST_LITRE_PER_MINUTE = 1.0f;		// Debit is considered fast when at 1.0 litre / minute

		public const float	DEFAULT_SPEED_OF_SOUND = 343.4f;		// Default speed of sound at 1 bar and 20°C is 343.4 m/s

		public const float	TANK_CAPACITY_LITRES = 4000;			// Tank capacity in litres
		public const float	TANK_HEIGHT_FULL = 1.73f;				// Tank height when full (

		// Reference tank measurements at time of installation
// 		public const float	TANK_HEIGHT_REFERENCE = 1.395f;			// Tank height reference (measured for 3225L)
// 		public const uint	MEASURED_TIME_REFERENCE = 3724;			// Raw time reference value (measured for 3225L) (=0.6394108 meters from sensor) (sensor is at 2.034 m)
// 
// 			// Measured on June 21st at 18:00 (after replacement of faulty sensor by one that is inside a (hopefully) wataerproof container)
// 		public const float	TANK_HEIGHT_REFERENCE = 1.25f;			// Tank height reference (measured for 2900L)
// 		public const uint	MEASURED_TIME_REFERENCE = 4191;			// Raw time reference value (measured for 2900L) (=0.7195947 meters from sensor) (sensor is at 1.969 m)

			// Measured on June 23st at 9:30 (after another replacement of the container so it only covers a single exha)
		public const float	TANK_HEIGHT_REFERENCE = 1.11585f;		// Tank height reference (measured for 2580L)
		public const uint	MEASURED_TIME_REFERENCE = 5115;			// Raw time reference value (measured for 2580L) (=0.878 meters from sensor) (sensor is at 1.9941 m)

		#endregion

		#region TYPES

		class CommandTimeOutException : Exception {

		}

		[System.Diagnostics.DebuggerDisplay( "{Volume}L  ({m_timeStamp})" )]
		class LogEntry {

			public DateTime	m_timeStamp;
			public uint		m_rawTime_microSeconds;	// The raw sensor time of flight, in µs

			/// <summary>
			/// Tells if the entry represents an out of range measure
			/// </summary>
			public bool		IsOutOfRange => m_rawTime_microSeconds == ~0U;

			/// <summary>
			/// Tells if the volume measure is out of range (indicates a problem in the sensor measurement)
			/// </summary>
			public bool		IsVolumeOutOfRange {
				get {
					float	volume = Volume;
					return volume < 0 || volume > TANK_CAPACITY_LITRES;
				}
			}

			/// <summary>
			/// Gets the tank volume in litres
			/// </summary>
			public float	Volume => Distance2Volume( RawTime2Distance( m_rawTime_microSeconds ) );

			public LogEntry( uint _rawTime_microSeconds ) {
				m_timeStamp = DateTime.Now;
				m_rawTime_microSeconds = _rawTime_microSeconds;
			}

			public LogEntry( TextReader _R ) {
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
		FileInfo		m_fileNameLogEntries;
		FileInfo		m_fileNameSessionLog;

		List<LogEntry>	m_logEntries = new List<LogEntry>();
		List<string>	m_sessionLog = new List<string>();

		string			m_COMPortName = "COM11";		// Default COM port name
		SerialPort		m_COMPort = null;

		float			m_windowSize_Hours = 1.0f;		// Default window size is 1 hour
		DateTime		m_windowEndTime = DateTime.Now;	// Default window time is now
		TimeSpan		m_timeFromNow = TimeSpan.Zero;	// Default delta time between window end time and now is 0 so we track runtime data

		DateTime		m_timeReference = DateTime.Today + TimeSpan.FromDays( 10000 );	// Invalid time reference!
		float			m_lowWaterLevelWarningLimit_litres = 400;	// Default warning limit is 400 litres

		Pen				m_axesPen = new Pen( Color.Black );
		Brush			m_axesBrush = new SolidBrush( Color.Black );
		string			m_labelAxisX = "Time";
		string			m_labelAxisY = "Volume";
		int				m_margin = 16;
		float			m_arrowWidth = 8.0f;
		float			m_arrowLength = 10.0f;

 		public bool		FollowRuntime => m_timeFromNow.TotalHours >= 0.0;

		/// <summary>
		/// Gets or sets the size of the time window
		/// </summary>
		public float	WindowSize_Hours {
			get => m_windowSize_Hours;
			set {
				m_windowSize_Hours = value;
				SetRegKey( "WindowSize_Hours", m_windowSize_Hours.ToString() );
				UpdateGraph();
			}
		}

		/// <summary>
		/// Gets or sets the time at which the window is ending
		/// </summary>
		public DateTime		WindowEndTime {
			get => m_windowEndTime;
			set {
				m_windowEndTime = value;
				m_timeFromNow = value - DateTime.Now;
				SetRegKey( "WindowEndTime", m_windowEndTime.ToFileTime().ToString( "X08" ) );
				SetRegKey( "m_timeFromNow", m_timeFromNow.TotalHours.ToString() );
				UpdateGraph();
			}
		}

		/// <summary>
		/// Gets the time at which the window is starting
		/// </summary>
		public DateTime	WindowStartTime => WindowEndTime - TimeSpan.FromHours( WindowSize_Hours );

		/// <summary>
		/// Tells if the time reference is valid
		/// </summary>
		public bool	IsTimeReferenceValid => (DateTime.Now - m_timeReference).TotalSeconds >= 0;	// An invalid reference time is set in the future

		#endregion

		public WaterTankMonitorForm() {
 			m_appKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\WaterTankMonitor" );
//			m_applicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );
			m_applicationPath = Directory.GetCurrentDirectory();	// Makes more sense to use working directory!

			InitializeComponent();

			panelOutput.MouseDown += PanelOutput_MouseDown;
			panelOutput.MouseUp += PanelOutput_MouseUp;
			panelOutput.MouseMove += PanelOutput_MouseMove;
			panelOutput.MouseWheel += PanelOutput_MouseWheel;
			panelOutput.BitmapUpdating += PanelOutput_BitmapUpdating;

			Application.Idle += Application_Idle;
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );
			RestoreLocationAndSize( this );

			try {
				// Retrieve parameters
				m_COMPortName = GetRegKey( "m_COMPortName", m_COMPortName );
				m_windowSize_Hours = float.Parse( GetRegKey( "WindowSize_Hours", m_windowSize_Hours.ToString() ) );
				m_windowEndTime = DateTime.FromFileTime( long.Parse( GetRegKey( "WindowEndTime", m_windowEndTime.ToFileTime().ToString( "X08" ) ), System.Globalization.NumberStyles.HexNumber ) );
				m_timeFromNow = TimeSpan.FromHours( double.Parse( GetRegKey( "m_timeFromNow", m_timeFromNow.TotalHours.ToString() ) ) );
				if ( m_timeFromNow.TotalHours > 0 )
					m_windowEndTime = DateTime.Now + m_timeFromNow;

				m_timeReference = DateTime.FromFileTime( long.Parse( GetRegKey( "TimeReference", m_timeReference.ToFileTime().ToString( "X08" ) ), System.Globalization.NumberStyles.HexNumber ) );
				if ( !IsTimeReferenceValid ) {
					m_timeReference = DateTime.Today + TimeSpan.FromDays( 10000 );	// Make sure it's always invalid!
				}
				m_lowWaterLevelWarningLimit_litres = float.Parse( GetRegKey( "m_lowWaterLevelWarningLimit_litres", m_lowWaterLevelWarningLimit_litres.ToString() ) );

				// Open log file
				m_fileNameLogEntries = new FileInfo( GetRegKey( "LogFileName", Path.Combine( m_applicationPath, "Tank3.log" ) ) );
				if ( m_fileNameLogEntries.Exists ) {
					ReadLogEntries( m_fileNameLogEntries );
				}

				// Create a new session log file
				m_fileNameSessionLog = new FileInfo( Path.Combine( m_applicationPath, DateTime.Today.ToString( "MMMM dd HH-mm" ) + ".log" ) );

				// List available COM ports
				ListCOMPorts();

				// Render graph
				UpdateGraph();

			} catch ( Exception _e ) {
				MessageBoxError( "An exception occurred while opening the application!", _e );
			}

			// Start timer
			m_startTime = DateTime.Now;
			timerTick.Enabled = true;
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
		int			m_entriesCountOnLastLogSave = 0;
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
//return;
			// Check if we should save the log
			if ( (now - m_lastLogSaveTime).TotalMinutes > SAVE_LOG_INTERVAL_MINUTES
				|| (m_logEntries.Count - m_entriesCountOnLastLogSave) > SAVE_LOG_AFTER_UNSAVED_ENTRIES_COUNT ) {
				saveLogFileNowToolStripMenuItem_Click( null, EventArgs.Empty );

				m_lastLogSaveTime = DateTime.Now;
				m_entriesCountOnLastLogSave = m_logEntries.Count;
			}

			// Check if we should perform a new measurement
			LogEntry	currentEntry = m_logEntries.Count > 0 ? m_logEntries[m_logEntries.Count-1] : null;
			LogEntry	lastEntry = m_logEntries.Count > 1 ? m_logEntries[m_logEntries.Count-2] : currentEntry;

				// Estimate the debit
			float		debitEstimate_LitresPerMinute = 0;	// Assume steady state
			if ( currentEntry != null && lastEntry != null ) {
				float	deltaTime = (float) (currentEntry.m_timeStamp - lastEntry.m_timeStamp).TotalMinutes;
				float	deltaVolume = currentEntry.Volume - lastEntry.Volume;
				debitEstimate_LitresPerMinute = Math.Abs( deltaVolume / Math.Max( 1e-3f, deltaTime ) );
			}

				// Estimate the measurement interval based on the debit
			float	debitFactor = (debitEstimate_LitresPerMinute - DEBIT_SLOW_LITRE_PER_MINUTE) / (DEBIT_FAST_LITRE_PER_MINUTE - DEBIT_SLOW_LITRE_PER_MINUTE);
			float	measurementInterval_minutes = MEASURE_INTERVAL_MINUTES_MIN + (1.0f - debitFactor) * (MEASURE_INTERVAL_MINUTES_MAX - MEASURE_INTERVAL_MINUTES_MIN);
					measurementInterval_minutes = Math.Max( MEASURE_INTERVAL_MINUTES_MIN, Math.Min( MEASURE_INTERVAL_MINUTES_MAX, measurementInterval_minutes ) );

if ( checkBox1.Checked ) {
	measurementInterval_minutes = 5 / 60.0f;	// Every 5 seconds
}

			if ( (now - m_lastMeasurementTime).TotalMinutes > measurementInterval_minutes ) {
				PerformMeasurement(
					( string _error ) => {
						LogError( "An error occurred while sending a measurement command!\r\n" + _error );
						m_lastMeasurementTime = now - TimeSpan.FromMinutes( 2 * MEASURE_INTERVAL_MINUTES_MAX );	// Make sure we take a new measurement next time...
					}
				);

				m_lastMeasurementTime = now;
			}
		}

		bool	m_exitApplication = false;
		protected override void OnFormClosing( FormClosingEventArgs e ) {
			if ( e.CloseReason == CloseReason.UserClosing && !m_exitApplication ) {
				// Just hide the form...
				e.Cancel = true;
				Visible = false;
			}

			// Save the log before exiting
			try {
				WriteLogEntries( m_fileNameLogEntries );
				WriteSessionLog( m_fileNameSessionLog );
			} catch ( Exception _e ) {
				MessageBoxError( "Failed to save log file on exit!\r\nCan't close application...", _e );
				e.Cancel = true;
				return;
			}

			base.OnFormClosing( e );
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			base.OnFormClosed( e );

			// Abort COM thread so this application can close properly...
			m_COMThread?.Abort();
		}

		protected override void OnVisibleChanged( EventArgs e ) {
			base.OnVisibleChanged( e );
			UpdateGraph();
		}

		void	UpdateGraph() {
			if ( Visible )
				panelOutput.UpdateBitmap();
		}

		#region COM Ports / Serial Interface

		void	ListCOMPorts() {

			comboBoxCOMPort.Items.Clear();
			foreach ( string portName in SerialPort.GetPortNames() ) {
				comboBoxCOMPort.Items.Add( portName );
			}

			// Select the port, this will have the effect of trying to connect to it...
			comboBoxCOMPort.SelectedItem = m_COMPortName;
		}

		System.Threading.Thread	m_COMThread = null;

		[System.Diagnostics.DebuggerDisplay( "[{m_ID,d}] {m_command}" )]
		class COMCommand {
			public ushort			m_ID = 0;	// Invalid ID
			public string			m_command;
			public uint				m_timeOut_ms;
			public Action<string>	m_onSuccess;
			public Action<string>	m_onError;

			static ushort	ms_commandIDCounter = 0;

			public COMCommand( string _command, uint _timeOut_ms, Action<string> _onSuccess, Action<string> _onError ) {
				while( m_ID == 0 ) {
					m_ID = ++ms_commandIDCounter;
				}
				m_command = _command;
				m_timeOut_ms = _timeOut_ms;
				m_onSuccess = _onSuccess;
				m_onError = _onError;
			}

			public void	Send( SerialPort _COMPort ) {
				_COMPort.WriteLine( m_ID.ToString() + "," + m_command );
			}

		}
		Queue<COMCommand>		m_COMCommands = new Queue<COMCommand>();

		void	OpenCOMPort( string _COMPortName, Action _onPortOpen, Action _onPortClosed ) {

			// Abort any existing thread
			m_COMThread?.Abort();

			// Start a new thread that will open the COM port and start listening for commands and replies...
			m_COMThread = new System.Threading.Thread( ( object _userParm ) => {
				WaterTankMonitorForm	that = _userParm as WaterTankMonitorForm;

				try {
					lock ( that ) {

						// Dispose of existing port first...
						if ( m_COMPort != null ) {
							m_COMPort.Close();
							m_COMPort.Dispose();
							m_COMPort = null;
						}

						// Build the serial port object
//						m_COMPort = new SerialPort( m_COMPortName, DEFAULT_BAUD_RATE, Parity.Even, 8, StopBits.Two );	// I tried to follow Arduino's documentation but doesn't seem to be working?
						m_COMPort = new SerialPort( _COMPortName, DEFAULT_BAUD_RATE, Parity.None, 8, StopBits.One );
// m_COMPort = new SerialPort( m_COMPortName );
// m_COMPort.BaudRate = DEFAULT_BAUD_RATE;
						m_COMPort.NewLine = "\n";

						m_COMPort.ReadTimeout = 500;
						m_COMPort.WriteTimeout = 500;

//						m_COMPort.DataReceived += COMPort_DataReceived;

						m_COMPort.Open();

						// Wait for the port to be ready before issuing some commands...
						System.Threading.Thread.Sleep( 2000 );

						// Reset last measurement time so a new measurement is issued immediately...
						m_lastMeasurementTime = DateTime.FromFileTime( 0 );
					}

				} catch ( Exception _e ) {
					// Notify and exit thread...
					that.BeginInvoke( (Action) (() => {
						MessageBoxError( "Failed to open COM port!", _e );
					}) );
					return;
				}

				// Notify port is open
				_onPortOpen?.Invoke();

				try {
					// Listen for commands
					while ( true ) {
						COMCommand	command = null;
						lock ( this ) {
							if ( !m_COMPort.IsOpen )
								throw new InvalidOperationException( "Port closed while waiting for commands" );

							if ( m_COMCommands.Count == 0 ) {
								System.Threading.Thread.Sleep( 10 );
								continue;
							}
							command = m_COMCommands.Dequeue();
						}

						// Execute command and wait for reply
						command.Send( m_COMPort );

						// Wait for a response or a timeout
						COMReply	reply = null;
						DateTime	sendTime = DateTime.Now;
						while ( reply == null && (DateTime.Now - sendTime).TotalMilliseconds < command.m_timeOut_ms ) {
//while ( reply == null ) {
							#if true	// This code simply waits for a new line
								try {
									string	newLine = m_COMPort.ReadLine();
									reply = new COMReply( newLine );
								} catch ( TimeoutException ) {
									continue;	// Nothing on the line for us yet...
								}

							#else	// This code uses the COM strings filled up by the even handler
								System.Threading.Thread.Sleep( 10 );
								lock ( this ) {
									if ( m_COMReplies.Count > 0 )
										reply = m_COMReplies.Dequeue();
								}
							#endif

							if ( reply == null )
								continue;	// Still no reply

							if ( reply.m_type == COMReply.TYPE.LOG || reply.m_type == COMReply.TYPE.DEBUG ) {
								// Just log regular strings, don't count as actual replies to the command...
								Log( "<Module Reply " + reply.m_type + "> " + reply.m_string );
								reply = null;
							} else if ( reply.m_commandID != command.m_ID ) {
								// Ignore replies to other commands
								Log( "<Module Reply from other command " + reply.m_type + "> " + reply.m_string );
								reply = null;
							}

						}	// While no reply and no time out

						if ( reply == null ) {
							command.m_onError( "Timeout!" );
						} else if ( reply.m_type == COMReply.TYPE.REPLY ) {
							command.m_onSuccess( reply.m_string );
						} else if ( reply.m_type == COMReply.TYPE.ERROR ) {
							command.m_onError( reply.m_string );
						}

					}	// While ( true )

				} catch ( InvalidOperationException _e ) {
					if ( !m_COMPort.IsOpen ) {
						LogError( "COM Port closed unexpectedly" );
					} else {
						LogError( "Unknown invalid operation", _e );
					}
				} catch ( Exception _e ) {
					LogError( "Unknown error", _e );
				} finally {
					// Notify port closed
					_onPortClosed?.Invoke();
				}

			} );

			m_COMThread.Start( this );
		}

		[System.Diagnostics.DebuggerDisplay( "[{m_commandID}] {m_string} ({m_type})" )]
		class COMReply {
			public enum TYPE {
				UNKNOWN = -1,
				LOG = 0,
				REPLY,
				ERROR,
				DEBUG,
			}
			public ushort	m_commandID = 0;	// Invalid ID!
			public TYPE		m_type = TYPE.UNKNOWN;
			public string	m_string;

			static string[]	ms_stringTypes = new string[] {
				"<LOG> ",
				"<OK> ",
				"<ERROR> ",
				"<DEBUG> "
			};

			public COMReply( byte[] _buffer, int _length ) : this( System.Text.Encoding.ASCII.GetString( _buffer, 0, _length ) ) {
			}

			public COMReply( string _string ) {
				m_string = _string.Trim();

				// Check for command ID prefix
				int	indexOfComma = m_string.IndexOf( ',' );
				if ( indexOfComma != -1 ) {
					string	strCommandID = m_string.Substring( 0, indexOfComma );
					m_string = m_string.Substring( indexOfComma+1 );
					if ( !ushort.TryParse( strCommandID, out m_commandID ) )
						m_commandID = 0;	// Invalid command ID!
				}

				// Check for reply type
				for ( int stringTypeIndex=0; stringTypeIndex < 4; stringTypeIndex++ ) {
					if ( m_string.StartsWith( ms_stringTypes[stringTypeIndex] ) ) {
						m_string = m_string.Substring( ms_stringTypes[stringTypeIndex].Length );
						m_type = (TYPE) stringTypeIndex;
						break;
					}
				}
			}
		}

		Queue<COMReply>		m_COMReplies = new Queue<COMReply>();

// This code handle COM replies asynchronously but I eventually prefered to read new lines from the COM port continuously, it's easier and safer...
//		Action<COMReply>	m_onNewCOMReplyReceived = null;
//
// 		byte[]	m_tempCOMBuffer = new byte[4096];
// 		int		m_tempCOMBufferOffset = 0;
// 		private void COMPort_DataReceived( object sender, SerialDataReceivedEventArgs e ) {
// 			// Read new data
// 			int	bytesCount = m_COMPort.BytesToRead;
// 			m_COMPort.Read( m_tempCOMBuffer, m_tempCOMBufferOffset, bytesCount );
// 			int	oldTempCOMBufferOffset = m_tempCOMBufferOffset;
// 			m_tempCOMBufferOffset += bytesCount;
// 
// 			// Check if there's any new-line character in the temp buffer
// 			for ( int i=oldTempCOMBufferOffset; i < m_tempCOMBufferOffset; i++ ) {
// 				if ( m_tempCOMBuffer[i] == '\n' ) {
// 					// Okay, log a new line!
// 					COMReply	str = null;
// 					lock ( this ) {
// 						str = new COMReply( m_tempCOMBuffer, i+1 );
// 						m_COMReplies.Enqueue( str );
// 					}
// 
// 					// Copy what's left of the bytes after the '\n' so it's the begining of a new line
// 					Array.Copy( m_tempCOMBuffer, i+1, m_tempCOMBuffer, 0, m_tempCOMBufferOffset - i - 1 );
// 					m_tempCOMBufferOffset = 0;
// 
// 					// Notify of a new line
// 					m_onNewCOMReplyReceived?.Invoke( str );
// 					return;
// 				}
// 			}
// 		}

		/// <summary>
		/// Executes a new command
		/// </summary>
		void	ExecuteCommand( string _command, uint _timeOut_ms, Action<string> _onSuccess, Action<string> _onError ) {
			ExecuteCommand( new COMCommand( _command, _timeOut_ms, _onSuccess, _onError ) );
		}
		void	ExecuteCommand( COMCommand _command ) {
			lock ( this ) {
				m_COMCommands.Enqueue( _command );
			}
		}
/*
		/// <summary>
		/// Sends a command and waits for a response
		/// A command should consist of <COMMAND_NAME>,[<PAYLOAD>] with <COMMAND_NAME> being a 4 letter command name.
		/// </summary>
		/// <param name="_command"></param>
		/// <param name="_timeOut_ms"></param>
		/// <returns>The response to the command</returns>
		/// <exception cref="">Command time out</exception>
//		byte[]	m_serialBuffer = new byte[256];
		string	SendCommand( string _command, uint _timeOut_ms ) {
			SendString( _command );

			// Wait for a response or a timeout
			DateTime	sendTime = DateTime.Now;
			while ( (DateTime.Now - sendTime).TotalMilliseconds < _timeOut_ms ) {
				System.Threading.Thread.Sleep( 10 );

				int	bytesToRead = m_COMPort.BytesToRead;
				if ( bytesToRead > 0 ) {
// 					if ( bytesToRead > m_serialBuffer.Length )
// 						m_serialBuffer = new byte[2*bytesToRead];
//					m_COMPort.Read( m_serialBuffer, 0, bytesToRead );
					string	reply = m_COMPort.ReadLine();
					if ( reply.StartsWith( "<DEBUG>" ) ) {
						System.Diagnostics.Debug.WriteLine( reply );	// Ignore, not the reply we're waiting for!
					} else {
						return reply;
					}
				}
			}

			throw new CommandTimeOutException();
		}
*/

		/// <summary>
		/// Asks the module to perform a new volume measurement
		/// </summary>
		/// <returns></returns>
		void	PerformMeasurement( Action<string> _onError ) {
			PerformMeasurement(
				( LogEntry _newMeasurement ) => {
					// Register new measurement
					m_logEntries.Add( _newMeasurement );

					// Update graph (on main thread only!)
					this.BeginInvoke( (Action) (() => {
						if ( FollowRuntime ) {
							WindowEndTime = _newMeasurement.m_timeStamp + m_timeFromNow;	// Update window's end time to match this measurement and also keep its delta time from now constant
						} else {
							UpdateGraph();
						}
					}) );
				},
				_onError
			);
		}

		bool	m_pipoMeasurement = false;
		void	PerformMeasurement( Action<LogEntry> _onMeasurement, Action<string> _onError ) {
			ExecuteCommand( m_pipoMeasurement ? "MEASUREPIPO" : "MEASURE", 10 * 1000,
				( string _reply ) => {
					// Handle reply!

m_pipoMeasurement = false;

					if ( _reply.StartsWith( "TIME=" ) ) {
						// Prepare a new entry
						int	rawTime_microSeconds;
						if ( int.TryParse( _reply.Substring( 5 ), out rawTime_microSeconds ) ) {
							LogEntry	result = null;
							if ( rawTime_microSeconds == -1 ) {
								// Register an out of range entry
								result = new LogEntry( ~0U );
								LogError( "Out of range!" );
							} else if ( rawTime_microSeconds < 1000 ) {
								LogError( "Time < 1000µs, measurement error?" );
							} else {
								result = new LogEntry( (uint) rawTime_microSeconds );
							}

							// Notify
							if ( result != null ) {
								_onMeasurement?.Invoke( result );
							}

						} else {
							LogError( "Failed to parse proper time value from MEASURE command reply!" );
							LogError( "Reply = " + _reply );
						}

					} else {
						LogError( "Unexpected reply after MEASURE command!" );
						LogError( "Reply = " + _reply );
					}
				},

				( string _error ) => _onError?.Invoke( _error )
			);
		}

		/// <summary>
		/// Sets the module's reference time use for creating time stamps
		/// </summary>
		/// <param name="_referenceTime"></param>
		void	SetReferenceTime( DateTime _referenceTime ) {
			DateTime	baseTime = new DateTime( 2024, 06, 12, 0, 0, 0 );	// This is the day this application was born
			int			referenceTime_seconds = (int) (_referenceTime - baseTime).TotalSeconds;

			ExecuteCommand( "SETTIME=" + referenceTime_seconds, 10 * 1000,
				( string _reply ) => {
					Log( "Successfully set reference time!" );
				},

				( string _error ) => {
					LogError( "An error occurred while setting type reference!" );
					LogError( _error );
				}
			);
		}

// 		/// <summary>
// 		/// Retrieve any unread measurements stored in the buffer
// 		/// </summary>
// 		void	RetrieveUnreadMeasurements() {
// 			try {
// 				string	reply = SendCommand( "GETBUFFERSIZE", 10 * 1000 );
// 
// 				// Handle reply!
// 				if ( !reply.StartsWith( "<OK>" ) ) {
// 					throw new Exception( "Unexpected reply: " + reply );
// 				}
// 
// 				int	bufferSize = int.Parse( reply.Substring( 4 ) );
// 				if ( bufferSize == 0 )
// 					return;	// Buffer is empty...
// 
// 				reply = SendCommand( "READBUFFER", 10 * 1000 );
// 				if ( !reply.StartsWith( "<OK>" ) ) {
// 					throw new Exception( "Unexpected reply: " + reply );
// 				}
// 
// throw new Exception( "@TODO!" );
// // 				read lines
// // 				read checksum
// 
// 			} catch ( CommandTimeOutException ) {
// 				ShowError( "Timeout while retrieving unread measurements!" );
// 			} catch ( Exception _e ) {
// 				ShowError( "An error occurred while retrieving unread measurements!\r\n" + _e.Message );
// 			}
// 		}

		#endregion

		#region Log Files

		void	ReadLogEntries( FileInfo _fileNameLog ) {
			if ( !_fileNameLog.Exists )
				throw new FileNotFoundException();

			using ( StreamReader R = _fileNameLog.OpenText() ) {
				while ( !R.EndOfStream ) {
// 					string	line = R.ReadLine().Trim();
// 					if ( line.Length == 0 )
// 						break;	// End of stream...

					LogEntry	entry = new LogEntry( R );
					m_logEntries.Add( entry );
				}
			}

			m_entriesCountOnLastLogSave = m_logEntries.Count;
		}

		void	WriteLogEntries( FileInfo _fileNameLog ) {
			if ( _fileNameLog.Exists ) {
				// Backup any existing file first...
				string	backupFileName = _fileNameLog.FullName + ".bak";
				if ( File.Exists( backupFileName ) ) {
					File.Delete( backupFileName );
				}
				File.Copy( _fileNameLog.FullName, backupFileName );
			}

			using ( StreamWriter W = _fileNameLog.CreateText() ) {
				foreach ( LogEntry entry in m_logEntries ) {
					entry.Write( W );
				}
			}
		}

		void	WriteSessionLog( FileInfo _fileNameLog ) {
			using ( StreamWriter W = _fileNameLog.CreateText() ) {
				foreach ( string entry in m_sessionLog ) {
					W.WriteLine( entry );
				}
			}
		}

		#endregion

		#region Info / Warning / Error Notification

		void	Log( string _text ) {
			string	logText = DateTime.Now.ToString( "MM/dd HH:mm" ) + "> " + _text;
			m_sessionLog.Add( logText );

			System.Diagnostics.Debug.WriteLine( logText );
		}
		void	LogError( string _text, Exception _e=null ) {
			Log( "<ERROR> " + _text );
			if ( _e != null ) {
				Log( "	Exception: " + _e.Message );
			}
		}

// 		void	ShowWarning( string _message ) {
// 			notifyIcon.ShowBalloonTip( 5000, "Water Tank Monitor", _message, ToolTipIcon.Warning );
// 		}
// 
// 		void	ShowError( string _message ) {
// 			notifyIcon.ShowBalloonTip( 5000, "Water Tank Monitor", _message, ToolTipIcon.Error );
// 		}

		void	MessageBoxError( string _message, Exception _e ) {
			_message += "\r\n";
			_message += "Exception:\r\n";
			_message += _e.Message + "\r\n";
//			_message += _e.StackTrace;
			MessageBox( _message, MessageBoxIcon.Error );
		}

		DialogResult	MessageBox( string _message, MessageBoxIcon _icon, MessageBoxButtons _buttons=MessageBoxButtons.OK, MessageBoxDefaultButton _default=MessageBoxDefaultButton.Button1 ) {
			return System.Windows.Forms.MessageBox.Show( this, _message, "Water Tank Monitor", _buttons, _icon, _default );
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

		Point			m_mouseDownLocation;
		DateTime		m_mouseDown_WindowEndTime;
		MouseButtons	m_mouseButtonsDown = MouseButtons.None;
		private void PanelOutput_MouseMove( object sender, MouseEventArgs e ) {
			if ( m_mouseButtonsDown == MouseButtons.Left ) {
				// Scroll window
				float	windowSize_HoursPerPixel = WindowSize_Hours / (panelOutput.Width - 2 * m_margin);
				float	hours = windowSize_HoursPerPixel * (m_mouseDownLocation.X - e.X);

				DateTime	newEndTime = m_mouseDown_WindowEndTime + TimeSpan.FromHours( hours );
				if ( WindowStartTime > DateTime.Now ) {
					newEndTime = DateTime.Now + TimeSpan.FromHours( WindowSize_Hours );	// Make sure window can't go into the future
				}
				WindowEndTime = newEndTime;

			} else if ( m_mouseButtonsDown == MouseButtons.None ) {
				UpdateGraph();	// Display value at mouse position
			}
		}

		private void PanelOutput_MouseDown( object sender, MouseEventArgs e ) {
			m_mouseDownLocation = e.Location;
			m_mouseButtonsDown |= e.Button;
			m_mouseDown_WindowEndTime = WindowEndTime;
			panelOutput.Capture = true;
		}

		private void PanelOutput_MouseUp( object sender, MouseEventArgs e ) {
			m_mouseDownLocation = e.Location;
			m_mouseButtonsDown &= ~e.Button;
			m_mouseDown_WindowEndTime = WindowEndTime;
			panelOutput.Capture = false;
		}

		private void PanelOutput_MouseWheel( object sender, MouseEventArgs e ) {
			float	factor = 1.1f;
			WindowSize_Hours *= e.Delta < 0.0f ? factor : 1.0f / factor;
		}

//		SolidBrush	m_brushBackground = new SolidBrush( Color.FromArgb( 255, 255, 224 ) );	// Light yellow
		SolidBrush	m_brushBackground = new SolidBrush( Color.FromArgb( 255, 240, 200 ) );
		SolidBrush	m_brushMarker = new SolidBrush( Color.FromArgb( 255, 200, 100 ) );
		Brush		m_brushTextBackground = Brushes.IndianRed;
		Brush		m_brushTextMarker = Brushes.Orange;
		Pen			m_penGraph = new Pen( Color.Black, 2 );
		Pen			m_penLowLevelLimit = new Pen( Color.Red );			// Dash style will be changed
		Brush		m_brushLowLevelLimit = Brushes.Red;
		Pen			m_penReferenceTime = new Pen( Color.DarkGray, 2 );	// Dash style will be changed
		Brush		m_brushReferenceTime = Brushes.DarkGray;

		enum MARKER_TYPE {
			MONTH,
			WEEK,
			DAY,
			HOUR,
			QUARTER_HOUR,
		}

		string[]	m_monthNames = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

		private void PanelOutput_BitmapUpdating( int W, int H, Graphics G ) {

			m_penLowLevelLimit.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
			m_penReferenceTime.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

			G.FillRectangle( m_brushBackground, 0, 0, W, H );

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

			// Paint vertical markers
			float		markerSize_hours = 0.25f;	// Default marker size is 1/4 hour
			MARKER_TYPE	markerType = MARKER_TYPE.QUARTER_HOUR;
			if ( m_windowSize_Hours > 744.0f ) {
				markerSize_hours = 744.0f;		// Marker size is a month
				markerType = MARKER_TYPE.MONTH;
			} else if ( m_windowSize_Hours > 168.0f ) {
				markerSize_hours = 168.0f;		// Marker size is a week
				markerType = MARKER_TYPE.WEEK;
			} else if ( m_windowSize_Hours > 24.0f ) {
				markerSize_hours = 24.0f;		// Marker size is a day
				markerType = MARKER_TYPE.DAY;
			} else if ( m_windowSize_Hours > 1.0f ) {
				markerSize_hours = 1.0f;		// Marker size is an hour
				markerType = MARKER_TYPE.HOUR;
			}

			DateTime	now = DateTime.Now;
			DateTime	markerReferenceTime = new DateTime( now.Year, now.Month, 1 );
// 			switch ( markerType ) {
// 				case MARKER_TYPE.MONTH: markerReferenceTime = new DateTime( now.Year, now.Month, 0 ); break;
// 				case MARKER_TYPE.WEEK: markerReferenceTime = new DateTime( now.Year, now.Month, 7 * (now.Day / 7) ); break;
// 			}
// 			markerReferenceTime.d

			float	windowEndTime_hours = (float) (WindowEndTime - markerReferenceTime).TotalHours;
			float	windowEndTime_markers = windowEndTime_hours / markerSize_hours;	// How many markers can we fit to reach window's end?
			int		endMarkerIndex = (int) Math.Ceiling( windowEndTime_markers );

			float	windowStartTime_hours = (float) (WindowStartTime - markerReferenceTime).TotalHours;
//			float	windowStartTime_hours = windowEndTime_hours - m_windowSize_Hours;
			float	windowStartTime_markers = windowStartTime_hours / markerSize_hours;
			int		startMarkerIndex = (int) Math.Floor( windowStartTime_markers );

			float	markerWidth = W / (windowEndTime_markers - windowStartTime_markers);
			float	markerStartX = -markerWidth * windowStartTime_markers;
			for ( int markerIndex=startMarkerIndex; markerIndex < endMarkerIndex; markerIndex++ ) {
				if ( (markerIndex & 1) != 0 ) {
					G.FillRectangle( m_brushMarker, markerStartX + markerIndex * markerWidth, 0, markerWidth, H );
				}

				string	markerText = "";
				switch ( markerType ) {
					case MARKER_TYPE.MONTH: markerText = m_monthNames[(((markerReferenceTime.Month + markerIndex)%12)+12)%12]; break;
					case MARKER_TYPE.WEEK: break;
					case MARKER_TYPE.DAY: markerText = (markerReferenceTime + TimeSpan.FromDays( markerIndex )).DayOfWeek.ToString(); break;
					case MARKER_TYPE.HOUR: markerText = (markerReferenceTime + TimeSpan.FromHours( markerIndex )).Hour.ToString( "G02" ); break;
					case MARKER_TYPE.QUARTER_HOUR: markerText = ((markerIndex & 3) * 15).ToString( "G02" ); break;
				}
				SizeF	markerTextSize = G.MeasureString( markerText, panelOutput.Font );
				G.DrawString( markerText, panelOutput.Font, (markerIndex & 1) != 0 ? m_brushTextBackground : m_brushTextMarker, markerStartX + markerIndex * markerWidth + 0.5f * (markerWidth - markerTextSize.Width), 8 );
			}

			// =====================================
			// Paint level lines
			float	Y0 = Level2Client( 1.00f * TANK_CAPACITY_LITRES );	// 4000L line
			float	Y1 = Level2Client( 0.75f * TANK_CAPACITY_LITRES );	// 3000L line
			float	Y2 = Level2Client( 0.50f * TANK_CAPACITY_LITRES );	// 2000L line
			float	Y3 = Level2Client( 0.25f * TANK_CAPACITY_LITRES );	// 1000L line
			Pen		penLevelLine = Pens.Orange;
			Brush	brushLevelLine = Brushes.Orange;
			G.DrawLine( penLevelLine, m_margin, Y0, W - m_margin, Y0 );
			G.DrawLine( penLevelLine, m_margin, Y1, W - m_margin, Y1 );
			G.DrawLine( penLevelLine, m_margin, Y2, W - m_margin, Y2 );
			G.DrawLine( penLevelLine, m_margin, Y3, W - m_margin, Y3 );
			G.DrawString( "4000L", Font, brushLevelLine, W - m_margin - 30, Y0 + 0 );
			G.DrawString( "3000L", Font, brushLevelLine, W - m_margin - 30, Y1 + 0 );
			G.DrawString( "2000L", Font, brushLevelLine, W - m_margin - 30, Y2 + 0 );
			G.DrawString( "1000L", Font, brushLevelLine, W - m_margin - 30, Y3 + 0 );

			float	Y_warning = Level2Client( m_lowWaterLevelWarningLimit_litres );	// Low limit line
			G.DrawLine( m_penLowLevelLimit, m_margin, Y_warning, W - m_margin, Y_warning );
			G.DrawString( ((int) m_lowWaterLevelWarningLimit_litres).ToString(), Font, m_brushLowLevelLimit, W - m_margin - 30, Y_warning );

			// =====================================
			// Paint measurements
			LogEntry	mouseEntry = null;
			int			mouseEntyIndex = -1;
			float		mouseEntryX = 0, mouseEntryY = 0;
			float		closestMouseEntryDistance = float.MaxValue;
			float		mouseX = panelOutput.PointToClient( Control.MousePosition ).X;

			if ( m_logEntries.Count > 1 ) {
				LogEntry	previous = null;
				float		previousX = 0;
				float		previousY = 10;
				for ( int entryIndex=m_logEntries.Count-1; entryIndex >= 0; entryIndex-- ) {
					LogEntry	current = m_logEntries[entryIndex];
					float		X = TimeStamp2Client( current );
					float		Y = previousY;

					if ( current.IsOutOfRange ) {
						G.FillEllipse( Brushes.Red, X-3, Y-3, 7, 7 );
					} else if ( current.IsVolumeOutOfRange ) {
						G.FillEllipse( Brushes.DarkOrange, X-3, Y-3, 7, 7 );
					} else {
						Y = Level2Client( current );
						if ( previous != null ) {
							G.DrawLine( m_penGraph, X, Y, previousX, previousY );
						}
					}

					float	mouseDistance = Math.Abs( X - mouseX );
					if ( mouseDistance < closestMouseEntryDistance ) {
						// Update entry closest to mouse position
						closestMouseEntryDistance = mouseDistance;
						mouseEntyIndex = entryIndex;
						mouseEntry = current;
						mouseEntryX = X;
						mouseEntryY = Y;
					}

					if ( current.IsOutOfRange || current.IsVolumeOutOfRange ) {
						previous = null;	// Prevent drawing lines through invalid 
					} else {
						previous = current;
					}
					previousX = X;
					previousY = Y;
					if ( X < 0 )
						break;	// We've escaped through left side, no need to paint any more measurements...
				}
			}

			if ( closestMouseEntryDistance > 100 ) {
				mouseEntry = null;	// Ignore entries that are too far away from the mouse...
			}

			// =====================================
			// Paint reference time line
			float	X_timeRef = TimeStamp2Client( m_timeReference );
			if ( X_timeRef >= 0 && X_timeRef < W ) {
				G.DrawLine( m_penReferenceTime, X_timeRef, 0, X_timeRef, Height );

				string	strRefTime = m_timeReference.ToString( "dddd dd MMMM HH:mm" );
				G.DrawString( strRefTime, Font, m_brushReferenceTime, X_timeRef, 0 );
			}


			// =====================================
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
			G.ResetTransform();


			// =====================================
			// Paint mouse info on hovered entry
			if ( mouseEntry == null )
				return;

			// Compute volume derivative
			LogEntry	prevMouseEntry = mouseEntyIndex > 0 ? m_logEntries[mouseEntyIndex-1] : mouseEntry;
			LogEntry	nextMouseEntry = mouseEntyIndex < m_logEntries.Count-1 ? m_logEntries[mouseEntyIndex+1] : mouseEntry;
			float		deltaTime = (float) (nextMouseEntry.m_timeStamp - prevMouseEntry.m_timeStamp).TotalHours;
			float		deltaVolume = nextMouseEntry.Volume - prevMouseEntry.Volume;

			// Draw hovered log entry
			G.DrawLine( Pens.Red, mouseEntryX, 0, mouseEntryX, Height );
			G.DrawEllipse( Pens.Red, mouseEntryX-3, mouseEntryY-3, 7, 7 );

			// Draw text rectangle
			string	textEntry = null;
			if ( !mouseEntry.IsOutOfRange ) {
				textEntry = mouseEntry.m_timeStamp.ToString( "dd MMMM HH:mm" ) + "\n"
						  + "Volume = " + ((int) mouseEntry.Volume) + " L (raw = " + mouseEntry.m_rawTime_microSeconds + ")\n";

				if ( deltaTime > 1e-3f ) {
					textEntry += "Consumption = " + (int) (deltaVolume / deltaTime) + " L/h\n";
				} else {
					textEntry += "(Consumption non computable)\n";
				}

				if ( IsTimeReferenceValid ) {
					LogEntry	referenceEntry = FindEntry( m_timeReference );
					float		totalVolume = mouseEntry.Volume - referenceEntry.Volume;
					if ( referenceEntry != null ) {
						textEntry += "\n"
								  + (totalVolume < 0 ? "Consumed " : "♥ Collected +") + (int) totalVolume + " L since\n"
								  + "reference time " + m_timeReference.ToString( "dd MMMM HH:mm" ) + "\n";
					}
				}
			} else {
				textEntry = mouseEntry.m_timeStamp.ToString( "dd MMMM HH:mm" ) + "\n"
						  + "► Out Of Range! ◄";
			}

			SizeF	textBoxSize = G.MeasureString( textEntry, Font );
			float	textBoxMargin = 4;
			float	rectW = textBoxMargin + textBoxSize.Width + textBoxMargin;
			float	rectH = textBoxMargin + textBoxSize.Height + textBoxMargin;
			float	rectX =  mouseEntryX + 5;
			float	rectY =  mouseEntryY - 0.5f * rectH;

			if ( rectX + rectW > panelOutput.Width ) {
				rectX = mouseEntryX - 5 - rectW;
			}
			if ( rectY < 0 ) {
				rectY = 0;
			} else if ( rectY + rectH > panelOutput.Height ) {
				rectY = Height - rectH;
			}
			G.FillRectangle( Brushes.LemonChiffon, rectX, rectY, rectW, rectH );
			G.DrawRectangle( Pens.Black, rectX, rectY, rectW, rectH );
			G.DrawString( textEntry, Font, Brushes.Black, rectX + textBoxMargin, rectY + textBoxMargin );
		}

		float	TimeStamp2Client( LogEntry _entry ) {
			return TimeStamp2Client( _entry.m_timeStamp );
		}
		float	TimeStamp2Client( DateTime _timeStamp ) {
			float	W = panelOutput.Width - 2 * m_margin;
			float	deltaTime_Hours = (float) (m_windowEndTime - _timeStamp).TotalHours;
			return m_margin + W * (1.0f - deltaTime_Hours / m_windowSize_Hours);
		}

		DateTime	Client2TimeStamp( float _clientX ) {
			float	W = panelOutput.Width - 2 * m_margin;
			float	deltaTime_Hours = (1.0f - (_clientX - m_margin) / W) * m_windowSize_Hours;
			return m_windowEndTime - TimeSpan.FromHours( deltaTime_Hours );
		}

		// Transforms a volume in litres into a Y coordinate
		float	Level2Client( LogEntry _entry ) {
			return Level2Client( _entry.Volume );
		}
		float	Level2Client( float _volume_litres ) {
			float	H = panelOutput.Height - 3 * m_margin;
			return panelOutput.Height - m_margin - H * _volume_litres / TANK_CAPACITY_LITRES;
		}
		float	Client2Level( float _clientY ) {
			float	H = panelOutput.Height - 3 * m_margin;
			return TANK_CAPACITY_LITRES * (panelOutput.Height - m_margin - _clientY) / H;
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

		/// <summary>
		/// Finds the entry closest to the specified time
		/// </summary>
		/// <param name="_time"></param>
		/// <returns></returns>
		LogEntry	FindEntry( DateTime _time ) {
			LogEntry	result = null;
			double		closestSecondsToEntry = float.MaxValue;
			foreach ( LogEntry entry in m_logEntries ) {
				double	secondsToEntry = Math.Abs( (entry.m_timeStamp - _time).TotalSeconds );
				if ( secondsToEntry < closestSecondsToEntry ) {
					result = entry;
					closestSecondsToEntry = secondsToEntry;
				}
			}

			return result;
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

			OpenCOMPort( m_COMPortName,
				() => {
					// Change to OPEN color
					buttonRefreshCOMPorts.BeginInvoke( (Action) (() => {
						buttonRefreshCOMPorts.BackColor = Color.ForestGreen;
					}) );

					// Set the reference time
					SetReferenceTime( DateTime.Now );

					// Ask if there are some unread values in the buffer
//					RetrieveUnreadMeasurements();

					this.BeginInvoke( (Action) (() => { 
						UpdateGraph();
					}) );
				},

				() => {
					// Change to CLOSED color
					buttonRefreshCOMPorts.BeginInvoke( (Action) (() => {
						buttonRefreshCOMPorts.BackColor = Color.IndianRed;
					}) );
				}
			);
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

		private void buttonHour_Click( object sender, EventArgs e ) {
			WindowSize_Hours = 1;
		}

		private void buttonNow_Click( object sender, EventArgs e ) {
			m_timeFromNow = TimeSpan.Zero;
			WindowEndTime = DateTime.Now;
		}

		#region Context Menu

		private void contextMenuStripPanel_Opening( object sender, CancelEventArgs e ) {
			m_mouseDownLocation = panelOutput.PointToClient( Control.MousePosition );
		}

		private void setLowLevelWarningLimitToolStripMenuItem_Click( object sender, EventArgs e ) {
			float	value = Client2Level( m_mouseDownLocation.Y );
			if ( value < 100 || value > TANK_CAPACITY_LITRES ) {
				// Out of range!
				MessageBox( "The specified value is invalid.\r\nIt must be comprised in the [100," + TANK_CAPACITY_LITRES + "] litres range!", MessageBoxIcon.Warning );
				return;
			}
			m_lowWaterLevelWarningLimit_litres = value;
			SetRegKey( "m_lowWaterLevelWarningLimit_litres", m_lowWaterLevelWarningLimit_litres.ToString() );
			UpdateGraph();
		}

		private void setTimeReferenceToolStripMenuItem_Click( object sender, EventArgs e ) {
			m_timeReference = Client2TimeStamp( m_mouseDownLocation.X );
			SetRegKey( "TimeReference", m_timeReference.ToFileTime().ToString( "X08" ) );
			UpdateGraph();
		}

		private void clearTimeReferenceToolStripMenuItem_Click( object sender, EventArgs e ) {
			m_timeReference = DateTime.Today + TimeSpan.FromDays( 10000 );
			SetRegKey( "TimeReference", m_timeReference.ToFileTime().ToString( "X08" ) );
			UpdateGraph();
		}

		private void takeMeasurementToolStripMenuItem_Click( object sender, EventArgs e ) {
			PerformMeasurement( null );
		}

		private void saveLogFileNowToolStripMenuItem_Click( object sender, EventArgs e ) {
			try {
				WriteLogEntries( m_fileNameLogEntries );
			} catch ( Exception _e ) {
				LogError( "Failed to save log!", _e );
			}

			try {
				WriteSessionLog( m_fileNameSessionLog );
			} catch ( Exception _e ) {
				MessageBoxError( "Failed to save session log!", _e );
			}
		}

		private void exitToolStripMenuItem_Click( object sender, EventArgs e ) {
			if ( MessageBox( "Are you sure you want to exit the application?", MessageBoxIcon.Question, MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2 ) == DialogResult.Yes ) {
				m_exitApplication = true;
				Close();
			}
		}

		#endregion

		#endregion

		private void button1_Click( object sender, EventArgs e ) {
// 			ExecuteCommand( "GETBUFFERSIZE", 10 * 1000, ( string _reply ) => {
// 				System.Diagnostics.Debug.WriteLine( _reply );
// 			}, ( string _error ) => {
// 				System.Diagnostics.Debug.WriteLine( "Error: " + _error );
// 			} );

			// Simulate a dummy measurement
			m_pipoMeasurement = true;
			PerformMeasurement( null );
		}

		private void notifyIcon_MouseClick( object sender, MouseEventArgs e ) {
			if ( e.Button == MouseButtons.Left )
				this.Visible = !this.Visible;	// Toggle the form
		}
	}
}
