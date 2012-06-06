using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using WMath;

namespace ZonalHarmonicsFittingTest
{
	public partial class Form1 : Form
	{
		#region CONSTANTS

		protected const int		SH_ORDER = 6;
		protected const int		ZH_LOBES_COUNT = 4;

		// ZH Mapping data
		protected const int		INITIAL_DIRECTIONS_HEMISPHERE_SUBDIVISIONS_COUNT	= 10;
		protected const int		FUNCTION_EVALUATION_SPHERE_SUBDIVISIONS_COUNT		= 50;
		protected const double	BFGS_CONVERGENCE_TOLERANCE							= 1e-3;		// Don't exit unless we reach below this threshold...

		#endregion

		#region FIELDS

		// ZH Rotation test
		protected double[]		m_CoeffsZH = new double[SH_ORDER];
		protected double[]		m_RotatedCoeffsZH = new double[SH_ORDER*SH_ORDER];

		// SH Mapping test
		protected double[]		m_CoeffsSH = new double[SH_ORDER*SH_ORDER];
		protected double[][]	m_MappedCoeffsZH = new double[ZH_LOBES_COUNT][];
		protected Vector2D[]	m_MappedAxesZH = new Vector2D[ZH_LOBES_COUNT];
		protected double[]		m_MappedCoeffsSH = new double[SH_ORDER*SH_ORDER];
		protected double[]		m_RotatedMappedCoeffsSH = new double[SH_ORDER*SH_ORDER];

		#endregion

		#region METHODS

		public Form1()
		{
			InitializeComponent();
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			Text = "ZH Tests - SH Order = " + SH_ORDER + " - ZH Lobes Count = " + ZH_LOBES_COUNT;

// 			double	Theta = 0.0;
// 			double	Phi = 0.0;
// 			SphericalHarmonics.SHFunctions.CartesianToSpherical( new Vector( 0, 1, 0 ), out Theta, out Phi );
// 			Vector	Check1 = SphericalHarmonics.SHFunctions.SphericalToCartesian( Theta, Phi );
// 			SphericalHarmonics.SHFunctions.CartesianToSpherical( new Vector( 1, 0, 0 ), out Theta, out Phi );
// 			Vector	Check2 = SphericalHarmonics.SHFunctions.SphericalToCartesian( Theta, Phi );
// 			SphericalHarmonics.SHFunctions.CartesianToSpherical( new Vector( 0, 0, 1 ), out Theta, out Phi );
// 			Vector	Check3 = SphericalHarmonics.SHFunctions.SphericalToCartesian( Theta, Phi );
// 			SphericalHarmonics.SHFunctions.CartesianToSpherical( new Vector( -1, 0, 0 ), out Theta, out Phi );
// 			Vector	Check4 = SphericalHarmonics.SHFunctions.SphericalToCartesian( Theta, Phi );
// 			SphericalHarmonics.SHFunctions.CartesianToSpherical( new Vector( 0, 0, -1 ), out Theta, out Phi );
// 			Vector	Check5 = SphericalHarmonics.SHFunctions.SphericalToCartesian( Theta, Phi );
// 			SphericalHarmonics.SHFunctions.CartesianToSpherical( new Vector( -1, 1, 1 ).Normalize(), out Theta, out Phi );
// 			Vector	Check6 = SphericalHarmonics.SHFunctions.SphericalToCartesian( Theta, Phi );

			//////////////////////////////////////////////////////////////////////////
			// Build the original ZH coefficients
				// ----- Simple order 1 lobe on the Y axis -----
// 			for ( int CoeffIndex=0; CoeffIndex < SH_ORDER; CoeffIndex++ )
// 				m_CoeffsZH[CoeffIndex] = 0.0;
// 
// 			m_CoeffsZH[1] = 1.0;

				// ----- Arbitrary function -----
			SphericalHarmonics.SHFunctions.EncodeIntoZH( m_CoeffsZH, 1000, 1, new SphericalHarmonics.SHFunctions.EvaluateFunctionZH( FunctionToEvaluateZH ) );

			// Rotate the ZH so they lie on an arbitrary axis
			SphericalHarmonics.SHFunctions.ComputeRotatedZHCoefficients( m_CoeffsZH, new Vector( 1, 1, 1 ).Normalize(), m_RotatedCoeffsZH );


			//////////////////////////////////////////////////////////////////////////
			// Build the original SH coefficients

				// ----- Arbitrary function -----
			SphericalHarmonics.SHFunctions.EncodeIntoSH( m_CoeffsSH, 100, 200, 1, SH_ORDER, new SphericalHarmonics.SHFunctions.EvaluateFunctionSH( FunctionToEvaluateSH ) );






// 			//--------- DEBUG
// 			System.IO.FileInfo		Pipo = new System.IO.FileInfo( "Lobe0.Result" );
// 			System.IO.FileStream	Stream = Pipo.OpenRead();
// 			System.IO.BinaryReader	Reader = new System.IO.BinaryReader( Stream );
// 			for ( int CoeffIndex=0; CoeffIndex < SH_ORDER*SH_ORDER; CoeffIndex++ )
// 				m_CoeffsSH[CoeffIndex] = Reader.ReadDouble();
// 			Stream.Close();
// 			//--------- DEBUG






// 			// Rotate the ZH so they lie on the X axis
// 			SphericalHarmonics.SHFunctions.ComputeRotatedZHCoefficients( m_CoeffsZH, new Vector( 1, 0, 0 ), m_RotatedCoeffsZH );
// 
// 			// Rotate the ZH so they lie on the Z axis
// 			SphericalHarmonics.SHFunctions.ComputeRotatedZHCoefficients( m_CoeffsZH, new Vector( 0, 0, 1 ), m_RotatedCoeffsSH1 );


			// Rebuild spheres
			sphereViewOriginalZonalHarmonics.RebuildSpheres( new SphereView.GetValue( GetValueZonalHarmonic ) );
 			sphereViewRotatedZonalHarmonics.RebuildSpheres( new SphereView.GetValue( GetValueRotatedZonalHarmonic ) );
			sphereViewOriginalFunction.RebuildSpheres( new SphereView.GetValue( GetValueOriginalFunction ) );
 			sphereViewEvaluatedSHEncoding.RebuildSpheres( new SphereView.GetValue( GetValueEvaluatedSHEncoding ) );
		}

		// Evaluates a dummy function to encode into ZH
		protected double	FunctionToEvaluateZH( double _Theta )
		{
			return	4.0 * Math.Pow( Math.Max( 0.0, Math.Cos( _Theta ) ), 4.0 );
		}

		// Evaluates a dummy function to encode into SH
		protected double	FunctionToEvaluateSH( double _Theta, double _Phi )
		{
// 			return	Math.Max( 0.0, 5.0 * Math.Cos( _Theta ) - 4.0 ) +
// 					Math.Max( 0.0, -4.0 * Math.Cos( _Theta - Math.PI ) * Math.Sin( _Phi - 2.5 ) - 3.0 );

			return	0.5 * (	Math.Cos( 3 * _Theta ) * Math.Sin( 2 * _Phi ) -
							0.5 * (2.0 - Math.Sin( 4.0 * _Theta )) * Math.Cos( 2 * _Phi ) +
							0.25 * (0.7 + Math.Sin( 5 * _Phi )) * (1.5 + Math.Sin( 9 * _Theta )) +
							0.4 * (0.3 + Math.Cos( 9 * _Phi )) * (0.5 + Math.Cos( 7 * _Theta )));
		}

		//////////////////////////////////////////////////////////////////////////
		// Zonal Harmonics Simple Display (TOP PANELS)
		protected float		GetValueZonalHarmonic( float _x, float _y, float _z )
		{
			return	(float) SphericalHarmonics.SHFunctions.EvaluateZH( m_CoeffsZH, new Vector( _x, _y, _z  ) );
		}

		protected float		GetValueRotatedZonalHarmonic( float _x, float _y, float _z )
		{
			return	(float) SphericalHarmonics.SHFunctions.EvaluateSH( m_RotatedCoeffsZH, new Vector( _x, _y, _z  ), SH_ORDER );
		}


		//////////////////////////////////////////////////////////////////////////
		// Zonal Harmonics Mapping Display (BOTTOM PANELS)
		protected float		GetValueOriginalFunction( float _x, float _y, float _z )
		{
			double	Theta = 0.0;
			double	Phi = 0.0;
			SphericalHarmonics.SHFunctions.CartesianToSpherical( new Vector( _x, _y, _z ), out Theta, out Phi );

			return	(float) FunctionToEvaluateSH( Theta, Phi );
		}

		protected float		GetValueEvaluatedSHEncoding( float _x, float _y, float _z )
		{
			return	(float) SphericalHarmonics.SHFunctions.EvaluateSH( m_CoeffsSH, new Vector( _x, _y, _z  ), SH_ORDER );
		}

		protected float		GetValueEvaluatedMappedZHEncoding( float _x, float _y, float _z )
		{
			return	(float) SphericalHarmonics.SHFunctions.EvaluateSH( m_MappedCoeffsSH, new Vector( _x, _y, _z  ), SH_ORDER );
		}

		protected float		GetValueEvaluatedRotatedMappedZHEncoding( float _x, float _y, float _z )
		{
			return	(float) SphericalHarmonics.SHFunctions.EvaluateSH( m_RotatedMappedCoeffsSH, new Vector( _x, _y, _z  ), SH_ORDER );
		}

		#endregion

		#region EVENTS

		private void sphereView4_MouseUp( object sender, MouseEventArgs e )
		{
			// Determine the position on the sphere
			Vector	AlignAxis = null;
			if ( e.X < sphereViewRotatedZonalHarmonics.Width / 2 )
			{	// Front
				float	fDx = e.X / (.25f * sphereViewRotatedZonalHarmonics.Width) - 1.0f;
				float	fDy = 1.0f - e.Y / (.5f * sphereViewRotatedZonalHarmonics.Height);
				float	fSDistance = fDx * fDx + fDy * fDy;
				if ( fSDistance > 1.0f )
					return;

				AlignAxis = new Vector( fDx, fDy, (float) Math.Sqrt( 1.0f - fSDistance ) );
			}
			else
			{	// Front
				float	fDx = e.X / (.25f * sphereViewRotatedZonalHarmonics.Width) - 3.0f;
				float	fDy = 1.0f - e.Y / (.5f * sphereViewRotatedZonalHarmonics.Height);
				float	fSDistance = fDx * fDx + fDy * fDy;
				if ( fSDistance > 1.0f )
					return;

				AlignAxis = new Vector( fDx, fDy, -(float) Math.Sqrt( 1.0f - fSDistance ) );
			}

			if ( AlignAxis == null )
				return;

			// Rotate the ZH so they lie on an arbitrary axis
			SphericalHarmonics.SHFunctions.ComputeRotatedZHCoefficients( m_CoeffsZH, AlignAxis, m_RotatedCoeffsZH );

			sphereViewRotatedZonalHarmonics.RebuildSpheres( new SphereView.GetValue( GetValueRotatedZonalHarmonic ) );
			sphereViewRotatedZonalHarmonics.Refresh();
		}

		private void sphereViewRotatedZHEncodedFunction_MouseUp( object sender, MouseEventArgs e )
		{
			// Determine the position on the sphere
			Vector	AlignAxis = null;
			if ( e.X < sphereViewRotatedZHEncodedFunction.Width / 2 )
			{	// Front
				float	fDx = e.X / (.25f * sphereViewRotatedZHEncodedFunction.Width) - 1.0f;
				float	fDy = 1.0f - e.Y / (.5f * sphereViewRotatedZHEncodedFunction.Height);
				float	fSDistance = fDx * fDx + fDy * fDy;
				if ( fSDistance > 1.0f )
					return;

				AlignAxis = new Vector( fDx, fDy, (float) Math.Sqrt( 1.0f - fSDistance ) );
			}
			else
			{	// Front
				float	fDx = e.X / (.25f * sphereViewRotatedZHEncodedFunction.Width) - 3.0f;
				float	fDy = 1.0f - e.Y / (.5f * sphereViewRotatedZHEncodedFunction.Height);
				float	fSDistance = fDx * fDx + fDy * fDy;
				if ( fSDistance > 1.0f )
					return;

				AlignAxis = new Vector( fDx, fDy, -(float) Math.Sqrt( 1.0f - fSDistance ) );
			}

			if ( AlignAxis == null )
				return;

			// Compute rotation matrix
			Matrix3x3	Rotation = Matrix3x3.ComputeRotationMatrix( new Vector( 0, 1, 0 ), AlignAxis );

			// Rotate the ZH so they lie on an arbitrary axis
			m_RotatedMappedCoeffsSH = new double[SH_ORDER * SH_ORDER];
			for ( int LobeIndex=0; LobeIndex < ZH_LOBES_COUNT; LobeIndex++ )
			{
				Vector		RotatedLobeAxis = SphericalHarmonics.SHFunctions.SphericalToCartesian( m_MappedAxesZH[LobeIndex].x, m_MappedAxesZH[LobeIndex].y ) * Rotation;

				double[]	RotatedCoeffsZH = new double[SH_ORDER * SH_ORDER];
				SphericalHarmonics.SHFunctions.ComputeRotatedZHCoefficients( m_MappedCoeffsZH[LobeIndex], RotatedLobeAxis, RotatedCoeffsZH );

				for ( int CoefficientIndex=0; CoefficientIndex < SH_ORDER * SH_ORDER; CoefficientIndex++ )
					m_RotatedMappedCoeffsSH[CoefficientIndex] += RotatedCoeffsZH[CoefficientIndex];
			}

			sphereViewRotatedZHEncodedFunction.RebuildSpheres( new SphereView.GetValue( GetValueEvaluatedRotatedMappedZHEncoding ) );
			sphereViewRotatedZHEncodedFunction.Refresh();
		}


		//////////////////////////////////////////////////////////////////////////
		// ZH Mapping

		private void buttonMap_Click( object sender, EventArgs e )
		{
			buttonMap.Enabled = false;

			// Initialize arrays
			for ( int LobeIndex=0; LobeIndex < ZH_LOBES_COUNT; LobeIndex++ )
			{
				m_MappedCoeffsZH[LobeIndex] = new double[SH_ORDER];
				m_MappedAxesZH[LobeIndex] = new Vector2D( 0, 0 );
			}

			// Perform mapping
			double[]	RMS = new double[ZH_LOBES_COUNT];
			SphericalHarmonics.SHFunctions.MapSHIntoZH( m_CoeffsSH, SH_ORDER, m_MappedAxesZH, m_MappedCoeffsZH, INITIAL_DIRECTIONS_HEMISPHERE_SUBDIVISIONS_COUNT, FUNCTION_EVALUATION_SPHERE_SUBDIVISIONS_COUNT, BFGS_CONVERGENCE_TOLERANCE, out RMS, new SphericalHarmonics.SHFunctions.ZHMappingFeedback( MappingFeedback ) );
			progressBar.Value = progressBar.Minimum;

			// Recombine into an SH
			for ( int CoefficientIndex=0; CoefficientIndex < SH_ORDER * SH_ORDER; CoefficientIndex++ )
				m_MappedCoeffsSH[CoefficientIndex] = 0.0;
			for ( int LobeIndex=0; LobeIndex < ZH_LOBES_COUNT; LobeIndex++ )
			{
				// Rotate the lobe to its axis
				double[]	RotatedLobeSHCoefficients = new double[SH_ORDER*SH_ORDER];
				SphericalHarmonics.SHFunctions.ComputeRotatedZHCoefficients( m_MappedCoeffsZH[LobeIndex], SphericalHarmonics.SHFunctions.SphericalToCartesian( m_MappedAxesZH[LobeIndex].x, m_MappedAxesZH[LobeIndex].y ), RotatedLobeSHCoefficients );

				// Accumulate into final SH
				for ( int CoefficientIndex=0; CoefficientIndex < SH_ORDER * SH_ORDER; CoefficientIndex++ )
					m_MappedCoeffsSH[CoefficientIndex] += RotatedLobeSHCoefficients[CoefficientIndex];
			}

			// Rebuild the sphere
			sphereViewZHEncodedFunction.RebuildSpheres( new SphereView.GetValue( GetValueEvaluatedMappedZHEncoding ) );
			sphereViewZHEncodedFunction.Refresh();


			//////////////////////////////////////////////////////////////////////////
			// Compute RMS
			SphericalHarmonics.SHSamplesCollection	Samples = new SphericalHarmonics.SHSamplesCollection();
													Samples.Initialize( SH_ORDER, 200 );
			double	SumError = 0.0;
			foreach ( SphericalHarmonics.SHSamplesCollection.SHSample Sample in Samples )
			{
				double	GoalValue = 0.0;
				double	EstimateValue = 0.0;
				for ( int CoefficientIndex=0; CoefficientIndex < SH_ORDER * SH_ORDER; CoefficientIndex++ )
				{
					GoalValue += Sample.m_SHFactors[CoefficientIndex] * m_CoeffsSH[CoefficientIndex];
					EstimateValue += Sample.m_SHFactors[CoefficientIndex] * m_MappedCoeffsSH[CoefficientIndex];
				}

				SumError += (EstimateValue - GoalValue) * (EstimateValue - GoalValue);
			}

			SumError /= Samples.SamplesCount;

			// Compute error between coefficients
			double	SumCoeffsError = 0.0;
			for ( int CoefficientIndex=0; CoefficientIndex < SH_ORDER * SH_ORDER; CoefficientIndex++ )
				SumCoeffsError += (m_CoeffsSH[CoefficientIndex] - m_MappedCoeffsSH[CoefficientIndex]) * (m_CoeffsSH[CoefficientIndex] - m_MappedCoeffsSH[CoefficientIndex]);
			SumCoeffsError /= SH_ORDER * SH_ORDER;


//			labelRMSError.Text = SumError.ToString( "G8" );
			labelRMSError.Text = Math.Sqrt( SumCoeffsError ).ToString( "G8" ) + " - Square Difference = " + SumError.ToString( "G4" );
			//////////////////////////////////////////////////////////////////////////


			sphereViewRotatedZHEncodedFunction_MouseUp( null, new MouseEventArgs( MouseButtons.Left, 1, sphereViewRotatedZHEncodedFunction.Width / 2 - sphereViewRotatedZHEncodedFunction.Height / 3, sphereViewRotatedZHEncodedFunction.Height / 3, 0 ) );

			buttonMap.Enabled = true;
		}

		protected void	MappingFeedback( float _fProgress )
		{
			progressBar.Value = progressBar.Minimum + (int) ((progressBar.Maximum - progressBar.Minimum) * _fProgress);
			progressBar.Refresh();
			Application.DoEvents();
		}

		#endregion
	}
}