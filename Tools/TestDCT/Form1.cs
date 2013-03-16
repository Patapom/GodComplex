using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//using WMath;

namespace TestDCT
{
	public partial class Form1 : Form
	{
		public const float		MAX_Z = 6.0f;
		public const int		CURVE_SIZE = 200;

		#region NESTED TYPES

		[System.Diagnostics.DebuggerDisplay( "{x}, {y}, {z}, {w}" )]
		public class	Vector4
		{
			public float	x, y, z, w;
			public Vector4( float _x, float _y, float _z, float _w )
			{
				x = _x; y = _y; z = _z; w = _w;
			}

			public static Vector4	operator+( Vector4 a, Vector4 b )
			{
				return new Vector4( a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w );
			}

			public static Vector4	operator*( float a, Vector4 b )
			{
				return new Vector4( a * b.x, a * b.y, a * b.z, a * b.w );
			}

			public static Vector4	operator*( Vector4 b, float a )
			{
				return new Vector4( a * b.x, a * b.y, a * b.z, a * b.w );
			}

			public float	Dot( Vector4 b )
			{
				return x*b.x + y*b.y + z*b.z + w*b.w;
			}

			public Vector4	Cos()
			{
				return new Vector4( (float) Math.Cos( x ), (float) Math.Cos( y ), (float) Math.Cos( z ), (float) Math.Cos( w ) );
			}
		}

		[System.Diagnostics.DebuggerDisplay( "{x}, {y}, {z}" )]
		public class	Vector
		{
			public float	x, y, z;
			public Vector( float _x, float _y, float _z )
			{
				x = _x; y = _y; z = _z;
			}

			public static Vector	operator+( Vector a, Vector b )
			{
				return new Vector( a.x + b.x, a.y + b.y, a.z + b.z );
			}

			public float	Dot( Vector b )
			{
				return x*b.x + y*b.y + z*b.z;
			}

			public Vector	Normalize()
			{
				float	L = (float) Math.Sqrt( x*x + y*y + z*z );
						L = 1.0f / L;

				return new Vector( L*x, L*y, L*z );
			}
		}

		[System.Diagnostics.DebuggerDisplay( "{x}, {y}" )]
		public class	Vector2
		{
			public float	x, y;
			public Vector2( float _x, float _y )
			{
				x = _x; y = _y;
			}

			public static Vector2	operator+( Vector2 a, Vector2 b )
			{
				return new Vector2( a.x + b.x, a.y + b.y );
			}

			public static Vector2	operator-( Vector2 a, Vector2 b )
			{
				return new Vector2( a.x - b.x, a.y - b.y );
			}

			public float	Dot( Vector2 b )
			{
				return x*b.x + y*b.y;
			}

			public Vector2	Normalize()
			{
				float	L = (float) Math.Sqrt( x*x + y*y );
						L = 1.0f / L;

				return new Vector2( L*x, L*y );
			}
		}

		#endregion

		#region FIELDS

		#endregion

		public float	Smoothstep( float min, float max, float x )
		{
			float	t = (x - min) / (max - min);
					t = Math.Max( 0.0f, Math.Min( 1.0f, t ) );
			return t * t * (3.0f - 2.0f * t);
		}

		public Form1()
		{
			InitializeComponent();

			// Build the test curve
			float[][]	Curve = new float[CURVE_SIZE][];

			Random	RNG = new Random( 1 );
			float	StepSize = MAX_Z / CURVE_SIZE;
			float	TotalTransmittance = 1.0f;
			for ( int i=0; i < CURVE_SIZE; i++ )
			{
				float	x = MAX_Z * i / CURVE_SIZE;
				float	Rand = (float) RNG.NextDouble();
//				float	Density = Math.Max( 0.0f, Rand - 0.40f );
				float	Density = (float) Math.Exp( -8.0f * Rand );
						Density *= Density;
						Density *= Smoothstep( 0.0f, 3.0f, x );
						Density *= Smoothstep( 6.0f, 4.0f, x );
				float	ExtinctionCoeff = 8.0f * Density;
				float	Transmittance = (float) Math.Exp( -ExtinctionCoeff * StepSize );
				TotalTransmittance *= Transmittance;

				Curve[i] = new float[2] { i * MAX_Z / CURVE_SIZE, TotalTransmittance };
			}
			displayPanelCurve.m_Curve = Curve;

			// Apply DCT compression
			float	dx = 1.0f / CURVE_SIZE;					// Normalized Z step size

			Vector4	CosTerm0 = (float) Math.PI * new Vector4( 0, 1, 2, 3 );
			Vector4	CosTerm1 = (float) Math.PI * new Vector4( 4, 5, 6, 7 );
			Vector4	Angle0 = 0.5f * dx * CosTerm0;
			Vector4	Angle1 = 0.5f * dx * CosTerm1;
			Vector4	dAngle0 = dx * CosTerm0;				// This is the increment in phase
			Vector4	dAngle1 = dx * CosTerm1;

			Vector4	DCTCoeffs0 = new Vector4( 0, 0, 0, 0 );
			Vector4	DCTCoeffs1 = new Vector4( 0, 0, 0, 0 );

			for ( int i=0; i < CURVE_SIZE; i++ )
			{
				float	Transmittance = Curve[i][1];

//Transmittance = 1.0f - (float) i / CURVE_SIZE;
//Transmittance = 0.25f;
//Transmittance = 0.5f * (1.0f + (float) Math.Cos( 2*Math.PI * i / CURVE_SIZE ));
//Transmittance = (float) Math.Cos( 1*Math.PI * i / CURVE_SIZE );

				DCTCoeffs0 += Transmittance * dx * Angle0.Cos();
				DCTCoeffs1 += Transmittance * dx * Angle1.Cos();

				Angle0 += dAngle0;
				Angle1 += dAngle1;
			}
			DCTCoeffs0 *= 2.0f;
			DCTCoeffs1 *= 2.0f;
			displayPanelDCT.m_DCTCoefficients = new float[] { DCTCoeffs0.x, DCTCoeffs0.y, DCTCoeffs0.z, DCTCoeffs0.w, DCTCoeffs1.x, DCTCoeffs1.y, DCTCoeffs1.z, DCTCoeffs1.w };
		}
	}
}
