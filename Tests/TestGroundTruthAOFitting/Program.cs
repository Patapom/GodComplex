using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GenerateSelfShadowedBumpMap
{
	static class Program
	{
        [System.Runtime.InteropServices.DllImport( "kernel32.dll" )]
        static extern bool AttachConsole( int dwProcessId );
        private const int ATTACH_PARENT_PROCESS = -1;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main( string[] _args )
		{
// _args = new string[] {
// 	"-height",
// 	"../Example/heights.tga",
// 	"-normal",
// 	"../Example/normals.tga",
// 	"-ao",
// 	"../Example/occlusion3.png",
// 	"-texsize",
// 	"100",
// 	"-dispsize",
// 	"100",
// 	"-rayscount",
// 	"100",
// 	"-range",
// 	"10",
// 	"-coneangle",
// 	"180",
// 	"-clamp",
// 	"-bilateralRadius",
// 	"4.0",
// 	"-bilateralTolerance",
// 	"0.2",
// };



			if ( _args.Length == 0 ) {
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault( false );
				Application.Run( new GeneratorForm() );
				return;
			}

			// Attach to command console
            AttachConsole( ATTACH_PARENT_PROCESS );

			// Arguments-driven generation
			GeneratorForm.BuildArguments	buildArgs = new GeneratorForm.BuildArguments();
			try {
				for ( int i=0; i < _args.Length; i++ ) {
					string	arg = _args[i].ToLower().Trim();
					if ( arg.Length < 2 )
						throw new Exception( "Invalid argument at position " + i );
					if ( arg[0] != '-' )
						throw new Exception( "Argument \"" + arg + "\" missing heading '-' character" );

					switch ( arg.Substring( 1 ) ) {
						case "height":
							buildArgs.heightMapFileName = ReadNextArg( _args, ref i );
							break;
						case "normal":
							buildArgs.normalMapFileName = ReadNextArg( _args, ref i );
							break;
						case "ao":
							buildArgs.AOMapFileName = ReadNextArg( _args, ref i );
							break;
						case "texsize":
							buildArgs.textureSize_cm = ReadFloatArg( _args, ref i );
							break;
						case "dispsize":
							buildArgs.displacementSize_cm = ReadFloatArg( _args, ref i );
							break;
						case "rayscount":
							buildArgs.raysCount = ReadIntArg( _args, ref i );
							break;
						case "range":
							buildArgs.searchRange = ReadIntArg( _args, ref i );
							break;
						case "coneangle":
							buildArgs.coneAngle = ReadFloatArg( _args, ref i );
							break;
						case "clamp":
							buildArgs.tile = false;
							break;
						case "bilateralradius":
							buildArgs.bilateralRadius = ReadFloatArg( _args, ref i );
							break;
						case "bilateraltolerance":
							buildArgs.bilateralTolerance = ReadFloatArg( _args, ref i );
							break;

						default:
							throw new Exception( "Unrecognized argument \"" + arg + "\" at position " + i );
					}
				}

				if ( buildArgs.heightMapFileName == null )
					throw new Exception( "You failed to provide the mandatory height map name to generate AO for..." );
				if ( buildArgs.AOMapFileName == null )
					throw new Exception( "You failed to provide the mandatory target AO map name to save the results to..." );
			}
			catch ( Exception _e ) {
				Console.WriteLine( "Unexpected argument parsing error: " + _e.Message + "\r\n\r\n"
					+ "Usage: GenerateAmbientOcclusionMap.exe"
					+ " -height \"path/to/heightmap\""
					+ " -ao \"path/to/target/aomap\""
					+ " [-normal \"path/to/normalmap\"]"
					+ " [-texSize <physical texture size, in centimeters>]"
					+ " [-dispSize <displacement size encoded by height map, in centimeters>]"
					+ " [-raysCount <amount of rays per pixel>]"
					+ " [-range <search range, in pixels>]"
					+ " [-coneAngle <search cone aperture angle, in degrees>]"
					+ " [-clamp]"
					+ " [-bilateralRadius <bilateral filtering radius, in pixels>]"
					+ " [-bilateralTolerance <bilateral filtering height tolerance>]"
					);
				return;
			}

			try {
				GeneratorForm	F = new GeneratorForm();
				F.Build( buildArgs );
			} catch ( Exception _e ) {
				Console.WriteLine( "An error occurred during AO map generation: " + _e.Message + "\r\n" );
			}
		}

		static string	ReadNextArg( string[] _args, ref int i ) {
			if ( i >= _args.Length )
				throw new Exception( "Expected an argument following \"" + _args[i] + "\" but encountered end of arguments..." );

			return _args[++i];
		}

		static float	ReadFloatArg( string[] _args, ref int i ) {
			string	arg = ReadNextArg( _args, ref i );
			float	value;
			if ( float.TryParse( arg, out value ) )
				return value;

			throw new Exception( "Failed to parse floating point value for argument \"" + _args[i-1] + " " + _args[i] );
		}

		static int	ReadIntArg( string[] _args, ref int i ) {
			string	arg = ReadNextArg( _args, ref i );
			int		value;
			if ( int.TryParse( arg, out value ) )
				return value;

			throw new Exception( "Failed to parse integer value for argument \"" + _args[i-1] + " " + _args[i] );
		}
	}
}
