#include "../../GodComplex.h"
#include "SHProbe.h"

#pragma region Static Sample Directions

// Samples directions were generated using the Tools > Voronoï Visualizer project
//	1) Enter the amount of samples as "neighbors"
//	2) Press "Simulate" and "Render Cell"
//	3) Wait a bit until the cell is stabilized
//	4) Press "Copy Directions to Clipboard"
//	5) Paste here...
//
float3	SHProbe::ms_SampleDirections[SHProbe::SAMPLES_COUNT] = {
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

bool	SHProbe::IsInsideVoronoiCell( const float3& _Position ) const {
	const VoronoiProbeInfo*	pPlane = &m_VoronoiProbes[0];
	for ( int PlaneIndex=0; PlaneIndex < m_VoronoiProbes.GetCount(); PlaneIndex++, pPlane++ ) {
		float3	D = _Position - pPlane->Position;
		if ( D.Dot( pPlane->Normal ) < 0.0f )
			return false;	// Outside!
	}
	return true;
}

//////////////////////////////////////////////////////////////////////////
// I/O
//
FILE*	g_pFile = NULL;
template< typename T> void	Write( const T& _value ) {
	fwrite( &_value, sizeof(T), 1, g_pFile );
}
template< typename T> void	Read( T& _value ) {
	fread_s( &_value, sizeof(T), sizeof(T), 1, g_pFile );
}

void	SHProbe::Save( FILE* _pFile ) const {

	g_pFile = _pFile;

	// Write the mean, harmonic mean, min, max distances
	Write( (float) m_MeanDistance );
	Write( (float) m_MeanHarmonicDistance );
	Write( (float) m_MinDistance );
	Write( (float) m_MaxDistance );

	// Write the BBox
	Write( m_BBoxMin );
	Write( m_BBoxMax );

	// Write static SH
	for ( int i=0; i < 9; i++ )
		Write( m_pSHStaticLighting[i] );

	// Write occlusion SH
	for ( int i=0; i < 9; i++ )
		Write( m_pSHOcclusion[i] );

	// Write the result samples
	for ( U32 i=0; i < SHProbe::SAMPLES_COUNT; i++ ) {
		const Sample&	S = m_pSamples[i];

		// Write position, normal, albedo
		Write( S.Position );
		Write( S.Normal );
		Write( S.Tangent );
		Write( S.BiTangent );

		Write( S.Radius );

		Write( S.Albedo );

		Write( S.F0 );

		// Write the pixel coverage of the sample
		Write( S.SHFactor );

// No need: regenerated at runtime through fixed array of sample directions
// 		// Write SH coefficients
// 		for ( int i=0; i < 9; i++ ) {
// 			Write( S.SH[i] );
// 		}
	}

	// Write the emissive surfaces
	Write( m_EmissiveSurfacesCount );
	for ( U32 i=0; i < m_EmissiveSurfacesCount; i++ ) {
		const EmissiveSurface&	S = m_pEmissiveSurfaces[i];

		// Write emissive mat ID
		Write( S.MaterialID );

		// Write SH coefficients (we only write luminance here, we don't have the color info, which is provided at runtime)
		for ( int i=0; i < 9; i++ )
			Write( S.pSH[i] );
	}

	// Write nearest/farthest probe distances
	Write( m_NearestNeighborProbeDistance );
	Write( m_FarthestNeighborProbeDistance );

	// Write the neighbor probes
	Write( m_NeighborProbes.GetCount() );
	for ( int i=0; i < m_NeighborProbes.GetCount(); i++ ) {
		const NeighborProbeInfo&	NP = m_NeighborProbes[i];

		// Write probe ID, distance, solid angle, direction
		Write( NP.ProbeID );
		Write( NP.DirectlyVisible );
		Write( NP.Distance );
		Write( NP.SolidAngle );
		Write( NP.Direction );

		// Write SH coefficients (only luminance here since they're used for the product with the neighbor probe's SH)
		for ( int i=0; i < 9; i++ )
			Write( NP.SH[i] );
	}

	// Write the Voronoï probes
	Write( m_VoronoiProbes.GetCount() );
	for ( int i=0; i < m_VoronoiProbes.GetCount(); i++ ) {
		const VoronoiProbeInfo&	VP = m_VoronoiProbes[i];

		// Write probe ID, distance, solid angle, direction
		Write( VP.ProbeID );
		Write( VP.Position );
		Write( VP.Normal );
	}
}

void	SHProbe::Load( FILE* _pFile ) {

	g_pFile = _pFile;

	// Read the boundary infos
	Read( m_MeanDistance );
	Read( m_MeanHarmonicDistance );
	Read( m_MinDistance );
	Read( m_MaxDistance );

	Read( m_BBoxMin );
	Read( m_BBoxMax );

	// Read static SH
	for ( int i=0; i < 9; i++ )
		Read( m_pSHStaticLighting[i] );

	// Read occlusion SH
	for ( int i=0; i < 9; i++ )
		Read( m_pSHOcclusion[i] );

	// Read the samples
	for ( U32 SampleIndex=0; SampleIndex < SHProbe::SAMPLES_COUNT; SampleIndex++ ) {
		Sample&	S = m_pSamples[SampleIndex];

		// Read position, normal, albedo
		Read( S.Position );
		Read( S.Normal );
		Read( S.Tangent );
		Read( S.BiTangent );

		Read( S.Radius );

		Read( S.Albedo );
		S.Albedo = S.Albedo * INVPI;	// Ready for upload!

		Read( S.F0 );

		Read( S.SHFactor );
			
// No need: can be regenerated at runtime from normal direction
// 			// Read SH coefficients
// 			for ( int i=0; i < 9; i++ ) {
// 				fread_s( &S.pSHBounce[i], sizeof(S.pSHBounce[i]), sizeof(float), 1, _pFile );
// 			}

		// Transform sample's position/normal by probe's LOCAL=>WORLD
		S.Position = float3( m_pSceneProbe->m_Local2World.GetRow(3) ) + S.Position;
// 			NjFloat3	wsSetNormal = Set.Normal;
// 			NjFloat3	wsSetTangent = Set.Tangent;
// 			NjFloat3	wsSetBiTangent = Set.BiTangent;
// TODO: Handle non-identity matrices! Let's go fast for now...
// ARGH! That also means possibly rotating the SH!
// Let's just force the probes to be axis-aligned, shall we??? :) (lazy man talking) (no, seriously, it makes sense after all)
	}

	// Read the amount of emissive surfaces
	U32	EmissiveSurfacesCount;
	fread_s( &EmissiveSurfacesCount, sizeof(EmissiveSurfacesCount), sizeof(U32), 1, _pFile );
	EmissiveSurfacesCount = MIN( SHProbe::MAX_EMISSIVE_SURFACES, EmissiveSurfacesCount );	// Don't read more than we can chew!

	// Read the surfaces
	SHProbe::EmissiveSurface	DummyEmissiveSurface;
	for ( U32 SampleIndex=0; SampleIndex < EmissiveSurfacesCount; SampleIndex++ ) {
		SHProbe::EmissiveSurface&	S = SampleIndex < EmissiveSurfacesCount ? m_pEmissiveSurfaces[SampleIndex] : DummyEmissiveSurface;	// We load into a useless surface if out of range...

		// Read emissive material ID
		Read( S.MaterialID );

		// Read SH coefficients
		for ( int i=0; i < 9; i++ )
			Read( S.pSH[i] );
	}

	// Read nearest/farthest probe distances
	Read( m_NearestNeighborProbeDistance );
	Read( m_FarthestNeighborProbeDistance );

	// Read the amount of neighbor probes & distance infos
	U32	NeighborProbesCount;
	Read( NeighborProbesCount );
	m_NeighborProbes.Init( NeighborProbesCount );
	m_NeighborProbes.SetCount( NeighborProbesCount );

	for ( U32 NeighborProbeIndex=0; NeighborProbeIndex < NeighborProbesCount; NeighborProbeIndex++ ) {
		Read( m_NeighborProbes[NeighborProbeIndex].ProbeID );
		Read( m_NeighborProbes[NeighborProbeIndex].DirectlyVisible );
		Read( m_NeighborProbes[NeighborProbeIndex].Distance );
		Read( m_NeighborProbes[NeighborProbeIndex].SolidAngle );
		Read( m_NeighborProbes[NeighborProbeIndex].Direction );
		for ( int i=0; i < 9; i++ )
			Read( m_NeighborProbes[NeighborProbeIndex].SH[i] );
	}

	// Read the amount of Voronoï probes
	U32	VoronoiProbesCount;
	Read( VoronoiProbesCount );
	m_VoronoiProbes.Init( VoronoiProbesCount );
	m_VoronoiProbes.SetCount( VoronoiProbesCount );

	for ( U32 VoronoiProbeIndex=0; VoronoiProbeIndex < VoronoiProbesCount; VoronoiProbeIndex++ ) {
		Read( m_VoronoiProbes[VoronoiProbeIndex].ProbeID );
		Read( m_VoronoiProbes[VoronoiProbeIndex].Position );
		Read( m_VoronoiProbes[VoronoiProbeIndex].Normal );
	}
}