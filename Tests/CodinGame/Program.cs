//#define WIRE_CHALLENGE
//#define NETWORKING_CHALLENGE
//#define WAR_CHALLENGE
//#define DIVISORS_CHALLENGE
#define MARS_LANDER_CHALLENGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using SharpMath;

namespace CodinGame {
	static class Program {

		#region WIRE CHALLENGE

#if WIRE_CHALLENGE
/*
static string	ms_inputText = 
"9 11       	\r\n" +
"    [@]    	\r\n" +
"     |     	\r\n" +
"   [ & ]   	\r\n" +
"    | |    	\r\n" +
"  +-+ +-+  	\r\n" +
"  |     |  	\r\n" +
"[ | ] [ | ]	\r\n" +
" | |   | | 	\r\n" +
" 0 0   0 0 	\r\n";
//*/
/*
static string	ms_inputText = 
"11 11		\r\n" +
"    [@]    \r\n" +
"     |     \r\n" +
"   [ & ]   \r\n" +
"    | |    \r\n" +
"  +-+ +-+  \r\n" +
"  |     |  \r\n" +
" [~]   [~] \r\n" +
"  |     |  \r\n" +
"[ | ] [ & ]\r\n" +
" | |   | | \r\n" +
" 1 1   0 1 \r\n";
//*/
/*
static string	ms_inputText = 
"19 21				  \r\n" +
"         [@]         \r\n" +
"          |          \r\n" +
"         [~]         \r\n" +
"          |          \r\n" +
"        [ | ]        \r\n" +
"         | |         \r\n" +
"      +--+ +--+      \r\n" +
"      |       |      \r\n" +
"    [ | ]   [ | ]    \r\n" +
"     | |     | |     \r\n" +
"  +--+ +--+--+ +--+  \r\n" +
"  |       |       |  \r\n" +
"[ | ]   [ & ]   [ & ]\r\n" +
" | |     | |     | | \r\n" +
" | +--+--+ +--+--+ | \r\n" +
" |    |       |    | \r\n" +
"[~] [ & ]   [ | ] [~]\r\n" +
" |   | |     | |   | \r\n" +
" 0   1 1     1 0   0 \r\n";
//*/
/*
static string	ms_inputText = 
"15 17\r\n" +
"       [@]       \r\n" +
"        |        \r\n" +
"      [ & ]      \r\n" +
"       | |       \r\n" +
"     +-+ +-+     \r\n" +
"     |     |     \r\n" +
"   [ | ] [ & ]   \r\n" +
"    | |   | |    \r\n" +
"  +-+ ++ ++ +-+  \r\n" +
"  |    | |    |  \r\n" +
"  |   [ < ]   |  \r\n" +
"  |     |     |  \r\n" +
"[ + ] [ & ] [ + ]\r\n" +
" | |   | |   | | \r\n" +
" 0 0   0 0   1 1 \r\n";
//*/
/*
static string	ms_inputText = 
"30 25\r\n" +
"       [    @    ]       \r\n" +
"        |   |   |        \r\n" +
"    +---+   |   +---+    \r\n" +
"    |       |       |    \r\n" +
"  [ ^ ]   [ - ]   [ ^ ]  \r\n" +
"   | |     | |     | |   \r\n" +
"  ++ |     | ++ +--+ |   \r\n" +
"  |  |     |  | |    |   \r\n" +
"  | [~]   [~] | |   [~]  \r\n" +
"  |  |     |  | |    |   \r\n" +
"  |  +-+ +-+  | |    |   \r\n" +
"  |    | |    | |    |   \r\n" +
"  |   [ < ]  [ < ]   |   \r\n" +
"  |     |      |     |   \r\n" +
"[ & ] [ & ]  [ | ] [ & ] \r\n" +
" | |   | |    | |   | |  \r\n" +
" | |   | ++   | |   | |  \r\n" +
" | |   |  |   | |   | |  \r\n" +
" | |  [~] |   | ++ ++ |  \r\n" +
" | +-+ |  |   |  | |  |  \r\n" +
" |   | |  |   |  | |  |  \r\n" +
"[~] [ < ] +-+-+ [ < ] |  \r\n" +
" |    |     |     |   |  \r\n" +
" |  [ & ] [ | ] [ ^ ] |  \r\n" +
" |   | |   | |   | |  |  \r\n" +
" +-+-+ |  ++ ++ ++ ++-+  \r\n" +
"   |   |  |   | |   |    \r\n" +
" [ & ] |  |   | | [ & ]  \r\n" +
"  | |  |  |   | |  | |   \r\n" +
"  0 0  0  0   0 0  0 0   \r\n";
//*/
//*
static string	ms_inputText = 
"36 23\r\n"+
"       [   @   ]       \r\n" +
"        | | | |        \r\n" +
"  +-----+ | | +-----+  \r\n" +
"  |       | |       |  \r\n" +
"  |     +-+ +-+     |  \r\n" +
"  |     |     |     |  \r\n" +
"  |     |    [~]   [~] \r\n" +
"  |     |     |     |  \r\n" +
"[ + ] [ - ] [ ^ ] [ = ]\r\n" +
" | |   | |   | |   | | \r\n" +
" | |  ++ ++  | ++ ++ | \r\n" +
" | |  |   | [~] | |  | \r\n" +
" | | [~]  |  |  | |  | \r\n" +
" | ++ |   | ++  | |  | \r\n" +
" |  | |   | |   | |  | \r\n" +
" | [ < ] [ < ] [ < ] | \r\n" +
" |   |     |     |   | \r\n" +
" |  [~]    |    [~]  | \r\n" +
" |   |     |     |   | \r\n" +
" | [ + ] [ | ] [ | ] | \r\n" +
" |  | |   | |   | |  | \r\n" +
" |  |[~] [~]|   |[~] | \r\n" +
" |  | |   | |   | |  | \r\n" +
" |  | ++ ++ ++ ++ |  | \r\n" +
" |  |  | |   | |  |  | \r\n" +
" ++-+ [ < ] [ < ] +-++ \r\n" +
"  |     |     |     |  \r\n" +
"  |     |    [~]    |  \r\n" +
"  |     |     |     |  \r\n" +
"[ + ] [ & ] [ = ] [ + ]\r\n" +
" | |   | |   | |   | | \r\n" +
" | +-+-+ +-+-+ +-+-+ | \r\n" +
" |   |     |     |   | \r\n" +
" | [ & ] [ | ] [ & ] | \r\n" +
" |  | |   | |   | |  | \r\n" +
" 0  0 0   0 0   0 0  0 \r\n";
//*/
#endif

			#endregion

		#region NETWORKING CHALLENGE
		#if NETWORKING_CHALLENGE

		static string	ms_inputText =
"3\r\n" +
"-5 -3\r\n" +
"-9 2\r\n" +
"3 -4\r\n";
// 		static string	ms_inputText =
// "3\r\n" +
// "0 0\r\n" +
// "1 1\r\n" +
// "2 2\r\n";

		#endif
		#endregion

		#region WAR CHALLENGE
		#if WAR_CHALLENGE

//* 3 cards
		static string	ms_inputText =
"3\r\n" +
"AD\r\n" +
"KC\r\n" +
"QC\r\n" +
"3\r\n" +
"KH\r\n" +
"QS\r\n" +
"JC\r\n";
//*/
/* 26 cards
		static string	ms_inputText =
"26\r\n" +
"5C\r\n" +
"3D\r\n" +
"2C\r\n" +
"7D\r\n" +
"8C\r\n" +
"7S\r\n" +
"5D\r\n" +
"5H\r\n" +
"6D\r\n" +
"5S\r\n" +
"4D\r\n" +
"6H\r\n" +
"6S\r\n" +
"3C\r\n" +
"3S\r\n" +
"7C\r\n" +
"4S\r\n" +
"4H\r\n" +
"7H\r\n" +
"4C\r\n" +
"2H\r\n" +
"6C\r\n" +
"8D\r\n" +
"3H\r\n" +
"2D\r\n" +
"2S\r\n" +
"26\r\n" +
"AC\r\n" +
"9H\r\n" +
"KH\r\n" +
"KC\r\n" +
"KD\r\n" +
"KS\r\n" +
"10S\r\n" +
"10D\r\n" +
"9S\r\n" +
"QD\r\n" +
"JS\r\n" +
"10H\r\n" +
"8S\r\n" +
"QH\r\n" +
"JD\r\n" +
"AD\r\n" +
"JC\r\n" +
"AS\r\n" +
"QS\r\n" +
"AH\r\n" +
"JH\r\n" +
"10C\r\n" +
"9C\r\n" +
"8H\r\n" +
"QC\r\n" +
"9D\r\n";
//*/

/* 26 cards, medium length
		static string	ms_inputText =
"26\r\n" +
"6H\r\n" +
"7H\r\n" +
"6C\r\n" +
"QS\r\n" +
"7S\r\n" +
"8D\r\n" +
"6D\r\n" +
"5S\r\n" +
"6S\r\n" +
"QH\r\n" +
"4D\r\n" +
"3S\r\n" +
"7C\r\n" +
"3C\r\n" +
"4S\r\n" +
"5H\r\n" +
"QD\r\n" +
"5C\r\n" +
"3H\r\n" +
"3D\r\n" +
"8C\r\n" +
"4H\r\n" +
"4C\r\n" +
"QC\r\n" +
"5D\r\n" +
"7D\r\n" +
"26\r\n" +
"JH\r\n" +
"AH\r\n" +
"KD\r\n" +
"AD\r\n" +
"9C\r\n" +
"2D\r\n" +
"2H\r\n" +
"JC\r\n" +
"10C\r\n" +
"KC\r\n" +
"10D\r\n" +
"JS\r\n" +
"JD\r\n" +
"9D\r\n" +
"9S\r\n" +
"KS\r\n" +
"AS\r\n" +
"KH\r\n" +
"10S\r\n" +
"8S\r\n" +
"2S\r\n" +
"10H\r\n" +
"8H\r\n" +
"AC\r\n" +
"2C\r\n" +
"9H\r\n";
//*/

/* Battle
		static string	ms_inputText =
"5\r\n" +
"8C\r\n" +
"KD\r\n" +
"AH\r\n" +
"QH\r\n" +
"2S\r\n" +

"5\r\n" +
"8D\r\n" +
"2D\r\n" +
"3H\r\n" +
"4D\r\n" +
"3S\r\n";
//*/
/* One game, one battle
		static string	ms_inputText =
"26\r\n" +
"10H\r\n" +
"KD\r\n" +
"6C\r\n" +
"10S\r\n" +
"8S\r\n" +
"AD\r\n" +
"QS\r\n" +
"3D\r\n" +
"7H\r\n" +
"KH\r\n" +
"9D\r\n" +
"2D\r\n" +
"JC\r\n" +
"KS\r\n" +
"3S\r\n" +
"2S\r\n" +
"QC\r\n" +
"AC\r\n" +
"JH\r\n" +
"7D\r\n" +
"KC\r\n" +
"10D\r\n" +
"4C\r\n" +
"AS\r\n" +
"5D\r\n" +
"5S\r\n" +
"26\r\n" +
"2H\r\n" +
"9C\r\n" +
"8C\r\n" +
"4S\r\n" +
"5C\r\n" +
"AH\r\n" +
"JD\r\n" +
"QH\r\n" +
"7C\r\n" +
"5H\r\n" +
"4H\r\n" +
"6H\r\n" +
"6S\r\n" +
"QD\r\n" +
"9H\r\n" +
"10C\r\n" +
"4D\r\n" +
"JS\r\n" +
"6D\r\n" +
"3H\r\n" +
"8H\r\n" +
"3C\r\n" +
"7S\r\n" +
"9S\r\n" +
"8D\r\n" +
"2C\r\n";
// */
/* 2 chained battles
		static string	ms_inputText =
"9\r\n" +
"8C\r\n" +
"KD\r\n" +
"AH\r\n" +
"QH\r\n" +
"3D\r\n" +
"KD\r\n" +
"AH\r\n" +
"QH\r\n" +
"6D\r\n" +
"9\r\n" +
"8D\r\n" +
"2D\r\n" +
"3H\r\n" +
"4D\r\n" +
"3S\r\n" +
"2D\r\n" +
"3H\r\n" +
"4D\r\n" +
"7H\r\n";
//*/
/* Long game
		static string	ms_inputText =
"26\r\n" +
"AH\r\n" +
"4H\r\n" +
"5D\r\n" +
"6D\r\n" +
"QC\r\n" +
"JS\r\n" +
"8S\r\n" +
"2D\r\n" +
"7D\r\n" +
"JD\r\n" +
"JC\r\n" +
"6C\r\n" +
"KS\r\n" +
"QS\r\n" +
"9D\r\n" +
"2C\r\n" +
"5S\r\n" +
"9S\r\n" +
"6S\r\n" +
"8H\r\n" +
"AD\r\n" +
"4D\r\n" +
"2H\r\n" +
"2S\r\n" +
"7S\r\n" +
"8C\r\n" +
"26\r\n" +
"10H\r\n" +
"4C\r\n" +
"6H\r\n" +
"3C\r\n" +
"KC\r\n" +
"JH\r\n" +
"10C\r\n" +
"AS\r\n" +
"5H\r\n" +
"KH\r\n" +
"10S\r\n" +
"9H\r\n" +
"9C\r\n" +
"8D\r\n" +
"5C\r\n" +
"AC\r\n" +
"3H\r\n" +
"4S\r\n" +
"KD\r\n" +
"7C\r\n" +
"3S\r\n" +
"QH\r\n" +
"10D\r\n" +
"3D\r\n" +
"7H\r\n" +
"QD\r\n";
// */
/*
		static string	ms_inputText =
"26\r\n" +
"5S\r\n" +
"8D\r\n" +
"10H\r\n" +
"9S\r\n" +
"4S\r\n" +
"6H\r\n" +
"QC\r\n" +
"6C\r\n" +
"6D\r\n" +
"9H\r\n" +
"2C\r\n" +
"7S\r\n" +
"AC\r\n" +
"5C\r\n" +
"7D\r\n" +
"9D\r\n" +
"QS\r\n" +
"4D\r\n" +
"3C\r\n" +
"JS\r\n" +
"2D\r\n" +
"KD\r\n" +
"10S\r\n" +
"QD\r\n" +
"3H\r\n" +
"8H\r\n" +
"26\r\n" +
"4C\r\n" +
"JC\r\n" +
"8S\r\n" +
"10C\r\n" +
"5H\r\n" +
"7H\r\n" +
"3D\r\n" +
"AH\r\n" +
"KS\r\n" +
"10D\r\n" +
"JH\r\n" +
"6S\r\n" +
"2S\r\n" +
"KC\r\n" +
"8C\r\n" +
"9C\r\n" +
"KH\r\n" +
"3S\r\n" +
"AD\r\n" +
"JD\r\n" +
"4H\r\n" +
"7C\r\n" +
"2H\r\n" +
"QH\r\n" +
"5D\r\n" +
"AS\r\n";
*/

		#endif
		#endregion

		#region DIVISORS CHALLENGE
#if DIVISORS_CHALLENGE
		static string	ms_inputText = "4";
#endif
		#endregion

#region MARS LANDER CONTROLLER

		static string	ms_inputText = 
"7\r\n" + 
"0 100\r\n" + 
"1000 500\r\n" + 
"1500 1500\r\n" + 
"3000 1000\r\n" + 
"4000 150\r\n" + 
"5500 150\r\n" + 
"6999 800\r\n" +
"2500 2700 0 0 550 0 0";


		class MarsLander {
			class Writer : StringWriter {
				public MarsLander	m_owner;
				public override void WriteLine( string value ) {
					System.Diagnostics.Debug.WriteLine( value );
					m_owner.React( value );
				}
			}
			class Reader : TextReader {
				public List< string >	m_lines = new List< string >();
				public override string ReadLine() {
					string	top = m_lines[0];
					m_lines.RemoveAt( 0 );
					return top;
				}
			}
			Writer	m_writer = new Writer();
			Reader	m_reader = new Reader();

			List< float2 >	m_landscape = new List< float2 >();

			float2	Pos;
			float2	Vel;
			int		Fuel;
			int		Rotation, Thrust;

			public MarsLander( string _initialLines ) {
				m_writer.m_owner = this;
				Console.SetIn( m_reader );
				Console.SetOut( m_writer );

				string[]	initialLines = _initialLines.Split( '\n' );
				for ( int i=0; i < initialLines.Length; i++ )
					m_reader.m_lines.Add( initialLines[i] );

				int			landscapePointsCount = int.Parse( initialLines[0] );
				for ( int i=0; i < landscapePointsCount; i++ ) {
					string[]	coords = initialLines[1+i].Split( ' ' );
					m_landscape.Add( new float2( float.Parse( coords[0] ), float.Parse( coords[1] ) ) );
				}

				string[]	initialValues = initialLines[1+landscapePointsCount].Split( ' ' );
				Pos = new float2( int.Parse( initialValues[0] ), int.Parse( initialValues[1] ) );
				Vel = new float2( int.Parse( initialValues[2] ), int.Parse( initialValues[3] ) );
				Fuel = int.Parse( initialValues[4] );
				Rotation = int.Parse( initialValues[5] );
				Thrust = int.Parse( initialValues[6] );
			}

			void	React( string _userCommands ) {
				string[]	userCommands = _userCommands.Split( ' ' );
				int			newRotation = int.Parse( userCommands[0] );
				if ( newRotation < -90 || newRotation > 90 ) throw new Exception( "Too large rotation!" );
				if ( Math.Abs( newRotation - Rotation ) > 15 ) throw new Exception( "Too large rotation delta!" );
				int			newThrust = int.Parse( userCommands[1] );
				if ( newThrust < 0 || newThrust > 4 ) throw new Exception( "Too large thrust!" );
				if ( Math.Abs( newThrust - Thrust ) > 1 ) throw new Exception( "Too large thrust delta!" );

				Rotation = newRotation;
				Thrust = newThrust;
				Fuel--;

				float2	oldPos = Pos;

				float2	Acc = Thrust * new float2( (float) Math.Sin( Rotation * Math.PI / 180 ), (float) Math.Cos( Rotation * Math.PI / 180 ) );
				Vel += Acc;
				Pos += Vel;
				CheckCrash( oldPos );

				string	newLine = ((int) Pos.x) + " " + ((int) Pos.y) + " " + ((int) Vel.x) + " " + ((int) Vel.y) + " " + Fuel + " " + Rotation + " " + Thrust;
				m_reader.m_lines.Add( newLine );
			}

			void	CheckCrash( float2 _oldPos ) {
				for ( int i=0; i < m_landscape.Count-1; i++ ) {
					float2	P0 = m_landscape[i];
					float2	P1 = m_landscape[i+1];
					float2	N = (P1 - P0).Normalized;
							N.Set( -N.y, N.x );

					float	dot0 = (_oldPos - P0).Dot( N );
					float	dot1 = (Pos - P0).Dot( N );
					if ( dot0 > 0.0f && dot1 <= 0 ) {
						throw new Exception( "Crash!" );
					}
				}
			}

		}

#endregion

		class ConsoleWriterOverride : StringWriter {
			public override void WriteLine( string value ) {
				System.Diagnostics.Debug.WriteLine( value );
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );

			// Replace input and output streams
			Console.SetIn( new StringReader( ms_inputText ) );
			Console.SetError( new ConsoleWriterOverride() );

			#if MARS_LANDER_CHALLENGE
				MarsLander	controller = new MarsLander( ms_inputText );
			#endif

			// Execute
			Solution.Meuh( null );
		}
	}
}
