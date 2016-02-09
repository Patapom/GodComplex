using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

using RendererManaged;

namespace TestMSBSDF
{
	public class	LobeModel : WMath.BFGS.Model {

		#region NESTED TYPES

		public enum		LOBE_TYPE {
			MODIFIED_PHONG,
			BECKMANN,
			GGX,
		}

		public delegate void	ParametersChangedEventHandler( double[] _parameters );

		#endregion

		#region FIELDS

		LOBE_TYPE	m_lobeType;
		float3		m_direction;
		float3		m_centerOfMass;
		bool		m_fitUsingCenterOfMass;
//		double		m_IOR;
		double		m_incomingDirection_CosPhi;
		double		m_incomingDirection_SinPhi;
		double		m_oversizeFactor;

		// The data we want to fit against
		int			W, H;
		double[,]	m_histogramData;

		// The lobe parameters we need to find
		double[]	m_parameters = new double[5];

		double[]	m_constraintMin = new double[5] { 0.0, 1e-4, 1e-6, 1e-3, 0.0 };				// Used to be { 0.0, 1e-4, 1e-3, 1e-6, 0.0 }
		double[]	m_constraintMax = new double[5] { 0.4999 * Math.PI, 1.0, 10.0, 10.0, 1.0 };

		#endregion

		#region PROPERTIES

		public float3	CenterOfMass {
			get { return m_centerOfMass; }
			set { m_centerOfMass = value; }
		}

		public double[]	ConstrainMin { get { return m_constraintMin; } }
		public double[]	ConstrainMax { get { return m_constraintMax; } }

		public event ParametersChangedEventHandler	ParametersChanged;

		#endregion

		#region METHODS

		public LobeModel() {
		}

		/// <summary>
		/// Sets the contraints for each parameter
		/// </summary>
		/// <param name="_parameterIndex"></param>
		/// <param name="_min"></param>
		/// <param name="_max"></param>
		public void		SetConstraint( int _parameterIndex, double _min, double _max ) {
			m_constraintMin[_parameterIndex] = _min;
			m_constraintMax[_parameterIndex] = _max;
		}

		/// <summary>
		/// Initializes the array of data we need to fit with the lobe model
		/// Computes the Center of Mass of the lobe
		/// </summary>
		/// <param name="_texHistogram_CPU"></param>
		/// <param name="_scatteringOrder"></param>
		public void		InitTargetData(	Texture2D _texHistogram_CPU,
										int _scatteringOrder ) {

			int	scattMin = _scatteringOrder-1;		// Because scattering order 1 is actually stored in first slice of the texture array
			int	scattMax = scattMin+1;				// To simulate a single scattering order
//			int	scattMax = MAX_SCATTERING_ORDER;	// To simulate all scattering orders accumulated

			// =========================================================================
			// 1] Readback lobe texture data into an array
			W = _texHistogram_CPU.Width;
			H = _texHistogram_CPU.Height;
			m_histogramData = new double[W,H];

			for ( int scatteringOrder=scattMin; scatteringOrder < scattMax; scatteringOrder++ ) {
				PixelsBuffer	Content = _texHistogram_CPU.Map( 0, scatteringOrder );
				using ( BinaryReader R = Content.OpenStreamRead() )
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
							m_histogramData[X,Y] += W * H * R.ReadSingle();
				Content.CloseStream();
				_texHistogram_CPU.UnMap( 0, scatteringOrder );
			}

			// =========================================================================
			// 2] Compute center of mass from which we'll measure distances
			m_centerOfMass = float3.Zero;
			float3	wsOutgoingDirection = float3.Zero;
			for ( int Y=0; Y < H; Y++ ) {
				// Y = theta bin index = 2.0 * LOBES_COUNT_THETA * pow2( sin( 0.5 * theta ) )
				// We need theta:
				double	theta = 2.0 * Math.Asin( Math.Sqrt( 0.5 * Y / H ) );
				double	cosTheta = Math.Cos( theta );
				double	sinTheta = Math.Sin( theta );

				for ( int X=0; X < W; X++ ) {

					// X = phi bin index = LOBES_COUNT_PHI * X / (2PI)
					// We need phi:
					double	phi = 2.0 * Math.PI * X / W;
					double	cosPhi = Math.Cos( phi );
					double	sinPhi = Math.Sin( phi );

					// Build simulated microfacet reflection direction in macro-surface space
					double	outgoingIntensity = m_histogramData[X,Y];
					wsOutgoingDirection.Set( (float) (outgoingIntensity * cosPhi * sinTheta), (float) (outgoingIntensity * sinPhi * sinTheta), (float) (outgoingIntensity * cosTheta) );

					// Accumulate lobe position
					m_centerOfMass += wsOutgoingDirection;
				}
			}
			m_centerOfMass /= W*H;
		}

		/// <summary>
		/// Initialize the lobe model parameters
		/// </summary>
		/// <param name="_lobeType">Type of lobe to fit</param>
		/// <param name="_incomingDirection">Incoming light direction, pointing TOWARD the surface (Z-up)</param>
		/// <param name="_theta">Initial lobe theta angle</param>
		/// <param name="_roughness">Initial lobe roughness</param>
		/// <param name="_scale">Initial lobe scale</param>
		/// <param name="_flatteningFactor">Initial lobe flattening</param>
		/// <param name="_MaskingImportance">Initial lobe masking/shadowing importance</param>
		/// <param name="_oversizeFactor">Simulation oversize factor. A value of 1 makes the lobe fit exactly, a value higher than one will make the fit lobe smaller than the simulated data, a value lower than one will make the lob larger than the simulated data</param>
		/// <param name="_fitUsingCenterOfMass">If true, the fitting distance is measured against a sphere centered on the Center of Mass rather than the (0,0,0) origin. Precision is usually better when using this option.</param>
		public void		InitLobeData(	LOBE_TYPE _lobeType,
										float3 _incomingDirection,
										double _theta,
										double _roughness,
//										double _IOR,
										double _scale,
										double _flatteningFactor,
										double _maskingImportance,
										double _oversizeFactor,
										bool _fitUsingCenterOfMass ) {


			// =========================================================================
			// 4] Setup initial parameters
			m_lobeType = _lobeType;

			m_direction = _incomingDirection;
			m_direction.z = -m_direction.z;	// Mirror against surface to obtain reflected direction against which we'll compute masking/shadowing of incoming ray

//			m_IOR = _IOR;

			double	incomingPhi = Math.Atan2( m_direction.y, m_direction.x );
			m_incomingDirection_CosPhi = Math.Cos( incomingPhi );
			m_incomingDirection_SinPhi = Math.Sin( incomingPhi );

			m_oversizeFactor = _oversizeFactor;
			m_fitUsingCenterOfMass = _fitUsingCenterOfMass;

			Parameters = new double[] { _theta, _roughness, _scale, _flatteningFactor, _maskingImportance };
		}

		#region Model Implementation

		public double[]		Parameters {
			get { return m_parameters; }
			set {
				m_parameters = value;
				ParametersChanged( m_parameters );	// Notify
			}
		}

		public double Eval( double[] _newParameters ) {

			double	lobeTheta = _newParameters[0];
			double	lobeRoughness = _newParameters[1];
			double	lobeGlobalScale = _newParameters[2];
			double	lobeFlatten = _newParameters[3];
			double	maskingImportance = _newParameters[4];

			double	invLobeFlatten = 1.0 / lobeFlatten;

			// Compute constant masking term due to incoming direction
 			double	maskingIncoming = Masking( m_direction.z, lobeRoughness );		// Masking( incoming )

			// Compute lobe's reflection vector and tangent space using new parameters
			double	cosTheta = Math.Cos( lobeTheta );
			double	sinTheta = Math.Sin( lobeTheta );

			float3	lobe_normal = new float3( (float) (sinTheta * m_incomingDirection_CosPhi), (float) (sinTheta * m_incomingDirection_SinPhi), (float) cosTheta );
// 			if ( !m_fittingReflectedLobe ) {
// 				lobe_normal = Refract( lobe_normal, float3.UnitZ, (float) m_IOR );
// 				lobe_normal.z = -lobe_normal.z;
// 			}

			float3	lobe_tangent = new float3( (float) -m_incomingDirection_SinPhi, (float) m_incomingDirection_CosPhi, 0.0f );	// Always lying in the X^Y plane
			float3	lobe_biTangent = lobe_normal.Cross( lobe_tangent );

			// Compute sum
			double	phi, theta, cosPhi, sinPhi;
			double	outgoingIntensity_Simulated, length;
			double	outgoingIntensity_Analytical, lobeIntensity;
			double	difference;
			float3	wsOutgoingDirection = float3.Zero;
			float3	wsOutgoingDirection2 = float3.Zero;
			float3	lsOutgoingDirection = float3.Zero;
			double	maskingOutGoing = 0.0;

			double	sum = 0.0;
			double	sum_Simulated = 0.0;
			double	sum_Analytical = 0.0;
			double	sqSum_Simulated = 0.0;
			double	sqSum_Analytical = 0.0;
			for ( int Y=0; Y < H; Y++ ) {

				// Y = theta bin index = 2.0 * LOBES_COUNT_THETA * pow2( sin( 0.5 * theta ) )
				// We need theta:
				theta = 2.0 * Math.Asin( Math.Sqrt( 0.5 * Y / H ) );
				cosTheta = Math.Cos( theta );
				sinTheta = Math.Sin( theta );

				for ( int X=0; X < W; X++ ) {

					// X = phi bin index = LOBES_COUNT_PHI * X / (2PI)
					// We need phi:
					phi = 2.0 * Math.PI * X / W;
					cosPhi = Math.Cos( phi );
					sinPhi = Math.Sin( phi );

					// Build simulated microfacet reflection direction in macro-surface space
					outgoingIntensity_Simulated = m_histogramData[X,Y];
					wsOutgoingDirection.Set( (float) (cosPhi * sinTheta), (float) (sinPhi * sinTheta), (float) cosTheta );

					// Compute maksing term due to outgoing direction
					maskingOutGoing = Masking( wsOutgoingDirection.z, lobeRoughness );		// Masking( outgoing )

					// Compute projection of world space direction onto reflected direction
					float	Vz = wsOutgoingDirection.Dot( lobe_normal );

//Vz = Math.Min( 0.99f, Vz );

					float	cosTheta_M = Math.Max( 1e-6f, Vz );

					// Compute the lobe intensity in local space
					lobeIntensity = NDF( cosTheta_M, lobeRoughness );

					double	maskingShadowing = 1.0 + maskingImportance * (maskingIncoming * maskingOutGoing - 1.0);	// = 1 when importance = 0, = masking when importance = 1
 					lobeIntensity *= maskingShadowing;	// * Masking terms

					lobeIntensity *= lobeGlobalScale;

					// Apply additional lobe scaling/flattening along the normal
					// This scale is computed like this:
					//
					// We know that in the shader, local lobe coordinates are transformed into world space by doing:
					//	wsDirection_Scaled = float3( lsDirection.x, lsDirection.y, Scale * lsDirection.z );
					//	float	L = length( wsDirection_Scaled );
					//	wsDirection = wsDirection_Scaled / L;
					//
					// We need to compute L so we write:
					//
					//	lsDirection.x = L * wsDirection.x
					//	lsDirection.y = L * wsDirection.y
					//	lsDirection.z = L * wsDirection.z / Scale
					//
					// We know that:
					//	sqrt( lsDirection.x*lsDirection.x + lsDirection.y*lsDirection.y + lsDirection.z*lsDirection.z ) = 1
					//
					// So:
					//	L * sqrt( wsDirection.x*wsDirection.x + wsDirection.y*wsDirection.y + wsDirection.z*wsDirection.z / (Scale*Scale) ) = 1
					//
					// And:
					//	L = 1 / sqrt( 1 - wsDirection.z*wsDirection.x + wsDirection.z*wsDirection.z / (Scale*Scale) )
					//
					// So finally:
					//	L = 1 / sqrt( 1 + wsDirection.z*wsDirection.z * (1 / (Scale*Scale) - 1) )
					//
					length = 1.0 / Math.Sqrt( 1.0 + Vz*Vz * (invLobeFlatten*invLobeFlatten - 1.0) );

					outgoingIntensity_Analytical = lobeIntensity * length;	// Lobe intensity was estimated in lobe space, account for scaling when converting back in world space

					// Sum the difference between simulated intensity and lobe intensity
					outgoingIntensity_Analytical *= m_oversizeFactor;	// Apply tolerance factor so we're always a bit smaller than the simulated lobe


					if ( m_fitUsingCenterOfMass ) {
						double	difference0 = outgoingIntensity_Simulated - outgoingIntensity_Analytical;

						float3	wsLobePosition_Simulated = (float) outgoingIntensity_Simulated * wsOutgoingDirection;
						float3	wsLobePosition_Analytical = (float) outgoingIntensity_Analytical * wsOutgoingDirection;
						// Subtract center of mass
						wsLobePosition_Simulated -= m_centerOfMass;
						wsLobePosition_Analytical -= m_centerOfMass;
						// Compute new intensities, relative to center of mass
						outgoingIntensity_Simulated = wsLobePosition_Simulated.Length;
						outgoingIntensity_Analytical = wsLobePosition_Analytical.Length;

						double	difference1 = outgoingIntensity_Simulated - outgoingIntensity_Analytical;

						difference = 0.5 * difference0 + 0.5 * difference1;

//						difference *= (wsLobePosition_Simulated - wsLobePosition_Analytical).Length;
//						difference += (wsLobePosition_Simulated - wsLobePosition_Analytical).Length;	// We also add the distance between lobe positions so it goes to the best of the 2 minima!

					} else {
						difference = outgoingIntensity_Simulated - outgoingIntensity_Analytical;
//						difference = outgoingIntensity_Simulated / Math.Max( 1e-6, outgoingIntensity_Analytical ) - 1.0;
//						difference = outgoingIntensity_Analytical / Math.Max( 1e-6, outgoingIntensity_Simulated ) - 1.0;
					}


					sum += difference * difference;

					sum_Simulated += outgoingIntensity_Simulated;
					sum_Analytical += outgoingIntensity_Analytical;
					sqSum_Simulated += outgoingIntensity_Simulated*outgoingIntensity_Simulated;
					sqSum_Analytical += outgoingIntensity_Analytical*outgoingIntensity_Analytical;
				}
			}
			sum /= W * H;	// Not very useful since BFGS won't care but I'm doing it anyway to have some sort of normalized sum, better for us humans

			return sum;
		}

		public void Constrain( double[] _Parameters ) {
			_Parameters[0] = Math.Max( m_constraintMin[0], Math.Min( m_constraintMax[0], _Parameters[0] ) );
			_Parameters[1] = Math.Max( m_constraintMin[1], Math.Min( m_constraintMax[1], _Parameters[1] ) );
			_Parameters[2] = Math.Max( m_constraintMin[2], Math.Min( m_constraintMax[2], _Parameters[2] ) );
			_Parameters[3] = Math.Max( m_constraintMin[3], Math.Min( m_constraintMax[3], _Parameters[3] ) );
			_Parameters[4] = Math.Max( m_constraintMin[4], Math.Min( m_constraintMax[4], _Parameters[4] ) );
		}

		#endregion

		double	Masking( double _cosTheta_V, double _roughness ) {
			switch ( m_lobeType ) {
				case LOBE_TYPE.BECKMANN:	return BeckmannG1( _cosTheta_V, _roughness );
				case LOBE_TYPE.GGX:			return GGXG1( _cosTheta_V, _roughness );
				default:					return PhongG1( _cosTheta_V, _roughness );
			}
		}

		double	NDF( double _cosTheta_M, double _roughness ) {
			switch ( m_lobeType ) {
				case LOBE_TYPE.BECKMANN:	return BeckmannNDF( _cosTheta_M, _roughness );
				case LOBE_TYPE.GGX:			return GGXNDF( _cosTheta_M, _roughness );
				default:					return PhongNDF( _cosTheta_M, _roughness );
			}
		}

		#region Phong Lobe

		double	Roughness2PhongExponent( double _roughness ) {
//				return Math.Pow( 2.0, 10.0 * (1.0 - _roughness) + 1.0 );		// From https://seblagarde.wordpress.com/2011/08/17/hello-world/
			return Math.Pow( 2.0, 10.0 * (1.0 - _roughness) + 0.0 ) - 1.0;	// Actually, we'd like some fatter rough lobes
		}

		// D(m) = a² / (PI * cos(theta_m)^4 * (a² + tan(theta_m)²)²)
		// Simplified into  D(m) = a² / (PI * (cos(theta_m)²*(a²-1) + 1)²)
		double	PhongNDF( double _cosTheta_M, double _roughness ) {
			double	n = Roughness2PhongExponent( _roughness );
			return (n+2)*Math.Pow( _cosTheta_M, n ) / Math.PI;
		}

		// Same as Beckmann but with a modified a bit
		double	PhongG1( double _cosTheta_V, double _roughness ) {
			double	n = Roughness2PhongExponent( _roughness );
			double	sqCosTheta_V = _cosTheta_V * _cosTheta_V;
			double	tanThetaV = Math.Sqrt( (1.0 - sqCosTheta_V) / sqCosTheta_V );
			double	a = Math.Sqrt( 1.0 + 0.5 * n ) / tanThetaV;
			return a < 1.6 ? (3.535 * a + 2.181 * a*a) / (1.0 + 2.276 * a + 2.577 * a*a) : 1.0;
		}

		#endregion

		#region Beckmann Lobe

		// From Walter 2007
		// D(m) = exp( -tan( theta_m )² / a² ) / (PI * a² * cos(theta_m)^4)
		double	BeckmannNDF( double _cosTheta_M, double _roughness ) {
			double	sqCosTheta_M = _cosTheta_M * _cosTheta_M;
			double	a2 = _roughness*_roughness;
			return Math.Exp( -(1.0 - sqCosTheta_M) / (sqCosTheta_M * a2) ) / (Math.PI * a2 * sqCosTheta_M*sqCosTheta_M);
		}

		// Masking G1(v,m) = 2 / (1 + erf( 1/(a * tan(theta_v)) ) + exp(-a²) / (a*sqrt(PI)))
		double	BeckmannG1( double _cosTheta_V, double _roughness ) {
			double	sqCosTheta_V = _cosTheta_V * _cosTheta_V;
			double	tanThetaV = Math.Sqrt( (1.0 - sqCosTheta_V) / sqCosTheta_V );
			double	a = 1.0 / (_roughness * tanThetaV);

			#if true	// Simplified numeric version
				return a < 1.6 ? (3.535 * a + 2.181 * a*a) / (1.0 + 2.276 * a + 2.577 * a*a) : 1.0;
			#else	// Analytical
				return 2.0 / (1.0 + erf( a ) + exp( -a*a ) / (a * SQRTPI));
			#endif
		}

		#endregion

		#region GGX Lobe

		// D(m) = a² / (PI * cos(theta_m)^4 * (a² + tan(theta_m)²)²)
		// Simplified into  D(m) = a² / (PI * (cos(theta_m)²*(a²-1) + 1)²)
		double	GGXNDF( double _cosTheta_M, double _roughness ) {
			double	sqCosTheta_M = _cosTheta_M * _cosTheta_M;
			double	a2 = _roughness*_roughness;
//				return a2 / (Math.PI * sqCosTheta_M*sqCosTheta_M * pow2( a2 + (1.0-sqCosTheta_M)/sqCosTheta_M ));
			return a2 / (Math.PI * Math.Pow( sqCosTheta_M * (a2-1.0) + 1.0, 2.0 ));
		}

		// Masking G1(v,m) = 2 / (1 + sqrt( 1 + a² * tan(theta_v)² ))
		// Simplified into G1(v,m) = 2*cos(theta_v) / (1 + sqrt( cos(theta_v)² * (1-a²) + a² ))
		double	GGXG1( double _cosTheta_V, double _roughness ) {
			double	sqCosTheta_V = _cosTheta_V * _cosTheta_V;
			double	a2 = _roughness*_roughness;
			return 2.0 * _cosTheta_V / (1.0 + Math.Sqrt( sqCosTheta_V * (1.0 - a2) + a2 ));
		}

		#endregion

		#endregion
	}
}
