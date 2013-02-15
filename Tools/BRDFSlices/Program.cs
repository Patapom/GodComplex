//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
// 
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using WMath;

namespace BRDFSlices
{
	[System.Diagnostics.DebuggerDisplay( "x={x} y={y} z={z}" )]
	public struct	Vector3
	{
		public double	x, y, z;

		public void		Set( double _x, double _y, double _z )	{ x = _x; y = _y; z = _z; }
		public double	LengthSq()	{ return x*x + y*y + z*z; }
		public double	Length()	{ return Math.Sqrt( LengthSq() ); }
		public void		Normalize()
		{
			double	InvLength = 1.0 / Length();
			x *= InvLength;
			y *= InvLength;
			z *= InvLength;
		}

		public double	Dot( ref Vector3 a )
		{
			return x*a.x + y*a.y + z*a.z;
		}

		public void		Cross( ref Vector3 a, out Vector3 _Out )
		{
			_Out.x = y*a.z - z*a.y;
			_Out.y = z*a.x - x*a.z;
			_Out.z = x*a.y - y*a.x;
		}

		// Rotate vector along one axis
		private static Vector3	TempCross = new Vector3();
		public void		Rotate( ref Vector3 _Axis, double _Angle, out Vector3 _Out )
		{
			double	cos_ang = Math.Cos( _Angle );
			double	sin_ang = Math.Sin( _Angle );

			_Out.x = x * cos_ang;
			_Out.y = y * cos_ang;
			_Out.z = z * cos_ang;

			double	temp = Dot( ref _Axis );
					temp *= 1.0-cos_ang;

			_Out.x += _Axis.x * temp;
			_Out.y += _Axis.y * temp;
			_Out.z += _Axis.z * temp;

			_Axis.Cross( ref this, out TempCross );
	
			_Out.x += TempCross.x * sin_ang;
			_Out.y += TempCross.y * sin_ang;
			_Out.z += TempCross.z * sin_ang;
		}

		public double	this[int _Index]
		{
			get { return _Index == 0 ? x : (_Index == 1 ? y : z); }
			set
			{
				switch ( _Index )
				{
					case 0: x = value; break;
					case 1: y = value; break;
					case 2: z = value; break;
					default: throw new Exception( "Component index out of range!" );
				}
			}
		}

		public static Vector3	operator*( double a, Vector3 b )
		{
			return new Vector3() { x = a * b.x, y = a * b.y, z = a * b.z };
		}

		public static Vector3	operator+( Vector3 a, Vector3 b )
		{
			return new Vector3() { x = a.x + b.x, y = a.y + b.y, z = a.z + b.z };
		}
	}

	class Program
	{
		#region CONSTANTS

		const int		SAMPLES_COUNT_THETA = 90;				// Use 90 to cover the entire BRDF (1 458 000 samples).
		const double	BFGS_CONVERGENCE_TOLERANCE = 1e-4;		// Don't exit unless we reach below this threshold...
		const double	DERIVATIVE_OFFSET = 5e-3;

		#endregion

		static void Main( string[] args )
		{
			try
			{
				// Analyze arguments
				if ( args.Length != 1 )
					throw new Exception( "Usage: BRDFSlices \"Path to MERL BRDF\"" );

				FileInfo	SourceBRDF = new FileInfo( args[0] );
				if ( !SourceBRDF.Exists )
					throw new Exception( "Source BRDF file \"" + SourceBRDF.FullName + "\" does not exist!" );

				// Load the BRDF
				Vector3[,,]	BRDF = DisplayForm.LoadBRDF( SourceBRDF );


				DisplayForm	F = new DisplayForm( BRDF );
				Application.Run( F );
			}
			catch ( Exception _e )
			{
				MessageBox.Show( "An error occurred!\r\n\r\n" + _e.Message + "\r\n\r\n" + _e.StackTrace, "BRDF Fitting", MessageBoxButtons.OK, MessageBoxIcon.Warning );
			}
		}

		private static bool		IsValid( double _BRDFValue )
		{
			if ( _BRDFValue < 0.0 )
				return false;
			if ( _BRDFValue > 10000.0 )
				return false;

			if ( double.IsNaN( _BRDFValue ) )
				return false;

			return true;
		}
	}
}
