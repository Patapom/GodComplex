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

// 		static string	ms_inputText = 
// "7\r\n" + 
// "0 100\r\n" + 
// "1000 500\r\n" + 
// "1500 1500\r\n" + 
// "3000 1000\r\n" + 
// "4000 150\r\n" + 
// "5500 150\r\n" + 
// "6999 800\r\n" +
// "2500 2700 0 0 550 0 0";

		// Deep canyon
		static string	ms_inputText = 
"20\r\n" + 
"0 1000\r\n" + 
"300 1500\r\n" + 
"350 1400\r\n" + 
"500 2000\r\n" + 
"800 1800\r\n" + 
"1000 2500\r\n" + 
"1200 2100\r\n" + 
"1500 2400\r\n" + 
"2000 1000\r\n" + 
"2200 500\r\n" + 
"2500 100\r\n" + 
"2900 800\r\n" + 
"3000 500\r\n" + 
"3200 1000\r\n" + 
"3500 2000\r\n" + 
"3800 800\r\n" + 
"4000 200\r\n" + 
"5000 200\r\n" + 
"5500 1500\r\n" + 
"6999 2800\r\n" + 
"500 2700 100 0 800 -90 0";

// 		// High ground
// 		static string	ms_inputText = 
// "20\r\n" +
// "0 1000\r\n" +
// "300 1500\r\n" +
// "350 1400\r\n" +
// "500 2100\r\n" +
// "1500 2100\r\n" +
// "2000 200\r\n" +
// "2500 500\r\n" +
// "2900 300\r\n" +
// "3000 200\r\n" +
// "3200 1000\r\n" +
// "3500 500\r\n" +
// "3800 800\r\n" +
// "4000 200\r\n" +
// "4200 800\r\n" +
// "4800 600\r\n" +
// "5000 1200\r\n" +
// "5500 900\r\n" +
// "6000 500\r\n" +
// "6500 300\r\n" +
// "6999 500\r\n" +
// "6500 2700 -50 0 1000 90 0";

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
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo( "en-US" );
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );

			// Replace input and output streams
			Console.SetIn( new StringReader( ms_inputText ) );
			Console.SetError( new ConsoleWriterOverride() );

			#if MARS_LANDER_CHALLENGE
				TestForm.MarsLanderForm	form = new TestForm.MarsLanderForm( ms_inputText );
				Application.Run( form );
			#else
				// Execute
				Solution.Meuh( null );
			#endif
		}
	}
}
