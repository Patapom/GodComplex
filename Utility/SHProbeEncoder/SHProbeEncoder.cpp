#include "../../GodComplex.h"

const float	SHProbeEncoder::Z_INFINITY = 1e6f;
const float	SHProbeEncoder::Z_INFINITY_TEST = 0.99f * SHProbeEncoder::Z_INFINITY;

float		SHProbeEncoder::DISTANCE_THRESHOLD = 0.02f;						// 2cm
float		SHProbeEncoder::ANGULAR_THRESHOLD = acosf( 0.5f * PI / 180 );	// 0.5°
float		SHProbeEncoder::ALBEDO_HUE_THRESHOLD = 0.04f;					// Close colors!
float		SHProbeEncoder::ALBEDO_RGB_THRESHOLD = 0.16f;					// Close colors!

const double	SHProbeEncoder::SAMPLE_SH_NORMALIZER = 1.0 / SHProbeEncoder::PROBE_SAMPLES_COUNT;

#pragma region Static Sample Directions

// Samples directions were generated using the Tools > Voronoï Visualizer project
//	1) Enter the amount of samples as "neighbors"
//	2) Press "Simulate" and "Render Cell"
//	3) Wait a bit until the cell is stabilized
//	4) Press "Copy Directions to Clipboard"
//	5) Paste here...
//
float3	SHProbeEncoder::ms_SampleDirections[SHProbeEncoder::PROBE_SAMPLES_COUNT] = {
	float3( -0.06512748f, -0.9806018f, -0.184874f ),
	float3( 0.2651609f, -0.963407f, -0.03920109f ),
	float3( -0.3364103f, -0.9396475f, 0.06237629f ),
	float3( 0.3598919f, -0.8961947f, 0.2594475f ),
	float3( -0.3848802f, -0.8907829f, -0.2416054f ),
	float3( 0.2143864f, -0.9064168f, -0.3639329f ),
	float3( -0.2476965f, -0.8934737f, 0.3746345f ),
	float3( 0.08229525f, -0.9112423f, 0.4035655f ),
	float3( -0.1003698f, -0.8721034f, -0.4789171f ),
	float3( 0.5008324f, -0.8374052f, -0.2189056f ),
	float3( -0.5485207f, -0.7896309f, 0.2749692f ),
	float3( 0.5953481f, -0.799063f, 0.08401915f ),
	float3( -0.6287103f, -0.7764353f, -0.04326314f ),
	float3( 0.1291676f, -0.7389665f, -0.6612445f ),
	float3( -0.05498989f, -0.7613023f, 0.6460611f ),
	float3( 0.1534568f, -0.563389f, 0.8118152f ),
	float3( -0.1932014f, -0.6435598f, -0.7406106f ),
	float3( 0.7858406f, -0.6098233f, -0.1028104f ),
	float3( -0.7977792f, -0.5799627f, 0.1648988f ),
	float3( 0.581167f, -0.6974975f, 0.4192162f ),
	float3( -0.6550472f, -0.6681805f, -0.3527721f ),
	float3( 0.419537f, -0.7176616f, -0.5558333f ),
	float3( -0.3937273f, -0.7073917f, 0.5870057f ),
	float3( 0.3013652f, -0.7534088f, 0.5844265f ),
	float3( -0.3967903f, -0.7520766f, -0.5262493f ),
	float3( 0.6712496f, -0.6281286f, -0.393546f ),
	float3( -0.6733227f, -0.565805f, 0.4759215f ),
	float3( 0.8012968f, -0.5589657f, 0.2132624f ),
	float3( -0.8418803f, -0.516003f, -0.1580462f ),
	float3( 0.06062728f, -0.4960088f, -0.8661985f ),
	float3( -0.2067396f, -0.5451078f, 0.8124754f ),
	float3( -0.0135234f, -0.3130138f, 0.9496524f ),
	float3( -0.08399558f, -0.2372432f, -0.9678122f ),
	float3( 0.941248f, -0.3341865f, -0.04869918f ),
	float3( -0.9517626f, -0.3001086f, 0.06389888f ),
	float3( 0.7467493f, -0.4237861f, 0.5126117f ),
	float3( -0.5619847f, -0.5092604f, -0.6517876f ),
	float3( 0.6390445f, -0.4245374f, -0.6413971f ),
	float3( -0.5330486f, -0.431778f, 0.7276173f ),
	float3( 0.4824985f, -0.5209638f, 0.7041249f ),
	float3( -0.3415031f, -0.3915559f, -0.8544353f ),
	float3( 0.8565712f, -0.3631245f, -0.3666422f ),
	float3( -0.8575178f, -0.3269508f, 0.3971983f ),
	float3( 0.9242319f, -0.2675065f, 0.2724625f ),
	float3( -0.7920873f, -0.3738262f, -0.482547f ),
	float3( 0.3699216f, -0.4851609f, -0.7923238f ),
	float3( -0.3395496f, -0.2589717f, 0.9042344f ),
	float3( 0.3215487f, -0.2934381f, 0.900278f ),
	float3( -0.3478316f, -0.05664718f, -0.9358441f ),
	float3( 0.9481271f, -0.06919942f, -0.3102683f ),
	float3( -0.9581742f, -0.05305798f, 0.2812242f ),
	float3( 0.8241861f, -0.08944064f, 0.5592117f ),
	float3( -0.8134472f, -0.05967117f, -0.5785698f ),
	float3( 0.5305196f, -0.1686577f, -0.8307247f ),
	float3( -0.5368034f, -0.02535539f, 0.8433263f ),
	float3( 0.6192603f, -0.2281732f, 0.7513014f ),
	float3( -0.6107424f, -0.2060255f, -0.7645569f ),
	float3( 0.7807169f, -0.1347422f, -0.6101849f ),
	float3( -0.7276722f, -0.1989731f, 0.656432f ),
	float3( 0.999599f, -0.02614969f, 0.01086181f ),
	float3( -0.9428056f, -0.2125831f, -0.2567606f ),
	float3( 0.2407486f, -0.2056587f, -0.9485486f ),
	float3( -0.1987798f, 0.0105478f, 0.9799875f ),
	float3( 0.1273821f, -0.03292004f, 0.9913073f ),
	float3( -0.002547364f, 0.06687968f, -0.9977578f ),
	float3( 0.9536465f, 0.2358233f, -0.1869379f ),
	float3( -0.9990733f, 0.03723105f, -0.02160015f ),
	float3( 0.6906661f, 0.1618242f, 0.7048356f ),
	float3( -0.7522958f, 0.2701904f, -0.6008729f ),
	float3( 0.6176641f, 0.1348862f, -0.7747882f ),
	float3( -0.6514491f, 0.2400095f, 0.7197288f ),
	float3( 0.4705701f, 0.01217525f, 0.8822786f ),
	float3( -0.5776926f, 0.1217202f, -0.8071279f ),
	float3( 0.8364277f, 0.1642735f, -0.5228794f ),
	float3( -0.8359304f, 0.07170705f, 0.5441309f ),
	float3( 0.9476179f, 0.05754003f, 0.3141806f ),
	float3( -0.9381226f, 0.09691005f, -0.3324671f ),
	float3( 0.3239818f, 0.1051278f, -0.9402042f ),
	float3( -0.3718577f, 0.278695f, 0.8854665f ),
	float3( 0.272253f, 0.2451147f, 0.9304823f ),
	float3( -0.2721535f, 0.2557805f, -0.9276361f ),
	float3( 0.8088649f, 0.4513687f, -0.3768342f ),
	float3( -0.9221582f, 0.2692063f, 0.2777631f ),
	float3( 0.8300252f, 0.3087177f, 0.4644907f ),
	float3( -0.857693f, 0.3983283f, -0.3251113f ),
	float3( 0.3910697f, 0.3968202f, -0.8304205f ),
	float3( -0.4996046f, 0.530091f, 0.6851269f ),
	float3( 0.4946553f, 0.4020301f, 0.7705115f ),
	float3( -0.4935963f, 0.4407661f, -0.7497252f ),
	float3( 0.635273f, 0.4374456f, -0.6364507f ),
	float3( -0.7561093f, 0.4412021f, 0.4833629f ),
	float3( 0.9396623f, 0.3184428f, 0.1250164f ),
	float3( -0.9352374f, 0.353172f, -0.02450851f ),
	float3( 0.06936274f, 0.3622317f, -0.9295037f ),
	float3( -0.04819092f, 0.2942277f, 0.9545197f ),
	float3( 0.1667844f, 0.5328495f, 0.8296111f ),
	float3( -0.1822327f, 0.5435508f, -0.8193558f ),
	float3( 0.8248289f, 0.5602912f, -0.07570429f ),
	float3( -0.7954615f, 0.5801681f, 0.1750599f ),
	float3( 0.6398923f, 0.5480904f, 0.5386417f ),
	float3( -0.6483262f, 0.5866553f, -0.4852925f ),
	float3( 0.4392658f, 0.6834472f, -0.5830484f ),
	float3( -0.3104446f, 0.7584106f, 0.5730947f ),
	float3( 0.3634647f, 0.6954272f, 0.6198987f ),
	float3( -0.3818772f, 0.7156429f, -0.584829f ),
	float3( 0.6258243f, 0.7065871f, -0.3302707f ),
	float3( -0.5864075f, 0.7085361f, 0.3925592f ),
	float3( 0.7781015f, 0.5786623f, 0.2443525f ),
	float3( -0.7502664f, 0.6437194f, -0.1507497f ),
	float3( 0.1741025f, 0.6239144f, -0.7618525f ),
	float3( -0.186251f, 0.5542929f, 0.8112152f ),
	float3( 0.03052288f, 0.7557275f, 0.6541746f ),
	float3( -0.06895267f, 0.7775376f, -0.6250446f ),
	float3( 0.6115053f, 0.7912098f, 0.006967954f ),
	float3( -0.5787853f, 0.8137574f, 0.05297609f ),
	float3( 0.5305579f, 0.785412f, 0.3188049f ),
	float3( -0.4992356f, 0.8194184f, -0.2816331f ),
	float3( 0.1935211f, 0.8581434f, -0.4755414f ),
	float3( -0.07527816f, 0.9143383f, 0.3978928f ),
	float3( 0.239242f, 0.8908177f, 0.3862734f ),
	float3( -0.1890372f, 0.9122233f, -0.3634742f ),
	float3( 0.393363f, 0.8897788f, -0.2314289f ),
	float3( -0.3529578f, 0.9009489f, 0.2524119f ),
	float3( 0.3177822f, 0.944992f, 0.07748912f ),
	float3( -0.2825107f, 0.9572946f, -0.06143976f ),
	float3( 0.07187531f, 0.9781598f, -0.1950316f ),
	float3( -0.004785682f, 0.9944577f, 0.105029f ),
	float3( -0.01342054f, -0.9926256f, 0.1204758f ),
};

#pragma endregion

SHProbeEncoder::SHProbeEncoder() {

	//////////////////////////////////////////////////////////////////////////
	// Prepare the cube map face transforms
	// Here are the transform to render the 6 faces of a cube map
	// Remember the +Z face is not oriented the same way as our Z vector: http://msdn.microsoft.com/en-us/library/windows/desktop/bb204881(v=vs.85).aspx
	//
	//
	//		^ +Y
	//		|   +Z  (our actual +Z faces the other way!)
	//		|  /
	//		| /
	//		|/
	//		o------> +X
	//
	//
	float3	SideAt[6] = {
		float3(  1,  0,  0 ),
		float3( -1,  0,  0 ),
		float3(  0,  1,  0 ),
		float3(  0, -1,  0 ),
		float3(  0,  0,  1 ),
		float3(  0,  0, -1 ),
	};
	float3	SideRight[6] = {
		float3(  0, 0, -1 ),
		float3(  0, 0,  1 ),
		float3(  1, 0,  0 ),
		float3(  1, 0,  0 ),
		float3(  1, 0,  0 ),
		float3( -1, 0,  0 ),
	};

	for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
	{
		float4x4	Camera2Local;
		Camera2Local.SetRow( 0, SideRight[CubeFaceIndex], 0 );
		Camera2Local.SetRow( 1, SideAt[CubeFaceIndex].Cross( SideRight[CubeFaceIndex] ), 0 );
		Camera2Local.SetRow( 2, SideAt[CubeFaceIndex], 0 );
		Camera2Local.SetRow( 3, float3::Zero, 1 );

		m_Side2World[CubeFaceIndex] = Camera2Local;	// We don't care about the local=>world transform of the probe, we assume it's the identity matrix
	}

	//////////////////////////////////////////////////////////////////////////
	// Prepare cube map pixels
	m_pCubeMapPixels = new Pixel[6*CUBE_MAP_SIZE*CUBE_MAP_SIZE];

	double	dA = 4.0 / (CUBE_MAP_SIZE*CUBE_MAP_SIZE);	// Cube face is supposed to be in [-1,+1], yielding a 2x2 square units
	double	SumSolidAngle = 0.0;

	Pixel*	pPixel = m_pCubeMapPixels;
	for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
		for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
			for ( int X=0; X < CUBE_MAP_SIZE; X++, pPixel++ ) {
				pPixel->Index = pPixel - m_pCubeMapPixels;
				pPixel->CubeFaceIndex = CubeFaceIndex;
				pPixel->CubeFaceX = X;
				pPixel->CubeFaceY = Y;

				// Build world-space view vector
				float3	csView( 2.0f * (0.5f + X) / CUBE_MAP_SIZE - 1.0f, 1.0f - 2.0f * (0.5f + Y) / CUBE_MAP_SIZE, 1.0f );
				float	Distance2Texel = csView.Length();
						csView = csView / Distance2Texel;
				float3	wsView = float4( csView, 0 ) * m_Side2World[CubeFaceIndex];
				pPixel->View = wsView;

				// Retrieve the cube map texel's solid angle (from http://people.cs.kuleuven.be/~philip.dutre/GI/TotalCompendium.pdf)
				// dw = cos(Theta).dA / r²
				// cos(Theta) = Adjacent/Hypothenuse = 1/r
				//
				double	SolidAngle = dA / (Distance2Texel * Distance2Texel * Distance2Texel);
				SumSolidAngle += SolidAngle;

				pPixel->SolidAngle = SolidAngle;

				// Build SH coefficients
				pPixel->InitSH();
// for ( int i=0; i < 9; i++ ) {
// 	int	l = int( floorf( sqrtf( float( i ) ) ) );
// 	int	m = i - l*(l+1);
// 	pPixel->SHCoeffs[i] = SH::ComputeSHCoeff( l, m, pPixel->View );
// }
			}

	//////////////////////////////////////////////////////////////////////////
	// Build samples

#if 1
	// Use the array of sample directions generated by the external tool "Voronoï Visualizer"
	for ( int SampleIndex=0; SampleIndex < PROBE_SAMPLES_COUNT; SampleIndex++ ) {
		Sample&	S = m_pSamples[SampleIndex];
		S.Index = SampleIndex;
		S.OriginalPixelsCount = 0;
		S.View = ms_SampleDirections[SampleIndex];
	}

#else
	// Prepare equal subdivisions of the sphere using Hammersley sampling of the sphere and grouping
	for ( int SampleIndex=0; SampleIndex < PROBE_SAMPLES_COUNT; SampleIndex++ ) {
		Sample&	S = m_pSamples[SampleIndex];

		S.Index = SampleIndex;
		S.OriginalPixelsCount = 0;

		// Build the sample's direction
		float	Phi = 2.0f * PI * (SampleIndex+0.5f) / PROBE_SAMPLES_COUNT;
		float	Y = 2.0f * ReverseBits( 1+SampleIndex ) - 1.0f;
		float	Theta = acosf( Y );
		S.View.Set( sinf(Phi)*sinf(Theta), cosf(Theta), cosf(Phi)*sinf(Theta) );
	}
#endif

	// Assign their sample to each pixel
	double	SH[9] = { 0.0 };

	int	PixelsCount = 6*CUBE_MAP_FACE_SIZE;
	pPixel = m_pCubeMapPixels;
	for ( int i=0; i < PixelsCount; i++, pPixel++ ) {
		Sample*	pSample = m_pSamples;
		float	BestSampleWeight = 0.0f;
		for ( int SampleIndex=0; SampleIndex < PROBE_SAMPLES_COUNT; SampleIndex++, pSample++ ) {
			float	SampleWeight = pSample->View.Dot( pPixel->View );
			if ( SampleWeight <= BestSampleWeight ) {
				continue;
			}

			pPixel->pParentSample = pSample;	// Found a better sample for the pixel!
			BestSampleWeight = SampleWeight;
		}

		pSample = pPixel->pParentSample;
		if ( pSample->pCenterPixel == NULL || pSample->pCenterPixel->Importance < BestSampleWeight ) {
			pSample->pCenterPixel = pPixel;		// Found a better center pixel for the sample!
		}
		pPixel->Importance = BestSampleWeight;	// Assign the sample's weight as temporary importance for the pixel so the sample can compare and choose the best center pixel

		pSample->OriginalPixelsCount++;

		// Debug SH
		for ( int i=0; i < 9; i++ )
			SH[i] += pPixel->SolidAngle * pPixel->SHCoeffs[i];
//			SH[i] += pPixel->SHCoeffs[i] / PixelsCount;
	}

	// Build each sample's SH coefficients
	memset( SH, 0, 9*sizeof(double) );
	pPixel = m_pCubeMapPixels;
	for ( int i=0; i < PixelsCount; i++, pPixel++ ) {
		for ( int SHCoeffIndex=0; SHCoeffIndex < 9; SHCoeffIndex++ ) {
			pPixel->pParentSample->SH[SHCoeffIndex] += pPixel->SolidAngle * pPixel->SHCoeffs[SHCoeffIndex];
		}
	}

	// Statistics on samples
	m_MinSamplePixelsCount = ~0U;
	m_MaxSamplePixelsCount = 0;
	m_AverageSamplePixelsCount = 0;
	for ( int SampleIndex=0; SampleIndex < PROBE_SAMPLES_COUNT; SampleIndex++ ) {
		const Sample&	S = m_pSamples[SampleIndex];

		for ( int i=0; i < 9; i++ )
			SH[i] += S.SH[i];

		m_MaxSamplePixelsCount = max( m_MaxSamplePixelsCount, S.OriginalPixelsCount );	// Keep track of the maximum amount of pixels encountered across all samples
		m_MinSamplePixelsCount = min( m_MinSamplePixelsCount, S.OriginalPixelsCount );
		m_AverageSamplePixelsCount += S.OriginalPixelsCount;
	}
	m_AverageSamplePixelsCount /= PROBE_SAMPLES_COUNT;


	//////////////////////////////////////////////////////////////////////////
	// Pre-allocate the maximum amount of radix nodes
	Pixel::ms_RadixNodes[0] = new SHProbeEncoder::Pixel::RadixNode_t[6*CUBE_MAP_FACE_SIZE];
	Pixel::ms_RadixNodes[1] = new SHProbeEncoder::Pixel::RadixNode_t[6*CUBE_MAP_FACE_SIZE];

	m_SamplePixelGroups.Init( m_MaxSamplePixelsCount );	// Worst case scenario: only 1 pixel per group in each sample so as many groups as pixels!
	m_EmissiveSurfaces.Init( 6*CUBE_MAP_FACE_SIZE );	// Worst case scenario: all pixels in the cube map are a different emissive material!
}

SHProbeEncoder::~SHProbeEncoder() {
	SAFE_DELETE_ARRAY( Pixel::ms_RadixNodes[1] );
	SAFE_DELETE_ARRAY( Pixel::ms_RadixNodes[0] );
	SAFE_DELETE_ARRAY( m_pCubeMapPixels );
}

void	SHProbeEncoder::EncodeProbeCubeMap( Texture2D& _StagingCubeMap, U32 _ProbeID, U32 _ProbesCount, U32 _SceneTotalFacesCount ) {
	int	TotalPixelsCount = 6*CUBE_MAP_FACE_SIZE;

	//////////////////////////////////////////////////////////////////////////
	// 1] Read back probe data and prepare pixels for encoding
	ReadBackProbeCubeMap( _StagingCubeMap, _SceneTotalFacesCount );


	//////////////////////////////////////////////////////////////////////////
	// 2] Encode static lighting & directional occlusion
	//

	// 2.1) ======== Build occlusion & static lighting SH ========
	double	SHR[9] = { 0.0 };
	double	SHG[9] = { 0.0 };
	double	SHB[9] = { 0.0 };
	double	SHOcclusion[9] = { 0.0 };

	for ( int PixelIndex=0; PixelIndex < TotalPixelsCount; PixelIndex++ ) {
		Pixel&	P = m_pCubeMapPixels[PixelIndex];
		for ( int i=0; i < 9; i++ ) {
			// Accumulate smoothed out static lighting
			SHR[i] += P.SHCoeffs[i] * P.SolidAngle * P.SmoothedStaticLitColor.x;
			SHG[i] += P.SHCoeffs[i] * P.SolidAngle * P.SmoothedStaticLitColor.y;
			SHB[i] += P.SHCoeffs[i] * P.SolidAngle * P.SmoothedStaticLitColor.z;

			// No obstacle means direct lighting from the ambient sky...
			// Accumulate SH coefficients in that direction, weighted by the solid angle
			SHOcclusion[i] += P.SolidAngle * P.SmoothedInfinity * P.SHCoeffs[i];
		}
	}

	for ( int i=0; i < 9; i++ ) {
		m_StaticSH[i].Set( (float) SHR[i], (float) SHG[i], (float) SHB[i] );
		m_OcclusionSH[i] = (float) SHOcclusion[i];
	}

	// 2.2) ======== Apply filtering ========
// 	SphericalHarmonics.SHFunctions.FilterLanczos( m_StaticSH, 3 );		// Lanczos should be okay for static lighting
// 	SphericalHarmonics.SHFunctions.FilterHanning( m_OcclusionSH, 3 );


// 	// 2.3) ======== Compute the influence of the probe on each scene face ========
// 	m_MaxFaceIndex = 0;
// 	m_ProbeInfluencePerFace.Clear();
// 	for ( int PixelIndex=0; PixelIndex < TotalPixelsCount; PixelIndex++ ) {
// 		Pixel&	P = m_pCubeMapPixels[PixelIndex];
// 		if ( P.Infinity )
// 			continue;
// 
// 		double*	pFaceInfluence = m_ProbeInfluencePerFace.Get( P.FaceIndex );
// 		if ( pFaceInfluence == NULL ) {
// 			pFaceInfluence = &m_ProbeInfluencePerFace.Add( P.FaceIndex, 0.0 );
// 		}
// 
// 		*pFaceInfluence += P.SolidAngle;
// 		m_MaxFaceIndex = max( m_MaxFaceIndex, P.FaceIndex );
// 	}


	//////////////////////////////////////////////////////////////////////////
	// 3] Build the neighbor probes network
	//
	m_NeighborProbes.Init( _ProbesCount );
	m_NeighborProbes.Clear();

	Dictionary<NeighborProbe*>	NeighborProbeID2Probe;
	for ( int PixelIndex=0; PixelIndex < TotalPixelsCount; PixelIndex++ ) {
		Pixel&	P = m_pCubeMapPixels[PixelIndex];
		if ( P.NeighborProbeID == ~0UL ) {
			continue;
		}

		NeighborProbe**	NP = NeighborProbeID2Probe.Get( P.NeighborProbeID );
		if ( NP == NULL ) {
			NeighborProbe&	Temp = m_NeighborProbes.Append();
			Temp.ProbeID = P.NeighborProbeID;
			NP = &NeighborProbeID2Probe.Add( P.NeighborProbeID, &Temp );
		}

		// Accumulate direction, solid angle & distance
		(*NP)->SolidAngle += P.SolidAngle;
		(*NP)->Distance += P.NeighborProbeDistance;
		(*NP)->Direction = (*NP)->Direction + P.View;

		// Accumulate SH for neighbor's exchange of energy
		for ( int i=0; i < 9; i++ ) {
			(*NP)->SH[i] += P.SolidAngle * P.SHCoeffs[i];
		}

		(*NP)->PixelsCount++;
	}

	// Normalize everything
	m_NearestNeighborProbeDistance = FLT_MAX;
	m_FarthestNeighborProbeDistance = 0.0f;
	for ( int NeighborIndex=0; NeighborIndex < m_NeighborProbes.GetCount(); NeighborIndex++ ) {
		NeighborProbe&	NP = m_NeighborProbes[NeighborIndex];

		NP.Distance /= NP.PixelsCount;
		NP.Direction.Normalize();

		m_NearestNeighborProbeDistance = min( m_NearestNeighborProbeDistance, NP.Distance );
		m_FarthestNeighborProbeDistance = max( m_FarthestNeighborProbeDistance, NP.Distance );
	}

	// Sort from most important to least important neighbor
	{
		class	Comparer : public IComparer<NeighborProbe> {
		public:	virtual int		Compare( const NeighborProbe& a, const NeighborProbe& b ) const {
				if ( a.SolidAngle < b.SolidAngle )
					return +1;
				if ( a.SolidAngle > b.SolidAngle )
					return -1;

				return 0;
			}
		} Comp;
	 	m_NeighborProbes.Sort( Comp );
	}

	//////////////////////////////////////////////////////////////////////////
	// 4] Build samples by flood filling
	//
	ComputeFloodFill( 1.0f, 1.0f, 1.0f, 0.5f );
}

FILE*	g_pFile = NULL;
template< typename T> void	Write( const T& _value ) {
	fwrite( &_value, sizeof(T), 1, g_pFile );
}

void	SHProbeEncoder::Save( const char* _FileName ) const {

	fopen_s( &g_pFile, _FileName, "wb" );
	ASSERT( g_pFile != NULL, "Locked!" );

	// Write the mean, harmonic mean, min, max distances
	Write( (float) m_MeanDistance );
	Write( (float) m_MeanHarmonicDistance );
	Write( (float) m_MinDistance );
	Write( (float) m_MaxDistance );

	// Write the BBox
	Write( m_BBoxMin.x );
	Write( m_BBoxMin.y );
	Write( m_BBoxMin.z );
	Write( m_BBoxMax.x );
	Write( m_BBoxMax.y );
	Write( m_BBoxMax.z );

	// Write static SH
	for ( int i=0; i < 9; i++ )
	{
		Write( m_StaticSH[i].x );
		Write( m_StaticSH[i].y );
		Write( m_StaticSH[i].z );
	}

	// Write occlusion SH
	for ( int i=0; i < 9; i++ )
		Write( m_OcclusionSH[i] );

	// Write the result samples
	for ( U32 i=0; i < PROBE_SAMPLES_COUNT; i++ ) {
		const Sample&	S = m_pSamples[i];

		// Write position, normal, albedo
		Write( S.Position.x );
		Write( S.Position.y );
		Write( S.Position.z );

		Write( S.Normal.x );
		Write( S.Normal.y );
		Write( S.Normal.z );

		Write( S.Tangent.x );
		Write( S.Tangent.y );
		Write( S.Tangent.z );

		Write( S.BiTangent.x );
		Write( S.BiTangent.y );
		Write( S.BiTangent.z );

		Write( S.Radius );

		Write( (float) (S.Albedo.x * INVPI) );
		Write( (float) (S.Albedo.y * INVPI) );
		Write( (float) (S.Albedo.z * INVPI) );

		Write( S.F0.x );
		Write( S.F0.y );
		Write( S.F0.z );

		// Write the pixel coverage of the sample
		Write( float( S.PixelsCount ) / S.OriginalPixelsCount );

// No need: can be regenerated at runtime from normal direction
// 		// Write SH coefficients
// 		for ( int i=0; i < 9; i++ ) {
// 			Write( S.SH[i] );
// 		}
	}

	// Write the emissive surfaces
	Write( m_EmissiveSurfacesCount );
	for ( U32 i=0; i < m_EmissiveSurfacesCount; i++ ) {
		EmissiveSurface&	S = *m_ppEmissiveSurfaces[i];

		// Write emissive mat
		Write( S.EmissiveMatID );

		// Write SH coefficients (we only write luminance here, we don't have the color info, which is provided at runtime)
		for ( int i=0; i < 9; i++ )
			Write( S.SH[i] );
	}

	// Write the neighbor probes
	Write( m_NeighborProbes.GetCount() );

	// Write nearest/farthest probe distance
	Write( m_NearestNeighborProbeDistance );
	Write( m_FarthestNeighborProbeDistance );

	for ( int i=0; i < m_NeighborProbes.GetCount(); i++ ) {
		const NeighborProbe&	NP = m_NeighborProbes[i];

		// Write probe ID, distance, solid angle, direction
		Write( NP.ProbeID );
		Write( NP.Distance );
		Write( (float) NP.SolidAngle );
		Write( NP.Direction.x );
		Write( NP.Direction.y );
		Write( NP.Direction.z );

		// Write SH coefficients (only luminance here since they're used for the product with the neighbor probe's SH)
		for ( int i=0; i < 9; i++ )
			Write( (float) NP.SH[i] );
	}

	fclose( g_pFile );

// We now save a single file
// // Save probe influence for each scene face
// FileInfo	InfluenceFileName = new FileInfo( Path.Combine( Path.GetDirectoryName( _FileName.FullName ), Path.GetFileNameWithoutExtension( _FileName.FullName ) + ".FaceInfluence" ) );
// using ( FileStream S = InfluenceFileName.Create() )
// 	using ( BinaryWriter W = new BinaryWriter( S ) )
// 	{
// 		for ( U32 FaceIndex=0; FaceIndex < m_MaxFaceIndex; FaceIndex++ )
// 		{
// 			W.Write( (float) (m_ProbeInfluencePerFace.ContainsKey( FaceIndex ) ? m_ProbeInfluencePerFace[FaceIndex] : 0.0) );
// 		}
// 	}
}

void	SHProbeEncoder::SavePixels( const char* _FileName ) const {

	fopen_s( &g_pFile, _FileName, "wb" );
	ASSERT( g_pFile != NULL, "Locked!" );

	Write( U32(CUBE_MAP_SIZE) );

	const Pixel*	P = m_pCubeMapPixels;
	for ( int i=0; i < 6*CUBE_MAP_FACE_SIZE; i++, P++ ) {

		Write( P->pParentSample->Index );
		Write( P->bUsedForSampling );

		Write( P->Position.x );
		Write( P->Position.y );
		Write( P->Position.z );
		Write( P->Normal.x );
		Write( P->Normal.y );
		Write( P->Normal.z );

		Write( P->Albedo.x );
		Write( P->Albedo.y );
		Write( P->Albedo.z );
		Write( P->F0.x );
		Write( P->F0.y );
		Write( P->F0.z );

		Write( P->StaticLitColor.x );
		Write( P->StaticLitColor.y );
		Write( P->StaticLitColor.z );
		Write( P->SmoothedStaticLitColor.x );
		Write( P->SmoothedStaticLitColor.y );
		Write( P->SmoothedStaticLitColor.z );

		Write( P->FaceIndex );
		Write( P->EmissiveMatID );
		Write( P->NeighborProbeID );
		Write( P->NeighborProbeDistance );

		Write( P->Importance );
		Write( P->Distance );
		Write( P->SmoothedDistance );
		Write( P->Infinity );
		Write( P->SmoothedInfinity );
	}

	// Write samples
	Write( PROBE_SAMPLES_COUNT );
	for ( U32 SampleIndex=0; SampleIndex < PROBE_SAMPLES_COUNT; SampleIndex++ ) {
		const Sample&	S = m_pSamples[SampleIndex];

		Write( S.Position.x );
		Write( S.Position.y );
		Write( S.Position.z );

		Write( S.Normal.x );
		Write( S.Normal.y );
		Write( S.Normal.z );

		Write( S.Tangent.x );
		Write( S.Tangent.y );
		Write( S.Tangent.z );

		Write( S.BiTangent.x );
		Write( S.BiTangent.y );
		Write( S.BiTangent.z );

		Write( S.Radius );

		Write( S.Albedo.x );
		Write( S.Albedo.y );
		Write( S.Albedo.z );
		Write( S.F0.x );
		Write( S.F0.y );
		Write( S.F0.z );

		Write( S.PixelsCount );

		// Write the pixel coverage of the sample
		Write( float(S.PixelsCount) / S.OriginalPixelsCount );

// No need: can be regenerated at runtime from normal direction
// 		// Write SH coefficients
// 		for ( int i=0; i < 9; i++ ) {
// 			Write( S.SH[i] );
// 		}
	}

	// Write the emissive surfaces
	Write( m_EmissiveSurfacesCount );
	for ( U32 i=0; i < m_EmissiveSurfacesCount; i++ ) {
		EmissiveSurface&	S = *m_ppEmissiveSurfaces[i];

		// Write emissive mat
		Write( S.EmissiveMatID );

		// Write SH coefficients (we only write luminance here, we don't have the color info that is provided at runtime)
		for ( int i=0; i < 9; i++ )
			Write( S.SH[i] );
	}

	fclose( g_pFile );
}

#pragma region Computes Sample Pixels by Flood Fill Method

int	DEBUG_PixelIndex = 0;

void	SHProbeEncoder::ComputeFloodFill( float _SpatialDistanceWeight, float _NormalDistanceWeight, float _AlbedoDistanceWeight, float _MinimumImportanceDiscardThreshold ) {
	int	TotalPixelsCount = 6*CUBE_MAP_FACE_SIZE;
 	U32	DiscardThreshold = U32( 0.004f * m_ScenePixelsCount );		// Discard surfaces that contain less than 0.4% of the total amount of scene pixels (arbitrary!)

	// Setup the reference thresholds for pixels' acceptance
//	Pixel.IMPORTANCE_THRESOLD = (float) ((4.0f * Math.PI / CUBE_MAP_FACE_SIZE) / (m_MeanDistance * m_MeanDistance));	// Compute an average solid angle threshold based on average pixels' distance
	Pixel::IMPORTANCE_THRESOLD = (float) (0.1f * _MinimumImportanceDiscardThreshold / (m_MeanHarmonicDistance * m_MeanHarmonicDistance));	// Simply use the mean harmonic distance as a good approximation of important pixels
																									// Pixels that are further or not facing the probe will have less importance...

	DISTANCE_THRESHOLD = 0.30f * _SpatialDistanceWeight;						// 30cm
	ANGULAR_THRESHOLD = acosf( 45.0f * _NormalDistanceWeight * PI / 180.0f );	// 45° (we're very generous here!)
	ALBEDO_HUE_THRESHOLD = 0.04f * _AlbedoDistanceWeight;						// Close colors!
	ALBEDO_RGB_THRESHOLD = 0.32f * _AlbedoDistanceWeight;						// Close colors!


	//////////////////////////////////////////////////////////////////////////
	// Initialize the list of pixels belonging to each sample
	{
		for ( int SampleIndex=0; SampleIndex < PROBE_SAMPLES_COUNT; SampleIndex++ ) {
			Sample&	S = m_pSamples[SampleIndex];
			S.PixelsCount = 0;
			S.pPixels = NULL;	// No pixel in that sample at the moment...
		}

		Pixel*	pPixel = m_pCubeMapPixels;
		for ( int PixelIndex=0; PixelIndex < TotalPixelsCount; PixelIndex++, pPixel++ ) {
			pPixel->pNext = pPixel->pParentSample->pPixels;
			pPixel->pParentSample->pPixels = pPixel;
			pPixel->pParentSample->PixelsCount++;
			pPixel->pParentList = NULL;
			pPixel->pNextInList = NULL;
		}
	}


// DEBUG: Small experiment designed to see if SH roughly sum to 1
// double	AccumSH[9];
// memset( AccumSH, 0, 9*sizeof(double) );
// for ( int SampleIndex=0; SampleIndex < MAX_PROBE_SAMPLES; SampleIndex++ ) {
// 	Sample&	S = m_pSamples[SampleIndex];
// 
// 	double	SH[9];
// 	SH::BuildSHCosineLobe_YUp( S.View, SH );
// 
// 	double	SHNormalizer = SAMPLE_SH_NORMALIZER;
// 	for ( int i=0; i < 9; i++ )
// 		AccumSH[i] += SHNormalizer * SH[i];
// }


	//////////////////////////////////////////////////////////////////////////
	// Process each sample
	double	GroupImportanceThreshold = _MinimumImportanceDiscardThreshold / PROBE_SAMPLES_COUNT;

GroupImportanceThreshold = 0.0;	// No rejection for now...

	for ( int SampleIndex=0; SampleIndex < PROBE_SAMPLES_COUNT; SampleIndex++ ) {
		Sample&	S = m_pSamples[SampleIndex];

		// Build the lists of pixel groups for that sample
		m_SamplePixelGroups.Clear();
		Pixel*	pPixel = S.pPixels;
		while ( pPixel != NULL ) {
			if ( pPixel->pParentList == NULL && pPixel->IsFloodFillAcceptable( S ) ) {

				// Propagate from the current pixel and form a coherent group
				PixelsList&	AcceptedPixels = m_SamplePixelGroups.Append();
				AcceptedPixels.PixelsCount = 0;
				AcceptedPixels.pPixels = NULL;
				AcceptedPixels.Importance = 0.0;

				PixelsList	RejectedPixels;

				m_ScanlinePixelIndex = 0;		// VEEERY important line where we reset the pixel index of the pool of flood filled pixels!
				FloodFill( S, pPixel, pPixel, AcceptedPixels, RejectedPixels );
				ASSERT( m_ScanlinePixelIndex > 0, "Can't have empty samples!" );

				// Restore pixels rejected by that group since they may be useful for another group
				while ( RejectedPixels.pPixels != NULL ) {
					Pixel*	pTemp = RejectedPixels.pPixels;
					ASSERT( pTemp >= m_pCubeMapPixels && pTemp < m_pCubeMapPixels+6*CUBE_MAP_FACE_SIZE, "Oh!" );
					RejectedPixels.pPixels = RejectedPixels.pPixels->pNextInList;
					pTemp->pParentList = NULL;
					pTemp->pNextInList = NULL;
				}
			}
			pPixel = pPixel->pNext;
		}

		// Keep only the most interesting group
		PixelsList*	pBestGroup = NULL;
		for ( int GroupIndex=0; GroupIndex < m_SamplePixelGroups.GetCount(); GroupIndex++ ) {
			PixelsList&	Group = m_SamplePixelGroups[GroupIndex];
			if ( pBestGroup == NULL || Group.Importance > pBestGroup->Importance ) {
				pBestGroup = &Group;
			}
		}

		if ( pBestGroup == NULL || pBestGroup->Importance < GroupImportanceThreshold ) {
			// Discard this sample entirely as it's not important enough
			S.pPixels = NULL;
			S.PixelsCount = 0;
			S.Radius = 0.0f;	// !IMPORTANT! ==> A radius of 0 will discard the sample at runtime!
			continue;
		}

		// Clear used flag for all pixels
		pPixel = S.pPixels;
		while ( pPixel != NULL ) {
			pPixel->bUsedForSampling = false;
			pPixel = pPixel->pNext;
		}

		// Build the resulting position, normal, albedo and average direction for the group
		S.Position = float3::Zero;
		S.Normal = float3::Zero;
		S.Direction = float3::Zero;
		S.Albedo = float3::Zero;

		pPixel = pBestGroup->pPixels;
		while ( pPixel != NULL ) {
			S.Position = S.Position + pPixel->Position;
			S.Normal = S.Normal + pPixel->Normal;
			S.Direction = S.Direction + pPixel->View;
			S.Albedo = S.Albedo + pPixel->Albedo;
			pPixel->bUsedForSampling = true;	// Mark the pixel as used for sampling
			pPixel = pPixel->pNextInList;
		}

		float	Normalizer = 1.0f / pBestGroup->PixelsCount;
		S.Position = S.Position * Normalizer;
		S.Normal.Normalize();
		S.Direction.Normalize();
		S.Albedo = S.Albedo * Normalizer;

		// Build the radius
		// This is an important data as a value of 0 will discard the sample at runtime
		float	AverageSqDistance = 0.0f;
		pPixel = pBestGroup->pPixels;
		while ( pPixel != NULL ) {
			float3	D = pPixel->Position - S.Position;
			AverageSqDistance += D.LengthSq();
			pPixel = pPixel->pNextInList;
		}
		AverageSqDistance *= Normalizer;
		S.Radius = sqrtf( AverageSqDistance);
	}


	//////////////////////////////////////////////////////////////////////////
	// Build the emissive surfaces
	m_EmissiveSurfaces.Clear();

	EmissiveSurface*	MatID2Surface[1024];	// Maximum of 1024 emissive materials, should be enough
	memset( MatID2Surface, 0, 1024*sizeof(EmissiveSurface*) );

	Pixel*	pPixel = m_pCubeMapPixels;
	for ( int i=0; i < TotalPixelsCount; i++, pPixel++ ) {
		if ( pPixel->EmissiveMatID == ~0UL ) {
			continue;
		}

		ASSERT( pPixel->EmissiveMatID < 1024, "Emissive material ID out of range!" );
		EmissiveSurface*	pSurface = MatID2Surface[pPixel->EmissiveMatID];
		if ( pSurface == NULL ) {
			pSurface = &m_EmissiveSurfaces.Append();
			MatID2Surface[pPixel->EmissiveMatID] = pSurface;
		}

		// Accumulate SH
		for ( int i=0; i < 9; i++ ) {
			pSurface->SH[i] += pPixel->SHCoeffs[i];
		}
		pSurface->SolidAngle += pPixel->SolidAngle;

		// Link
		pPixel->pNext = pSurface->pPixels;
		pSurface->pPixels = pPixel;
		pSurface->PixelsCount++;
	}

	// Sort surfaces
	m_EmissiveSurfacesCount = 0;
	for ( U32 SortedSurfaceIndex=0; SortedSurfaceIndex < MAX_PROBE_EMISSIVE_SURFACES; SortedSurfaceIndex++ ) {

		EmissiveSurface*	pBestSurface = NULL;
		for ( int SurfaceIndex=0; SurfaceIndex < m_EmissiveSurfaces.GetCount(); SurfaceIndex++ ) {
			EmissiveSurface&	Surface = m_EmissiveSurfaces[SurfaceIndex];
			if ( Surface.EmissiveMatID != ~0U )
				continue;	// Already sorted out!
			if ( pBestSurface == NULL || Surface.SolidAngle > pBestSurface->SolidAngle ) {
				pBestSurface = &Surface;
			}
		}

		if ( pBestSurface == NULL || pBestSurface->PixelsCount < DiscardThreshold ) {
			break;	// We're done or we've encountered insignificant surfaces!
		}

		// Assign new best emissive surface
		pBestSurface->ID = SortedSurfaceIndex;
		m_ppEmissiveSurfaces[m_EmissiveSurfacesCount++] = pBestSurface;
	}
}


#pragma region Flood Fill Algorithm

// This should be a faster version (and much less recursive!) than the original flood fill I wrote some time ago
// The idea here is to process an entire scanline first (going left and right and collecting valid pixels along the way)
//  then for each of these pixels we move up/down and fill the top/bottom scanlines from these new seeds...
//
void	SHProbeEncoder::FloodFill( Sample& _Sample, Pixel* _PreviousPixel, Pixel* _P, PixelsList& _AcceptedPixels, PixelsList& _RejectedPixels ) const {

	static int	RecursionLevel = 0;	// For debugging purpose

	if ( !CheckAndAcceptPixel( _Sample, *_PreviousPixel, *_P, _AcceptedPixels, _RejectedPixels ) )
		return;

	//////////////////////////////////////////////////////////////////////////
	// Check the entire scanline
	int	ScanlineStartIndex = m_ScanlinePixelIndex;
	m_ppScanlinePixelsPool[m_ScanlinePixelIndex++] = _P;	// This pixel is implicitly on the scanline

	{	// Start going right
		CubeMapPixelWalker	P( *this, *_P );
		Pixel*	Previous = _P;
		Pixel*	Current = &P.Right();
		while ( CheckAndAcceptPixel( _Sample, *Previous, *Current, _AcceptedPixels, _RejectedPixels ) ) {
			m_ppScanlinePixelsPool[m_ScanlinePixelIndex++] = Current;
			Previous = Current;
			Current = &P.Right();
		}
	}

	{	// Start going left
		CubeMapPixelWalker	P( *this, *_P );
		Pixel*	Previous = _P;
		Pixel*	Current = &P.Left();
		while ( CheckAndAcceptPixel( _Sample, *Previous, *Current, _AcceptedPixels, _RejectedPixels ) ) {
			m_ppScanlinePixelsPool[m_ScanlinePixelIndex++] = Current;
			Previous = Current;
			Current = &P.Left();
		}
	}

	int	ScanlineEndIndex = m_ScanlinePixelIndex;

	//////////////////////////////////////////////////////////////////////////
	// Recurse into each pixel of the top scanline
	RecursionLevel++;

	for ( int ScanlinePixelIndex=ScanlineStartIndex; ScanlinePixelIndex < ScanlineEndIndex; ScanlinePixelIndex++ ) {
		Pixel*	P = m_ppScanlinePixelsPool[ScanlinePixelIndex];

		CubeMapPixelWalker	Walker( *this, *P );
		Pixel*	Top = &Walker.Up();
		FloodFill( _Sample, P, Top, _AcceptedPixels, _RejectedPixels );
	}

	//////////////////////////////////////////////////////////////////////////
	// Recurse into each pixel of the bottom scanline
	for ( int ScanlinePixelIndex=ScanlineStartIndex; ScanlinePixelIndex < ScanlineEndIndex; ScanlinePixelIndex++ ) {
		Pixel*	P = m_ppScanlinePixelsPool[ScanlinePixelIndex];

		CubeMapPixelWalker	Walker( *this, *P );
		Pixel*	Bottom = &Walker.Down();
		FloodFill( _Sample, P, Bottom, _AcceptedPixels, _RejectedPixels );
	}

	RecursionLevel--;
}

bool	SHProbeEncoder::CheckAndAcceptPixel( Sample& _Sample, Pixel& _PreviousPixel, Pixel& _P, PixelsList& _AcceptedPixels, PixelsList& _RejectedPixels ) const {
	// Start by checking if we can use that pixel at all
	if ( !_P.IsFloodFillAcceptable( _Sample ) ) {
		return false;
	}

	// Check some additional criterions for a match
	bool	Accepted = false;

	// First, let's check the angular discrepancy
	float	Dot = _PreviousPixel.Normal | _P.Normal;
	if ( Dot > ANGULAR_THRESHOLD ) {
		// Next, let's check the distance discrepancy
		float3	P0 = _PreviousPixel.SmoothedDistance * _PreviousPixel.View;
		float3	P1 = _P.SmoothedDistance * _P.View;
		float	DistanceDiff = (P1 - P0).LengthSq();
		if ( DistanceDiff < DISTANCE_THRESHOLD*DISTANCE_THRESHOLD ) {
			// Next, let's check color discrepancy (I'm using the simplest metric here...)
			float	ColorDiff = (_PreviousPixel.Albedo - _P.Albedo).LengthSq();
			if ( ColorDiff < ALBEDO_RGB_THRESHOLD*ALBEDO_RGB_THRESHOLD ) {
				Accepted = true;	// Winner!
			}
		}
	}

	// Add the pixel to the proper list
	PixelsList&	Target = Accepted ? _AcceptedPixels : _RejectedPixels;
	_P.pNextInList = Target.pPixels;
	ASSERT( _P.pNextInList == NULL || (_P.pNextInList >= m_pCubeMapPixels && _P.pNextInList < m_pCubeMapPixels+6*CUBE_MAP_FACE_SIZE), "Oh!" );
	_P.pParentList = &Target;
	Target.pPixels = &_P;
	Target.PixelsCount++;
	Target.Importance += _P.Importance * _P.SolidAngle;

	return Accepted;
}

#pragma region Adjacency Walker

// Contains the new cube face index when stepping outside of a cube face through the left/right/top/bottom edge
static int	GoLeft[6] = {
	4,	// Step to +Z
	5,	// Step to -Z
	1,	// Step to -X
	1,	// Step to -X
	1,	// Step to -X
	0,	// Step to +X
};
static int	GoRight[6] = {
	5,	// Step to -Z
	4,	// Step to +Z
	0,	// Step to +X
	0,	// Step to +X
	0,	// Step to +X
	1,	// Step to -X
};
static int	GoDown[6] = {	// Go down means V+1!
	3,	// Step to -Y
	3,	// Step to -Y
	4,	// Step to -Z
	5,	// Step to +Z
	3,	// Step to -Y
	3,	// Step to -Y
};
static int	GoUp[6] = {		// Go up means V-1!
	2,	// Step to +Y
	2,	// Step to +Y
	5,	// Step to -Z
	4,	// Step to +Z
	2,	// Step to +Y
	2,	// Step to +Y
};

// Contains the matrices that indicate how the (U,V) pixel coordinates should be transformed to step from one face to the other
// Transforms arrays are simple 3x2 matrices:
//
//	    | Ru Rv |
//	M = | Du Dv |
//	    | Tu Tv |
//
// Which are used like this:
//
//	UV' = [U V 1] * M
//	 R' = [R R 0] * M
//	 D' = [D D 0] * M
//
// So for example:
//	U' = U * Ru + V * Du + Tu
//	V' = U * Rv + V * Dv + Tv
//
// You have to see the R(ight) and D(own) vectors in M as "what is the new Right/Down vector in this adjacent face?"
// Normally there's a jpeg image right next to this file that explains how I view the whole cubemap thing, following
//	the orientations described in https://msdn.microsoft.com/en-us/library/windows/desktop/bb204881%28v=vs.85%29.aspx
//
const int C = SHProbeEncoder::CUBE_MAP_SIZE;
const int C_ = SHProbeEncoder::CUBE_MAP_SIZE-1;
const int C2 = 2*C-1;

// Transforms are first ordered by cube face index, then by direction:
//	0 = Go left		(UV=[-1, V])
//	1 = Go right	(UV=[ C, V])
//	2 = Go up		(UV=[U, -1])
//	3 = Go down		(UV=[U,  C])
static const int	FaceTransforms[6][4][6] = {

	// Face #0 +X
	{
		// Going left sends us to +Z
		{	 1,  0,
			 0,  1,
			 C,  0 },	// UV' = [C-1, V]
		// Going right sends us to -Z
		{	 1,  0,
			 0,  1,
			-C,  0 },	// UV' = [0, V]
		// Going up sends us to +Y
		{	 0, -1,
			 1,  0,
			 C, C_ },	// UV' = [C-1, C-1-U]
		// Going down sends us to -Y
		{	 0,  1,
			-1,  0,
			C2,  0 },	// UV' = [C-1,U]
	},

	// Face #1 -X
	{
		// Going left sends us to -Z
		{	 1,  0,
			 0,  1,
			 C,  0 },	// UV' = [C-1, V]
		// Going right sends us to +Z
		{	 1,  0,
			 0,  1,
			-C,  0 },	// UV' = [0, V]
		// Going up sends us to +Y
		{	 0,  1,
			-1,  0,
			-1,  0 },	// UV' = [0, U]
		// Going down sends us to -Y
		{	 0, -1,
			 1,  0,
			-C, C_ },	// UV' = [0, C-1-U]
	},

	// Face #2 +Y
	{
		// Going left sends us to -X
		{	 0, -1,
			 1,  0,
			 0, -1 },	// UV' = [V,0]
		// Going right sends us to +X
		{	 0,  1,
			-1,  0,
			C_, -C },	// UV' = [C-1-V, 0]
		// Going up sends us to -Z
		{	-1,  0,
			 0, -1,
			C_, -1 },	// UV' = [C-1-U, 0]
		// Going down sends us to +Z
		{	 1,  0,
			 0,  1,
			 0, -C },	// UV' = [U, 0]
	},

	// Face #3 -Y
	{
		// Going left sends us to -X
		{	 0,  1,
			-1,  0,
			C_,  C },	// UV' = [C-1-V,C-1]
		// Going right sends us to +X
		{	 0, -1,
			 1,  0,
			 0, C2 },	// UV' = [V,C-1]
		// Going up sends us to +Z
		{	 1,  0,
			 0,  1,
			 0,  C },	// UV' = [U, C-1]
		// Going down sends us to -Z
		{	-1,  0,
			 0, -1,
			C_, C2 },	// UV' = [C-1-U, C-1]
	},

	// Face #4 +Z
	{
		// Going left sends us to -X
		{	 1,  0,
			 0,  1,
			 C,  0 },	// UV' = [C-1, V]
		// Going right sends us to +X
		{	 1,  0,
			 0,  1,
			-C,  0 },	// UV' = [0, V]
		// Going up sends us to +Y
		{	 1,  0,
			 0,  1,
			 0,  C },	// UV' = [U, C-1]
		// Going down sends us to -Y
		{	 1,  0,
			 0,  1,
			 0, -C },	// UV' = [U, 0]
	},

	// Face #5 -Z
	{
		// Going left sends us to +X
		{	 1,  0,
			 0,  1,
			 C,  0 },	// UV' = [C-1, V]
		// Going right sends us to -X
		{	 1,  0,
			 0,  1,
			-C,  0 },	// UV' = [0, V]
		// Going up sends us to +Y
		{	-1,  0,
			 0, -1,
			C_, -1 },	// UV' = [C-1-U, 0]
		// Going down sends us to -Y
		{	-1,  0,
			 0, -1,
			C_, C2 },	// UV' = [C-1-U, C-1]
	},
};

void SHProbeEncoder::CubeMapPixelWalker::Set( Pixel& _Pixel ) {
	CubeFaceIndex = _Pixel.CubeFaceIndex;
	pUV[0] = _Pixel.CubeFaceX;
	pUV[1] = _Pixel.CubeFaceY;
	pRight[0] = 1;	pRight[1] = 0;
	pDown[0] = 0;	pDown[1] = 1;
}
SHProbeEncoder::Pixel&	SHProbeEncoder::CubeMapPixelWalker::Get() const {
	int		FinalPixelIndex = CUBE_MAP_FACE_SIZE * CubeFaceIndex + CUBE_MAP_SIZE * pUV[1] + pUV[0];
	Pixel&	Result = Owner.m_pCubeMapPixels[FinalPixelIndex];
	return Result;
}
SHProbeEncoder::Pixel& SHProbeEncoder::CubeMapPixelWalker::Left() {
	GoToAdjacentPixel( -1, 0 );	// U-1
	return Get();
}
SHProbeEncoder::Pixel& SHProbeEncoder::CubeMapPixelWalker::Right() {
	GoToAdjacentPixel( +1, 0 );	// U+1
	return Get();
}
SHProbeEncoder::Pixel& SHProbeEncoder::CubeMapPixelWalker::Up() {
	GoToAdjacentPixel( 0, -1 );	// V-1
	return Get();
}
SHProbeEncoder::Pixel& SHProbeEncoder::CubeMapPixelWalker::Down() {
	GoToAdjacentPixel( 0, +1 );	// V+1
	return Get();
}
void	SHProbeEncoder::CubeMapPixelWalker::TransformUV( const int _Transform[6] ) {
	// Transform position
	int	TempU = pUV[0] * _Transform[0] + pUV[1] * _Transform[2] + _Transform[4];
	int	TempV = pUV[0] * _Transform[1] + pUV[1] * _Transform[3] + _Transform[5];
	pUV[0] = TempU;
	pUV[1] = TempV;
	ASSERT( pUV[0]==0 || pUV[0]==C-1 || pUV[1]==0 || pUV[1]==C-1, "At least one of the coordinates should be at an edge!" );

	// Transform Right & Down directions
	int	pOldRight[2] = { pRight[0], pRight[1] };
	int	pOldDown[2] = { pDown[0], pDown[1] };
	pRight[0] = pOldRight[0] * _Transform[0] + pOldRight[1] * _Transform[2];
	pRight[1] = pOldRight[0] * _Transform[1] + pOldRight[1] * _Transform[3];
	pDown[0] = pOldDown[0] * _Transform[0] + pOldDown[1] * _Transform[2];
	pDown[1] = pOldDown[0] * _Transform[1] + pOldDown[1] * _Transform[3];
}

void	SHProbeEncoder::CubeMapPixelWalker::GoToAdjacentPixel( int _dU, int _dV ) {
	pUV[0] += _dU * pRight[0] + _dV * pDown[0];
	pUV[1] += _dU * pRight[1] + _dV * pDown[1];

	if ( pUV[0] < 0 ) {
		// Stepped out through left side
		TransformUV( FaceTransforms[CubeFaceIndex][0] );
		CubeFaceIndex = GoLeft[CubeFaceIndex];
	}
	if ( pUV[0] >= CUBE_MAP_SIZE ) {
		// Stepped out through right side
		TransformUV( FaceTransforms[CubeFaceIndex][1] );
		CubeFaceIndex = GoRight[CubeFaceIndex];
	}
	if ( pUV[1] < 0 ) {
		// Stepped out through top side
		TransformUV( FaceTransforms[CubeFaceIndex][2] );
		CubeFaceIndex = GoUp[CubeFaceIndex];
	}
	if ( pUV[1] >= CUBE_MAP_SIZE ) {
		// Stepped out through bottom side
		TransformUV( FaceTransforms[CubeFaceIndex][3] );
		CubeFaceIndex = GoDown[CubeFaceIndex];
	}
}

#pragma endregion

#pragma endregion

#pragma endregion


//////////////////////////////////////////////////////////////////////////
//
void	SHProbeEncoder::ReadBackProbeCubeMap( Texture2D& _StagingCubeMap, U32 _SceneTotalFacesCount ) {

	m_NearestNeighborProbeDistance = 0.0f;
	m_FarthestNeighborProbeDistance = 0.0f;

	m_ScenePixelsCount = 0;

	m_MeanDistance = 0.0;
	m_MeanHarmonicDistance = 0.0;
	m_MinDistance = 1e6;
	m_MaxDistance = 0.0;
	m_BBoxMin =  float3::MaxFlt;
	m_BBoxMax = -float3::MaxFlt;

	// Clear face influences
	m_ProbeInfluencePerFace.Init( _SceneTotalFacesCount );
	m_ProbeInfluencePerFace.SetCount( _SceneTotalFacesCount );
	memset( &m_ProbeInfluencePerFace[0], 0, _SceneTotalFacesCount*sizeof(double) );

	// Read back pixels
	int		NegativeImportancePixelsCount = 0;
	for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ ) {
		Pixel*	pCubeMapPixels = &m_pCubeMapPixels[CubeFaceIndex*CUBE_MAP_FACE_SIZE];

		// Fill up albedo
		D3D11_MAPPED_SUBRESOURCE	Map0 = _StagingCubeMap.Map( 0, 6*0+CubeFaceIndex );
		D3D11_MAPPED_SUBRESOURCE	Map1 = _StagingCubeMap.Map( 0, 6*1+CubeFaceIndex );
		D3D11_MAPPED_SUBRESOURCE	Map2 = _StagingCubeMap.Map( 0, 6*2+CubeFaceIndex );
		D3D11_MAPPED_SUBRESOURCE	Map3 = _StagingCubeMap.Map( 0, 6*3+CubeFaceIndex );

		float4*	pFaceData0 = (float4*) Map0.pData;
		float4*	pFaceData1 = (float4*) Map1.pData;
		float4*	pFaceData2 = (float4*) Map2.pData;
		float4*	pFaceData3 = (float4*) Map3.pData;

		Pixel*	P = pCubeMapPixels;
		for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
			for ( int X=0; X < CUBE_MAP_SIZE; X++, P++, pFaceData0++, pFaceData1++, pFaceData2++, pFaceData3++ ) {
				// ==== Read back albedo & unique face ID ====
				float	Red = pFaceData0->x;
				float	Green = pFaceData0->y;
				float	Blue = pFaceData0->z;

// 				// Work with better precision
// 				Red *= PI;
// 				Green *= PI;
// 				Blue *= PI;

				P->SetAlbedo( float3( Red, Green, Blue ) );
				P->FaceIndex = ((U32&) pFaceData0->w);

				// ==== Read back static lighting & emissive material IDs ====
				P->StaticLitColor = *pFaceData2;
				P->EmissiveMatID = ((U32&) pFaceData2->w);

				// ==== Read back position & normal ====
				float	Nx = pFaceData1->x;
				float	Ny = pFaceData1->y;
				float	Nz = pFaceData1->z;
				float	Distance = pFaceData1->w;

				float3	wsPosition( Distance * P->View.x, Distance * P->View.y, Distance * P->View.z );

				P->Position = wsPosition;
				P->Normal.Set( Nx, Ny, Nz );
				P->Normal.Normalize();

				// ==== Read back neighbor probes ID ====
				P->NeighborProbeID = ((U32&) pFaceData3->x);
				P->NeighborProbeDistance = pFaceData3->y;


				// ==== Finalize pixel information ====
				P->Importance = -P->View.Dot( P->Normal ) / (Distance * Distance);
				if ( P->Importance < 0.0 ) {
// P->Normal = -P->Normal;
// P->Importance = -P->View.Dot( P->Normal ) / (Distance * Distance);
// //P->Importance = -P->Importance;
NegativeImportancePixelsCount++;
//					throw new Exception( "WTH?? Negative importance here!" );
				}
				P->Distance = Distance;
				P->Infinity = Distance > Z_INFINITY_TEST;

				if ( P->Infinity )
					continue;	// Not part of the scene's geometry!

				// Account for a new scene pixel (i.e. not infinity)
				m_ScenePixelsCount++;

				// Accumulate face influence
				ASSERT( P->FaceIndex != ~0UL, "Invalid face index!" );
				ASSERT( P->FaceIndex < U32(m_ProbeInfluencePerFace.GetAllocatedSize()), "Face index out of range!" );
				m_ProbeInfluencePerFace[P->FaceIndex] += P->SolidAngle * P->Importance;

				// Update dimensions
				m_MeanDistance += Distance;
				m_MeanHarmonicDistance += 1.0 / Distance;
				m_MinDistance = min( m_MinDistance, Distance );
				m_MaxDistance = max( m_MaxDistance, Distance );
				m_BBoxMin.Max( wsPosition );
				m_BBoxMax.Min( wsPosition );
			}

		_StagingCubeMap.UnMap( 0, 6*3+CubeFaceIndex );
		_StagingCubeMap.UnMap( 0, 6*2+CubeFaceIndex );
		_StagingCubeMap.UnMap( 0, 6*1+CubeFaceIndex );
		_StagingCubeMap.UnMap( 0, 6*0+CubeFaceIndex );
	}

	if ( float(NegativeImportancePixelsCount) / (CUBE_MAP_SIZE * CUBE_MAP_SIZE * 6) > 0.1f )
		ASSERT( false, "More than 10% invalid pixels with inverted normals in that probe!" );

	m_MeanDistance /= (CUBE_MAP_SIZE * CUBE_MAP_SIZE * 6);
	m_MeanHarmonicDistance = (CUBE_MAP_SIZE * CUBE_MAP_SIZE * 6) / m_MeanHarmonicDistance;

	// Perform bilateral filtered smoothing of adjacent distances, static lit colors & infinity values for smoother SH encoding
	for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ ) {
		Pixel*	pCubeMapPixels = &m_pCubeMapPixels[CubeFaceIndex*CUBE_MAP_FACE_SIZE];
		Pixel*	P = pCubeMapPixels;
		for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ ) {
			for ( int X=0; X < CUBE_MAP_SIZE; X++, P++ ) {

				// Gather the 8 pixels around this one
				CubeMapPixelWalker	Walk( *this, *P );
				Pixel&	P01 = Walk.Up();	// Top
				Pixel&	P02 = Walk.Right();	// Top Right
				Pixel&	P12 = Walk.Down();	// Right
				Pixel&	P22 = Walk.Down();	// Bottom Right
				Pixel&	P21 = Walk.Left();	// Bottom
				Pixel&	P20 = Walk.Left();	// Bottom Left
				Pixel&	P10 = Walk.Up();	// Left
				Pixel&	P00 = Walk.Up();	// Top left

				// Average distance, filtering out the pixels at infinity
				float	SumDistance = !P->Infinity ? P->Distance : 0.0f;
				int		Count = P->Infinity ? 0 : 1;
				if ( !P00.Infinity ) { SumDistance += P00.Distance; Count++; }
				if ( !P01.Infinity ) { SumDistance += P01.Distance; Count++; }
				if ( !P02.Infinity ) { SumDistance += P02.Distance; Count++; }
				if ( !P10.Infinity ) { SumDistance += P10.Distance; Count++; }
				if ( !P12.Infinity ) { SumDistance += P12.Distance; Count++; }
				if ( !P20.Infinity ) { SumDistance += P20.Distance; Count++; }
				if ( !P21.Infinity ) { SumDistance += P21.Distance; Count++; }
				if ( !P22.Infinity ) { SumDistance += P22.Distance; Count++; }
				P->SmoothedDistance = (!P->Infinity && Count > 0) ? SumDistance / Count : Z_INFINITY;

				// Average static lit color
				float3	SumColor = !P->Infinity ? P->StaticLitColor : float3::Zero;
				Count = P->Infinity ? 0 : 1;
				if ( !P00.Infinity ) { SumColor = SumColor + P00.StaticLitColor; Count++; }
				if ( !P01.Infinity ) { SumColor = SumColor + P01.StaticLitColor; Count++; }
				if ( !P02.Infinity ) { SumColor = SumColor + P02.StaticLitColor; Count++; }
				if ( !P10.Infinity ) { SumColor = SumColor + P10.StaticLitColor; Count++; }
				if ( !P12.Infinity ) { SumColor = SumColor + P12.StaticLitColor; Count++; }
				if ( !P20.Infinity ) { SumColor = SumColor + P20.StaticLitColor; Count++; }
				if ( !P21.Infinity ) { SumColor = SumColor + P21.StaticLitColor; Count++; }
				if ( !P22.Infinity ) { SumColor = SumColor + P22.StaticLitColor; Count++; }
				P->SmoothedStaticLitColor = Count > 0 ? SumColor / float(Count) : float3::Zero;

				// Average infinity
				float	SumInfinity  = P->Infinity;
						SumInfinity += P00.Infinity;
						SumInfinity += P01.Infinity;
						SumInfinity += P02.Infinity;
						SumInfinity += P10.Infinity;
						SumInfinity += P12.Infinity;
						SumInfinity += P20.Infinity;
						SumInfinity += P21.Infinity;
						SumInfinity += P22.Infinity;
				P->SmoothedInfinity = SumInfinity / 9;
			}
		}
	}
}

//////////////////////////////////////////////////////////////////////////
//
const double	SHProbeEncoder::Pixel::f0 = 0.5 / sqrt(PI);
const double	SHProbeEncoder::Pixel::f1 = sqrt(3.0) * SHProbeEncoder::Pixel::f0;
const double	SHProbeEncoder::Pixel::f2 = sqrt(15.0) * SHProbeEncoder::Pixel::f0;
const double	SHProbeEncoder::Pixel::f3 = sqrt(5.0) * 0.5 * SHProbeEncoder::Pixel::f0;

float	SHProbeEncoder::Pixel::IMPORTANCE_THRESOLD = 0.0f;

SHProbeEncoder::Pixel::RadixNode_t*	SHProbeEncoder::Pixel::ms_RadixNodes[2] = { NULL, NULL };
void	SHProbeEncoder::Pixel::Sort( Pixel*& _pList, ISortKeyProvider& _KeyProvider, bool _ReverseSortOnExit ) {
	// Convert linked-list into a sortable list
	Pixel*			pSource = _pList;
	RadixNode_t*	pTarget = ms_RadixNodes[0];
	while ( pSource != NULL ) {
		pTarget->Key = _KeyProvider.GetKey( *pSource );
		pTarget->pPixel = pSource;
		pTarget++;
		pSource = pSource->pNext;
	}

	U32	ElementsCount = pTarget - ms_RadixNodes[0];
	if ( ElementsCount < 2 ) {
		return;	// Nothing to sort here...
	}

	// Sort
	Sort( ElementsCount, ms_RadixNodes[0], ms_RadixNodes[1] );

	// Rebuild sorted linked-list
	if ( _ReverseSortOnExit ) {
		// Reversed, largest to smallest sort
		RadixNode_t*	pNode = ms_RadixNodes[1];
		_pList = NULL;
		for ( U32 i=0; i < ElementsCount; i++, pNode++ ) {
			pNode->pPixel->pNext = _pList;
			_pList = pNode->pPixel;
		}
	} else {
		// Standard, smallest to largest sort
		RadixNode_t*	pNode = ms_RadixNodes[1];
		for ( U32 i=0; i < ElementsCount-1; i++, pNode++ ) {
			pNode->pPixel->pNext = pNode[1].pPixel;
		}
		pNode->pPixel->pNext = NULL;
		_pList = ms_RadixNodes[1]->pPixel;
	}
}

//////////////////////////////////////////////////////////////////////////
// 11-bits Radix sort from Michael Herf (http://stereopsis.com/radix.html)
// (without the floating-point sign flipping because we don't care about that here)
//

// ---- utils for accessing 11-bit quantities
#define _0(x)	(x & 0x7FF)
#define _1(x)	(x >> 11 & 0x7FF)
#define _2(x)	(x >> 22 )

void	SHProbeEncoder::Pixel::Sort( U32 _ElementsCount, RadixNode_t* _pList, RadixNode_t* _pSorted ) {
	U32		i;

	// 3 histograms on the stack:
	const U32	kHist = 2048;		// 11 bits
	U32			b0[kHist * 3];
	U32*		b1 = b0 + kHist;
	U32*		b2 = b1 + kHist;

	for ( i = 0; i < kHist * 3; i++ ) {
		b0[i] = 0;
	}
	//memset(b0, 0, kHist * 12);

	// 1. parallel histogramming pass
	//
	RadixNode_t*	pNode = _pList;
	for ( i=0; i < _ElementsCount; i++, pNode++ ) {
		U32	fi = pNode->Key;
		b0[_0(fi)]++;
		b1[_1(fi)]++;
		b2[_2(fi)]++;
	}
	
	// 2. Sum the histograms -- each histogram entry records the number of values preceding itself.
	{
		U32 sum0 = 0, sum1 = 0, sum2 = 0;
		U32 tsum;
		for ( i=0; i < kHist; i++ ) {

			tsum = b0[i] + sum0;
			b0[i] = sum0 - 1;
			sum0 = tsum;

			tsum = b1[i] + sum1;
			b1[i] = sum1 - 1;
			sum1 = tsum;

			tsum = b2[i] + sum2;
			b2[i] = sum2 - 1;
			sum2 = tsum;
		}
	}

	// byte 0: read/write histogram, write out to sorted
	pNode = _pList;
	for ( i=0; i < _ElementsCount; i++, pNode++ ) {
		U32 fi = pNode->Key;
		U32 pos = _0(fi);
		_pSorted[++b0[pos]] = *pNode;
	}

	// byte 1: read/write histogram, copy sorted -> original
	pNode = _pSorted;
	for ( i=0; i < _ElementsCount; i++, pNode++ ) {
		U32	si = pNode->Key;
		U32	pos = _1(si);
		_pList[++b1[pos]] = *pNode;
	}

	// byte 2: read/write histogram, copy original -> sorted
	pNode = _pList;
	for ( i=0; i < _ElementsCount; i++, pNode++ ) {
		U32 ai = pNode->Key;
		U32 pos = _2(ai);
		_pSorted[++b2[pos]] = *pNode;
	}
}
