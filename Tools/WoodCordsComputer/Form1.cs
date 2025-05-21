using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WoodCordsComputer {
	public partial class Form1 : Form {

		[System.Diagnostics.DebuggerDisplay( "#{m_index,d} depth={m_depth} text={m_text}" )]
		class	LogLine {
			int				m_index;		// Log line index
			public string	m_text = null;	// The text containing polygon coordinates
			public float	m_depth = 0;	// The depth of the line (in meters)

			string			m_parsingError = null;
			public string	ParsingError => m_parsingError;

			public LogLine( int _index ) { m_index = _index; }
			public void	ReadFromRegistry() {
				m_text = GetRegKey( "textBoxCoordinates.Text.line" + m_index, "" );
				m_depth = GetRegKeyFloat( "floatTrackbarControlLogDepth.Value.line" + m_index, 1.0f );
			}

			public float	ComputeVolume( float _unitX, float _unitY, float _coverage, out float _area ) {
				float	volume = 0.0f;
				_area = 0;

				try {
					// Decode polygon points
					List<Tuple<float,float>>	points = new List<Tuple<float, float>>();

					string[]	lines = m_text.Split( '\n' );
					foreach ( string line in lines ) {
						if ( line.Trim().Length == 0 ) {
							continue;
						}

						string[]	strPoints = line.Split( ';' );
						foreach ( string strPoint in strPoints ) {
							string[]	strCoordinates = strPoint.Split( ',' );
							if ( strCoordinates.Length != 2 )
								throw new Exception( "Point #" + (points.Count+1).ToString() + " is badly formed and doesn't contain 2 components!" );

							// Read coordinates
							float	X, Y;
							if ( !float.TryParse( strCoordinates[0], out X ) )
								throw new Exception( "Point #" + (points.Count+1).ToString() + " X component is not a float!" );
							if ( !float.TryParse( strCoordinates[1], out Y ) )
								throw new Exception( "Point #" + (points.Count+1).ToString() + " Y component is not a float!" );

							// Store in meters
							X *= _unitX;
							Y *= _unitY;
							points.Add( new Tuple<float, float>( X, Y ) );
						}
					}

					if ( points.Count < 2 ) {
						m_parsingError = "Empty";
						return 0.0f;
					}

					// Process polygon area
					float	area = 0.0f;
					Tuple<float,float>	previous = points[0];
					for ( int pointIndex=1; pointIndex < points.Count; pointIndex++ ) {
						Tuple<float,float>	current = points[pointIndex];

						// Sum area of a trapezoid
						float	Dx = current.Item1 - previous.Item1;
						area += Dx * 0.5f * (current.Item2 + previous.Item2);

						previous = current;
					}

					// Report area without coverage %
					_area = area;

					// Apply coverage (because there are gaps between logs)
					area *= _coverage;

					// Convert into volume
					volume = area * m_depth;

				} catch ( Exception _e ) {
					m_parsingError = "An exception occurred: " + _e.Message;
				}

				return volume;
			}
		}

		private static Microsoft.Win32.RegistryKey	ms_appKey;

		int			m_lineIndex = -1;
		LogLine[]	m_logLines = new LogLine[4];

		int		LineIndex {
			get => m_lineIndex;
			set {
				value = Math.Max( 0, Math.Min( 3, value ) );
				if ( value == m_lineIndex )
					return;	// No change

				m_lineIndex = value;

				// Update UI
				textBoxCoordinates.Text = m_logLines[m_lineIndex].m_text;
				floatTrackbarControlLogDepth.Value = m_logLines[m_lineIndex].m_depth;
			}
		}

		public Form1() {
			// 2024 - Mild winter (2 weeks of snow + freeze / drain)
// 			ms_appKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( @"Software\JollyFiche\WoodCordsComputer" );	//		  1.830 m³ (polygon area = 6.423 m²)
// 																														//		+ 1.436 m³ (polygon area = 5.037 m²)
// 																														//		+ 2.734 m³ (polygon area = 9.114 m²)
// 																														//		+ 2.938 m³ (polygon area = 9.794 m²)
// 																														// Total = 8.94 m³ (2.47 cords)

			// 2025 - 
			//	1 gros douglas fir ~= 22% de notre consommation de 2024
			//	==> 4.42 douglas firs pour couvrir nos besoins!
			//
			ms_appKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( @"Software\JollyFiche\WoodCordsComputer_2025" );	//		
																															//

			// Restore line info
			for ( int lineIndex=0; lineIndex < 4; lineIndex++ ) {
				m_logLines[lineIndex] = new LogLine( lineIndex );
				m_logLines[lineIndex].ReadFromRegistry();
			}

			InitializeComponent();

			LineIndex = GetRegKeyInt( "LineIndex", 0 );
			Recompute();
		}

		void	Recompute() {
			float	unitX = floatTrackbarControlUnitX.Value;
			float	unitY = floatTrackbarControlUnitY.Value;
			float	coverage = 0.01f * floatTrackbarControlSurfaceCoverage.Value;

			string	text = "";
			float	totalVolume = 0;
			for ( int lineIndex=0; lineIndex < 4; lineIndex++ ) {
				LogLine	line = m_logLines[lineIndex];
				float	area;
				float	volume = line.ComputeVolume( unitX, unitY, coverage, out area );
				string	lineText = volume > 0 ? volume.ToString( "G4" ) + " m³		(polygon area = " + area.ToString( "G4" ) + " m²)" : line.ParsingError;
				text += "line #" + (1+lineIndex) + " = " + lineText + "\r\n";
				totalVolume += volume;
			}
			text += "\r\n"
				 +	"------------------------------------------------\r\n"
				 +	"Total volume = " + totalVolume.ToString( "G3" ) + " m³\r\n"
				 +	"Total cords  = " + (totalVolume / 3.624556f).ToString( "G3" ) + " cords\r\n";

			textBoxResults.Text = text;
		}

		private void radioButton_CheckedChanged( object sender, EventArgs e ) {
			int	radioIndex = 1;
			if ( sender == radioButton2 )				radioIndex = 2;
			else if ( sender == radioButton3 )			radioIndex = 3;
			else if ( sender == radioButton4 )			radioIndex = 4;
			LineIndex = radioIndex - 1;
		}

		private void floatTrackbarControlUnitX_ValueChanged( UIUtility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			Recompute();
		}

		private void floatTrackbarControlUnitY_ValueChanged( UIUtility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			Recompute();
		}

		private void floatTrackbarControlSurfaceCoverage_ValueChanged( UIUtility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			Recompute();
		}

		private void floatTrackbarControlLogDepth_ValueChanged( UIUtility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			m_logLines[m_lineIndex].m_depth = _Sender.Value;
			SetRegKeyFloat( "floatTrackbarControlLogDepth.Value.line" + m_lineIndex, m_logLines[m_lineIndex].m_depth );
			Recompute();
		}

		private void textBoxCoordinates_TextChanged( object sender, EventArgs e ) {
			m_logLines[m_lineIndex].m_text = textBoxCoordinates.Text;
			SetRegKey( "textBoxCoordinates.Text.line" + m_lineIndex, m_logLines[m_lineIndex].m_text );
			Recompute();
		}

		#region Registry

		public static string	GetRegKey( string _key, string _default, bool _setIfDoesntExist=false ) {
			string	result = ms_appKey.GetValue( _key ) as string;
			if ( result != null )
				return result;
			if ( _setIfDoesntExist )
				SetRegKey( _key, _default );
			return _default;
		}
		public static void	SetRegKey( string _key, string _value ) {
			ms_appKey.SetValue( _key, _value );
		}

		public static bool	GetRegKeyBool( string _key, bool _default ) {
			int	value = GetRegKeyInt( _key, _default ? 1 : 0 );
			return value != 0;
		}

		public static void	SetRegKeyBool( string _key, bool _value ) {
			SetRegKeyInt( _key, _value ? 1 : 0 );
		}

		public static float	GetRegKeyFloat( string _key, float _default ) {
			string	value = GetRegKey( _key, _default.ToString() );
			float	result;
			float.TryParse( value, out result );
			return result;
		}

		public static void	SetRegKeyFloat( string _key, float _value ) {
			SetRegKey( _key, _value.ToString() );
		}

		public static int	GetRegKeyInt( string _key, int _default ) {
			string	value = GetRegKey( _key, _default.ToString() );
			int		result;
			int.TryParse( value, out result );
			return result;
		}

		public static void	SetRegKeyInt( string _key, int _value ) {
			SetRegKey( _key, _value.ToString() );
		}

		#endregion
	}
}
