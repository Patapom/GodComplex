#include "../../GodComplex.h"

const float	SHProbeEncoder::Z_INFINITY = 1e6f;
const float	SHProbeEncoder::Z_INFINITY_TEST = 0.99f * SHProbeEncoder::Z_INFINITY;

float		SHProbeEncoder::DISTANCE_THRESHOLD = 0.02f;						// 2cm
float		SHProbeEncoder::ANGULAR_THRESHOLD = acosf( 0.5f * PI / 180 );	// 0.5°
float		SHProbeEncoder::ALBEDO_HUE_THRESHOLD = 0.04f;					// Close colors!
float		SHProbeEncoder::ALBEDO_RGB_THRESHOLD = 0.16f;					// Close colors!


SHProbeEncoder::SHProbeEncoder() {
	m_pCubeMapPixels = new Pixel[6*CUBE_MAP_SIZE*CUBE_MAP_SIZE];
	Pixel*	pPixel = m_pCubeMapPixels;
	for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
		for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
			for ( int X=0; X < CUBE_MAP_SIZE; X++, pPixel++ ) {
				pPixel->Index = pPixel - m_pCubeMapPixels;
				pPixel->CubeFaceIndex = CubeFaceIndex;
				pPixel->CubeFaceX = X;
				pPixel->CubeFaceY = Y;
			}

	m_AllSurfaces.Init( 6*CUBE_MAP_SIZE*CUBE_MAP_SIZE );

	Pixel::ms_RadixNodes[0] = new SHProbeEncoder::Pixel::RadixNode_t[6*CUBE_MAP_FACE_SIZE];	// Pre-allocate the maximum amount of radix nodes
	Pixel::ms_RadixNodes[1] = new SHProbeEncoder::Pixel::RadixNode_t[6*CUBE_MAP_FACE_SIZE];	// Pre-allocate the maximum amount of radix nodes

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
	SAFE_DELETE_ARRAY( Pixel::ms_RadixNodes[1] );
	SAFE_DELETE_ARRAY( Pixel::ms_RadixNodes[0] );
	SAFE_DELETE_ARRAY( m_pCubeMapPixels );
}

void	SHProbeEncoder::EncodeProbeCubeMap( Texture2D& _StagingCubeMap, U32 _ProbeID ) {
	int	TotalPixelsCount = 6*CUBE_MAP_FACE_SIZE*CUBE_MAP_FACE_SIZE;

	//////////////////////////////////////////////////////////////////////////
	// 1] Read back probe data and prepare pixels for encoding
	ReadBackProbeCubeMap( _StagingCubeMap );


	//////////////////////////////////////////////////////////////////////////
	// 2] Encode

	// 2.1) ======== Compute occlusion & static lighting SH ========
	double	SHR[9] = { 0 };
	double	SHG[9] = { 0 };
	double	SHB[9] = { 0 };
	double	SHOcclusion[9] = { 0 };

	for ( int PixelIndex=0; PixelIndex < TotalPixelsCount; PixelIndex++ ) {
		Pixel&	P = m_pCubeMapPixels[PixelIndex];

		if ( !P.Infinity ) {
			for ( int i=0; i < 9; i++ )
			{
				SHR[i] += P.SHCoeffs[i] * P.SolidAngle * P.StaticLitColor.x;
				SHG[i] += P.SHCoeffs[i] * P.SolidAngle * P.StaticLitColor.y;
				SHB[i] += P.SHCoeffs[i] * P.SolidAngle * P.StaticLitColor.z;
			}
		} else {
			// No obstacle means direct lighting from the ambient sky...
			// Accumulate SH coefficients in that direction, weighted by the solid angle
			for ( int i=0; i < 9; i++ )
				SHOcclusion[i] += P.SolidAngle * P.SHCoeffs[i];
		}
	}

	for ( int i=0; i < 9; i++ ) {
		m_StaticSH[i].Set( (float) SHR[i], (float) SHG[i], (float) SHB[i] );
		m_OcclusionSH[i] = (float) SHOcclusion[i];
	}

	// Apply filtering
// 	SphericalHarmonics.SHFunctions.FilterLanczos( m_StaticSH, 3 );		// Lanczos should be okay for static lighting
// 	SphericalHarmonics.SHFunctions.FilterHanning( m_OcclusionSH, 3 );


// 	// 2.2) ======== Compute the influence of the probe on each scene face ========
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


	// 2.3) ======== Build the neighbor probes network ========
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

	// 2.4) ======== Build surfaces by flood filling ========
	ComputeFloodFill( MAX_PROBE_PATCHES, MAX_SAMPLES_PER_PATCH, 1.0f, 1.0f, 1.0f, 0.5f );
}

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

	// Write the result surfaces
	Write( m_SurfacesCount );
	for ( U32 i=0; i < m_SurfacesCount; i++ ) {
		Surface&	P = *m_ppSurfaces[i];

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
			Surface::Sample&	S = P.Samples[j];

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

	// Write the emissive surfaces
	Write( m_EmissiveSurfacesCount );

	for ( U32 i=0; i < m_EmissiveSurfacesCount; i++ ) {
		Surface&	P = *m_ppEmissiveSurfaces[i];

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
	Write( m_NeighborProbes.GetCount() );

	// Write nearest/farthest probe distance
	Write( m_NearestNeighborProbeDistance );
	Write( m_FarthestNeighborProbeDistance );

	for ( int i=0; i < m_NeighborProbes.GetCount(); i++ ) {
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


#pragma region Computes Patches by Flood Fill Method

int	DEBUG_PixelIndex = 0;

void	SHProbeEncoder::ComputeFloodFill( int _MaxSetsCount, int _MaxLightingSamplesCount, float _SpatialDistanceWeight, float _NormalDistanceWeight, float _AlbedoDistanceWeight, float _MinimumImportanceDiscardThreshold ) {
	int	TotalPixelsCount = 6*CUBE_MAP_FACE_SIZE*CUBE_MAP_FACE_SIZE;
 	U32	DiscardThreshold = U32( 0.004f * m_ScenePixelsCount );		// Discard surfaces that contain less than 0.4% of the total amount of scene pixels (arbitrary!)

	// Clear the parent surfaces for each pixel
	for ( int i=0; i < TotalPixelsCount; i++ )
		m_pCubeMapPixels[i].pParentSurface = NULL;

	// Setup the reference thresholds for pixels' acceptance
//	Pixel.IMPORTANCE_THRESOLD = (float) ((4.0f * Math.PI / CUBE_MAP_FACE_SIZE) / (m_MeanDistance * m_MeanDistance));	// Compute an average solid angle threshold based on average pixels' distance
	Pixel::IMPORTANCE_THRESOLD = (float) (0.01f * _MinimumImportanceDiscardThreshold / (m_MeanHarmonicDistance * m_MeanHarmonicDistance));	// Simply use the mean harmonic distance as a good approximation of important pixels
																									// Pixels that are further or not facing the probe will have less importance...

	DISTANCE_THRESHOLD = 0.02f * _SpatialDistanceWeight;						// 2cm
	ANGULAR_THRESHOLD = acosf( 45.0f * _NormalDistanceWeight * PI / 180.0f );	// 45° (we're very generous here!)
	ALBEDO_HUE_THRESHOLD = 0.04f * _AlbedoDistanceWeight;						// Close colors!
	ALBEDO_RGB_THRESHOLD = 0.32f * _AlbedoDistanceWeight;						// Close colors!


	//////////////////////////////////////////////////////////////////////////
	// 1] Iterate on the list of free pixels that belong to no surface and create new surfaces
	Surface*	pRegularSurfaces = NULL;
	Surface*	pEmissiveSurfaces = NULL;
	for ( int PixelIndex=0; PixelIndex < TotalPixelsCount; PixelIndex++ ) {
		Pixel&	P0 = m_pCubeMapPixels[PixelIndex];

DEBUG_PixelIndex = PixelIndex;

		if ( !P0.IsFloodFillAcceptable() ) {
			continue;	// Not part of the scene geometry or too far away
		}

		// Create a new surface for this pixel
		Surface&	S = m_AllSurfaces.Append();

		S.Position = P0.Position;
		S.Normal = P0.Normal;
		S.Distance = P0.Distance;
		S.EmissiveMatID = P0.EmissiveMatID;
		S.SetAlbedo( P0.Albedo );


// if ( PixelIndex == 0x2680 )
// 	P0.Albedo.x += 1e-6f;

// if ( P0.IsEmissive )
//  	P0.Albedo.x += 1e-6f;


		// Flood fill adjacent pixels based on a criterion
		S.PixelsCount = 0;
		S.pPixels = NULL;
	 	Pixel*	pRejectedPixels = NULL;

		m_ScanlinePixelIndex = 0;	// VEEERY important line where we reset the pixel index of the pool of flood filled pixels!
		FloodFill( S, &S, &P0, pRejectedPixels );

		ASSERT( m_ScanlinePixelIndex != 0, "Can't have empty surfaces!" );

		// Remove rejected pixels from the surface (we only temporarily marked them to avoid them being processed twice by the flood filler)
		while ( pRejectedPixels != NULL ) {
			pRejectedPixels->pParentSurface = NULL;	// Ready for another round!
			pRejectedPixels = pRejectedPixels->pNext;
		}

		// Finalize importance
		S.Importance /= S.PixelsCount;

 		if ( S.PixelsCount < DiscardThreshold || S.Importance < Pixel::IMPORTANCE_THRESOLD ) {
			continue;	// This surface is not important enough
		}

		// Add the surface to the proper list
		if ( P0.EmissiveMatID == ~0UL ) {
			// One more regular surface
			S.pNext = pRegularSurfaces;
			pRegularSurfaces = &S;
		} else {
			// One more emissive surface
			S.pNext = pEmissiveSurfaces;
			pEmissiveSurfaces = &S;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// 2] Try and merge surfaces together

	// 2.1) Merge separate emissive surfaces that have the same mat ID together
	Surface*	P0 = pEmissiveSurfaces;
	while ( P0 != NULL ) {
		if ( P0->pParentSurface != NULL ) {

			// Merge with any other surface with same Mat ID
			Surface*	PreviousP1 = P0;
			Surface*	P1 = (Surface*) P0->pNext;
			while ( P1 != NULL ) {
				if ( P1->EmissiveMatID == P0->EmissiveMatID ) {

					// Compute new importance of the merged surfaces
					P0->Importance = (P0->PixelsCount * P0->Importance + P1->PixelsCount * P1->Importance) / (P0->PixelsCount + P1->PixelsCount);
					P0->PixelsCount += P1->PixelsCount;

					// Merge pixels
					Pixel*	pPixel1 = P1->pPixels;
					while ( pPixel1 != NULL ) {
						Pixel*	pTemp = pPixel1;
						pPixel1 = pPixel1->pNext;
						pTemp->pNext = P0->pPixels;
						P0->pPixels = pTemp;
					}

					// Remove the merged surface
					PreviousP1->pNext = P1->pNext;	// Link over that surface
					P1->pParentSurface = P0;			// Mark P0 as its parent so it doesn't get processed again
				}
				P1 = (Surface*) P1->pNext;
			}
		}
		P0 = (Surface*) P0->pNext;
	}

	// 2.2) Merge close enough surfaces
	// TODO?


	//////////////////////////////////////////////////////////////////////////
	// 3] Sort and cull unimportant surfaces
	{
		class	PixelKey : public Pixel::ISortKeyProvider {
		public:	virtual U32	GetKey( const Pixel& _Pixel ) const {
				return ((const Surface&) _Pixel).PixelsCount;
			}
		} Comp;

		Pixel*	pTemp0 = (Pixel*) pRegularSurfaces;
		Pixel*	pTemp1 = (Pixel*) pEmissiveSurfaces;
		Pixel::Sort( pTemp0, Comp, true );
		Pixel::Sort( pTemp1, Comp, true );
		pRegularSurfaces = (Surface*) pTemp0;
		pEmissiveSurfaces = (Surface*) pTemp1;

		// Copy down our selected surfaces
		m_SurfacesCount = 0;
		Surface*	S = pRegularSurfaces;
		while ( S != NULL && m_SurfacesCount < MAX_PROBE_PATCHES ) {
			m_ppSurfaces[m_SurfacesCount++] = S;
			S = (Surface*) S->pNext;
		}

		m_EmissiveSurfacesCount = 0;
		S = pEmissiveSurfaces;
		while ( S != NULL && m_EmissiveSurfacesCount < MAX_PROBE_EMISSIVE_PATCHES ) {
			m_ppEmissiveSurfaces[m_EmissiveSurfacesCount++] = S;
			S = (Surface*) S->pNext;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Gather information on each surface
	m_MeanDistance = 0.0;
	m_MeanHarmonicDistance = 0.0;
	m_MinDistance = 1e6;
	m_MaxDistance = 0.0;

	int	SumCardinality = 0;
	for ( U32 SurfaceIndex=0; SurfaceIndex < m_SurfacesCount; SurfaceIndex++ ) {
		Surface&	S = *m_ppSurfaces[SurfaceIndex];
		S.ID = SurfaceIndex;

		// Post-process the pixels to find the one closest to the probe to use as our centroid
		Pixel*	pBestPixel = S.pPixels;
		float3	AverageNormal = float3::Zero;
		float3	AverageAlbedo = float3::Zero;

		Pixel*	pPixel = S.pPixels;
		while ( pPixel != NULL ) {
			if ( pPixel->Distance < pBestPixel->Distance )
				pBestPixel = pPixel;

			AverageNormal = AverageNormal + pPixel->Normal;
			AverageAlbedo = AverageAlbedo + pPixel->Albedo;

			// Update min/max/avg
			m_MeanDistance += pPixel->Distance;
			m_MinDistance = min( m_MinDistance, pPixel->Distance );
			m_MaxDistance = max( m_MaxDistance, pPixel->Distance );
			m_MeanHarmonicDistance += 1.0 / pPixel->Distance;

			pPixel = pPixel->pNext;
		}

		AverageNormal = AverageNormal / float(S.PixelsCount);
		AverageAlbedo = AverageAlbedo / float(S.PixelsCount);

		S.Position = pBestPixel->Position;	// Our new winner!
		S.Normal = AverageNormal;
		S.SetAlbedo( AverageAlbedo );

		// Count pixels in the surface for statistics
		SumCardinality += S.PixelsCount;

		// Finally, encode SH & find principal axes
		S.EncodeSH();

// Find a faster way!
//		S.FindPrincipalAxes();
	}

	m_MeanHarmonicDistance = SumCardinality / m_MeanHarmonicDistance;
	m_MeanDistance /= SumCardinality;

	// Do the same for emissive surfaces
	for ( U32 SurfaceIndex=0; SurfaceIndex < m_EmissiveSurfacesCount; SurfaceIndex++ ) {
		Surface&	S = *m_ppEmissiveSurfaces[SurfaceIndex];
		S.ID = SurfaceIndex;

		// Post-process the pixels to find the one closest to the probe to use as our centroid
		Pixel*	pBestPixel = S.pPixels;
		Pixel*	pPixel = S.pPixels;
		while ( pPixel != NULL ) {
			if ( pPixel->Distance < pBestPixel->Distance ) {
				pBestPixel = pPixel;
			}
			pPixel = pPixel->pNext;
		}

		S.Position = pBestPixel->Position;	// Our new winner!

		// Finally, encode SH & find principal axes & samples
		S.EncodeEmissiveSH();
	}

	// Generate sampling points for regular surfaces
	U32	TotalSamplesCount = _MaxLightingSamplesCount;
	if ( TotalSamplesCount < m_SurfacesCount ) {
		// Force samples count to match surfaces count!
//		MessageBox( "The amount of samples for the probe was chosen to be " + TotalSamplesCount + " which is inferior to the amount of surfaces, this would mean some surfaces wouldn't even get sampled so the actual amount of samples is at least surface to the amount of surfaces (" + m_Patches.Length + ")", MessageBoxButtons.OK, MessageBoxIcon.Warning );
		TotalSamplesCount = m_SurfacesCount;
	}

	for ( int SurfaceIndex=m_SurfacesCount-1; SurfaceIndex >= 0; SurfaceIndex-- ) {	// We start from the smallest surfaces to ensure they get some samples
		Surface&	S = *m_ppSurfaces[SurfaceIndex];

		U32	SamplesCount = TotalSamplesCount * S.PixelsCount / SumCardinality;
			SamplesCount = max( 1, SamplesCount );				// Ensure we have at least 1 sample no matter what!
			SamplesCount = min( SamplesCount, S.PixelsCount );	// Can't have more samples than pixels!

		ASSERT( SamplesCount > 0 , "We have a surface with NO light sample!" );

		S.GenerateSamples( SamplesCount );

		// Reduce the amount of available samples and the count of remaining pixels so the remaining surfaces share the remaining samples...
		TotalSamplesCount -= SamplesCount;
		SumCardinality -= S.PixelsCount;
	}
}


#pragma region Flood Fill Algorithm

// This should be a faster version (and much less recursive!) than the original flood fill I wrote some time ago
// The idea here is to process an entire scanline first (going left and right and collecting valid scanline pixels along the way)
//  then for each of these pixels we move up/down and fill the top/bottom scanlines from these new seeds...
//
void	SHProbeEncoder::FloodFill( Surface& _Patch, Pixel* _PreviousPixel, Pixel* _P, Pixel*& _RejectedPixels ) const {

	static int	RecursionLevel = 0;	// For debugging purpose

	if ( !CheckAndAcceptPixel( _Patch, *_PreviousPixel, *_P, _RejectedPixels ) )
		return;

	//////////////////////////////////////////////////////////////////////////
	// Check the entire scanline
	int	ScanlineStartIndex = m_ScanlinePixelIndex;
	m_ppScanlinePixelsPool[m_ScanlinePixelIndex++] = _P;	// This pixel is implicitly on the scanline

	{	// Start going right
		CubeMapPixelWalker	P( *this, *_P );
		Pixel*	Previous = _P;
		Pixel*	Current = &P.Right();
		while ( CheckAndAcceptPixel( _Patch, *Previous, *Current, _RejectedPixels ) ) {
			m_ppScanlinePixelsPool[m_ScanlinePixelIndex++] = Current;
			Previous = Current;
			Current = &P.Right();
		}
	}

	{	// Start going left
		CubeMapPixelWalker	P( *this, *_P );
		Pixel*	Previous = _P;
		Pixel*	Current = &P.Left();
		while ( CheckAndAcceptPixel( _Patch, *Previous, *Current, _RejectedPixels ) ) {
			m_ppScanlinePixelsPool[m_ScanlinePixelIndex++] = Current;
			Previous = Current;
			Current = &P.Left();
		}
	}

	RecursionLevel++;

	int	ScanlineEndIndex = m_ScanlinePixelIndex;

	//////////////////////////////////////////////////////////////////////////
	// Recurse into each pixel of the top scanline
	for ( int ScanlinePixelIndex=ScanlineStartIndex; ScanlinePixelIndex < ScanlineEndIndex; ScanlinePixelIndex++ ) {
		Pixel*	P = m_ppScanlinePixelsPool[ScanlinePixelIndex];
		CubeMapPixelWalker	Walker( *this, *P );
		Pixel*	Top = &Walker.Up();
		FloodFill( _Patch, P, Top, _RejectedPixels );
	}

	//////////////////////////////////////////////////////////////////////////
	// Recurse into each pixel of the bottom scanline
	for ( int ScanlinePixelIndex=ScanlineStartIndex; ScanlinePixelIndex < ScanlineEndIndex; ScanlinePixelIndex++ ) {
		Pixel*	P = m_ppScanlinePixelsPool[ScanlinePixelIndex];
		CubeMapPixelWalker	Walker( *this, *P );
		Pixel*	Bottom = &Walker.Down();
		FloodFill( _Patch, P, Bottom, _RejectedPixels );
	}

	RecursionLevel--;
}

bool	SHProbeEncoder::CheckAndAcceptPixel( Surface& _Patch, Pixel& _PreviousPixel, Pixel& _P, Pixel*& _pRejectedPixels ) const {
	// Start by checking if we can use that pixel
	if ( !_P.IsFloodFillAcceptable() ) {
		return false;
	}

	// Check some additional criterions for a match
	bool	Accepted = false;

	if ( _PreviousPixel.EmissiveMatID != ~0UL && _P.EmissiveMatID != ~0UL ) {
		// Emissive pixels get grouped together
		Accepted = _PreviousPixel.EmissiveMatID == _P.EmissiveMatID;
	} else {
		// First, let's check the angular discrepancy
		float	Dot = _PreviousPixel.Normal | _P.Normal;
		if ( Dot > ANGULAR_THRESHOLD ) {
			// Next, let's check the distance discrepancy
			float	DistanceDiff = fabsf( _PreviousPixel.SmoothedDistance - _P.SmoothedDistance );
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
				float	ColorDiff = (_PreviousPixel.Albedo - _P.Albedo).LengthSq();
				if ( ColorDiff < ALBEDO_RGB_THRESHOLD*ALBEDO_RGB_THRESHOLD ) {
					Accepted = true;	// Winner!
				}
			}
		}
	}

	// Mark the pixel as member of this surface, even if it's rejected (rejected pixels get removed from surface in the end)
	_P.pParentSurface = &_Patch;

	if ( Accepted ) {
		// We got a new member for the surface!
		_P.pNext = _Patch.pPixels;
		_Patch.pPixels = &_P;
		_Patch.Importance += _P.Importance;	// Accumulate average importance
		_Patch.PixelsCount++;
	}
	else {
		// Sorry buddy, we'll add you to the rejects...
		_P.pNext = _pRejectedPixels;
		_pRejectedPixels = &_P;
	}

	return Accepted;
}

#pragma region Adjacency Walker

// Contains the new cube face index when stepping outside of a cube face by the left/right/top/bottom
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
// Transforms arrays are simple matrices:
//	| Tu Xu Yy |
//	| Tv Xv Yv |
//
// Which are used like this:
//	U' = Tu * CUBE_MAP_SIZE + Xu * U + Yu * V
//	V' = Tv * CUBE_MAP_SIZE + Xv * U + Yv * V
//
const int C = SHProbeEncoder::CUBE_MAP_SIZE;
const int C2 = 2*C-1;
const int C_ = SHProbeEncoder::CUBE_MAP_SIZE-1;
int	GoLeftTransforms[6][6] = {	// U=-1
	// Going left from +X sends us to +Z
	{	 C,  1,  0,		// U' = C + U	(C is the CUBE_MAP_SIZE)
		 0,  0,  1 },	// V' = V
	// Going left from -X sends us to -Z
	{	 C,  1,  0,		// U' = C + U
		 0,  0,  1 },	// V' = V
	// Going left from +Y sends us to -X
	{	 0,  0,  1,		// U' = V
		-1, -1,  0 },	// V' = -1 - U
	// Going left from -Y sends us to -X
	{	C_,  0, -1,		// U' = C-1 - V
		 C,  1,  0 },	// V' = C + U
	// Going left from +Z sends us to -X
	{	 C,  1,  0,		// U' = C + U
		 0,  0,  1 },	// V' = V
	// Going left from -Z sends us to +X
	{	 C,  1,  0,		// U' = C + U
		 0,  0,  1 },	// V' = V
};
int	GoRightTransforms[6][6] = {	// U=C
	// Going right from +X sends us to -Z
	{	-C,  1,  0,		// U' = -C + U	(C is the CUBE_MAP_SIZE)
		 0,  0,  1 },	// V' = V
	// Going right from -X sends us to +Z
	{	-C,  1,  0,		// U' = -C + U
		 0,  0,  1 },	// V' = V
	// Going right from +Y sends us to +X
	{	C_,  0, -1,		// U' = C-1 - V
		-C,  1,  0 },	// V' = -C + U
	// Going right from -Y sends us to +X
	{	 0,  0,  1,		// U' = V
		C2, -1,  0 },	// V' = 2C-1 - U
	// Going right from +Z sends us to +X
	{	-C,  1,  0,		// U' = -C + U
		 0,  0,  1 },	// V' = V
	// Going right from -Z sends us to -X
	{	-C,  1,  0,		// U' = -C + U
		 0,  0,  1 },	// V' = V
};
int	GoDownTransforms[6][6] = {	// V=C
	// Going down from +X sends us to -Y
	{	C2,  0, -1,		// U' = 2C-1 - V	(C is the CUBE_MAP_SIZE)
		 0,  1,  0 },	// V' = U
	// Going down from -X sends us to -Y
	{	-C,  0,  1,		// U' = -C + V
		C_, -1,  0 },	// V' = C-1 - U
	// Going down from +Y sends us to +Z
	{	 0,  1,  0,		// U' = U
		-C,  0,  1 },	// V' = -C + V
	// Going down from -Y sends us to -Z
	{	C_, -1,  0,		// U' = C-1 - U
		C2,  0, -1 },	// V' = 2C-1 - V
	// Going down from +Z sends us to -Y
	{	 0,  1,  0,		// U' = U
		-C,  0,  1 },	// V' = -C + V
	// Going down from -Z sends us to -Y
	{	C_, -1,  0,		// U' = C-1 - U
		C2,  0, -1 },	// V' = 2C-1 - V
};
int	GoUpTransforms[6][6] = {	// V=-1
	// Going up from +X sends us to +Y
	{	C-2,  0, -1,	// U' = C-2 - V	(C is the CUBE_MAP_SIZE)
		C_,  -1,  0 },	// V' = C-1 - U
	// Going up from -X sends us to +Y
	{	-1,  0, -1,		// U' = -1 - V
		 0,  1,  0 },	// V' = U
	// Going up from +Y sends us to -Z
	{	C_, -1,  0,		// U' = C-1 - U
		-1,  0, -1 },	// V' = -1 - V
	// Going up from -Y sends us to +Z
	{	 0,  1,  0,		// U' = U
		 C,  0,  1 },	// V' = C + V
	// Going up from +Z sends us to +Y
	{	 0,  1,  0,		// U' = U
		 C,  0,  1 },	// V' = C + V
	// Going up from -Z sends us to +Y
	{	C_, -1,  0,		// U' = C-1 - U
		-1,  0, -1 },	// V' = -1 - V
};

void SHProbeEncoder::CubeMapPixelWalker::Set( Pixel& _Pixel ) {
	CubeFaceIndex = _Pixel.CubeFaceIndex;
	U = _Pixel.CubeFaceX;
	V = _Pixel.CubeFaceY;
	Ux = 1;	Uy = 0;
	Vx = 0; Vy = 1;
}
SHProbeEncoder::Pixel&	SHProbeEncoder::CubeMapPixelWalker::Get() const {
	int	FinalPixelIndex = CUBE_MAP_FACE_SIZE * CubeFaceIndex + CUBE_MAP_SIZE * V + U;
	Pixel&	Result = Owner.m_pCubeMapPixels[FinalPixelIndex];
	return Result;
}
SHProbeEncoder::Pixel& SHProbeEncoder::CubeMapPixelWalker::Left() {
//	GoToAdjacentPixel( -1, 0 );	// U-1
	return Get();
}
SHProbeEncoder::Pixel& SHProbeEncoder::CubeMapPixelWalker::Right() {
	GoToAdjacentPixel( 0, +1 );	// U+1
	return Get();
}
SHProbeEncoder::Pixel& SHProbeEncoder::CubeMapPixelWalker::Down() {
	GoToAdjacentPixel( 0, +1 );	// V+1
	return Get();
}
SHProbeEncoder::Pixel& SHProbeEncoder::CubeMapPixelWalker::Up() {
	GoToAdjacentPixel( 0, -1 );	// V-1
	return Get();
}
void	SHProbeEncoder::CubeMapPixelWalker::TransformUV( int _Transform[6] ) {
	// Transform position
	int	TempX = _Transform[0] + _Transform[1] * U + _Transform[2] * V;
	int	TempY = _Transform[3] + _Transform[4] * U + _Transform[5] * V;
	U = TempX;
	V = TempY;
	ASSERT( U==0 || U==C-1 || V==0 || V==C-1, "One of the coordinates should be at an edge!" );

	// Transform directions
	int	OldUx = Ux, OldUy = Uy;
	int	OldVx = Vx, OldVy = Vy;
	Ux = _Transform[1] * OldUx + _Transform[4] * OldUy;
	Uy = _Transform[2] * OldUx + _Transform[5] * OldUy;
	Vx = _Transform[1] * OldVx + _Transform[4] * OldVy;
	Vy = _Transform[2] * OldVx + _Transform[5] * OldVy;
}

void	SHProbeEncoder::CubeMapPixelWalker::GoToAdjacentPixel( int _dU, int _dV ) {
	U += _dU * Ux + _dV * Vx;
	V += _dU * Uy + _dV * Vy;

	if ( U < 0 ) {
		// Stepped out through left side
		TransformUV( GoLeftTransforms[CubeFaceIndex] );
		CubeFaceIndex = GoLeft[CubeFaceIndex];
	}
	if ( U >= CUBE_MAP_SIZE ) {
		// Stepped out through right side
		TransformUV( GoRightTransforms[CubeFaceIndex] );
		CubeFaceIndex = GoRight[CubeFaceIndex];
	}
	if ( V < 0 ) {
		// Stepped out through top side
		TransformUV( GoUpTransforms[CubeFaceIndex] );
		CubeFaceIndex = GoUp[CubeFaceIndex];
	}
	if ( V >= CUBE_MAP_SIZE ) {
		// Stepped out through bottom side
		TransformUV( GoDownTransforms[CubeFaceIndex] );
		CubeFaceIndex = GoDown[CubeFaceIndex];
	}
}

#pragma endregion

#pragma endregion

#pragma endregion


//////////////////////////////////////////////////////////////////////////
//
void	SHProbeEncoder::ReadBackProbeCubeMap( Texture2D& _StagingCubeMap ) {

	m_NearestNeighborProbeDistance = 0.0f;
	m_FarthestNeighborProbeDistance = 0.0f;

	m_MeanDistance = 0.0;
	m_MeanHarmonicDistance = 0.0;
	m_MinDistance = 0.0;
	m_MaxDistance = 0.0;

	m_pScenePixels = NULL;
	m_ScenePixelsCount = 0;

	double	dA = 4.0 / (CUBE_MAP_SIZE*CUBE_MAP_SIZE);	// Cube face is supposed to be in [-1,+1], yielding a 2x2 square units
	double	SumSolidAngle = 0.0;

	m_MeanDistance = 0.0;
	m_MeanHarmonicDistance = 0.0;
	m_MinDistance = 1e6;
	m_MaxDistance = 0.0;
	m_BBoxMin =  float3::MaxFlt;
	m_BBoxMax = -float3::MaxFlt;

	int		NegativeImportancePixelsCount = 0;
	for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ ) {
		Pixel*	pCubeMapPixels = &m_pCubeMapPixels[CubeFaceIndex*CUBE_MAP_FACE_SIZE];

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
			for ( int X=0; X < CUBE_MAP_SIZE; X++, P++, pFaceData0++, pFaceData1++, pFaceData2++, pFaceData3++ ) {
				// ==== Read back albedo & unique face ID ====
				float	Red = pFaceData0->x;
				float	Green = pFaceData0->y;
				float	Blue = pFaceData0->z;

				// Work with better precision
				Red *= PI;
				Green *= PI;
				Blue *= PI;

				P->SetAlbedo( float3( Red, Green, Blue ) );
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
				P->Normal.Set( Nx, Ny, Nz );
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
				P->SmoothedDistance = 0.0f;
				P->Infinity = Distance > Z_INFINITY_TEST;

				P->InitSH();

				if ( P->Infinity )
					continue;	// Not part of the scene's geometry!

				// Link-in a new scene pixel (i.e. not infinity)
				P->pNext = m_pScenePixels;
				m_pScenePixels = P;
				m_ScenePixelsCount++;

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

	// Perform bilateral filtered smoothing of adjacent distances
	for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ ) {
		Pixel*	pCubeMapPixels = &m_pCubeMapPixels[CubeFaceIndex*CUBE_MAP_FACE_SIZE];
		Pixel*	P = pCubeMapPixels;
		for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
			for ( int X=0; X < CUBE_MAP_SIZE; X++, P++ ) {
				if ( P->Infinity ) {
					continue;	// This only concerns scene pixels...
				}

				CubeMapPixelWalker	Walk( *this, *P );
				Pixel&	P01 = Walk.Up();	// Top
				Pixel&	P02 = Walk.Right();	// Top Right
				Pixel&	P12 = Walk.Down();	// Right
				Pixel&	P22 = Walk.Down();	// Bottom Right
				Pixel&	P21 = Walk.Left();	// Bottom
				Pixel&	P20 = Walk.Left();	// Bottom Left
				Pixel&	P10 = Walk.Up();	// Left
				Pixel&	P00 = Walk.Up();	// Top left

				float	SumDistance = P->Distance;
				float	Count = 1;
				if ( !P00.Infinity ) { SumDistance += P00.Distance; Count++; }
				if ( !P01.Infinity ) { SumDistance += P01.Distance; Count++; }
				if ( !P02.Infinity ) { SumDistance += P02.Distance; Count++; }
				if ( !P10.Infinity ) { SumDistance += P10.Distance; Count++; }
				if ( !P12.Infinity ) { SumDistance += P12.Distance; Count++; }
				if ( !P20.Infinity ) { SumDistance += P20.Distance; Count++; }
				if ( !P21.Infinity ) { SumDistance += P21.Distance; Count++; }
				if ( !P22.Infinity ) { SumDistance += P22.Distance; Count++; }
				P->SmoothedDistance = SumDistance / Count;
			}
	}
}

//////////////////////////////////////////////////////////////////////////
//
const double	SHProbeEncoder::Pixel::f0 = 0.5 / PI;
const double	SHProbeEncoder::Pixel::f1 = sqrt(3.0) * SHProbeEncoder::Pixel::f0;
const double	SHProbeEncoder::Pixel::f2 = sqrt(15.0) * SHProbeEncoder::Pixel::f0;
const double	SHProbeEncoder::Pixel::f3 = sqrt(5.0) * 0.5 * SHProbeEncoder::Pixel::f0;

float	SHProbeEncoder::Pixel::IMPORTANCE_THRESOLD = 0.0f;

void	SHProbeEncoder::Surface::GenerateSamples( int _SamplesCount ) {
	ASSERT( false, "TODO!" );
/*
	Sample*	Samples = new Sample[_SamplesCount];
	ASSERT( _SamplesCount <= PixelsCount, "More samples than pixels in the surface! This is useless!" );

	int	PixelGroupSize = max( 1, PixelsCount / _SamplesCount );
	for ( int SampleIndex=0; SampleIndex < _SamplesCount; SampleIndex++ ) {
		Pixel	P = SetPixels[SampleIndex * PixelGroupSize];	// Arbitrary!
// TODO: Choose well spaced pixels to cover the maximum area for this set!

		// Find the nearest pixels around that pixel
		List<Pixel*>	NearestPixels = new List<Pixel*>();
		NearestPixels.AddRange( SetPixels );
		__ReferencePixel = P;
		NearestPixels.Sort( this );	// Our comparer will sort by dot product of the view vector with the reference pixel's view vector, "efficiently" grouping pixels as disks
		NearestPixels.RemoveRange( PixelGroupSize, NearestPixels.Count-PixelGroupSize );	// Remove all pixels outside of the group

		// Compute an average normal & the disc radius (i.e. farthest pixel from reference)
		float	Radius = 0.0f;
		float3	AverageNormal = float3::Zero;
		Pixel*	P2 = NearestPixels[0];
		foreach ( Pixel P2 in NearestPixels ) {
			AverageNormal += P2.Normal;

			float	Distance = (P2.Position - P.Position).Length;
			Radius = max( Radius, Distance );
		}
//		AverageNormal /= NearestPixels.Count;
		AverageNormal.Normalize();

		// Store sample
		Samples[SampleIndex].Position = P.Position;
		Samples[SampleIndex].Normal = AverageNormal;
		Samples[SampleIndex].Radius = Radius;
	}

	// Associate pixels to their closest sample
	Pixel*	P = pPixels;
	while ( P != NULL) {
		float	ClosestDistance = 1e6f;
		int		ClosestSampleIndex = -1;
		for ( int SampleIndex=0; SampleIndex < SamplesCount; SampleIndex++ ) {
			float	Distance2Sample = (P->Position - Samples[SampleIndex].Position).LengthSq();
			if ( Distance2Sample >= ClosestDistance )
				continue;

			ClosestDistance = Distance2Sample;
			ClosestSampleIndex = SampleIndex;
		}

		P->ParentPatchSampleIndex = ClosestSampleIndex;
		P = P->pNext;
	}*/
}

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
// Without the floating-point sign flipping because we don't care about that here.
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
