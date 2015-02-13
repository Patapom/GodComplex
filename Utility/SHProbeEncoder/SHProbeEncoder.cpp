#include "../../GodComplex.h"

const float	SHProbeEncoder::Z_INFINITY = 1e6f;
const float	SHProbeEncoder::Z_INFINITY_TEST = 0.99f * SHProbeEncoder::Z_INFINITY;

SHProbeEncoder::SHProbeEncoder() {
	m_CubeMapPixels = new Pixel[6*CUBE_MAP_SIZE*CUBE_MAP_SIZE];
	Pixel*	pPixel = m_CubeMapPixels;
	for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
		for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
			for ( int X=0; X < CUBE_MAP_SIZE; X++, pPixel++ ) {
				pPixel->Index = pPixel - m_CubeMapPixels;
				pPixel->CubeFaceIndex = CubeFaceIndex;
				pPixel->CubeFaceX = X;
				pPixel->CubeFaceY = Y;
			}

	m_ProbePixels.Init( 6*CUBE_MAP_SIZE*CUBE_MAP_SIZE );
	m_ScenePixels.Init( 6*CUBE_MAP_SIZE*CUBE_MAP_SIZE );

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
}

SHProbeEncoder::~SHProbeEncoder() {
	SAFE_DELETE_ARRAY( m_CubeMapPixels );
}

void	SHProbeEncoder::EncodeProbeCubeMap( Texture2D& _StagingCubeMap, U32 _ProbeID ) {

	// 1] Read back probe data and prepare pixels for encoding
	ReadBackProbeCubeMap( _StagingCubeMap );

	// 2] Encode
	m_PatchesCount = 0;
	m_EmissivePatchesCount = 0;
	m_NeighborProbesCount = 0;

}

#pragma region Computes Sets by Flood Fill Method

int	DEBUG_PixelIndex = 0;

void	SHProbeEncoder::ComputeFloodFill( int _MaxSetsCount, int _MaxLightingSamplesCount, float _SpatialDistanceWeight, float _NormalDistanceWeight, float _AlbedoDistanceWeight, float _MinimumImportanceDiscardThreshold ) {
	// Clear the sets for each pixel
	foreach ( Pixel P in m_ProbePixels )
		P.ParentSet = NULL;

	// Setup the reference thresholds for pixels' acceptance
//			Pixel.IMPORTANCE_THRESOLD = (float) ((4.0f * Math.PI / CUBE_MAP_FACE_SIZE) / (m_MeanDistance * m_MeanDistance));	// Compute an average solid angle threshold based on average pixels' distance
	Pixel::IMPORTANCE_THRESOLD = (float) (0.01f * _MinimumImportanceDiscardThreshold / (m_MeanHarmonicDistance * m_MeanHarmonicDistance));	// Simply use the mean harmonic distance as a good approximation of important pixels
																									// Pixels that are further or not facing the probe will have less importance...

	DISTANCE_THRESHOLD = 0.02f * _SpatialDistanceWeight;							// 2cm
	ANGULAR_THRESHOLD = (float) acosf( 45.0 * _NormalDistanceWeight * PI / 180 );	// 45° (we're very generous here!)
	ALBEDO_HUE_THRESHOLD = 0.04f * _AlbedoDistanceWeight;							// Close colors!
	ALBEDO_RGB_THRESHOLD = 0.32f * _AlbedoDistanceWeight;							// Close colors!

	//////////////////////////////////////////////////////////////////////////
	// 1] Compute occlusion & static lighting SH
	double	SHR[9];
	double	SHG[9];
	double	SHB[9];
	double	SHOcclusion[9];

	for ( int PixelIndex=0; PixelIndex < m_ProbePixels.GetCount(); PixelIndex++ )
	{
		Pixel	P = m_ProbePixels[PixelIndex];
		for ( int i=0; i < 9; i++ )
		{
			SHR[i] += P.SHCoeffs[i] * P.SolidAngle * P.StaticLitColor.x;
			SHG[i] += P.SHCoeffs[i] * P.SolidAngle * P.StaticLitColor.y;
			SHB[i] += P.SHCoeffs[i] * P.SolidAngle * P.StaticLitColor.z;
		}

		if ( !P.Infinity )
			continue;

		// No obstacle means direct lighting from the ambient sky...
		// Accumulate SH coefficients in that direction, weighted by the solid angle
		for ( int i=0; i < 9; i++ )
			SHOcclusion[i] += P.SolidAngle * P.SHCoeffs[i];
	}

// DON'T NORMALIZE	=> 4PI is part of the integral!!!
//			double	Normalizer = 1.0 / (4.0 * Math.PI);
	double	Normalizer = 1.0;
	for ( int i=0; i < 9; i++ )
	{
		m_StaticSH[i].Set( (float) (Normalizer * SHR[i]), (float) (Normalizer * SHG[i]), (float) (Normalizer * SHB[i]) );
		m_OcclusionSH[i] = (float) (Normalizer * SHOcclusion[i]);
	}

	// Apply filtering
// 	SphericalHarmonics.SHFunctions.FilterLanczos( m_StaticSH, 3 );		// Lanczos should be okay for static lighting
// 	SphericalHarmonics.SHFunctions.FilterHanning( m_OcclusionSH, 3 );


	//////////////////////////////////////////////////////////////////////////
	// 2] Compute the influence of the probe on each scene face
	m_MaxFaceIndex = 0;
	m_ProbeInfluencePerFace.Clear();
	for ( int PixelIndex=0; PixelIndex < m_ProbePixels.GetCount(); PixelIndex++ )
	{
		Pixel	P = m_ProbePixels[PixelIndex];
		if ( P.Infinity )
			continue;

		if ( !m_ProbeInfluencePerFace.ContainsKey( P.FaceIndex ) )
			m_ProbeInfluencePerFace[P.FaceIndex] = 0.0f;

		m_ProbeInfluencePerFace[P.FaceIndex] += P.SolidAngle;
		m_MaxFaceIndex = max( m_MaxFaceIndex, P.FaceIndex );
	}


	//////////////////////////////////////////////////////////////////////////
	// 3] Build the neighbor probes network
	m_NeighborProbes.Clear();

	Dictionary<int,NeighborProbe>	NeighborProbeID2Probe = new Dictionary<int,NeighborProbe>();
	for ( int PixelIndex=0; PixelIndex < m_ProbePixels.Count; PixelIndex++ )
	{
		Pixel	P = m_ProbePixels[PixelIndex];
		if ( P.NeighborProbeID == -1 )
			continue;

		NeighborProbe	NP = null;
		if ( !NeighborProbeID2Probe.ContainsKey( P.NeighborProbeID ) )
		{
			NP = new NeighborProbe();
			NP.ProbeID = P.NeighborProbeID;
			NeighborProbeID2Probe[P.NeighborProbeID] = NP;
			m_NeighborProbes.Add( NP );
		}
		else
			NP = NeighborProbeID2Probe[P.NeighborProbeID];

		// Accumulate direction, solid angle & distance
		NP.SolidAngle += P.SolidAngle;
		NP.Distance += P.NeighborProbeDistance;
		NP.Direction += P.View;

		// Accumulate SH
		for ( int i=0; i < 9; i++ )
			NP.SH[i] += P.SolidAngle * P.SHCoeffs[i];

		NP.PixelsCount++;
	}

	// Normalize everything
	m_NearestNeighborProbeDistance = FLT_MAX;
	m_FarthestNeighborProbeDistance = 0.0f;
	foreach ( NeighborProbe NP in m_NeighborProbes )
	{
		NP.Distance /= NP.PixelsCount;
		NP.Direction.Normalize();
		for ( int i=0; i < 9; i++ )
			NP.SH[i] /= 4.0 * PI;

		m_NearestNeighborProbeDistance = min( m_NearestNeighborProbeDistance, NP.Distance );
		m_FarthestNeighborProbeDistance = max( m_FarthestNeighborProbeDistance, NP.Distance );
	}

	// Sort from most important to least important
	m_NeighborProbes.Sort( this );


	//////////////////////////////////////////////////////////////////////////
	// 4] Iterate on the list of free pixels that belong to no set and create iterative sets
	List<Patch*>	Patches;
	for ( int PixelIndex=0; PixelIndex < m_ProbePixels.GetCount(); PixelIndex++ )
	{
DEBUG_PixelIndex = PixelIndex;

		Pixel	P0 = m_ProbePixels[PixelIndex];
		if ( P0.IsFloodFillAcceptable() )
		{
			// Create a new set for this pixel
			Patch	S = new Patch() { Position = P0.Position, Normal = P0.Normal, Distance = P0.Distance, EmissiveMatID = P0.EmissiveMatID };
				S.SetAlbedo( P0.Albedo );
			Patches.Add( S );


// if ( PixelIndex == 0x2680 )
// 	P0.Albedo.x += 1e-6f;

// if ( P0.IsEmissive )
//  	P0.Albedo.x += 1e-6f;


			// Flood fill adjacent pixels based on a criterion
 			List<Pixel*>	SetRejectedPixels;
			try
			{
				S.Pixels.Clear();

				m_ScanlinePixelIndex = 0;	// VEEERY important line where we reset the pixel index of the pool of flood filled pixels!
				FloodFill( S, S, P0, SetRejectedPixels );

				ASSERT( m_ScanlinePixelIndex != 0, "Can't have empty sets!" );
			}
			catch ( Exception )
			{
				// Oops! Stack overflow!
				continue;
			}

			// Remove rejected pixels from the set (we only temporarily marked them to avoid them being processed twice by the flood filler)
			foreach ( Pixel P in SetRejectedPixels )
				P.ParentSet = null;	// Ready for another round!

			// Finalize importance
			S.Importance /= S.SetPixels.Count;
// DEBUG
//break;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Try and merge sets together

		// Merge separate emissive sets that have the same mat ID together
	List<Patch*>	EmissivePatches = new List<Patch*>();

	m_Patches = Patches.ToArray();
	for ( int SetIndex0=0; SetIndex0 < m_Patches.Length-1; SetIndex0++ )
	{
		Patch	S0 = m_Patches[SetIndex0];
		if ( S0.IsEmissive && S0.ParentSet == null )
		{
			// Remove it from regular sets and inscribe it as an emissive set
			EmissivePatches.Add( S0 );
			Patches.Remove( S0 );

			// Merge with any other set with same Mat ID
			for ( int SetIndex1=SetIndex0+1; SetIndex1 < m_Patches.Length; SetIndex1++ )
			{
				Patch	S1 = m_Patches[SetIndex1];
				if ( !S1.IsEmissive || S1.EmissiveMatID != S0.EmissiveMatID )
					continue;

				// Merge!
				S0.Importance *= S0.SetPixels.Count;
				S1.Importance *= S1.SetPixels.Count;
				S0.SetPixels.AddRange( S1.SetPixels );
				S0.Importance = (S0.Importance + S1.Importance) / S0.SetPixels.Count;

				// Remove the merged set
				Patches.Remove( S1 );
				S1.ParentSet = S0;	// Mark S0 as its parent so it doesn't get processed again
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Sort and cull unimportant sets

	// Sort and cull sets above our designated maximum
	Patches.Sort( this );
	if ( Patches.Count > _MaxSetsCount )
	{	// Cull sets above our chosen value
		for ( int SetIndex=_MaxSetsCount; SetIndex < Patches.Count; SetIndex++ )
			Patches[SetIndex].SetIndex = -1;	// Invalid index for invalid sets!

		Patches.RemoveRange( _MaxSetsCount, Patches.Count - _MaxSetsCount );
	}

	// Do the same for emissive sets
	EmissivePatches.Sort( this );
	if ( EmissivePatches.Count > _MaxSetsCount )
	{	// Cull sets above our chosen value
		for ( int SetIndex=_MaxSetsCount; SetIndex < EmissivePatches.Count; SetIndex++ )
			EmissivePatches[SetIndex].SetIndex = -1;	// Invalid index for invalid sets!

		EmissivePatches.RemoveRange( _MaxSetsCount, EmissivePatches.Count - _MaxSetsCount );
	}
	m_EmissivePatches = EmissivePatches.ToArray();	// Store as our final emissive sets

	// Remove sets that contain less than 0.4% of our pixels (arbitrary!)
	m_Patches = Patches.ToArray();
	int	DiscardThreshold = (int) (0.004f * m_ScenePixels.Count);
	foreach ( Patch S in m_Patches )
		if ( S.SetPixels.Count < DiscardThreshold )
		{
			S.SetIndex = -1;	// Now invalid!
			Patches.Remove( S );
		}

	// Remove sets that are not important enough
	m_Patches = Patches.ToArray();
	foreach ( Patch S in m_Patches )
		if ( S.Importance < Pixel.IMPORTANCE_THRESOLD )
		{
			S.SetIndex = -1;	// Now invalid!
			Patches.Remove( S );
		}

	m_Patches = Patches.ToArray();


	//////////////////////////////////////////////////////////////////////////
	// Compute informations on each set
	m_MeanDistance = 0.0;
	m_MeanHarmonicDistance = 0.0;
	m_MinDistance = 1e6;
	m_MaxDistance = 0.0;

	int		SumCardinality = 0;
	for ( int SetIndex=0; SetIndex < m_PatchesCount; SetIndex++ )
	{
		Patch&	S = m_Patches[SetIndex];
		S.SetIndex = SetIndex;

		// Post-process the pixels to find the one closest to the probe as our new centroid
		Pixel	BestPixel = S.SetPixels[0];
		float3	AverageNormal = float3::Zero;
		float3	AverageAlbedo = float3::Zero;
		foreach ( Pixel P in S.SetPixels )
		{
			if ( P.Distance < BestPixel.Distance )
				BestPixel = P;

			AverageNormal += P.Normal;
			AverageAlbedo += P.Albedo;

			// Update min/max/avg
			m_MeanDistance += P.Distance;
			m_MinDistance = min( m_MinDistance, P.Distance );
			m_MaxDistance = max( m_MaxDistance, P.Distance );
			m_MeanHarmonicDistance += 1.0 / P.Distance;
		}

		AverageNormal /= S.Pixels.GetCount();
		AverageAlbedo /= S.Pixels.GetCount();

		S.Position = BestPixel.Position;	// Our new winner!
		S.Normal = AverageNormal;
		S.SetAlbedo( AverageAlbedo );

		// Count pixels in the set for statistics
		SumCardinality += S.Pixels.GetCount();

		// Finally, encode SH & find principal axes
		S.EncodeSH();

// Find a faster way!
//		S.FindPrincipalAxes();
	}

	m_MeanHarmonicDistance = SumCardinality / m_MeanHarmonicDistance;
	m_MeanDistance /= SumCardinality;

	// Do the same for emissive sets
	for ( int SetIndex=0; SetIndex < m_EmissivePatchesCount; SetIndex++ )
	{
		Patch	S = m_EmissivePatches[SetIndex];
		S.SetIndex = SetIndex;

		// Post-process the pixels to find the one closest to the probe as our new centroid
		Pixel	BestPixel = S.Pixels[0];
		foreach ( Pixel P in S.Pixels )
		{
			if ( P.Distance < BestPixel.Distance )
				BestPixel = P;
		}

		S.Position = BestPixel.Position;	// Our new winner!

		// Finally, encode SH & find principal axes & samples
		S.EncodeEmissiveSH();
	}


	// Generate samples for regular sets
	int		TotalSamplesCount = _MaxLightingSamplesCount;
	if ( TotalSamplesCount < m_PatchesCount )
	{	// Force samples count to match sets count!
//		MessageBox( "The amount of samples for the probe was chosen to be " + TotalSamplesCount + " which is inferior to the amount of sets, this would mean some sets wouldn't even get sampled so the actual amount of samples is at least set to the amount of sets (" + m_Patches.Length + ")", MessageBoxButtons.OK, MessageBoxIcon.Warning );
		TotalSamplesCount = m_PatchesCount;
	}

	for ( int SetIndex=m_PatchesCount-1; SetIndex >= 0; SetIndex-- )	// We start from the smallest sets to ensure they get some samples
	{
		Patch	S = m_Patches[SetIndex];
		if ( S.EmissiveMatID != ~0UL )
			ASSERT( false, "Shouldn't be any emissive set left in the list of regular sets??!" );

		int	SamplesCount = TotalSamplesCount * S.SetPixels.Count / SumCardinality;
			SamplesCount = Math.Max( 1, SamplesCount );					// Ensure we have at least 1 sample no matter what!
			SamplesCount = Math.Min( SamplesCount, S.SetPixels.Count );	// Can't have more samples than pixels!

		if ( SamplesCount == 0 )
			throw new Exception( "We have a set with NO light sample!" );

		S.GenerateSamples( SamplesCount );

		// Reduce the amount of available samples and the count of remaining pixels so the remaining sets share the remaining samples...
		TotalSamplesCount -= SamplesCount;
		SumCardinality -= S.SetPixels.Count;
	}


// 	//////////////////////////////////////////////////////////////////////////
// 	// Sum all SH for UI display
// 	for ( int i=0; i < 9; i++ )
// 	{
// 		m_SHSumDynamic[i] = float3.Zero;
// 		foreach ( Patch S in m_Patches )
// 			m_SHSumDynamic[i] += S.SH[i];
// 	}
// 
// 	for ( int i=0; i < 9; i++ )
// 	{
// 		m_SHSumEmissive[i] = float3.Zero;
// 		foreach ( Patch S in m_EmissivePatches )
// 			m_SHSumEmissive[i] += S.SH[i];
// 	}
}

#pragma region Flood Fill Algorithm

float		DISTANCE_THRESHOLD = 0.02f;						// 2cm
float		ANGULAR_THRESHOLD = acosf( 0.5 * PI / 180 );	// 0.5°
float		ALBEDO_HUE_THRESHOLD = 0.04f;					// Close colors!
float		ALBEDO_RGB_THRESHOLD = 0.16f;					// Close colors!

int			RecursionLevel = 0;

int			m_ScanlinePixelIndex = 0;
Pixel[]		m_ScanlinePixelsPool = new Pixel[6 * CUBE_MAP_SIZE * CUBE_MAP_SIZE];

/// <summary>
/// This should be a faster version (and much less recursive!) than the original flood fill
/// The idea here is to process an entire scanline first (going left and right and collecting valid scanline pixels along the way)
///  then for each of these pixels we move up/down and fill the top/bottom scanlines from these new seeds...
/// </summary>
/// <param name="_S"></param>
/// <param name="_PreviousPixel"></param>
/// <param name="_P"></param>
/// <param name="_SetPixels"></param>
/// <param name="_SetRejectedPixels"></param>
void	SHProbeEncoder::FloodFill( Patch _S, Pixel _PreviousPixel, Pixel _P, List<Pixel*> _SetRejectedPixels ) {
	if ( !CheckAndAcceptPixel( _S, _PreviousPixel, _P, _SetRejectedPixels ) )
		return;

	//////////////////////////////////////////////////////////////////////////
	// Check the entire scanline
	int	ScanlineStartIndex = m_ScanlinePixelIndex;

	m_ScanlinePixelsPool[m_ScanlinePixelIndex++] = _P;	// This pixel is implicitly on the scanline

	// Start going right
	Pixel	Previous = _P;
	Pixel	Current = FindAdjacentPixel( Previous, 1, 0 );
	while ( CheckAndAcceptPixel( _S, Previous, Current, _SetRejectedPixels ) )
	{
		m_ScanlinePixelsPool[m_ScanlinePixelIndex++] = Current;
		Previous = Current;
		Current = FindAdjacentPixel( Current, 1, 0 );
	}

	// Start going left
	Previous = _P;
	Current = FindAdjacentPixel( Previous, -1, 0 );
	while ( CheckAndAcceptPixel( _S, Previous, Current, _SetRejectedPixels ) )
	{
		m_ScanlinePixelsPool[m_ScanlinePixelIndex++] = Current;
		Previous = Current;
		Current = FindAdjacentPixel( Current, -1, 0 );
	}

	RecursionLevel++;

	int	ScanlineEndIndex = m_ScanlinePixelIndex;

	//////////////////////////////////////////////////////////////////////////
	// Recurse into each pixel of the top scanline
	for ( int ScanlinePixelIndex=ScanlineStartIndex; ScanlinePixelIndex < ScanlineEndIndex; ScanlinePixelIndex++ )
	{
		Pixel	P = m_ScanlinePixelsPool[ScanlinePixelIndex];
		Pixel	Top = FindAdjacentPixel( P, 0, 1 );

		FloodFill( _S, P, Top, _SetRejectedPixels );
	}

	//////////////////////////////////////////////////////////////////////////
	// Recurse into each pixel of the bottom scanline
	for ( int ScanlinePixelIndex=ScanlineStartIndex; ScanlinePixelIndex < ScanlineEndIndex; ScanlinePixelIndex++ )
	{
		Pixel	P = m_ScanlinePixelsPool[ScanlinePixelIndex];
		Pixel	Bottom = FindAdjacentPixel( P, 0, -1 );
		FloodFill( _S, P, Bottom, _SetRejectedPixels );
	}

	RecursionLevel--;
}

bool	SHProbeEncoder::CheckAndAcceptPixel( Patch _S, Pixel _PreviousPixel, Pixel _P, List<Pixel*> _SetRejectedPixels )
{
	if ( !_P.IsFloodFillAcceptable() )
		return false;

	//////////////////////////////////////////////////////////////////////////
	// Check some criterions for a match
	bool	Accepted = false;

	// Emissive pixels get grouped together
	if ( _PreviousPixel.IsEmissive || _P.IsEmissive )
	{
		Accepted = _PreviousPixel.EmissiveMatID == _P.EmissiveMatID;
	}
	else
	{
		// First, let's check the angular discrepancy
		float	Dot = _PreviousPixel.Normal | _P.Normal;
		if ( Dot > ANGULAR_THRESHOLD )
		{
			// Next, let's check the distance discrepancy
			float	DistanceDiff = Math.Abs( _PreviousPixel.Distance - _P.Distance );
			float	ToleranceFactor = -(_P.Normal | _P.View);						// Weight by the surface's slope to be more tolerant for slant surfaces
					DistanceDiff *= ToleranceFactor;
			if ( DistanceDiff < DISTANCE_THRESHOLD )
			{
				// Next, let's check the hue discrepancy
// 						float	HueDiff0 = Math.Abs( _PreviousPixel.AlbedoHSL.x - _P.AlbedoHSL.x );
// 						float	HueDiff1 = 6.0f - HueDiff0;
// 						float	HueDiff = Math.Min( HueDiff0, HueDiff1 );
// //								HueDiff *= 0.5f * (_PreviousPixel.AlbedoHSL.y + _P.AlbedoHSL.y);	// Weight by saturation to be less severe with unsaturated colors that can change in hue quite fast
// 								HueDiff *= Math.Max( _PreviousPixel.AlbedoHSL.y, _P.AlbedoHSL.y );	// Weight by saturation to be less severe with unsaturated colors that can change in hue quite fast
// 						if ( HueDiff < ALBEDO_HUE_THRESHOLD )
// 						{
// 							Accepted = true;	// Winner!
// 						}


				// Next, let's check color discrepancy
				// I'm using the simplest metric here...
				float	ColorDiff = (_PreviousPixel.Albedo - _P.Albedo).Length;
				if ( ColorDiff < ALBEDO_RGB_THRESHOLD )
				{
					Accepted = true;	// Winner!
				}
			}
		}
	}

	// Mark the pixel as member of this set, even if it's temporary (rejected pixels get removed in the end)
	_P.ParentSet = _S;

	if ( Accepted )
	{
		_S.SetPixels.Add( _P );			// We got a new member for the set!
		_S.Importance += _P.Importance;	// Accumulate average importance
	}
	else
		_SetRejectedPixels.Add( _P );	// Sorry buddy, we'll add you to the rejects...

	return Accepted;
}

#pragma region Adjacency Walker

const int	CUBE_MAP_FACE_SIZE = CUBE_MAP_SIZE * CUBE_MAP_SIZE;

// Contains the new cube face index when stepping outside of a cube face by the left/right/top/bottom
int[]	GoLeft = new int[6] {
	4,	// Step to +Z
	5,	// Step to -Z
	1,	// Step to -X
	1,	// Step to -X
	1,	// Step to -X
	0,	// Step to +X
};
int[]	GoRight = new int[6] {
	5,	// Step to -Z
	4,	// Step to +Z
	0,	// Step to +X
	0,	// Step to +X
	0,	// Step to +X
	1,	// Step to -X
};
int[]	GoDown = new int[6] {
	2,	// Step to +Y
	2,	// Step to +Y
	4,	// Step to +Z
	5,	// Step to -Z
	2,	// Step to +Y
	2,	// Step to +Y
};
int[]	GoUp = new int[6] {
	3,	// Step to -Y
	3,	// Step to -Y
	5,	// Step to -Z
	4,	// Step to +Z
	3,	// Step to -Y
	3,	// Step to -Y
};

// Contains the matrices that indicate how the (X,Y) pixel coordinates should be transformed to step from one face to the other
// Transforms arrays are simple matrices:
//	Tx Xx Xy
//	Ty Yx Yy
//
// Which are used like this:
//	X' = Tx * CUBE_MAP_SIZE + Xx * X + Xy * Y
//	Y' = Ty * CUBE_MAP_SIZE + Yx * X + Yy * Y
//
const int C = CUBE_MAP_SIZE;
const int C2 = 2*C;
const int C_ = CUBE_MAP_SIZE-1;
int[][]	GoLeftTransforms = new int[6][] {
	// Going left from +X sends us to +Z
	new int[6] {	C,  1,  0,		// X' = C + X	(C is the CUBE_MAP_SIZE)
					0,  0,  1 },	// Y' = Y
	// Going left from -X sends us to -Z
	new int[6] {	C,  1,  0,		// X' = C + X
					0,  0,  1 },	// Y' = Y
	// Going left from +Y sends us to -X
	new int[6] {	C_,  0, -1,		// X' = C - Y
					0, -1,  0 },	// Y' = -X
	// Going left from -Y sends us to -X
	new int[6] {	0,  0,  1,		// X' = Y
					C,  1,  0 },	// Y' = C + X
	// Going left from +Z sends us to -X
	new int[6] {	C,  1,  0,		// X' = C + X
					0,  0,  1 },	// Y' = Y
	// Going left from -Z sends us to +X
	new int[6] {	C,  1,  0,		// X' = C + X
					0,  0,  1 },	// Y' = Y
};
int[][]	GoRightTransforms = new int[6][] {
	// Going right from +X sends us to -Z
	new int[6] {	-C,  1,  0,		// X' = -C + X	(C is the CUBE_MAP_SIZE)
					0,  0,  1 },	// Y' = Y
	// Going right from -X sends us to +Z
	new int[6] {	-C,  1,  0,		// X' = -C + X
					0,  0,  1 },	// Y' = Y
	// Going right from +Y sends us to +X
	new int[6] {	0,  0,  1,		// X' = Y
					-C, 1,  0 },	// Y' = -C + X
	// Going right from -Y sends us to +X
	new int[6] {	C_, 0,  -1,		// X' = C - Y
					C,  1,  0 },	// Y' = C + X
	// Going right from +Z sends us to +X
	new int[6] {	-C,  1,  0,		// X' = -C + X
					0,  0,  1 },	// Y' = Y
	// Going right from -Z sends us to -X
	new int[6] {	-C,  1,  0,		// X' = -C + X
					0,  0,  1 },	// Y' = Y
};
int[][]	GoDownTransforms = new int[6][] {
	// Going down from +X sends us to +Y
	new int[6] {	C,  0,  1,		// X' = C + Y	(C is the CUBE_MAP_SIZE)
					0,  1,  0 },	// Y' = X
	// Going down from -X sends us to +Y
	new int[6] {	0,  0, -1,		// X' = -Y
					C_, -1,  0 },	// Y' = C - X
	// Going down from +Y sends us to +Z
	new int[6] {	0,  1,  0,		// X' = X
					0,  0, -1 },	// Y' = -Y
	// Going down from -Y sends us to -Z
	new int[6] {	C_, -1,  0,		// X' = C - X
					C,  0,  1 },	// Y' = C + Y
	// Going down from +Z sends us to +Y
	new int[6] {	0,  1,  0,		// X' = X
					0,  0, -1 },	// Y' = -Y
	// Going down from -Z sends us to +Y
	new int[6] {	C_, -1,  0,		// X' = C - X
					C,  0,  1 },	// Y' = C + Y
};
int[][]	GoUpTransforms = new int[6][] {
	// Going up from +X sends us to -Y
	new int[6] {	C2,  0, -1,		// X' = 2C - Y	(C is the CUBE_MAP_SIZE)
					C_, -1,  0 },	// Y' = C - X
	// Going up from -X sends us to -Y
	new int[6] {	-C,  0,  1,		// X' = -C + Y
					0,  1,  0 },	// Y' = X
	// Going up from +Y sends us to -Z
	new int[6] {	C_, -1,  0,		// X' = C - X
					-C, 0,  1 },	// Y' = -C + Y
	// Going up from -Y sends us to +Z
	new int[6] {	0,  1,  0,		// X' = X
					C2,  0, -1 },	// Y' = 2C - Y
	// Going up from +Z sends us to -Y
	new int[6] {	0,  1,  0,		// X' = X
					C2,  0, -1 },	// Y' = 2C - Y
	// Going up from -Z sends us to -Y
	new int[6] {	C_, -1,  0,		// X' = C - X
					-C, 0,  1 },	// Y' = -C + Y
};

SHProbeEncoder::Pixel	SHProbeEncoder::FindAdjacentPixel( Pixel _P, int _Dx, int _Dy )
{
	int	CubeFaceIndex = _P.CubeFaceIndex;
	int	X = _P.CubeFaceX;
	int	Y = _P.CubeFaceY;

	X += _Dx;
	if ( X < 0 )
	{	// Stepped out through left side
		TransformXY( GoLeftTransforms[CubeFaceIndex], ref X, ref Y );
		CubeFaceIndex = GoLeft[CubeFaceIndex];
	}
	if ( X >= CUBE_MAP_SIZE )
	{	// Stepped out through right side
		TransformXY( GoRightTransforms[CubeFaceIndex], ref X, ref Y );
		CubeFaceIndex = GoRight[CubeFaceIndex];
	}

	Y += _Dy;
	if ( Y < 0 )
	{	// Stepped out through bottom side
		TransformXY( GoDownTransforms[CubeFaceIndex], ref X, ref Y );
		CubeFaceIndex = GoDown[CubeFaceIndex];
	}
	if ( Y >= CUBE_MAP_SIZE )
	{	// Stepped out through top side
		TransformXY( GoUpTransforms[CubeFaceIndex], ref X, ref Y );
		CubeFaceIndex = GoUp[CubeFaceIndex];
	}

	int	FinalPixelIndex = CUBE_MAP_FACE_SIZE * CubeFaceIndex + CUBE_MAP_SIZE * Y + X;

	return m_ProbePixels[FinalPixelIndex];
}

void	TransformXY( int[] _Transform, ref int _X, ref int _Y )
{
	int	TempX = _Transform[0] + _Transform[1] * _X + _Transform[2] * _Y;
	int	TempY = _Transform[3] + _Transform[4] * _X + _Transform[5] * _Y;
	_X = TempX;
	_Y = TempY;
}

#pragma endregion

#pragma endregion

#pragma endregion


//////////////////////////////////////////////////////////////////////////
//
void	SHProbeEncoder::ReadBackProbeCubeMap( Texture2D& _StagingCubeMap ) {
	m_MaxFaceIndex = 0;

	m_NearestNeighborProbeDistance = 0.0f;
	m_FarthestNeighborProbeDistance = 0.0f;

	m_MeanDistance = 0.0;
	m_MeanHarmonicDistance = 0.0;
	m_MinDistance = 0.0;
	m_MaxDistance = 0.0;

	m_ProbePixels.Clear();
	m_ScenePixels.Clear();

	double	dA = 4.0 / (CUBE_MAP_SIZE*CUBE_MAP_SIZE);	// Cube face is supposed to be in [-1,+1], yielding a 2x2 square units
	double	SumSolidAngle = 0.0;

	m_MeanDistance = 0.0;
	m_MeanHarmonicDistance = 0.0;
	m_MinDistance = 1e6;
	m_MaxDistance = 0.0;
	m_BBoxMin =  float3::MaxFlt;
	m_BBoxMax = -float3::MaxFlt;

	int		NegativeImportancePixelsCount = 0;
	for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
	{
		Pixel*	pCubeMapPixels = &m_CubeMapPixels[CubeFaceIndex*CUBE_MAP_SIZE*CUBE_MAP_SIZE];

		// Fill up albedo
		D3D11_MAPPED_SUBRESOURCE	Map0 = _StagingCubeMap.Map( 0, 4*CubeFaceIndex+0 );
		D3D11_MAPPED_SUBRESOURCE	Map1 = _StagingCubeMap.Map( 0, 4*CubeFaceIndex+1 );
		D3D11_MAPPED_SUBRESOURCE	Map2 = _StagingCubeMap.Map( 0, 4*CubeFaceIndex+2 );
		D3D11_MAPPED_SUBRESOURCE	Map3 = _StagingCubeMap.Map( 0, 4*CubeFaceIndex+3 );

		float4*	pFaceData0 = (float4*) Map0.pData;
		float4*	pFaceData1 = (float4*) Map1.pData;
		float4*	pFaceData2 = (float4*) Map2.pData;
		float4*	pFaceData3 = (float4*) Map3.pData;

		Pixel*	P = pCubeMapPixels;
		for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
			for ( int X=0; X < CUBE_MAP_SIZE; X++, P++, pFaceData0++, pFaceData1++, pFaceData2++, pFaceData3++ ) 
			{
				// ==== Read back albedo & unique face ID ====
				float	Red = pFaceData0->x;
				float	Green = pFaceData0->y;
				float	Blue = pFaceData0->z;

				// Work with better precision
				Red *= PI;
				Green *= PI;
				Blue *= PI;

				P->SetAlbedo( Red, Green, Blue );
				P->FaceIndex = ((U32&) pFaceData0->w);

				// ==== Read back static lighting & emissive material IDs ====
				P->StaticLitColor = *pFaceData1;
				P->EmissiveMatID = ((U32&) pFaceData1->w);

				// ==== Read back position & normal ====
				float	Nx = pFaceData2->x;
				float	Ny = pFaceData2->y;
				float	Nz = pFaceData2->z;
				float	Distance = pFaceData2->w;

				float3	csView( 2.0f * (0.5f + X) / CUBE_MAP_SIZE - 1.0f, 1.0f - 2.0f * (0.5f + Y) / CUBE_MAP_SIZE, 1.0f );
				float	Distance2Texel = csView.Length();
						csView = csView / Distance2Texel;
				float3	wsView = float4( csView, 0 ) * m_Side2World[CubeFaceIndex];

				float3	wsPosition( Distance * wsView.x, Distance * wsView.y, Distance * wsView.z );

				P->Position = wsPosition;
				P->View = wsView;
				P->Normal.Patch( Nx, Ny, Nz );
				P->Normal.Normalize();

				// ==== Read back neighbor probes ID ====
				P->NeighborProbeID = ((U32&) pFaceData3->x);
				P->NeighborProbeDistance = pFaceData3->y;


				// ==== Finalize pixel information ====

				// Retrieve the cube map texel's solid angle (from http://people.cs.kuleuven.be/~philip.dutre/GI/TotalCompendium.pdf)
				// dw = cos(Theta).dA / r²
				// cos(Theta) = Adjacent/Hypothenuse = 1/r
				//
				double	SolidAngle = dA / (Distance2Texel * Distance2Texel * Distance2Texel);
				SumSolidAngle += SolidAngle;

				P->SolidAngle = SolidAngle;
				P->Importance = -(wsView | P->Normal) / (Distance * Distance);
				if ( P->Importance < 0.0 )
				{
P->Normal = -P->Normal;
P->Importance = -(wsView | P->Normal) / (Distance * Distance);
//P->Importance = -P->Importance;
NegativeImportancePixelsCount++;
//					throw new Exception( "WTH?? Negative importance here!" );
				}
				P->Distance = Distance;
				P->Infinity = Distance > Z_INFINITY_TEST;

				P->InitSH();

				m_ProbePixels.Append( P );
				if ( P->Infinity )
					continue;	// Not part of the scene's geometry!

				// Account for a new scene pixel (i.e. not infinity)
				m_ScenePixels.Append( P );

				// Update dimensions
				m_MeanDistance += Distance;
				m_MeanHarmonicDistance += 1.0 / Distance;
				m_MinDistance = min( m_MinDistance, Distance );
				m_MaxDistance = max( m_MaxDistance, Distance );
				m_BBoxMin.Max( wsPosition );
				m_BBoxMax.Min( wsPosition );


			}

		_StagingCubeMap.UnMap( 0, 4*CubeFaceIndex+3 );
		_StagingCubeMap.UnMap( 0, 4*CubeFaceIndex+2 );
		_StagingCubeMap.UnMap( 0, 4*CubeFaceIndex+1 );
		_StagingCubeMap.UnMap( 0, 4*CubeFaceIndex+0 );
	}

	if ( float(NegativeImportancePixelsCount) / (CUBE_MAP_SIZE * CUBE_MAP_SIZE * 6) > 0.1f )
		ASSERT( false, "More than 10% invalid pixels with inverted normals in that probe!" );

	m_MeanDistance /= (CUBE_MAP_SIZE * CUBE_MAP_SIZE * 6);
	m_MeanHarmonicDistance = (CUBE_MAP_SIZE * CUBE_MAP_SIZE * 6) / m_MeanHarmonicDistance;
}

//////////////////////////////////////////////////////////////////////////
//
FILE*	g_pFile = NULL;
template< typename T> void	Write( const T& _value ) {
	fwrite( &_value, sizeof(T), 1, g_pFile );
}

void	SHProbeEncoder::Save( const char* _FileName ) {

	fopen_s( &g_pFile, _FileName, "rb" );
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

	// Write the result sets
	Write( m_PatchesCount );
	for ( U32 i=0; i < m_PatchesCount; i++ ) {
		Patch&	P = m_Patches[i];

		// Write position, normal, albedo
		Write( P.Position.x );
		Write( P.Position.y );
		Write( P.Position.z );

		Write( P.Normal.x );
		Write( P.Normal.y );
		Write( P.Normal.z );

		Write( P.Tangent.x );
		Write( P.Tangent.y );
		Write( P.Tangent.z );

		Write( P.BiTangent.x );
		Write( P.BiTangent.y );
		Write( P.BiTangent.z );

			// Not used, just for information purpose
		Write( (float) (P.Albedo.x * INVPI) );
		Write( (float) (P.Albedo.y * INVPI) );
		Write( (float) (P.Albedo.z * INVPI) );

		// Write SH coefficients (albedo is already factored in)
		for ( int i=0; i < 9; i++ )
		{
			Write( P.SH[i].x );
			Write( P.SH[i].y );
			Write( P.SH[i].z );
		}

		// Write amount of samples
		Write( (U32) P.SamplesCount );

		// Write each sample
		for ( int j=0; j < P.SamplesCount; j++ ) {
			Patch::Sample&	S = P.Samples[j];

			// Write position
			Write( S.Position.x );
			Write( S.Position.y );
			Write( S.Position.z );

			// Write normal
			Write( S.Normal.x );
			Write( S.Normal.y );
			Write( S.Normal.z );

			// Write radius
			Write( S.Radius );
		}
	}

	// Write the emissive sets
	Write( m_EmissivePatchesCount );

	for ( U32 i=0; i < m_EmissivePatchesCount; i++ ) {
		Patch&	P = m_EmissivePatches[i];

		// Write position, normal, albedo
		Write( P.Position.x );
		Write( P.Position.y );
		Write( P.Position.z );

		Write( P.Normal.x );
		Write( P.Normal.y );
		Write( P.Normal.z );

		Write( P.Tangent.x );
		Write( P.Tangent.y );
		Write( P.Tangent.z );

		Write( P.BiTangent.x );
		Write( P.BiTangent.y );
		Write( P.BiTangent.z );

		// Write emissive mat
		Write( P.EmissiveMatID );

		// Write SH coefficients (we only write luminance here, we don't have the color info that is provided at runtime)
		for ( int i=0; i < 9; i++ )
			Write( P.SH[i].x );
	}

	// Write the neighbor probes
	Write( m_NeighborProbesCount );

	// Write nearest/farthest probe distance
	Write( m_NearestNeighborProbeDistance );
	Write( m_FarthestNeighborProbeDistance );

	for ( U32 i=0; i < m_NeighborProbesCount; i++ ) {
		NeighborProbe&	NP = m_NeighborProbes[i];

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
// 			// Save probe influence for each scene face
// 			FileInfo	InfluenceFileName = new FileInfo( Path.Combine( Path.GetDirectoryName( _FileName.FullName ), Path.GetFileNameWithoutExtension( _FileName.FullName ) + ".FaceInfluence" ) );
// 			using ( FileStream S = InfluenceFileName.Create() )
// 				using ( BinaryWriter W = new BinaryWriter( S ) )
// 				{
// 					for ( U32 FaceIndex=0; FaceIndex < m_MaxFaceIndex; FaceIndex++ )
// 					{
// 						W.Write( (float) (m_ProbeInfluencePerFace.ContainsKey( FaceIndex ) ? m_ProbeInfluencePerFace[FaceIndex] : 0.0) );
// 					}
// 				}
}

const double	SHProbeEncoder::Pixel::f0 = 0.5 / PI;
const double	SHProbeEncoder::Pixel::f1 = sqrt(3.0) * SHProbeEncoder::Pixel::f0;
const double	SHProbeEncoder::Pixel::f2 = sqrt(15.0) * SHProbeEncoder::Pixel::f0;
const double	SHProbeEncoder::Pixel::f3 = sqrt(5.0) * 0.5 * SHProbeEncoder::Pixel::f0;

float	SHProbeEncoder::Pixel::IMPORTANCE_THRESOLD = 0.0f;


/* TODO! At the moment we only read back sets from disk that were computed by the probe SH encoder tool (in Tools.sln)
	But when the probe SH encoder tool is complete, I'll have to re-write it in C++ for in-place probe encoding...
	---------------------------------------------------------------------------------------------------------------------

		double	dA = 4.0 / (CUBE_MAP_SIZE*CUBE_MAP_SIZE);	// Cube face is supposed to be in [-1,+1], yielding a 2x2 square units
		double	SumSolidAngle = 0.0;

		double	pSHOcclusion[9];
		memset( pSHOcclusion, 0, 9*sizeof(double) );

		for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
		{
			D3D11_MAPPED_SUBRESOURCE&	MappedFaceAlbedo = pRTCubeMapStaging->Map( 0, CubeFaceIndex );
			D3D11_MAPPED_SUBRESOURCE&	MappedFaceGeometry = pRTCubeMapStaging->Map( 0, 6+CubeFaceIndex );

			// Update cube map face camera transform
			float4x4	Camera2World = Side2Local[CubeFaceIndex] * ProbeLocal2World;

			pCBCubeMapCamera->m.Camera2World = Side2Local[CubeFaceIndex] * ProbeLocal2World;

			float3	View( 0, 0, 1 );
			for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
			{
				float4*	pScanlineAlbedo = (float4*) ((U8*) MappedFaceAlbedo.pData + Y * MappedFaceAlbedo.RowPitch);
				float4*	pScanlineGeometry = (float4*) ((U8*) MappedFaceGeometry.pData + Y * MappedFaceGeometry.RowPitch);

				View.y = 1.0f - 2.0f * (0.5f + Y) / CUBE_MAP_SIZE;
				for ( int X=0; X < CUBE_MAP_SIZE; X++ )
				{
					float4	Albedo = *pScanlineAlbedo++;
					float4	Geometry = *pScanlineGeometry++;

					// Rebuild view direction
					View.x = 2.0f * (0.5f + X) / CUBE_MAP_SIZE - 1.0f;

					// Retrieve the cube map texel's solid angle (from http://people.cs.kuleuven.be/~philip.dutre/GI/TotalCompendium.pdf)
					// dw = cos(Theta).dA / r²
					// cos(Theta) = Adjacent/Hypothenuse = 1/r
					//
					float	SqDistance2Texel = View.LengthSq();
					float	Distance2Texel = sqrtf( SqDistance2Texel );

					double	SolidAngle = dA / (Distance2Texel * SqDistance2Texel);
					SumSolidAngle += SolidAngle;	// CHECK! => Should amount to 4PI at the end of the iteration...

					// Check if we hit an obstacle, in which case we should accumulate direct ambient lighting
					if ( Geometry.w > Z_INFINITY_TEST )
					{	// No obstacle means direct lighting from the ambient sky...
						float3	ViewWorld = float4( View, 0.0f ) * Camera2World;	// View vector in world space
						ViewWorld.Normalize();

						// Accumulate SH coefficients in that direction, weighted by the solid angle
 						double	pSHCoeffs[9];
 						BuildSHCoeffs( ViewWorld, pSHCoeffs );
						for ( int i=0; i < 9; i++ )
							pSHOcclusion[i] += SolidAngle * pSHCoeffs[i];

						continue;
					}
				}
			}

			pRTCubeMapStaging->UnMap( 0, 6*0+CubeFaceIndex );
			pRTCubeMapStaging->UnMap( 0, 6*1+CubeFaceIndex );
		}

		//////////////////////////////////////////////////////////////////////////
		// 3] Store direct ambient and indirect reflection of static lights on static geometry
// DON'T NORMALIZE		double	Normalizer = 4*PI / SumSolidAngle;
		for ( int i=0; i < 9; i++ )
		{
			Probe.pSHOcclusion[i] = float( Normalizer * pSHOcclusion[i] );

// TODO! At the moment we don't compute static SH coeffs
Probe.pSHBounceStatic[i] = float3::Zero;
		}

		//////////////////////////////////////////////////////////////////////////
		// 4] Compute solid sets for that probe
		// This part is really important as it will attempt to isolate the important geometric zones near the probe to
		//	approximate them using simple planar impostors that will be lit instead of the entire probe's pixels
		// Each solid set is then lit by dynamic lights in real-time and all pixels belonging to the set add their SH
		//	contribution to the total SH of the probe, this allows us to perform dynamic light bounce on the scene cheaply!
		//

*/
