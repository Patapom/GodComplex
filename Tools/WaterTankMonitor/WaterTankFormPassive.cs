#define USE_DUAL_MEASUREMENTS	// Compute volume using both high & low measurements (slanted sensor assumption), as opposed as only a single measurement (vertical sensor assumption)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.IO.Ports;


// @TODO: Meilleur filtre!

// Débugger ce putain de safe handle disposed quand on abort la thread de COM!

namespace WaterTankMonitorPassive {

	public partial class WaterTankMonitorForm : Form {

		#region CONSTANTS

		public const int	DEFAULT_BAUD_RATE = 19200;				// Default baud rate to use to communicate with the Arduino

		public const int	SAVE_LOG_INTERVAL_MINUTES = 15;			// Save log every 15 minutes
		public const int	SAVE_LOG_AFTER_UNSAVED_ENTRIES_COUNT = 10;	// Save log if we have more than 10 unsaved entries

		public const float	MEASUREMENT_FILTER_KERNEL_SIZE_MINUTES = 30.0f;	// Filter measures in a 30 minutes kernel
//		public const float	MEASUREMENT_FILTER_KERNEL_SIGMA = 0.466f;		// Gaussian filter is exp( -(delta_t / kernel half size)^2 / sigma^2 ) so we choose sigma so that we have a sensible small value for exp( -1 / sigma^2 ) = 0.01 => sigma = sqrt( -1 / log( 0.01 ) )
		public const float	MEASUREMENT_FILTER_KERNEL_SIGMA = 0.3f;		// Gaussian filter is exp( -(delta_t / kernel half size)^2 / sigma^2 ) so we choose sigma so that we have a sensible small value for exp( -1 / sigma^2 ) = 0.01 => sigma = sqrt( -1 / log( 0.01 ) )

		public const int	DELAY_BETWEEN_WARNINGS_MINUTES = 30;	// Warn every 30 minutes

		public const float	DEFAULT_SPEED_OF_SOUND = 343.4f;		// Default speed of sound at 1 bar and 20°C is 343.4 m/s

		public const float	TANK_CAPACITY_LITRES = 4000;			// Tank capacity in litres
		public const float	TANK_MINIMAL_CAPACITY_LITRES = 312;		// Tank minimal capacity in litres, where the water pipe is connected (13.5cm is the top of the pipe from the bottom of the tank)

		public const float	TANK_MAX_CAPACITY_LITRES = 4150;		// Absolute maximum tank capacity in litres, when the tank is completely full and is spilling into the neighbor tank (used to detect out of range measurements)


		//////////////////////////////////////////////////////////////////////////
		/// Sensor Version 0
		///
		public const float	TANK_HEIGHT_FULL0 = 1.73f;				// Tank height when full (

		// Reference tank measurements at time of installation
// 		public const float	TANK_HEIGHT_REFERENCE0 = 1.395f;		// Tank height reference (measured for 3225L)
// 		public const uint	MEASURED_TIME_REFERENCE0 = 3724;		// Raw time reference value (measured for 3225L) (=0.6394108 meters from sensor) (sensor is at 2.034 m)
// 
// 			// Measured on June 21st at 18:00 (after replacement of faulty sensor by one that is inside a (hopefully) wataerproof container)
// 		public const float	TANK_HEIGHT_REFERENCE0 = 1.25f;			// Tank height reference (measured for 2900L)
// 		public const uint	MEASURED_TIME_REFERENCE0 = 4191;		// Raw time reference value (measured for 2900L) (=0.7195947 meters from sensor) (sensor is at 1.969 m)

			// Measured on June 23st at 9:30 (after another replacement of the container so it only covers a single exha)
		public const float	TANK_HEIGHT_REFERENCE0 = 1.11585f;		// Tank height reference (measured for 2580L)
		public const uint	MEASURED_TIME_REFERENCE0 = 5115;		// Raw time reference value (measured for 2580L) (=0.878 meters from sensor) (sensor is at 1.9941 m)

		//////////////////////////////////////////////////////////////////////////
		/// Sensor Version 1 (SR-04 module is separate from the actual speaker/microphone)
		///
		#if USE_DUAL_MEASUREMENTS
			// Tank swap on July 22nd => We finally have a measurement for a low water level (~100L)
			// Mesure du tank 4U au moment du changement : 3100 L / 1.335m => 3146µs => 0.6775282 m (sensor would be at 2.012 m)
			//
			public const float	MEASURED_TANK_VOLUME_HIGH = 3397.0f;	// Approximate tank volume when high
			public const int	MEASURED_TIME_REFERENCE_HIGH = 3128;	// Raw time reference value (= 0.5370 meters from sensor)
																		// Measured height = 1.472 m => sensor would be at 2.009 m


			public const float	MEASURED_TANK_VOLUME_LOW = 100.0f;		// Approximate tank volume when low
			public const int	MEASURED_TIME_REFERENCE_LOW = 10786;	// Raw time reference value (= 1.8519 meters from sensor)
																		// Measured height = 0.18 m => sensor would be at 2.032 m (totally coherent!)
		#else
			// Measured on July 2nd at 14:50 (sensor upgrade!)
			public const float	TANK_HEIGHT_FULL1 = 1.733f;				// Tank height when full

			public const float	TANK_HEIGHT_REFERENCE1 = 1.472f;		// Tank height reference (measured for 3397L)
			public const uint	MEASURED_TIME_REFERENCE1 = 3128;		// Raw time reference value (measured for 3397L) (=0.5370 meters from sensor) (sensor is at 2.009 m)
		#endif

		#endregion

		#region TYPES

		class CommandTimeOutException : Exception { }

		[System.Diagnostics.DebuggerDisplay( "{Volume,d}L  ({m_timeStamp})" )]
		class LogEntry {

			enum SENSOR_VERSION {
				VERSION0,	// First sensor, HR-SR04 from 
				VERSION1,	// New sensor changed on July 2nd at 15h
				VERSION2,	// Switch to tank 4 July 22nd at 14h
			}

			public DateTime	m_timeStamp;
			SENSOR_VERSION	m_sensorVersion;
			public uint		m_rawTime_microSeconds;	// The raw sensor time of flight, in µs

			public float	m_filteredVolume = -1;	// Filtered version of the volume

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
					return volume < 0 || volume > TANK_MAX_CAPACITY_LITRES;
				}
			}

			/// <summary>
			/// Gets the tank volume in litres
			/// </summary>
			public float	Volume {
				get {
					return m_sensorVersion == SENSOR_VERSION.VERSION0 ? RawTime2Volume0( m_rawTime_microSeconds ) : RawTime2Volume1( m_rawTime_microSeconds );
				}
			}

			static DateTime	S_SENSOR_CHANGE0_1 = new DateTime( 2024, 7, 2, 15, 0, 0 );	// Date of change from sensor version 0 to version 1

			public LogEntry( uint _rawTime_microSeconds ) : this( _rawTime_microSeconds, DateTime.Now ) {
			}

			public LogEntry( uint _rawTime_microSeconds, DateTime _timeStamp ) {
				m_timeStamp = _timeStamp;
				m_sensorVersion = m_timeStamp <  S_SENSOR_CHANGE0_1 ? SENSOR_VERSION.VERSION0 : SENSOR_VERSION.VERSION1;
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
			public static float	RawTime2Volume0( uint _rawTime_microSeconds ) {
				float	distance_meters = RawTime2Distance( _rawTime_microSeconds );
				float	distance_Ref = RawTime2Distance( MEASURED_TIME_REFERENCE0 );
				float	distance_4000L = distance_Ref - (TANK_HEIGHT_FULL0 - TANK_HEIGHT_REFERENCE0);	// Measured distance when the tank is full
				float	distance_0L = distance_4000L + TANK_HEIGHT_FULL0;								// Measure distance when the tank is empty
				return (distance_meters - distance_0L) * TANK_CAPACITY_LITRES / (distance_4000L - distance_0L);
			}

			public static float	RawTime2Volume1( uint _rawTime_microSeconds ) {
				#if USE_DUAL_MEASUREMENTS	// Dual measurement (high + low)
					float	t = (float) ((int) _rawTime_microSeconds - MEASURED_TIME_REFERENCE_LOW) / (MEASURED_TIME_REFERENCE_HIGH - MEASURED_TIME_REFERENCE_LOW);
					float	volume = MEASURED_TANK_VOLUME_LOW + t * (MEASURED_TANK_VOLUME_HIGH - MEASURED_TANK_VOLUME_LOW);
					return volume;
				#else		// Single measurement
					float	distance_meters = RawTime2Distance( _rawTime_microSeconds );

					float	distance_Ref = RawTime2Distance( MEASURED_TIME_REFERENCE1 );
					float	distance_4000L = distance_Ref - (TANK_HEIGHT_FULL1 - TANK_HEIGHT_REFERENCE1);	// Measured distance when the tank is full
					float	distance_0L = distance_4000L + TANK_HEIGHT_FULL1;								// Measure distance when the tank is empty
					return (distance_meters - distance_0L) * TANK_CAPACITY_LITRES / (distance_4000L - distance_0L);
				#endif
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

		[System.Diagnostics.DebuggerDisplay( "{m_label} - From {m_timeStart.ToString( \"MMMM dd HH:mm\" )} to {m_timeEnd.ToString( \"MMMM dd HH:mm\" )}" )]
		class Interval {
			public DateTime	m_timeStart;
			public DateTime	m_timeEnd;
			public string	m_label = "";

			public Interval( DateTime _timeStart, DateTime _timeEnd, string _label ) {
				m_timeStart = _timeStart;
				m_timeEnd = _timeEnd;
				m_label = _label;
			}

			public Interval( TextReader _R ) {
				string		line = _R.ReadLine();
				string[]	parts = line.Split( ';' );
				m_timeStart = DateTime.FromFileTime( long.Parse( parts[0].Trim(), System.Globalization.NumberStyles.HexNumber ) );
				m_timeEnd = DateTime.FromFileTime( long.Parse( parts[1].Trim(), System.Globalization.NumberStyles.HexNumber ) );
				if ( (m_timeEnd - m_timeStart).TotalSeconds < 10 )
					m_timeEnd = m_timeStart + TimeSpan.FromSeconds( 5*60 );	// Make the interval last at least 5 minutes!

				m_label = parts[2].Trim();
			}

			public void	Write( StreamWriter _W ) {
				_W.WriteLine( m_timeStart.ToFileTime().ToString( "X08" ) + "; " + m_timeEnd.ToFileTime().ToString( "X08" ) + "; " + m_label );
			}
		}

		[System.Diagnostics.DebuggerDisplay( "{m_entry} {FilteredVolume}" )]
		class FilterItem {
			public LogEntry	m_entry;
			public double	m_sumValues = 0.0;
			public double	m_sumWeights = 0.0;

			public float	FilteredVolume => (float) (m_sumValues / m_sumWeights);

			public FilterItem( LogEntry _entry ) {
				m_entry = _entry;
			}

			/// <summary>
			/// Adds the entry's contribution to the filtered volume
			/// </summary>
			/// <param name="_entry"></param>
			/// <returns>True if the entry is inside the filtering window</returns>
			public bool	AddContribution( LogEntry _entry ) {
				if ( _entry.IsOutOfRange || _entry.IsVolumeOutOfRange )
					return true;	// Just ignore...

				TimeSpan	deltaTimeEntry = m_entry.m_timeStamp - _entry.m_timeStamp;
				double		Dt = deltaTimeEntry.TotalMinutes / MEASUREMENT_FILTER_KERNEL_SIZE_MINUTES;	// Normalized time
				if ( Math.Abs( Dt ) >= 1.0 )
					return false;	// Outside of filter window!

				double		weight = Math.Exp( -Dt * Dt / (MEASUREMENT_FILTER_KERNEL_SIGMA * MEASUREMENT_FILTER_KERNEL_SIGMA) );
				m_sumValues += weight * _entry.Volume;
				m_sumWeights += weight;

				return true;
			}

			public void	CommitFilteredVolume() {
				m_entry.m_filteredVolume = FilteredVolume;
			}
		}

		#endregion

		#region FIELDS

		RegistryKey		m_appKey;
		string			m_applicationPath;
		FileInfo		m_fileNameLogEntries;
		FileInfo		m_fileNameIntervals;
		FileInfo		m_fileNameSessionLog;

		List<LogEntry>	m_logEntries = new List<LogEntry>();
		List<Interval>	m_intervals = new List<Interval>();
		List<string>	m_sessionLog = new List<string>();

		string			m_COMPortName = "COM11";		// Default COM port name
		SerialPort		m_COMPort = null;

		float			m_windowSize_Hours = 1.0f;		// Default window size is 1 hour
		DateTime		m_windowEndTime = DateTime.Now;	// Default window time is now
		TimeSpan		m_timeFromNow = TimeSpan.Zero;	// Default delta time between window end time and now is 0 so we track runtime data

		DateTime		m_timeReference = DateTime.Today + TimeSpan.FromDays( 10000 );	// Invalid time reference!

		// Low level warning
		float			m_lowWaterLevelWarningLimit_litres = 400;	// Default warning limit is 400 litres
		DateTime		m_lastWarningTimeStamp = DateTime.Now - TimeSpan.FromDays( 1000 );	// Last warning was a long time ago

		Pen				m_axesPen = new Pen( Color.Black );
		Brush			m_axesBrush = new SolidBrush( Color.Black );
		string			m_labelAxisX = "Time";
		string			m_labelAxisY = "Volume";
		int				m_margin = 16;
		float			m_arrowWidth = 8.0f;
		float			m_arrowLength = 10.0f;

		// Notify icon text
		string			m_notifyIconTitle = "Water Level Monitor";
		string			m_notifyIconWarning = null;
		string			m_notifyIconText = null;

		string	NotifyIconWarning {
			get => m_notifyIconWarning;
			set {
				if ( value == m_notifyIconWarning )
					return;

				m_notifyIconWarning = value;
				UpdateNotifyIcon();
			}
		}

		string	NotifyIconText {
			get => m_notifyIconText;
			set {
				if ( value == m_notifyIconText )
					return;

				m_notifyIconText = value;
				UpdateNotifyIcon();
			}
		}

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
				string	logFileName = GetRegKey( "LogFileName", "Tank3.log" );


//logFileName = "dummyTank.log";


				m_fileNameLogEntries = new FileInfo( GetRegKey( "LogFileName", Path.Combine( m_applicationPath, logFileName ) ) );
				if ( m_fileNameLogEntries.Exists ) {
					ReadLogEntries( m_fileNameLogEntries );
					FilterGraph( 0 );
				}

				// Open interval file
				m_fileNameIntervals = new FileInfo( GetRegKey( "IntervalsFileName", Path.Combine( m_applicationPath, "Intervals.log" ) ) );
				if ( m_fileNameIntervals.Exists ) {
					ReadIntervals( m_fileNameIntervals );
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

			// Always center the warning panel
			Size	marginSize = panelOutput.Size - panelWarning.Size;
			panelWarning.Location = new Point( marginSize.Width / 2, marginSize.Height / 2 );
		}

		protected override void OnLocationChanged( EventArgs e ) {
			base.OnLocationChanged( e );
			SaveLocationAndSize( this );
		}

		DateTime	m_startTime;
		DateTime	m_lastLogSaveTime = DateTime.Now;
		int			m_entriesCountOnLastLogSave = 0;

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
				WriteIntervals( m_fileNameIntervals );
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
			if ( Visible )
				Focus();
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
					}

				} catch ( Exception _e ) {
					// Notify and exit thread...
					that.BeginInvoke( (Action) (() => {
						that.NotifyIconWarning = _e.Message.Length > 0 ? _e.Message : "Failed to open COM port!";	// Update icon if port failed to open...
						MessageBoxError( "Failed to open COM port!", _e );
					}) );
					return;
				}

				// Notify port is open
				_onPortOpen?.Invoke();

				// Update icon if port is now open...
				that.NotifyIconWarning = null;

				try {
					// Listen for commands
					while ( true ) {
						COMReply	reply = null;
						try {
							string	newLine = "";
							lock ( that ) {
								if ( !m_COMPort.IsOpen )
									throw new InvalidOperationException( "Port closed while waiting for commands" );

								if ( m_COMPort.BytesToRead == 0 ) {
									System.Threading.Thread.Sleep( 10 );
									continue;
								}

								// Read a little bit of the message
								string	messageBit = m_COMPort.ReadTo( "\n" );
								newLine += messageBit;
							}

							if ( !newLine.EndsWith( "\r" ) )
								continue;	// Line is not complete yet!

							// Register a new reply
							reply = new COMReply( newLine );
							newLine = "";

						} catch ( TimeoutException ) {
							continue;	// Nothing on the line for us yet...
						}

						if ( reply == null )
							continue;	// Invalid reply... Can we even reach there?

						if ( reply.m_type == COMReply.TYPE.LOG || reply.m_type == COMReply.TYPE.DEBUG ) {
							// Just log regular strings, don't count as actual replies to the command...
							Log( "<Module Log " + reply.m_type + "> " + reply.m_string );
						} else if ( reply.m_type == COMReply.TYPE.REPLY ) {
							Log( "<Module Reply " + reply.m_type + "> " + reply.m_string );
							RegisterNewMeasurements( reply );
						} else if ( reply.m_type == COMReply.TYPE.ERROR ) {
							Log( "<Module Error " + reply.m_type + "> " + reply.m_string );
						}
					}

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

					that.NotifyIconWarning = "COM port closed!";	// Update icon if port closed...
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
				int	indexOfReplyType = m_string.IndexOf( '<' );
				if ( indexOfComma != -1 && indexOfComma < indexOfReplyType ) {
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

		/// <summary>
		/// Registers new measurements from the device
		/// </summary>
		/// <param name="_measurementsReply"></param>
		void	RegisterNewMeasurements( COMReply _measurementsReply ) {

			// Parse reply
			// The reply string should be of the form "#<measurements count> = <relative time #0>,<measurement #0>, <relative time #1>,<measurement #1>, (...)
			int			measurementsCount = 0;
			string[]	measurementParts = null;
			try {
				if ( _measurementsReply.m_string.Length == 0 )
					throw new Exception( "String is empty!" );
				if ( _measurementsReply.m_string[0] != '#' )
					throw new Exception( "String doesn't start with a '#'!" );

				int	indexOfEqual = _measurementsReply.m_string.IndexOf( "=" );
				if ( indexOfEqual == -1 )
					throw new Exception( "String doesn't contain a '=' separator!" );

				if ( !int.TryParse( _measurementsReply.m_string.Substring( 1, indexOfEqual-1 ), out measurementsCount ) )
					throw new Exception( "Failed to parse the amount of measurements in the string!" );

				measurementParts = _measurementsReply.m_string.Substring( indexOfEqual+1 ).Trim().Split( ',' );

			} catch ( Exception _e ) {
				LogError( "Failed to parse the reply string!", _e );
				return;
			}

			// Check how many measurements we were given
			if ( measurementParts.Length != 2 * measurementsCount ) {
				// Measurements count mismatch! Adjust...
				LogError( "The amount of measurements received (" + (measurementParts.Length / 2) + ") doesn't match the advertised amount by the reply! (" + measurementsCount + ")" );
				measurementsCount = measurementParts.Length >> 1;	// Can't read more than what's provided...
			}

			if ( measurementsCount == 0 ) {
				// Received nothing!
				LogError( "Received a reply without any measurement..." );
				return;
			}

			// Register as many new measurements as possible
			// The idea here is that the device always returns its last 16 measurements (ideally), it's up to us to know if we've already received them or not
			//	• By definition, the first measurement it sends is a *new measurement* so we should always accept it
			//	• But other measurements may have already been received if the monitor is actively... monitoring
			//		=> Sometimes though, after the monitor wakes up (e.g. turning the PC on in the morning), we'll have missed all the measurements issued during the night
			//			so receiving a bunch of them is actually useful to go back in time and have up to 2 hours and 40 minutes (if flow is 0) of vision of what happened to the tanks...
			//
			try {
				DateTime	now = DateTime.Now;
				LogEntry	earliestMeasurement = null;
				for ( int measurementIndex=0; measurementIndex < measurementsCount; measurementIndex++ ) {
					// Create the measurement
					uint	relativeTime_s = uint.Parse( measurementParts[2*measurementIndex+0] );
					uint	rawTime_microseconds = uint.Parse( measurementParts[2*measurementIndex+1] );

					DateTime	measurementTime = now - TimeSpan.FromSeconds( relativeTime_s );	// Next measurements are in the past

					// Check if we already have registered this time
					LogEntry	existingEntry = null;
					int	entriesCount = Math.Min( m_logEntries.Count, measurementsCount );
					for ( int entryIndex=0; entryIndex < entriesCount; entryIndex++ ) {
						existingEntry = m_logEntries[m_logEntries.Count-1-entryIndex];	// Start from the end...

						// Compare measurement time and allow a 1 minute clearance
						float	totalSeconds = (float) (existingEntry.m_timeStamp - measurementTime).TotalSeconds;
						if ( Math.Abs( totalSeconds ) < 30 )
							break;	// Found an existing measurement!

						// Doesn't match...
						existingEntry = null;
					}

					if ( measurementIndex > 0 && existingEntry != null )	// Always accept at least the first, new measurement!
						continue;	// This measurement was already registered...

					// Register a new measurement
					LogEntry	measurement = new LogEntry( rawTime_microseconds, measurementTime );
					LogEntry	insertionEntry = FindEntry( measurementTime );
					if ( insertionEntry == null ) {
						// Just add it...
						m_logEntries.Add( measurement );
					} else {
						// Insert into the list
						int	insertionIndex = m_logEntries.IndexOf( insertionEntry );
						if ( insertionEntry.m_timeStamp < measurementTime ) {
							insertionIndex++;	// Insert after
						}

						m_logEntries.Insert( insertionIndex, measurement );
					}

					// Any additionnal measurement takes us back further into the past...
					earliestMeasurement = measurement;
				}

				// Filter the new measurements
				int	startMeasurementIndex = FindMeasurementIndex( earliestMeasurement.m_timeStamp - TimeSpan.FromMinutes( 0.5f * MEASUREMENT_FILTER_KERNEL_SIZE_MINUTES ) );
				FilterGraph( startMeasurementIndex );

				// Update graph (on main thread only!)
				this.BeginInvoke( (Action) (() => {
					LogEntry	lastMeasurement = m_logEntries[m_logEntries.Count-1];

					if ( FollowRuntime ) {
						WindowEndTime = lastMeasurement.m_timeStamp + m_timeFromNow;	// Update window's end time to match this measurement and also keep its delta time from now constant
					} else {
						UpdateGraph();
					}

					// Should we trigger a warning?
					if ( !lastMeasurement.IsVolumeOutOfRange && lastMeasurement.m_filteredVolume < m_lowWaterLevelWarningLimit_litres && (lastMeasurement.m_timeStamp - m_lastWarningTimeStamp).TotalMinutes > DELAY_BETWEEN_WARNINGS_MINUTES ) {
						EnterWarningState( "Low water level in tank!" );
					}

					// Update notify icon text with current measurement
					NotifyIconText = ((int) lastMeasurement.m_filteredVolume).ToString() + " L (" + lastMeasurement.m_timeStamp.ToString( "dd MMMM HH:mm" ) + ")";

				}) );

			} catch ( Exception _e ) {
				LogError( "An error occurred while parsing the measurements!", _e );
			}
		}

/*
		/// <summary>
		/// Asks the module to perform a new volume measurement
		/// </summary>
		/// <returns></returns>
		void	PerformMeasurement( Action<string> _onError ) {
			PerformMeasurement(
				( LogEntry _newMeasurement ) => {
					// Register new measurement
					m_logEntries.Add( _newMeasurement );

					// Filter the measurement
					int	startMeasurementIndex = FindMeasurementIndex( _newMeasurement.m_timeStamp - TimeSpan.FromMinutes( 0.5f * MEASUREMENT_FILTER_KERNEL_SIZE_MINUTES ) );
					FilterGraph( startMeasurementIndex );

					// Update graph (on main thread only!)
					this.BeginInvoke( (Action) (() => {
						if ( FollowRuntime ) {
							WindowEndTime = _newMeasurement.m_timeStamp + m_timeFromNow;	// Update window's end time to match this measurement and also keep its delta time from now constant
						} else {
							UpdateGraph();
						}

						// Should we trigger a warning?
						if ( !_newMeasurement.IsVolumeOutOfRange && _newMeasurement.m_filteredVolume < m_lowWaterLevelWarningLimit_litres && (_newMeasurement.m_timeStamp - m_lastWarningTimeStamp).TotalMinutes > DELAY_BETWEEN_WARNINGS_MINUTES ) {
							EnterWarningState( "Low water level in tank!" );
						}

						// Update notify icon text with current measurement
						NotifyIconText = ((int) _newMeasurement.m_filteredVolume).ToString() + " L (" + _newMeasurement.m_timeStamp.ToString( "dd MMMM HH:mm" ) + ")";

					}) );
				},
				_onError
			);
		}

		bool	m_pipoMeasurement = false;
		void	PerformMeasurement( Action<LogEntry> _onMeasurement, Action<string> _onError ) {
			ExecuteCommand( m_pipoMeasurement ? "MEASUREPIPO" : "MEASURE", 60 * 1000,

				// Handle reply
				( string _reply ) => {
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

				// Handle error
				( string _error ) => _onError?.Invoke( _error )
			);

m_pipoMeasurement = false;
		}
*/

		/// <summary>
		/// Filters the volume measurements of the graph using a gaussian filter kernel
		/// </summary>
		/// <param name="_startIndex"></param>
		void	FilterGraph( int _startIndex ) {
			if ( m_logEntries.Count == 0 )
				return;

			List<FilterItem>	filterItems = new List<FilterItem>();	
			for ( int index=_startIndex; index < m_logEntries.Count; index++ ) {
				// Enter a new item
				LogEntry	entry = m_logEntries[index];
				filterItems.Add( new FilterItem( entry ) );

				// Add contribution of this new item to items in the list
				int	removeIndex = 0;
				for ( int itemIndex=0; itemIndex < filterItems.Count; itemIndex++ ) {
					FilterItem	filterItem = filterItems[itemIndex];
					if ( !filterItem.AddContribution( entry ) )
						removeIndex = itemIndex;
				}

				// Remove values outside of the filtering window
				while ( removeIndex > 0 ) {
					FilterItem	filterItem = filterItems[0];
					filterItem.CommitFilteredVolume();
					filterItems.RemoveAt( 0 );
					removeIndex--;
				}
			}

			// Finalize remaining items
			while ( filterItems.Count > 0 ) {
				FilterItem	filterItem = filterItems[0];
				filterItem.CommitFilteredVolume();
				filterItems.RemoveAt( 0 );
			}
		}

		#endregion

		#region Search Entries/Intervals

		/// <summary>
		/// Finds the log entry closest to the specified time
		/// </summary>
		/// <param name="_time"></param>
		int	FindMeasurementIndex( DateTime _time ) {

			// Build the current interval covering the entire set of entries
			int		intervalStart = 0;
			int		intervalSize = m_logEntries.Count;
			int		pivotIndex = m_logEntries.Count / 2;	// Pivot starts right in the middle

			float	closestDelta = float.MaxValue;
			int		closestEntryIndex = 0;
			while ( intervalSize > 0 ) {

				// Examine new entry
				LogEntry	entry = m_logEntries[pivotIndex];
				float		delta = (float) (_time - entry.m_timeStamp).TotalMinutes;
				if ( Math.Abs( delta ) < closestDelta ) {
					// Found a closer entry...
					closestDelta = Math.Abs( delta );
					if ( closestDelta < 1e-3f )
						return pivotIndex;	// Found an "exact" value
					closestEntryIndex = pivotIndex;
				}

				// Construct left & right intervals
				int		intervalStart_Left = intervalStart;
				int		intervalSize_Left = pivotIndex - intervalStart;
				int		intervalStart_Right = pivotIndex + 1;
				int		intervalSize_Right = intervalStart + intervalSize - pivotIndex - 1;

				if ( delta < 0 ) {
					// Go left
					intervalStart = intervalStart_Left;
					intervalSize = intervalSize_Left;
				} else {
					// Go right
					intervalStart = intervalStart_Right;
					intervalSize = intervalSize_Right;
				}

				// Update pivot
				pivotIndex = intervalStart + intervalSize / 2;
			}

			return closestEntryIndex;
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

		/// <summary>
		/// Finds the interval containing the client position
		/// </summary>
		/// <param name="_clientX"></param>
		/// <returns></returns>
		Interval	FindInterval( int _clientX ) {
			return FindInterval( Client2TimeStamp( _clientX ) );
		}

		/// <summary>
		/// Finds the interval containg the time stamp
		/// </summary>
		/// <param name="_timeStamp"></param>
		/// <returns></returns>
		Interval	FindInterval( DateTime _timeStamp ) {
			foreach ( Interval I in m_intervals ) {
				if ( _timeStamp >= I.m_timeStart && _timeStamp <= I.m_timeEnd )
					return I;
			}

			return null;
		}

		/// <summary>
		/// Finds the volume at the specified time stamp
		/// </summary>
		/// <param name="_timeStamp"></param>
		/// <returns></returns>
		float	FindVolume( DateTime _timeStamp ) {
			int	count = m_logEntries.Count;
			if ( count == 0 )
				return 0;

			// Handle edge cases
			if ( _timeStamp < m_logEntries[0].m_timeStamp )
				return m_logEntries[0].Volume;
			if ( _timeStamp >= m_logEntries[count-1].m_timeStamp )
				return m_logEntries[count-1].Volume;

			// Handle general case
			LogEntry	entryStart = null;
			LogEntry	entryEnd = m_logEntries[0];
			for ( int entryIndex=0; entryIndex < count; entryIndex++ ) {
				entryStart = entryEnd;
				entryEnd = m_logEntries[entryIndex];
				if ( entryEnd.m_timeStamp >= _timeStamp )
					break;	// Found the interval!
			}

			// Compute interpolated volume
			float	volumeStart = entryStart.Volume;
			float	volumeEnd = entryEnd.Volume;
			float	t = Math.Max( 0, Math.Min( 1, (float) ((_timeStamp - entryStart.m_timeStamp).TotalSeconds / Math.Max( 1, (entryEnd.m_timeStamp - entryStart.m_timeStamp).TotalSeconds )) ) );
			return volumeStart + t * (volumeEnd - volumeStart);
		}

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

		void	ReadIntervals( FileInfo _fileNameIntervals ) {
			if ( !_fileNameIntervals.Exists )
				return;

			m_intervals.Clear();
			using ( StreamReader R = _fileNameIntervals.OpenText() ) {
				while ( !R.EndOfStream ) {
					Interval	interval = new Interval( R );
					m_intervals.Add( interval );
				}
			}
		}

		void	WriteIntervals( FileInfo _fileNameIntervals ) {
			if ( _fileNameIntervals.Exists ) {
				// Backup any existing file first...
				string	backupFileName = _fileNameIntervals.FullName + ".bak";
				if ( File.Exists( backupFileName ) ) {
					File.Delete( backupFileName );
				}
				File.Copy( _fileNameIntervals.FullName, backupFileName );
			}

			using ( StreamWriter W = _fileNameIntervals.CreateText() ) {
				foreach ( Interval interval in m_intervals ) {
					interval.Write( W );
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

		/// <summary>
		/// Updates the notify icon text and icon
		/// </summary>
		void	UpdateNotifyIcon() {
			if ( this.IsDisposed )
				return;

			string	text = m_notifyIconTitle;
			bool	isInWarningOrErrorState = false;
			if ( m_notifyIconWarning != null ) {
				text += "\r\n" + m_notifyIconWarning;
				isInWarningOrErrorState  = true;
			}
			if ( m_notifyIconText != null ) {
				text += "\r\n" + m_notifyIconText;
			}

			if ( text.Length > 63 )
				text = text.Substring( 0, 63 );

			notifyIcon.Text = text;

			bool	isInError = isInWarningOrErrorState	// Did an error occurred?
							  | panelWarning.Visible;	// Did a critical warning occurred?

			notifyIcon.Icon = isInError ? WaterTankMonitor.Properties.Resources.IconWarning : WaterTankMonitor.Properties.Resources.Icon; 
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
		
		// Interval manipulation
		Interval		m_mouseDownInterval = null;
		DateTime		m_mouseDownIntervalStart;
		DateTime		m_mouseDownIntervalEnd;
		INTERVAL_MANIPULATION_TYPE	m_mouseDownIntervalManipulationType = INTERVAL_MANIPULATION_TYPE.NONE;

		private void PanelOutput_MouseMove( object sender, MouseEventArgs e ) {
			if ( m_mouseButtonsDown == MouseButtons.None ) {
				UpdateGraph();	// Display value at mouse position
				return;
			}

			if ( m_mouseButtonsDown != MouseButtons.Left )
				return;

			if ( m_mouseDownInterval != null && m_mouseDownIntervalManipulationType != INTERVAL_MANIPULATION_TYPE.NONE ) {
				// Manipulate interval
				DateTime	mouseDownTimeStamp = Client2TimeStamp( m_mouseDownLocation.X );
				DateTime	currentTimeStamp = Client2TimeStamp( e.X );
				TimeSpan	deltaTime = currentTimeStamp - mouseDownTimeStamp;

				switch ( m_mouseDownIntervalManipulationType ) {
					case INTERVAL_MANIPULATION_TYPE.BOTH:
						m_mouseDownInterval.m_timeStart = m_mouseDownIntervalStart + deltaTime;
						m_mouseDownInterval.m_timeEnd = m_mouseDownIntervalEnd + deltaTime;
						break;

					case INTERVAL_MANIPULATION_TYPE.LEFT: {
						DateTime	newTime = m_mouseDownIntervalStart + deltaTime;
						if ( (m_mouseDownInterval.m_timeEnd - newTime).TotalMinutes < 5 )
							newTime = m_mouseDownInterval.m_timeEnd - TimeSpan.FromMinutes( 5 );	// Prevent empty interval!
						m_mouseDownInterval.m_timeStart = newTime;
						break;
					}

					case INTERVAL_MANIPULATION_TYPE.RIGHT: {
						DateTime	newTime = m_mouseDownIntervalEnd + deltaTime;
						if ( (newTime - m_mouseDownInterval.m_timeStart).TotalMinutes < 5 )
							newTime = m_mouseDownInterval.m_timeStart + TimeSpan.FromMinutes( 5 );	// Prevent empty interval!
						m_mouseDownInterval.m_timeEnd = newTime;
						break;
					}
				}

				UpdateGraph();

			} else {
				// Scroll window
				float	windowSize_HoursPerPixel = WindowSize_Hours / (panelOutput.Width - 2 * m_margin);
				float	hours = windowSize_HoursPerPixel * (m_mouseDownLocation.X - e.X);

				DateTime	newEndTime = m_mouseDown_WindowEndTime + TimeSpan.FromHours( hours );
				if ( WindowStartTime > DateTime.Now ) {
					newEndTime = DateTime.Now + TimeSpan.FromHours( WindowSize_Hours );	// Make sure window can't go into the future
				}
				WindowEndTime = newEndTime;
			}
		}

		private void PanelOutput_MouseDown( object sender, MouseEventArgs e ) {
			panelOutput.Capture = true;

			m_mouseDownLocation = e.Location;
			m_mouseButtonsDown |= e.Button;
			m_mouseDown_WindowEndTime = WindowEndTime;

			// Validate hovered interval and manipulation type
			m_mouseDownInterval = m_mouseHoverInterval;
			if ( m_mouseHoverInterval != null ) {
				m_mouseDownIntervalStart = m_mouseHoverInterval.m_timeStart;
				m_mouseDownIntervalEnd = m_mouseHoverInterval.m_timeEnd;
			}
			m_mouseDownIntervalManipulationType = m_mouseHoverIntervalManipulationType;
		}

		private void PanelOutput_MouseUp( object sender, MouseEventArgs e ) {
			m_mouseDownLocation = e.Location;
			m_mouseButtonsDown &= ~e.Button;
			m_mouseDown_WindowEndTime = WindowEndTime;

			panelOutput.Capture = m_mouseButtonsDown != MouseButtons.None;
		}

		private void PanelOutput_MouseWheel( object sender, MouseEventArgs e ) {
			float	factor = 1.1f;

			DateTime	centerTime = Client2TimeStamp( e.X );
			WindowSize_Hours *= e.Delta < 0.0f ? factor : 1.0f / factor;

			DateTime	newCenterTime = Client2TimeStamp( e.X );
//			WindowEndTime = centerTime + TimeSpan.FromHours( WindowSize_Hours * (1.0f - (float) e.X / (panelOutput.Width - 2 * m_margin)) );
			WindowEndTime += centerTime - newCenterTime;
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

		Brush		m_brushInterval = new System.Drawing.Drawing2D.HatchBrush( System.Drawing.Drawing2D.HatchStyle.Percent50, Color.Plum, Color.Transparent );
		Pen			m_penInterval = new Pen( Color.Plum, 2 );
//		SolidBrush	m_brushIntervalString = new SolidBrush( Color.Plum );	// Plum = 0xFFDDA0DD
		SolidBrush	m_brushIntervalString = new SolidBrush( Color.FromArgb( unchecked( (int) 0xFF6E506E ) ) );	// Dark Plum
//		SolidBrush	m_brushIntervalString = new SolidBrush( Color.FromArgb( unchecked( (int) 0xFF372837 ) ) );	// Dark Plum

		enum MARKER_TYPE {
			MONTH,
			WEEK,
			DAY,
			HOUR,
			QUARTER_HOUR,
		}

		string[]	m_monthNames = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

		enum INTERVAL_MANIPULATION_TYPE {
			NONE,
			LEFT,
			RIGHT,
			BOTH
		}
		Interval	m_mouseHoverInterval = null;
		INTERVAL_MANIPULATION_TYPE	m_mouseHoverIntervalManipulationType = INTERVAL_MANIPULATION_TYPE.NONE;

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

			float	mouseX = panelOutput.PointToClient( Control.MousePosition ).X;

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
					case MARKER_TYPE.MONTH: markerText = m_monthNames[(((markerReferenceTime.Month - 1 + markerIndex)%12)+12)%12]; break;
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

				// Bare minimum line (right above where the water pipe is connected)
			float	Ymin = Level2Client( TANK_MINIMAL_CAPACITY_LITRES );

			Pen		penMinLevelLine = Pens.DarkSlateGray;
			Brush	brushMinLevelLine = Brushes.DarkSlateGray;
			G.DrawLine( penMinLevelLine, m_margin, Ymin, W - m_margin, Ymin );
			G.DrawString( TANK_MINIMAL_CAPACITY_LITRES.ToString() + "L", Font, brushMinLevelLine, W - m_margin - 30, Ymin + 0 );

			float	Y_warning = Level2Client( m_lowWaterLevelWarningLimit_litres );	// Low limit line
			G.DrawLine( m_penLowLevelLimit, m_margin, Y_warning, W - m_margin, Y_warning );
			G.DrawString( ((int) m_lowWaterLevelWarningLimit_litres).ToString(), Font, m_brushLowLevelLimit, W - m_margin - 30, Y_warning );

			// =====================================
			// Paint intervals
			m_mouseHoverInterval = null;
			m_mouseHoverIntervalManipulationType = INTERVAL_MANIPULATION_TYPE.NONE;

			foreach ( Interval I in m_intervals ) {
				float	startX = TimeStamp2Client( I.m_timeStart );
				float	endX = TimeStamp2Client( I.m_timeEnd );
				if ( startX > W || endX < 0 )
					continue;	// Outside the window

				if ( mouseX >= startX && mouseX < endX ) {
					// Select this interval
					m_mouseHoverInterval = I;
					m_mouseHoverIntervalManipulationType = INTERVAL_MANIPULATION_TYPE.BOTH;
					if ( mouseX < startX + 5 )
						m_mouseHoverIntervalManipulationType = INTERVAL_MANIPULATION_TYPE.LEFT;
					if ( mouseX > endX - 5 )
						m_mouseHoverIntervalManipulationType = INTERVAL_MANIPULATION_TYPE.RIGHT;
				}

				G.FillRectangle( m_brushInterval, startX, 0, endX - startX, H );
				G.DrawLine( m_penInterval, startX, 0, startX, H );
				G.DrawLine( m_penInterval, endX, 0, endX, H );
				if ( I.m_label.Length != 0 ) {
					SizeF	labelSize = G.MeasureString( I.m_label, Font );
					G.DrawString( I.m_label, Font, m_brushIntervalString, (startX + endX - labelSize.Width) / 2, 0 );
				}
			}

			// Change mouse cursor
			if ( m_mouseButtonsDown == MouseButtons.None ) {
				switch ( m_mouseHoverIntervalManipulationType ) {
					case INTERVAL_MANIPULATION_TYPE.NONE: Cursor = Cursors.Default; break;
					case INTERVAL_MANIPULATION_TYPE.BOTH: Cursor = Cursors.Hand; break;
					case INTERVAL_MANIPULATION_TYPE.LEFT: Cursor = Cursors.SizeWE; break;
					case INTERVAL_MANIPULATION_TYPE.RIGHT: Cursor = Cursors.SizeWE; break;
				}
			}

			// =====================================
			// Paint measurements
			LogEntry	mouseEntry = null;
//			int			mouseEntyIndex = -1;
			float		mouseEntryX = 0, mouseEntryY = 0;
			float		closestMouseEntryDistance = float.MaxValue;

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
//						mouseEntyIndex = entryIndex;
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
// 			LogEntry	prevMouseEntry = mouseEntyIndex > 0 ? m_logEntries[mouseEntyIndex-1] : mouseEntry;
// 			LogEntry	nextMouseEntry = mouseEntyIndex < m_logEntries.Count-1 ? m_logEntries[mouseEntyIndex+1] : mouseEntry;
// 			float		deltaTime = (float) (nextMouseEntry.m_timeStamp - prevMouseEntry.m_timeStamp).TotalHours;
// //			float		deltaVolume = nextMouseEntry.Volume - prevMouseEntry.Volume;
// 			float		deltaVolume = nextMouseEntry.m_filteredVolume - prevMouseEntry.m_filteredVolume;

			int			startMeasureIndex = FindMeasurementIndex( mouseEntry.m_timeStamp - TimeSpan.FromMinutes( 10 ) );
			int			endMeasureIndex = FindMeasurementIndex( mouseEntry.m_timeStamp + TimeSpan.FromMinutes( 10 ) );
			LogEntry	previousEntry = null;
			float		deltaTime_hours = 0.0f;
			float		deltaVolume = 0.0f;
			for ( int index=startMeasureIndex; index <= endMeasureIndex; index++ ) {
				LogEntry	currentEntry = m_logEntries[index];
				if ( previousEntry != null ) {
					deltaTime_hours += (float) (currentEntry.m_timeStamp - previousEntry.m_timeStamp).TotalHours;
					deltaVolume += currentEntry.m_filteredVolume - previousEntry.m_filteredVolume;
				}
				previousEntry = currentEntry;
			}

			// Draw hovered log entry
			G.DrawLine( Pens.Red, mouseEntryX, 0, mouseEntryX, Height );
			G.DrawEllipse( Pens.Red, mouseEntryX-3, mouseEntryY-3, 7, 7 );

			// Draw text rectangle
			string	textEntry = null;
			if ( !mouseEntry.IsOutOfRange ) {
				textEntry = mouseEntry.m_timeStamp.ToString( "dd MMMM HH:mm" ) + "\n"
						  + "Volume = " + ((int) mouseEntry.m_filteredVolume) + " L\n";

				// Daily consumption from midnight
				DateTime	mouseEntryMidnight = new DateTime( mouseEntry.m_timeStamp.Year, mouseEntry.m_timeStamp.Month, mouseEntry.m_timeStamp.Day );
				float		midnightVolume = FindVolume( mouseEntryMidnight );
				if ( previousEntry != null ) {
					float	todayConsumption = previousEntry.Volume - midnightVolume;
					float	todayHours = (float) (previousEntry.m_timeStamp - mouseEntryMidnight).TotalHours;
					if ( todayHours > 1e-3f ) {
						textEntry += (todayConsumption > 0 ? "♥ " : "") + "Today's consumption: " + (todayConsumption > 0 ? "+" : "") + ((int) todayConsumption).ToString() + " L (" + ((int) (todayConsumption / todayHours)) + " L/h)\n";
					} else {
						textEntry += "(Daily consumption non computable)\n";
					}
				} else {
					textEntry += "(Daily consumption non computable)\n";
				}

				// Show consumption within the hovered interval
				if ( m_mouseHoverInterval != null ) {
					float	volumeStart = FindVolume( m_mouseHoverInterval.m_timeStart );
					float	volumeEnd = FindVolume( m_mouseHoverInterval.m_timeEnd );
					float	intervalVolume = volumeEnd - volumeStart;
					TimeSpan	intervalDuration = m_mouseHoverInterval.m_timeEnd - m_mouseHoverInterval.m_timeStart;
					float	intervalDuration_hours = (float) intervalDuration.TotalHours;

					// Show sensible duration information
					string	strIntervalDuration =  intervalDuration_hours > 24 ? Math.Floor( intervalDuration_hours / 24 ) + " days " : "";
					intervalDuration_hours -= 24 * (float) Math.Floor( intervalDuration_hours / 24 );
							strIntervalDuration += intervalDuration_hours > 1 ? Math.Floor( intervalDuration_hours ) + " hours " : "";
					intervalDuration_hours -= (float) Math.Floor( intervalDuration_hours );
							strIntervalDuration += 60.0f * intervalDuration_hours > 1 ? Math.Floor( 60.0f * intervalDuration_hours ) + " minutes " : "";
//					intervalDuration_hours -= (float) Math.Floor( 60.0f * intervalDuration_hours ) / 60.0f;

					textEntry += "\n"
							   + "Interval " + m_mouseHoverInterval.m_label + ":\n"
							   + "  • Start " + m_mouseHoverInterval.m_timeStart.ToString( "dd MMMM HH:mm" ) + "\n"
							   + "  • Duration " + strIntervalDuration + "\n"
							   + (intervalVolume >= 0 ? "  ♥ Collected +" : "  • Consumed ") + ((int) intervalVolume) + "L (" + ((int) (intervalVolume / Math.Max( 1e-3, intervalDuration.TotalHours ))) + " L/h)\n";
				}

				// Show consumption since time reference
				if ( IsTimeReferenceValid ) {
					float		referenceVolume = FindVolume( m_timeReference );
//					float		totalVolume = mouseEntry.Volume - referenceEntry.Volume;
					float		totalVolume = mouseEntry.m_filteredVolume - referenceVolume;
					if ( referenceVolume > 0 ) {
						textEntry += "\n"
								  + (totalVolume < 0 ? "Consumed " : "♥ Collected +") + (int) totalVolume + " L since "//\n"
								  + "reference time " + m_timeReference.ToString( "dd MMMM HH:mm" ) + "\n"
								  + "Estimate: " + (int) (totalVolume / (mouseEntry.m_timeStamp - m_timeReference).TotalDays) + " L per day\n";
					}
				}

				// Advanced info (raw volume & immediate consumption)
				textEntry += "\n"
						   + "Raw volume = " + ((int) mouseEntry.Volume) + " L (" + mouseEntry.m_rawTime_microSeconds + " µs)\n";
				if ( deltaTime_hours > 1e-3f ) {
					textEntry += "Consumption = " + (int) (deltaVolume / deltaTime_hours) + " L/h\n";
				} else {
					textEntry += "(Immediate consumption non computable)\n";
				}

			} else {
				textEntry = mouseEntry.m_timeStamp.ToString( "dd MMMM HH:mm" ) + "\n"
						  + "► Out Of Range! ◄";
			}

			SizeF	textBoxSize = G.MeasureString( textEntry, Font );
			float	textBoxMargin = 4;
			float	rectW = textBoxMargin + textBoxSize.Width + textBoxMargin;
			float	rectH = textBoxMargin + textBoxSize.Height + textBoxMargin;
			float	rectX =  mouseEntryX + 50;
			float	rectY =  mouseEntryY - 0.5f * rectH;

			if ( rectX + rectW > panelOutput.Width ) {
				rectX = mouseEntryX - 50 - rectW;
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
			return Level2Client( _entry.m_filteredVolume );
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

		#endregion

		#region Warning Panel Handling

		/// <summary>
		/// Enters the warning state
		/// </summary>
		/// <param name="_warningMessage"></param>
		void	EnterWarningState( string _warningMessage ) {
			panelWarning.Message = _warningMessage;
			panelWarning.Visible = true;

			// Serious enough to warrant a balloon warning!
 			notifyIcon.ShowBalloonTip( 5000, "Water Tank Monitor", _warningMessage, ToolTipIcon.Error );

			m_lastWarningTimeStamp = DateTime.Now;
		}

		private void panelWarning_VisibleChanged( object sender, EventArgs e ) {
			NotifyIconWarning = panelWarning.Visible ? panelWarning.Message : null;
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

					BeginInvoke( (Action) (() => { 
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

		void	UpdateIntervals() {
			WriteIntervals( m_fileNameIntervals );
			UpdateGraph();
		}

		Interval	m_contextMenuInterval = null;
		private void contextMenuStripPanel_Opening( object sender, CancelEventArgs e ) {
			m_mouseDownLocation = panelOutput.PointToClient( Control.MousePosition );
			m_contextMenuInterval = FindInterval( m_mouseDownLocation.X );

			// Enable interval items only if we're hovering an actual interval
			setIntervalStartTimeToolStripMenuItem.Enabled = m_contextMenuInterval != null;
			setIntervalEndTimeToolStripMenuItem.Enabled = m_contextMenuInterval != null;
			renameIntervalToolStripMenuItem.Enabled = m_contextMenuInterval != null;
			deleteIntervalToolStripMenuItem.Enabled = m_contextMenuInterval != null;
		}

		private void createIntervalToolStripMenuItem_Click( object sender, EventArgs e ) {
			WaterTankMonitor.FormInterval	F = new WaterTankMonitor.FormInterval();
			if ( F.ShowDialog( this ) != DialogResult.OK )
				return;

			DateTime	startTime = Client2TimeStamp( m_mouseDownLocation.X );
			TimeSpan	defaultInterval = TimeSpan.FromHours( WindowSize_Hours / 20.0f );

			Interval	I = new Interval( startTime, startTime + defaultInterval, F.Label );
			m_intervals.Add( I );
			UpdateIntervals();
		}

		private void setIntervalStartTimeToolStripMenuItem_Click( object sender, EventArgs e ) {
			m_contextMenuInterval.m_timeStart = Client2TimeStamp( m_mouseDownLocation.X );
			UpdateIntervals();
		}

		private void setIntervalEndTimeToolStripMenuItem_Click( object sender, EventArgs e ) {
			m_contextMenuInterval.m_timeEnd = Client2TimeStamp( m_mouseDownLocation.X );
			UpdateIntervals();
		}

		private void renameIntervalToolStripMenuItem_Click( object sender, EventArgs e ) {
			WaterTankMonitor.FormInterval	F = new WaterTankMonitor.FormInterval();
			F.Label = m_contextMenuInterval.m_label;
			if ( F.ShowDialog( this ) != DialogResult.OK )
				return;

			m_contextMenuInterval.m_label = F.Label;
			UpdateIntervals();
		}

		private void deleteIntervalToolStripMenuItem_Click( object sender, EventArgs e ) {
			if ( MessageBox( "Are you sure you want to delete the interval \"" + m_contextMenuInterval.m_label + "\"?", MessageBoxIcon.Question, MessageBoxButtons.YesNo ) != DialogResult.Yes )
				return;

			m_intervals.Remove( m_contextMenuInterval );
			UpdateIntervals();
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

		private void saveLogFileNowToolStripMenuItem_Click( object sender, EventArgs e ) {
			try {
				WriteLogEntries( m_fileNameLogEntries );
			} catch ( Exception _e ) {
				LogError( "Failed to save log!", _e );
			}

			try {
				WriteIntervals( m_fileNameIntervals );
			} catch ( Exception _e ) {
				LogError( "Failed to save intervals!", _e );
			}

			try {
				WriteSessionLog( m_fileNameSessionLog );
			} catch ( Exception _e ) {
				MessageBoxError( "Failed to save session log!", _e );
			}
		}

		private void exitToolStripMenuItem_Click( object sender, EventArgs e ) {
			if ( (DateTime.Now - m_startTime).TotalMinutes < 5	// Don't piss user about closing if we just opened!
				|| MessageBox( "Are you sure you want to exit the application?", MessageBoxIcon.Question, MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2 ) == DialogResult.Yes ) {
				m_exitApplication = true;
				Close();
			}
		}

		#endregion

		private void button1_Click( object sender, EventArgs e ) {
// 			ExecuteCommand( "GETBUFFERSIZE", 10 * 1000, ( string _reply ) => {
// 				System.Diagnostics.Debug.WriteLine( _reply );
// 			}, ( string _error ) => {
// 				System.Diagnostics.Debug.WriteLine( "Error: " + _error );
// 			} );

// 			// Simulate a dummy measurement
// 			m_pipoMeasurement = true;
// 			PerformMeasurement( null );

			EnterWarningState( "Oh god! A horrible thing happened!\r\nDO SOMETHING!!!!" );
		}

		private void notifyIcon_MouseClick( object sender, MouseEventArgs e ) {
			if ( e.Button != MouseButtons.Left )
				return;

			if ( Visible ) {
				Hide();
			} else {
				// Source: ? But Activate() seems to do the trick...
				Show();
				WindowState = FormWindowState.Normal;
				Activate();
			}
		}

		#endregion
	}
}
