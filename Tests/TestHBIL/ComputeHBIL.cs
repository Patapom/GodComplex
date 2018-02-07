#define SAMPLE_NEIGHBOR_RADIANCE
using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

using Renderer;
using ImageUtility;
using SharpMath;

namespace TestHBIL
{
	public class ComputeHBIL : IDisposable {

		#region CONSTANTS

		const float			TAN_HALF_FOV = 0.6f;
		const float			Z_FAR = 100.0f;
		const float			GATHER_SPHERE_MAX_RADIUS_P = 100;

		// Not const so we can change it while debugging
//		uint		MAX_ANGLES = 8;
		uint		MAX_ANGLES = 1;
		uint		MAX_SAMPLES = 32;

		#endregion

		#region FIELDS

		private Device		m_device;

		private Texture2D	m_texDepth_Staging;
		private Texture2D	m_texNormal_Staging;
		private Texture2D	m_texIrradiance_Staging;

		private float4[][,]	m_arrayDepth;
		private float4[][,]	m_arrayNormal;
		private float4[][,]	m_arrayIrradiance;

		private float3		m_wsConePosition = float3.Zero;
		private float3		m_wsConeDirection = float3.UnitY;
		private float		m_averageConeAngle = 0.25f * Mathf.PI;
		private float		m_stdDeviation = 0.0f;

		private float2		m_resolution;
		private float		m_gatherSphereMaxRadius_m = 4.0f;
		private float4x4	m_camera2World = float4x4.Identity;
		private float4x4	m_world2Proj = float4x4.Identity;

		// Computed result
		private float4		m_sumIrradiance;
		private float4		m_bentCone;

		#endregion

		#region PROPERTIES

		public float	GatherSphereMaxRadius_m	{
			get { return m_gatherSphereMaxRadius_m; }
			set { m_gatherSphereMaxRadius_m = value; }
		}

		public float4x4	Camera2World {
			get { return m_camera2World; }
			set { m_camera2World = value; }
		}

		public float4x4	World2Proj {
			get { return m_world2Proj; }
			set { m_world2Proj = value; }
		}

		public float3	wsConePosition	{ get { return m_wsConePosition; } }
		public float3	wsConeDirection	{ get { return m_wsConeDirection; } }
		public float	averageConeAngle{ get { return m_averageConeAngle; } }
		public float	stdDeviation	{ get { return m_stdDeviation; } }

		#endregion

		#region METHODS

		public ComputeHBIL( Device _device ) {
			m_device = _device;
		}

		public void		Setup( Texture2D _texDepth, Texture2D _texNormal, Texture2D _texIrradiance ) {
			if ( m_texDepth_Staging == null ) {
				m_texDepth_Staging = new Texture2D( m_device, _texDepth.Width, _texDepth.Height, (int) _texDepth.ArraySize, _texDepth.MipLevelsCount, _texDepth.PixelFormat, _texDepth.ComponentFormat, true, false, null );
			}
			if ( m_texNormal_Staging == null ) {
				m_texNormal_Staging = new Texture2D( m_device, _texNormal.Width, _texNormal.Height, (int) _texNormal.ArraySize, _texNormal.MipLevelsCount, _texNormal.PixelFormat, _texNormal.ComponentFormat, true, false, null );
			}
			if ( m_texIrradiance_Staging == null ) {
				m_texIrradiance_Staging = new Texture2D( m_device, _texIrradiance.Width, _texIrradiance.Height, (int) _texIrradiance.ArraySize, _texIrradiance.MipLevelsCount, _texIrradiance.PixelFormat, _texIrradiance.ComponentFormat, true, false, null );
			}

			m_texDepth_Staging.CopyFrom( _texDepth );
			m_texNormal_Staging.CopyFrom( _texNormal );
			m_texIrradiance_Staging.CopyFrom( _texIrradiance );

			// Read back content
			ReadBack( m_texDepth_Staging, out m_arrayDepth );
			ReadBack( m_texNormal_Staging, out m_arrayNormal );
			ReadBack( m_texIrradiance_Staging, out m_arrayIrradiance );

			m_resolution = new float2( m_texDepth_Staging.Width, m_texDepth_Staging.Height );
		}

		public void		Compute( uint _X, uint _Y ) {
			float2	__Position = new float2( 0.5f + _X, 0.5f + _Y );
			float2	UV = __Position / m_resolution;
			uint	pixelPositionX = (uint) Mathf.Floor( __Position.x );
			uint	pixelPositionY = (uint) Mathf.Floor( __Position.y );
			float	noise = 0.0f;//_tex_blueNoise[pixelPosition & 0x3F];


PerformIntegrationTest();


			// Setup camera ray
			float3	csView = BuildCameraRay( UV );
			float	Z2Distance = csView.Length;
					csView /= Z2Distance;
			float3	wsView = (new float4( csView, 0.0f ) * m_camera2World ).xyz;

			// Read back depth, normal & central radiance value from last frame
			float	Z = FetchDepth( __Position, 0.0f );

			Z -= 1e-2f;	// Prevent acnea by offseting the central depth closer

//			float	distance = Z * Z2Distance;
			float3	wsNormal = m_arrayNormal[0][pixelPositionX,pixelPositionY].xyz.Normalized;

			// Read back last frame's radiance value that we always can use as a default for neighbor areas
			float3	centralRadiance = m_arrayIrradiance[0][pixelPositionX,pixelPositionY].xyz;

			// Compute local camera-space
			float3	wsPos = m_camera2World[3].xyz + Z * Z2Distance * wsView;
			float3	wsRight = wsView.Cross( m_camera2World[1].xyz ).Normalized;
			float3	wsUp = wsRight.Cross( wsView );
			float3	wsAt = -wsView;

			float4x4	localCamera2World = new float4x4( new float4( wsRight, 0 ), new float4( wsUp, 0 ), new float4( wsAt, 0 ), new float4( wsPos, 1 ) );

			// Compute local camera-space normal
			float3	N = new float3( wsNormal.Dot( wsRight ), wsNormal.Dot( wsUp ), wsNormal.Dot( wsAt ) );
					N.z = Math.Max( 1e-4f, N.z );	// Make sure it's never 0!

//			float3	T, B;
//			BuildOrthonormalBasis( N, T, B );

			// Compute screen radius of gather sphere
			float	screenSize_m = 2.0f * Z * TAN_HALF_FOV;	// Vertical size of the screen in meters when extended to distance Z
			float	sphereRadius_pixels = m_resolution.y * m_gatherSphereMaxRadius_m / screenSize_m;
					sphereRadius_pixels = Mathf.Min( GATHER_SPHERE_MAX_RADIUS_P, sphereRadius_pixels );									// Prevent it to grow larger than our fixed limit
			float	radiusStepSize_pixels = Mathf.Max( 1.0f, sphereRadius_pixels / MAX_SAMPLES );										// This gives us our radial step size in pixels
			uint	samplesCount = Mathf.Clamp( (uint) Mathf.Ceiling( sphereRadius_pixels / radiusStepSize_pixels ), 1, MAX_SAMPLES );	// Reduce samples count if possible
			float	radiusStepSize_meters = sphereRadius_pixels * screenSize_m / (samplesCount * m_resolution.y);						// This gives us our radial step size in meters

			// Start gathering radiance and bent normal by subdividing the screen-space disk around our pixel into Z slices
			float4	GATHER_DEBUG = float4.Zero;
			float3	sumIrradiance = float3.Zero;
			float3	csAverageBentNormal = float3.Zero;
			float	averageConeAngle = 0.0f;
			float	varianceConeAngle = 0.0f;
			for ( uint angleIndex=0; angleIndex < MAX_ANGLES; angleIndex++ ) {
				float	phi = (angleIndex + noise) * Mathf.PI / MAX_ANGLES;

//phi = 0.0f;

				float2	csDirection;
				csDirection.x = Mathf.Cos( phi );
				csDirection.y = Mathf.Sin( phi );

				// Gather irradiance and average cone direction for that slice
				float3	csBentNormal;
				float2	coneAngles;
				sumIrradiance += GatherIrradiance( csDirection, localCamera2World, N, radiusStepSize_meters, samplesCount, centralRadiance, out csBentNormal, out coneAngles, ref GATHER_DEBUG );

				csAverageBentNormal += csBentNormal;

				// We're using running variance computation from https://www.johndcook.com/blog/standard_deviation/
				//	Avg(N) = Avg(N-1) + [V(N) - Avg(N-1)] / N
				//	S(N) = S(N-1) + [V(N) - Avg(N-1)] * [V(N) - Avg(N)]
				// And variance = S(finalN) / (finalN-1)
				//
				float	previousAverageConeAngle = averageConeAngle;
				averageConeAngle += (coneAngles.x - averageConeAngle) / (2*angleIndex+1);
				varianceConeAngle += (coneAngles.x - previousAverageConeAngle) * (coneAngles.x - averageConeAngle);

				previousAverageConeAngle = averageConeAngle;
				averageConeAngle += (coneAngles.y - averageConeAngle) / (2*angleIndex+2);
				varianceConeAngle += (coneAngles.y - previousAverageConeAngle) * (coneAngles.y - averageConeAngle);
			}

			// Finalize bent cone & irradiance
			varianceConeAngle /= 2.0f*MAX_ANGLES - 1.0f;
			csAverageBentNormal = csAverageBentNormal.Normalized;
			float	stdDeviation = Mathf.Sqrt( varianceConeAngle );

			sumIrradiance *= Mathf.PI / MAX_ANGLES;

			sumIrradiance.Max( float3.Zero );

			//////////////////////////////////////////////////////////////////////////
			// Finalize results
			m_sumIrradiance = new float4( sumIrradiance, 0 );
			m_bentCone = new float4( Mathf.Max( 0.01f, Mathf.Cos( averageConeAngle ) ) * csAverageBentNormal, 1.0f - stdDeviation / (0.5f * Mathf.PI) );


float3	DEBUG_VALUE = new float3( 1,0,1 );
DEBUG_VALUE = csAverageBentNormal;
DEBUG_VALUE = csAverageBentNormal.x * wsRight - csAverageBentNormal.y * wsUp - csAverageBentNormal.z * wsAt;	// World-space normal
//DEBUG_VALUE = cos( averageConeAngle );
//DEBUG_VALUE = dot( ssAverageBentNormal, N );
//DEBUG_VALUE = 0.01 * Z;
//DEBUG_VALUE = sphereRadius_pixels / GATHER_SPHERE_MAX_RADIUS_P;
//DEBUG_VALUE = 0.1 * (radiusStepSize_pixels-1);
//DEBUG_VALUE = 0.5 * float(samplesCount) / MAX_SAMPLES;
//DEBUG_VALUE = varianceConeAngle;
//DEBUG_VALUE = stdDeviation;
//DEBUG_VALUE = float3( GATHER_DEBUG.xy, 0 );
//DEBUG_VALUE = float3( GATHER_DEBUG.zw, 0 );
DEBUG_VALUE = GATHER_DEBUG.xyz;
//DEBUG_VALUE = N;
//m_bentCone = float4( DEBUG_VALUE, 1 );

			//////////////////////////////////////////////////////////////////////////
			// Finalize bent code debug info
			m_wsConePosition = wsPos;
			m_wsConeDirection = csAverageBentNormal.x * wsRight + csAverageBentNormal.y * wsUp + csAverageBentNormal.z * wsAt;
//m_wsConeDirection = wsNormal;
			m_averageConeAngle = averageConeAngle;
			m_stdDeviation = stdDeviation;
		}

		float3	GatherIrradiance( float2 _csDirection, float4x4 _localCamera2World, float3 _csNormal, float _stepSize_meters, uint _stepsCount, float3 _centralRadiance, out float3 _csBentNormal, out float2 _coneAngles, ref float4 _DEBUG ) {

			// Pre-compute factors for the integrals
			float2	integralFactors_Front = ComputeIntegralFactors( _csDirection, _csNormal );
			float2	integralFactors_Back = ComputeIntegralFactors( -_csDirection, _csNormal );

			// Compute initial cos(angle) for front & back horizons
			// We do that by projecting the screen-space direction ssDirection onto the tangent plane given by the normal
			//	then the cosine of the angle from the Z axis is simply given by the Pythagorean theorem:
			//                             P
			//			   N\  |Z		  -*-
			//				 \ |	  ---  ^
			//				  \|  ---      |
			//             --- *..........>+ ssDirection
			//        --- 
			//
			float	hitDistance_Front = -_csDirection.Dot( _csNormal.xy ) / _csNormal.z;
			float	maxCosTheta_Front = hitDistance_Front / Mathf.Sqrt( hitDistance_Front*hitDistance_Front + 1.0f );
			float	maxCosTheta_Back = -maxCosTheta_Front;	// Back cosine is simply the mirror value

			// Gather irradiance from front & back directions while updating the horizon angles at the same time
			float3	sumRadiance = float3.Zero;
			float3	previousRadiance_Front = _centralRadiance;
			float3	previousRadianceBack = _centralRadiance;
/*
//			float2	radius = float2.Zero;
			float2	csStep = _stepSize_meters * _csDirection;
			float2	csPosition_Front = float2.Zero;
			float2	csPosition_Back = float2.Zero;
			for ( uint stepIndex=0; stepIndex < _stepsCount; stepIndex++ ) {
//				radius += _stepSize_meters;
				csPosition_Front += csStep;
				csPosition_Back -= csStep;

//				float2	mipLevel = ComputeMipLevel( radius, _radialStepSizes );
float2	mipLevel = float2.Zero;

				sumRadiance += SampleIrradiance( csPosition_Front, _localCamera2World, mipLevel, integralFactors_Front, ref previousRadiance_Front, ref maxCosTheta_Front );
				sumRadiance += SampleIrradiance( csPosition_Back, _localCamera2World, mipLevel, integralFactors_Back, ref previousRadianceBack, ref maxCosTheta_Back );
			}
//*/
			// Accumulate bent normal direction by rebuilding and averaging the front & back horizon vectors
			#if USE_NUMERICAL_INTEGRATION
				// Half brute force where we perform the integration numerically as a sum...
				// This solution is prefered to the analytical integral that shows some precision artefacts unfortunately...
				//
				float	thetaFront = acos( maxCosTheta_Front );
				float	thetaBack = -acos( maxCosTheta_Back );

				_csBentNormal = 0.001 * _N;
				for ( uint i=0; i < USE_NUMERICAL_INTEGRATION; i++ ) {
					float	theta = lerp( thetaBack, thetaFront, (i+0.5) / USE_NUMERICAL_INTEGRATION );
					float	sinTheta, cosTheta;
					sincos( theta, sinTheta, cosTheta );
					float3	ssUnOccludedDirection = float3( sinTheta * _csDirection, cosTheta );

					float	cosAlpha = saturate( dot( ssUnOccludedDirection, _N ) );

					float	weight = cosAlpha * abs(sinTheta);		// cos(alpha) * sin(theta).dTheta  (be very careful to take abs(sin(theta)) because our theta crosses the pole and becomes negative here!)
					_csBentNormal += weight * ssUnOccludedDirection;
				}

				float	dTheta = (thetaFront - thetaBack) / USE_NUMERICAL_INTEGRATION;
				_csBentNormal *= dTheta;
			#else
				// Analytical solution
				float	cosTheta0 = maxCosTheta_Front;
				float	cosTheta1 = maxCosTheta_Back;
				float	sinTheta0 = Mathf.Sqrt( 1.0f - cosTheta0*cosTheta0 );
				float	sinTheta1 = Mathf.Sqrt( 1.0f - cosTheta1*cosTheta1 );
				float	cosTheta0_3 = cosTheta0*cosTheta0*cosTheta0;
				float	cosTheta1_3 = cosTheta1*cosTheta1*cosTheta1;
				float	sinTheta0_3 = sinTheta0*sinTheta0*sinTheta0;
				float	sinTheta1_3 = sinTheta1*sinTheta1*sinTheta1;

				float2	sliceSpaceNormal = new float2( _csNormal.xy.Dot( _csDirection ), _csNormal.z );

				float	averageX = sliceSpaceNormal.x * (cosTheta0_3 + cosTheta1_3 - 3.0f * (cosTheta0 + cosTheta1) + 4.0f)
								 + sliceSpaceNormal.y * (sinTheta0_3 - sinTheta1_3);

				float	averageY = sliceSpaceNormal.x * (sinTheta0_3 - sinTheta1_3)
								 + sliceSpaceNormal.y * (2.0f - cosTheta0_3 - cosTheta1_3);

				_csBentNormal = new float3( averageX * _csDirection, averageY );
			#endif

//			_csBentNormal = _csBentNormal.Normalized;
//scale par Delta-Theta plutôt ??

			// Compute cone angles
			float3	ssHorizon_Front = new float3( Mathf.Sqrt( 1.0f - maxCosTheta_Front*maxCosTheta_Front ) * _csDirection, maxCosTheta_Front );
			float3	ssHorizon_Back = new float3( -Mathf.Sqrt( 1.0f - maxCosTheta_Back*maxCosTheta_Back ) * _csDirection, maxCosTheta_Back );
			#if USE_FAST_ACOS
				_coneAngles.x = FastPosAcos( saturate( dot( _csBentNormal, ssHorizon_Front ) ) );
				_coneAngles.y = FastPosAcos( saturate( dot( _csBentNormal, ssHorizon_Back ) ) ) ;
			#else
				_coneAngles.x = Mathf.Acos( Mathf.Saturate( _csBentNormal.Dot( ssHorizon_Front ) ) );
				_coneAngles.y = Mathf.Acos( Mathf.Saturate( _csBentNormal.Dot( ssHorizon_Back ) ) );
			#endif

//_DEBUG = float4( _coneAngles, 0, 0 );
_DEBUG = _coneAngles.x / (0.5f*Mathf.PI) * float4.One;


			return sumRadiance;
		}

		// Integrates the dot product of the normal with a vector interpolated between slice horizon angle theta0 and theta1
		// We're looking to obtain the irradiance reflected from a tiny slice of terrain:
		//	E = Integral[theta0, theta1]{ L(Xi) (Wi.N) sin(theta) dTheta }
		// Where
		//	Xi, the location of the neighbor surface reflecting radiance
		//	L(Xi), the surface's radiance at that location
		//	Wi, the direction of the vector within the slice that we're integrating Wi = { cos(phi).sin(theta), sin(phi).sin(theta), cos(theta) } expressed in screen space
		//	phi, the azimutal angle of the screen-space slice
		//	N, the surface's normal at current location (not neighbor location!)
		//
		// The integral transforms into an irradiance-agnostic version:
		//	E = L(Xi) * Integral[theta0, theta1]{ (Wi.N) sin(theta) dTheta } = L(Xi) * I
		//
		// With:
		//	I = Integral[theta0, theta1]{ cos(phi).sin(theta).Nx.sin(theta) dTheta }	<= I0
		//	  + Integral[theta0, theta1]{ sin(phi).sin(theta).Ny.sin(theta) dTheta }	<= I1
		//	  + Integral[theta0, theta1]{          cos(theta).Nz.sin(theta) dTheta }	<= I2
		//
		//	I0 = [cos(phi).Nx] * Integral[theta0, theta1]{ sin²(theta) dTheta } = [cos(phi).Nx] * { 1/2 * (theta - sin(theta)*cos(theta)) }[theta0,theta1]
		//	I1 = [sin(phi).Ny] * Integral[theta0, theta1]{ sin²(theta) dTheta } = [sin(phi).Ny] * { 1/2 * (theta - sin(theta)*cos(theta)) }[theta0,theta1]
		//	I2 = Nz * Integral[theta0, theta1]{ cos(theta).sin(theta) dTheta } = Nz * { 1/2 * cos²(theta)) }[theta1,theta0]	<== Watch out for theta reverse here!
		//
		float	IntegrateSolidAngle( float2 _integralFactors, float _cosTheta0, float _cosTheta1 ) {
			float	sinTheta0 = Mathf.Sqrt( 1.0f - _cosTheta0*_cosTheta0 );
			float	sinTheta1 = Mathf.Sqrt( 1.0f - _cosTheta1*_cosTheta1 );

			// SUPER EXPENSIVE PART!
			#if USE_FAST_ACOS
				float	theta0 = FastPosAcos( _cosTheta0 );
				float	theta1 = FastPosAcos( _cosTheta1 );
			#else
				float	theta0 = Mathf.Acos( _cosTheta0 );
				float	theta1 = Mathf.Acos( _cosTheta1 );
			#endif

			float	I0 = (theta1 - sinTheta1*_cosTheta1)
					   - (theta0 - sinTheta0*_cosTheta0);
			float	I1 = _cosTheta0*_cosTheta0 - _cosTheta1*_cosTheta1;
			return 0.5f * (_integralFactors.x * I0 + _integralFactors.y * I1);
		}
		float2	ComputeIntegralFactors( float2 _csDirection, float3 _N ) {
			return new float2( _N.xy.Dot( _csDirection ), _N.z );
		}

		// Samples the irradiance reflected from a screen-space position located around the center pixel position
		//	_ssPosition, the screen-space position to sample
		//	_H0, the center height
		//	_radius_m, the radius (in meters) from the center position
		//	_integralFactors, some pre-computed factors to feed the integral
		//	_maxCos, the floating maximum cos(theta) that indicates the angle of the perceived horizon
		//	_optionnal_centerRho, the reflectance of the center pixel (fallback only necessary if no albedo map is available and if it's only irradiance that is stored in the source irradiance map instead of radiance, in which case albedo is already pre-mutliplied)
		//
		float3	SampleIrradiance( float2 _csPosition, float4x4 _localCamera2World, float2 _mipLevel, float2 _integralFactors, ref float3 _previousRadiance, ref float _maxCos ) {

			// Transform camera-space position into screen space
			float3	wsNeighborPosition = _localCamera2World[3].xyz + _csPosition.x * _localCamera2World[0].xyz + _csPosition.y * _localCamera2World[1].xyz;
// 			float3	deltaPos = wsNeighborPosition = _Camera2World[3].xyz;
// 			float2	gcsPosition = float3( dot( deltaPos, _Camera2World[0].xyz ), dot( deltaPos, _Camera2World[1].xyz ), dot( deltaPos, _Camera2World[2].xyz ) );


// #TODO: Optimize!
float4	projPosition = new float4( wsNeighborPosition, 1.0f ) * m_world2Proj;
		projPosition /= projPosition.w;
float2	ssPosition = new float2( 0.5f * (1.0f + projPosition.x) * m_resolution.x, 0.5f * (1.0f - projPosition.y) * m_resolution.y );


			// Sample new depth and rebuild final world-space position
			float	neighborZ = FetchDepth( ssPosition, _mipLevel.x );
	
			float3	wsView = wsNeighborPosition - m_camera2World[3].xyz;		// Neighbor world-space position (not projected), relative to camera
					wsView /= Math.Abs(wsView.Dot(m_camera2World[2].xyz ));		// Scaled so its length against the camera's Z axis is 1
					wsView *= neighborZ;										// Scaled again so its length agains the camera's Z axis equals our sampled Z

			wsNeighborPosition = m_camera2World[3].xyz + wsView;				// Final reprojected world-space position

			// Update horizon angle following eq. (3) from the paper
			wsNeighborPosition -= _localCamera2World[3].xyz;					// Neighbor position - central position
			float3	csNeighborPosition = new float3( wsNeighborPosition.Dot( _localCamera2World[0].xyz ), wsNeighborPosition.Dot( _localCamera2World[1].xyz ), wsNeighborPosition.Dot( _localCamera2World[2].xyz ) );
			float	radius = csNeighborPosition.xy.Length;
			float	d = csNeighborPosition.z;
//					d *= BilateralFilterDepth( 0.0, d, _radius );	// Attenuate
			float	cosHorizon = d / Mathf.Sqrt( radius*radius + d*d );	// Cosine to horizon angle
			if ( cosHorizon <= _maxCos )
				return float3.Zero;	// Below the horizon... No visible contribution.

			#if SAMPLE_NEIGHBOR_RADIANCE
// NOW USELESS I THINK...
//				// Sample neighbor's incoming radiance value, only if difference in depth is not too large
//				float	bilateralWeight = BilateralFilterRadiance( _H0, neighborH, _radius );
//				if ( bilateralWeight > 0.0 )
//					_previousRadiance = lerp( _previousRadiance, FetchRadiance( _ssPosition ), bilateralWeight );	// Accept new height and its radiance value

				// Sample always (actually, it's okay now we accepted the height through the first bilateral filter earlier)
				_previousRadiance = FetchRadiance( ssPosition, _mipLevel.y );
			#endif

			// Integrate over horizon difference (always from smallest to largest angle otherwise we get negative results!)
			float3	incomingRadiance = _previousRadiance * IntegrateSolidAngle( _integralFactors, cosHorizon, _maxCos );

// #TODO: Integrate with linear interpolation of irradiance as well??
// #TODO: Integrate with Fresnel F0!

			_maxCos = cosHorizon;		// Register a new positive horizon

			return incomingRadiance;
		}

		#region Filters


		////////////////////////////////////////////////////////////////////////////////
		// Implement the methods expected by the HBIL header
		float	FetchDepth( float2 _pixelPosition, float _mipLevel ) {
//			return _tex_sourceRadiance.SampleLevel( LinearClamp, _pixelPosition / _resolution, _mipLevel ).w;
			return Z_FAR * SampleLevel( m_arrayDepth, _pixelPosition / m_resolution, _mipLevel ).x;
		}

		float3	FetchRadiance( float2 _pixelPosition, float _mipLevel ) {
			return SampleLevel( m_arrayIrradiance, _pixelPosition / m_resolution, _mipLevel ).xyz;
		}

		float	BilateralFilterDepth( float _centralZ, float _neighborZ, float _radius_m ) {
		//return 1.0;	// Accept always

			float	deltaZ = _neighborZ - _centralZ;

// 			#if 1
				// Relative test
				float	relativeZ = Math.Abs( deltaZ ) / _centralZ;
//				return smoothstep( _bilateralValues.y, _bilateralValues.x, relativeZ );
				return Mathf.Smoothstep( 0.4f, 0.0f, relativeZ );	// Discard when deltaZ is larger than 40% central Z (empirical value)
// 			#elif 0
// 				// Absolute test
// 				return smoothstep( _bilateralValues.y, _bilateralValues.x, abs(deltaZ) );
// 				return smoothstep( 1.0, 0.0, abs(deltaZ) );
// 			#else
// 				// Reject if outside of gather sphere radius
// 				float	r = _radius_m / _gatherSphereMaxRadius_m;
// 				float	sqSphereZ = 1.0 - r*r;
// 				return smoothstep( _bilateralValues.y*_bilateralValues.y * sqSphereZ, _bilateralValues.x*_bilateralValues.x * sqSphereZ, deltaZ*deltaZ );
// 				return smoothstep( 0.1*0.1 * sqSphereZ, 0.0 * sqSphereZ, deltaZ*deltaZ );	// Empirical values
// 			#endif
		}
		float	BilateralFilterRadiance( float _centralZ, float _neighborZ, float _radius_m ) {
		//return 1.0;	// Accept always
		//return 0.0;	// Reject always

			float	deltaZ = _neighborZ - _centralZ;

//			#if 1
				// Relative test
				float	relativeZ = Math.Abs( deltaZ ) / _centralZ;
//				return smoothstep( 0.1*_bilateralValues.y, 0.1*_bilateralValues.x, relativeZ );	// Discard when deltaZ is larger than 1% central Z
//				return smoothstep( 0.015, 0.0, relativeZ );	// Discard when deltaZ is larger than 1.5% central Z (empirical value)
				float	result = Mathf.Smoothstep( 0.15f, 0.0f, relativeZ );	// Discard when deltaZ is larger than 1.5% central Z (empirical value)
				return result;
// 			#elif 0
// 				// Absolute test
// 				return smoothstep( 1.0, 0.0, abs(deltaZ) );
// 			#else
// 				// Reject if outside of gather sphere radius
// 				float	r = _radius_m / _gatherSphereMaxRadius_m;
// 				float	sqSphereZ = 1.0 - r*r;
// 				return smoothstep( _bilateralValues.y*_bilateralValues.y * sqSphereZ, _bilateralValues.x*_bilateralValues.x * sqSphereZ, deltaZ*deltaZ );
// 				return smoothstep( 0.1*0.1 * sqSphereZ, 0.0 * sqSphereZ, deltaZ*deltaZ );	// Empirical values
// 			#endif
		}

		float2	ComputeMipLevel( float2 _radius, float2 _radialStepSizes ) {
return float2.Zero;
/*
			float	radiusPixel = _radius.x;
			float	deltaRadius = _radialStepSizes.x;
			float	pixelArea = Mathf.PI / (2.0f * MAX_ANGLES) * 2.0f * radiusPixel * deltaRadius;
//			return 0.5f * Mathf.Log2( pixelArea ) * _bilateralValues;
			return 0.5f * Mathf.Log2( pixelArea ) * new float2( 0, 2 );	// Unfortunately, sampling lower mips for depth gives very nasty halos! Maybe use max depth? Meh. Not conclusive either...
			return 1.5f * 0.5f * Mathf.Log2( pixelArea ) * float2.One;
			return 0.5f * Mathf.Log2( pixelArea ) * float2.One;
*/
		}


		#endregion

		#region IDisposable Members

		public void Dispose() {
			if ( m_texIrradiance_Staging != null )
				m_texIrradiance_Staging.Dispose();
			if ( m_texNormal_Staging != null )
				m_texNormal_Staging.Dispose();
			if ( m_texDepth_Staging != null )
				m_texDepth_Staging.Dispose();
		}

		#endregion

		#region CPU Texture Access

		void	ReadBack( Texture2D _texture_Staging, out float4[][,] _array ) {
			_array = new float4[_texture_Staging.MipLevelsCount][,];
			for ( uint mipLevelIndex=0; mipLevelIndex < _texture_Staging.MipLevelsCount; mipLevelIndex++ ) {
				uint		W = _texture_Staging.get_WidthAtMip( mipLevelIndex );
				uint		H = _texture_Staging.get_HeightAtMip( mipLevelIndex );
				float4[,]	mip = new float4[W,H];
				_array[mipLevelIndex] = mip;

				if ( _texture_Staging.PixelFormat == PIXEL_FORMAT.RGBA32F ) {
					_texture_Staging.ReadPixels( mipLevelIndex, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
						mip[_X,_Y].Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
					} );
				} else if ( _texture_Staging.PixelFormat == PIXEL_FORMAT.RGBA16F ) {
					half	R, G, B, A;
					_texture_Staging.ReadPixels( mipLevelIndex, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
						R.raw = _R.ReadUInt16();
						G.raw = _R.ReadUInt16();
						B.raw = _R.ReadUInt16();
						A.raw = _R.ReadUInt16();
						mip[_X,_Y].Set( R, G, B, A );
					} );
				} else if ( _texture_Staging.PixelFormat == PIXEL_FORMAT.R32F ) {
					_texture_Staging.ReadPixels( mipLevelIndex, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
						float	V = _R.ReadSingle();
						mip[_X,_Y].Set( V, V, V, V );
					} );
				} else
					throw new Exception( "Unsupproted format!" );
			}
		}


		float4	SampleLevel( float4[][,] _array, float2 _UV, float _mipLevel ) {
			float	fmip0 = Mathf.Floor( _mipLevel );
			float	t = _mipLevel - fmip0;
			uint	mip0 = Mathf.Clamp( (uint) fmip0, 0, (uint) (_array.Length-1) );
			uint	mip1 = Math.Min( (uint) (_array.Length-1), mip0+1 );

			float4	V0 = Sample( _array[mip0], _UV );
			float4	V1 = Sample( _array[mip0], _UV );
			float4	V = (1-t) * V0 + t * V1;
			return V;
		}

		float4	Sample( float4[,] _array, float2 _UV ) {
			uint	W = (uint) _array.GetLength( 0 );
			uint	H = (uint) _array.GetLength( 1 );
			float	fX = _UV.x * W - 0.5f;
			float	u = fX - Mathf.Floor( fX );
			uint	X0 = Mathf.Clamp( (uint) Math.Floor( fX ), 0, W-1 );
			uint	X1 = Math.Min( W-1, X0+1 );
			float	fY = _UV.y * H - 0.5f;
			float	v = fY - Mathf.Floor( fY );
			uint	Y0 = Mathf.Clamp( (uint) Math.Floor( fY ), 0, H-1 );
			uint	Y1 = Math.Min( H-1, Y0+1 );

			float4	V00 = _array[X0,Y0];
			float4	V10 = _array[X1,Y0];
			float4	V01 = _array[X0,Y1];
			float4	V11 = _array[X1,Y1];
			float4	V0 = (1-u) * V00 + u * V10;
			float4	V1 = (1-u) * V01 + u * V11;
			float4	V = (1-v) * V0 + v * V1;
			return V;
		}

		#endregion

		// Builds an **unnormalized** camera ray from a screen UV
		float3	BuildCameraRay( float2 _UV ) {
			_UV = 2.0f * _UV - float2.One;
			_UV.x *= TAN_HALF_FOV * m_resolution.x / m_resolution.y;	// Account for aspect ratio
			_UV.y *= -TAN_HALF_FOV;										// Positive Y as we go up the screen
			return new float3( _UV, 1.0f );								// Not normalized!
		}

		#region Integration Test

		/// <summary>
		/// This function performs a simple integration test by slicing a bent plane into N slice and using the integrals
		/// to compute the resulting average bent normal for each slice, then finalizing the resulting normal which should
		/// equal the initial test normal
		/// </summary>
		void	PerformIntegrationTest() {
			float3	csNormal = new float3( 10, 0, 1 ).Normalized;	// Simple 45° bent normal

			uint	SLICES_COUNT = 128;

			float3	csAverageBentNormal = float3.Zero;
			for ( uint sliceIndex=0; sliceIndex < SLICES_COUNT; sliceIndex++ ) {
				float	phi = sliceIndex * Mathf.PI / SLICES_COUNT;
				float2	csDirection = new float2( Mathf.Cos( phi ), Mathf.Sin( phi ) );

				// Compute initial horizon angles
				float	t = -csDirection.Dot( csNormal.xy ) / csNormal.z;
				float	maxCosTheta_Front = t / Mathf.Sqrt( t*t + 1.0f );
				float	maxCosTheta_Back = -maxCosTheta_Front;	// Back cosine is simply the mirror value

// 				float	theta_Front = Mathf.Acos( maxCosTheta_Front );
// 				float	theta_Back = -Mathf.Acos( maxCosTheta_Back );	// Technically, this is theta0 and it should be in [-PI,0] but we took its absolute value to ease our computation

// Here, the runtime algorithm is normally updating the horizon angles but we keep them flat: our goal is to obtain the original csNormal!

				// Express angles in local normal space
				float2	ssNormal_raw = new float2( csNormal.xy.Dot( csDirection ), csNormal.z );
				float	normalWeight = ssNormal_raw.Length;
				float2	ssNormal = ssNormal_raw / normalWeight;				// Slice-space normal
				float2	ssTangent = new float2( ssNormal.y, -ssNormal.x );	// Slice-space tangent

				float2	ssHorizon_Front = new float2( Mathf.Sqrt( 1.0f - maxCosTheta_Front*maxCosTheta_Front ), maxCosTheta_Front );	// Front horizon direction
				float2	ssHorizon_Back = new float2( -Mathf.Sqrt( 1.0f - maxCosTheta_Back*maxCosTheta_Back ), maxCosTheta_Back );		// Back horizon direction

				float	nsCosTheta_Front = ssHorizon_Front.Dot( ssNormal );
				float	nsCosTheta_Back = ssHorizon_Back.Dot( ssNormal );
				float	nsTheta_Front = Mathf.Acos( nsCosTheta_Front );
				float	nsTheta_Back = -Mathf.Acos( nsCosTheta_Back );

//*				// Numerical integration
				// Half brute force where we perform the integration numerically as a sum...
				//
				const uint	STEPS_COUNT = 256;

				float2	nsBentNormal = 0.001f * float2.UnitY;
				for ( uint i=0; i < STEPS_COUNT; i++ ) {
					float	theta = Mathf.Lerp( nsTheta_Back, nsTheta_Front, (i+0.5f) / STEPS_COUNT );
					float	sinTheta = Mathf.Sin( theta ), cosTheta = Mathf.Cos( theta );
					float2	nsUnOccludedDirection = new float2( sinTheta, cosTheta );

					float2	ssUnOccludedDirection = nsUnOccludedDirection.x * ssTangent + nsUnOccludedDirection.y * ssNormal;
					float3	csUnOccludedDirection = new float3( ssUnOccludedDirection.x * csDirection, ssUnOccludedDirection.y );
					float	cosAlpha = Mathf.Saturate( csUnOccludedDirection.Dot( csNormal ) );
//					float	cosAlpha = cosTheta;

					float	weight = cosAlpha * Mathf.Abs( sinTheta );		// cos(alpha) * sin(theta).dTheta  (be very careful to take abs(sin(theta)) because our theta crosses the pole and becomes negative here!)
					nsBentNormal += weight * nsUnOccludedDirection;
				}

				float	dTheta = (nsTheta_Front - nsTheta_Back) / STEPS_COUNT;
				nsBentNormal *= dTheta;
//				float3	csBentNormal = new float3( (nsBentNormal.y * ssNormal.x + nsBentNormal.x * ssTangent.x) * csDirection, nsBentNormal.y * ssNormal.y + nsBentNormal.x * ssTangent.y );
				float2	ssBentNormal = nsBentNormal.x * ssTangent + nsBentNormal.y * ssNormal;
				float3	csBentNormal = new float3( ssBentNormal.x * csDirection, ssBentNormal.y );

// Il ressort que X est fortement privilégié.
// Forcément puisque le plan est incliné vers X donc tous les vecteurs vont être 2 fois plus influencés par X que par Z ici...
// Ca signifie que c'est encore pas le bon calcul...

//*/
/*				// Analytical integration
				float	cosTheta0 = maxCosTheta_Front;
				float	cosTheta1 = maxCosTheta_Back;
				float	sinTheta0 = Mathf.Sqrt( 1.0f - cosTheta0*cosTheta0 );
				float	sinTheta1 = Mathf.Sqrt( 1.0f - cosTheta1*cosTheta1 );
				float	cosTheta0_3 = cosTheta0*cosTheta0*cosTheta0;
				float	cosTheta1_3 = cosTheta1*cosTheta1*cosTheta1;
				float	sinTheta0_3 = sinTheta0*sinTheta0*sinTheta0;
				float	sinTheta1_3 = sinTheta1*sinTheta1*sinTheta1;

				float2	sliceSpaceNormal = new float2( csNormal.xy.Dot( csDirection ), csNormal.z );

				float	averageX = sliceSpaceNormal.x * (cosTheta0_3 + cosTheta1_3 - 3.0f * (cosTheta0 + cosTheta1) + 4.0f)
								 + sliceSpaceNormal.y * (sinTheta0_3 - sinTheta1_3);

				float	averageY = sliceSpaceNormal.x * (sinTheta0_3 - sinTheta1_3)
								 + sliceSpaceNormal.y * (2.0f - cosTheta0_3 - cosTheta1_3);

//averageX *= sliceSpaceNormal.x;

				float3	csBentNormal = new float3( averageX * csDirection, averageY );
//*/

// Original routine normalizes each slice
//csBentNormal.Normalize();

// Try weighing by the angular gap that we covered instead
//csBentNormal /= theta_Front - theta_Back;

				csAverageBentNormal += csBentNormal;
			}

			float3	csFinalNormal = csAverageBentNormal.Normalized;
		}

		#endregion

		#endregion
	}
}
