using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//using WMath;

namespace TestImportanceSampling
{
	public partial class Form1 : Form
	{
		public const int	PROBA_ARRAY_SIZE = 128;
		public const int	SAMPLES_COUNT_PHI = 2000;
		public const int	SAMPLES_COUNT_THETA = 1000;

		#region NESTED TYPES

		[System.Diagnostics.DebuggerDisplay( "{x}, {y}, {z}" )]
		public class	Vector
		{
			public double	x, y, z;
			public Vector( double _x, double _y, double _z )
			{
				x = _x; y = _y; z = _z;
			}

			public static Vector	operator+( Vector a, Vector b )
			{
				return new Vector( a.x + b.x, a.y + b.y, a.z + b.z );
			}

			public double	Dot( Vector b )
			{
				return x*b.x + y*b.y + z*b.z;
			}

			public Vector	Normalize()
			{
				double	L = Math.Sqrt( x*x + y*y + z*z );
						L = 1.0 / L;

				return new Vector( L*x, L*y, L*z );
			}
		}

		[System.Diagnostics.DebuggerDisplay( "{x}, {y}" )]
		public class	Vector2
		{
			public double	x, y;
			public Vector2( double _x, double _y )
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

			public double	Dot( Vector2 b )
			{
				return x*b.x + y*b.y;
			}

			public Vector2	Normalize()
			{
				double	L = Math.Sqrt( x*x + y*y );
						L = 1.0 / L;

				return new Vector2( L*x, L*y );
			}
		}

		public class	ParametrizedSpace
		{
			public double[,]	m_Probabilities = new double[PROBA_ARRAY_SIZE,PROBA_ARRAY_SIZE];
			public void		Clear()
			{
				for ( int Y=0; Y < PROBA_ARRAY_SIZE; Y++ )
					for ( int X=0; X < PROBA_ARRAY_SIZE; X++ )
						m_Probabilities[X,Y] = 0.0;
			}
			public void		Accumulate( Vector2 _UV, double _Weight )
			{
				if ( _UV.x < -1e-6 ) throw new Exception( "FUCK U<0!" );
//				if ( _UV.x > 1.0+1e-6 ) throw new Exception( "FUCK U>1!" );
				if ( _UV.y < -1e-6 ) throw new Exception( "FUCK V<0!" );

// 				_UV.x = Math.Max( 0, Math.Min( 1, _UV.x ) );
// 				_UV.y = Math.Max( 0, Math.Min( 1, _UV.y ) );

// 				double	X = _UV.x * (PROBA_ARRAY_SIZE-1);
// 				int		X0 = (int) Math.Floor( X );
// 				double	x = X - X0;
// 				int		X1 = Math.Min( PROBA_ARRAY_SIZE-1, X0+1 );
// 						X0 = Math.Min( PROBA_ARRAY_SIZE-1, X0 );
// 
// 				double	Y = _UV.y * (PROBA_ARRAY_SIZE-1);
// 				int		Y0 = (int) Math.Floor( Y );
// 				double	y = Y - Y0;
// 				int		Y1 = Math.Min( PROBA_ARRAY_SIZE-1, Y0+1 );
// 						Y0 = Math.Min( PROBA_ARRAY_SIZE-1, Y0 );
// 
// 				m_Probabilities[X0,Y0] += (1-x)*(1-y) * _Weight;
// 				m_Probabilities[X1,Y0] += x*(1-y) * _Weight;
// 				m_Probabilities[X0,Y1] += (1-x)*y * _Weight;
// 				m_Probabilities[X1,Y1] += x*y * _Weight;

				int		X = (int) Math.Floor( _UV.x * PROBA_ARRAY_SIZE );
				int		Y = (int) Math.Floor( _UV.y * PROBA_ARRAY_SIZE );
				X = Math.Max( 0, Math.Min( PROBA_ARRAY_SIZE-1, X ) );
				Y = Math.Max( 0, Math.Min( PROBA_ARRAY_SIZE-1, Y ) );
				m_Probabilities[X,Y] += _Weight;
			}
		}

		#endregion

		#region FIELDS

		protected double[,]	m_Scales = new double[90,PROBA_ARRAY_SIZE];

		ParametrizedSpace	m_Param = new ParametrizedSpace();

		#endregion

		public Form1()
		{
			InitializeComponent();

			BuildScales();

			floatTrackbarControlCameraTheta_ValueChanged( floatTrackbarControlCameraTheta, 0 );
		}

		public void		BuildProbabilities( double _CameraTheta, ParametrizedSpace _Probas )
		{
			Vector	ViewTS = new Vector( Math.Sin( _CameraTheta ), 0.0, Math.Cos( _CameraTheta ) );
			Vector	LightTS = new Vector( 0, 0, 0 );

			double	L = 2.0 * _CameraTheta / Math.PI;
			Vector2	Center = new Vector2( 0, L );		// We choose the center to be (0,ThetaD0)
//			Vector2	U = new Vector2( L, -L );			// U direction goes down to (ThetaH0,0)
			Vector2	U = new Vector2( 0.5 / L, -0.5 / L );			// U direction goes down to (ThetaH0,0)
//			Vector2	V = new Vector2( 1-L, 1-L );		// V direction goes up toward (PI/2, PI/2) (singularity when _CameraTheta=PI/2 !)
//			Vector2	V = new Vector2( 0.5 / (1-L), 0.5 / (1-L) );		// V direction goes up toward (PI/2, PI/2) (singularity when _CameraTheta=PI/2 !)
			Vector2	V = new Vector2( 1.0 / (1-L), 1.0 / (1-L) );		// V direction goes up toward (PI/2, PI/2) (singularity when _CameraTheta=PI/2 !)

			double	Normalizer = 100.0 / (SAMPLES_COUNT_PHI * SAMPLES_COUNT_THETA);
			Vector2	ST = new Vector2( 0, 0 );
			Vector2	Delta = new Vector2( 0, 0 );
			Vector2	UV = new Vector2( 0, 0 );

			_Probas.Clear();

			int		ScaleAngle = Math.Min( 89, (int) (90 * L) );

			for ( var Y=0; Y < SAMPLES_COUNT_PHI; Y++ )
			{
				var	PhiL = Math.PI * ((double) Y / SAMPLES_COUNT_PHI - 0.5);
				var	CosPhiL = Math.Cos( PhiL );
				var	SinPhiL = Math.Sin( PhiL );

				for ( var X=0; X < SAMPLES_COUNT_THETA; X++ )
				{
//					var	ThetaL = 0.5 * Math.PI * X / SAMPLES_COUNT_THETA;
					var	ThetaL = Math.Asin( Math.Sqrt( (double) X / SAMPLES_COUNT_THETA ) );	// Account for cosine weight of samples
					var	CosThetaL = Math.Cos( ThetaL );
					var	SinThetaL = Math.Sin( ThetaL );

					LightTS.x = SinThetaL * SinPhiL;
					LightTS.y = SinThetaL * CosPhiL;
					LightTS.z = CosThetaL;

					var	Half = (ViewTS + LightTS).Normalize();

					var	ThetaH = Math.Acos( Half.z );
					var	ThetaD = Math.Acos( ViewTS.Dot( Half ) );

					// Normalize in ST space
					ST.x = 2*ThetaH/Math.PI;
					ST.y = 2*ThetaD/Math.PI;

					// Transform into UV space
					Delta = ST - Center;
					UV.x = Delta.Dot( U );
					UV.y = Delta.Dot( V );

 					// Find scaling factor
					int		ScaleX = Math.Max( 0, Math.Min( PROBA_ARRAY_SIZE-1, (int) (PROBA_ARRAY_SIZE * UV.x) ) );
					double	Scale = m_Scales[ScaleAngle,ScaleX];
					UV.y *= Scale;

					_Probas.Accumulate( UV, Normalizer );
				}
			}
		}

		protected void	BuildScales()
		{
			double	ThetaH, ThetaD;
			double	CosThetaL, dCosThetaL, Step;
			double	Length;
			for ( int Angle=0; Angle < 90; Angle++ )
			{
				double	ThetaV = Angle * Math.PI / 180.0;

				double	L = 2.0 * ThetaV / Math.PI;
				Vector2	Center = new Vector2( 0, ThetaV );		// We choose the center to be (0,ThetaV)
				Vector2	U = new Vector2( ThetaV / PROBA_ARRAY_SIZE, -ThetaV / PROBA_ARRAY_SIZE );			// U direction goes down to (ThetaV,0)

				for ( int X=0; X < PROBA_ARRAY_SIZE; X++ )
				{
					ThetaH = Center.x;
					ThetaD = Center.y;
					Length = 0;	// For the moment, we've traveled nowhere!
					for ( int i=0; i < 100; i++ )
					{
						dCosThetaL = ComputeDCosThetaL( ThetaV, ThetaH, ThetaD, out CosThetaL );
						if ( Math.Abs( CosThetaL ) < 1e-6 )
							break;	// It's close enough!

						Step = -CosThetaL / dCosThetaL;
						Step = Math.Sign( Step ) * Math.Min( 0.1*Math.PI, Math.Abs( Step ) );

						ThetaH += Step;
						ThetaD += Step;
						Length += Step;
					}

					// Store scale factor for the segment
					double	Scale = 2.0 * Length / (0.5*Math.PI - ThetaV);
					m_Scales[Angle,X] = Scale;

					Center += U;
				}
			}
		}

		protected double	ComputeCosThetaL( double _ThetaV, double _ThetaH, double _ThetaD )
		{
			double	CosGamma = (Math.Cos( _ThetaH ) - Math.Cos( _ThetaV )*Math.Cos( _ThetaD )) / Math.Max( 1e-6, Math.Sin( _ThetaV )*Math.Sin( _ThetaD ));
			double	CosThetaL = Math.Cos( _ThetaV ) * Math.Cos( 2.0 * _ThetaD ) + Math.Sin( _ThetaV ) * Math.Sin( 2.0 * _ThetaD ) * CosGamma;
			return CosThetaL;
		}

		protected double	ComputeDCosThetaL( double _ThetaV, double _ThetaH, double _ThetaD, out double _ThetaL )
		{
			double	StepSize = 1e-6;
			double	y0 = ComputeCosThetaL( _ThetaV, _ThetaH, _ThetaD );
			double	y1 = ComputeCosThetaL( _ThetaV, _ThetaH+StepSize, _ThetaD+StepSize );
			double	dy_dx = (y1 - y0) / StepSize;
			_ThetaL = y0;

			return dy_dx;
		}

		private void floatTrackbarControlCameraTheta_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			BuildProbabilities( _Sender.Value * Math.PI / 180.0, m_Param );

//			m_Param.m_Probabilities[64,64] = 1;

			displayPanel.Update( m_Param.m_Probabilities );
		}

		private void floatTrackbarControlProbaFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			displayPanel.m_Factor = _Sender.Value;
			displayPanel.Update( m_Param.m_Probabilities );
		}
	}
}
