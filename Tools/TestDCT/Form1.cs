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
		public const int		SAMPLING_SIZE = 32;

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

			Color	C = Color.SkyBlue;

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

				Density *= 1.0f - (x < 3.0f ? Smoothstep( 2.0f, 3.0f, x ) : Smoothstep( 4.0f, 3.0f, x ));


//Density = 0.0f;
//Density = 0.1f;

				float	ExtinctionCoeff = 8.0f * Density;
				float	Transmittance = (float) Math.Exp( -ExtinctionCoeff * StepSize );

//Transmittance = 1.0f;

				TotalTransmittance *= Transmittance;

//TotalTransmittance = Math.Max(0.0f, 1.0f - 1.5f * i / CURVE_SIZE);    // Linear transmittance



				Curve[i] = new float[2] { i * MAX_Z / CURVE_SIZE, TotalTransmittance };
			}
			displayPanelCurve.m_Curve = Curve;

			// Apply DCT compression
			float	dx = 1.0f / SAMPLING_SIZE;					// Normalized Z step size

			Vector4	CosTerm0 = (float) Math.PI * new Vector4( 0, 1, 2, 3 );
			Vector4	CosTerm1 = (float) Math.PI * new Vector4( 4, 5, 6, 7 );
			Vector4	dAngle0 = dx * CosTerm0;				// This is the increment in phase
			Vector4	dAngle1 = dx * CosTerm1;
// 			Vector4	Angle0 = 0.5f * dAngle0;
// 			Vector4	Angle1 = 0.5f * dAngle1;
			Vector4	Angle0 = 0.0f * dAngle0;
			Vector4	Angle1 = 0.0f * dAngle1;

			Vector4	DCTCoeffs0 = new Vector4( 0, 0, 0, 0 );
			Vector4	DCTCoeffs1 = new Vector4( 0, 0, 0, 0 );

			Vector4	CurrentCosAngle0 = new Vector4( 1, 1, 1, 1 );
			Vector4	CurrentCosAngle1 = new Vector4( 1, 1, 1, 1 );
			float	PreviousTransmittance = 1.0f;
			for ( int i=0; i < SAMPLING_SIZE; i++ )
			{
				Angle0 += dAngle0;
				Angle1 += dAngle1;

				int	CurveIndex = Math.Min( CURVE_SIZE-1, CURVE_SIZE * (1+i) / (SAMPLING_SIZE+1) );
//				int	CurveIndex = (int) (CURVE_SIZE * (0.0f+i) / (SAMPLING_SIZE+0.0f));
				float	Transmittance = Curve[CurveIndex][1];

				// Use average of transmittance
// 				float	TransmittanceToEncode = 0.5f * (PreviousTransmittance + Transmittance);
 				PreviousTransmittance = Transmittance;
				float	TransmittanceToEncode = Transmittance;

// 				// Square integration (very bad with not enough steps!)
//				DCTCoeffs0 += TransmittanceToEncode * Angle0.Cos();
//				DCTCoeffs1 += TransmittanceToEncode * Angle1.Cos();

/*				// Better trapezoidal integration
				Vector4	PreviousCosAngle0 = CurrentCosAngle0;
				Vector4	PreviousCosAngle1 = CurrentCosAngle1;
				CurrentCosAngle0 = Angle0.Cos();
				CurrentCosAngle1 = Angle1.Cos();

				Vector4	AverageCosAngle0 = 0.5f * (PreviousCosAngle0 + CurrentCosAngle0);
				Vector4	AverageCosAngle1 = 0.5f * (PreviousCosAngle1 + CurrentCosAngle1);

				DCTCoeffs0 += TransmittanceToEncode * AverageCosAngle0;
				DCTCoeffs1 += TransmittanceToEncode * AverageCosAngle1;
//*/
//*				// Better trapezoidal integration
				Vector4	PreviousCosAngle0 = CurrentCosAngle0;
				Vector4	PreviousCosAngle1 = CurrentCosAngle1;
				CurrentCosAngle0 = TransmittanceToEncode * Angle0.Cos();	// Include transmittance term in the integral! Tiny bit better!
				CurrentCosAngle1 = TransmittanceToEncode * Angle1.Cos();

				Vector4	AverageCosAngle0 = 0.5f * (PreviousCosAngle0 + CurrentCosAngle0);
				Vector4	AverageCosAngle1 = 0.5f * (PreviousCosAngle1 + CurrentCosAngle1);

				DCTCoeffs0 += AverageCosAngle0;
				DCTCoeffs1 += AverageCosAngle1;
//*/
			}
            DCTCoeffs0 *= 2.0f * dx;
            DCTCoeffs1 *= 2.0f * dx;
			displayPanelDCT.m_DCTCoefficients = new float[] { DCTCoeffs0.x, DCTCoeffs0.y, DCTCoeffs0.z, DCTCoeffs0.w, DCTCoeffs1.x, DCTCoeffs1.y, DCTCoeffs1.z, DCTCoeffs1.w };
		}
	}
}
