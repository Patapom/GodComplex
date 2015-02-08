#include "../../GodComplex.h"

const float	SHProbeEncoder::Z_INFINITY = 1e6f;
const float	SHProbeEncoder::Z_INFINITY_TEST = 0.99f * SHProbeEncoder::Z_INFINITY;

SHProbeEncoder::SHProbeEncoder() {
	m_CubeMapPixels = new Pixel[6*SHProbeNetwork::CUBE_MAP_SIZE*SHProbeNetwork::CUBE_MAP_SIZE];
	Pixel*	pPixel = m_CubeMapPixels;
	for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
		for ( int Y=0; Y < SHProbeNetwork::CUBE_MAP_SIZE; Y++ )
			for ( int X=0; X < SHProbeNetwork::CUBE_MAP_SIZE; X++, pPixel++ ) {
				pPixel->Index = pPixel - m_CubeMapPixels;
				pPixel->CubeFaceIndex = CubeFaceIndex;
				pPixel->CubeFaceX = X;
				pPixel->CubeFaceY = Y;
			}

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

	m_MaxFaceIndex = 0;

	m_NearestNeighborProbeDistance = 0.0f;
	m_FarthestNeighborProbeDistance = 0.0f;

	m_MeanDistance = 0.0;
	m_MeanHarmonicDistance = 0.0;
	m_MinDistance = 0.0;
	m_MaxDistance = 0.0;


	m_ProbePixels.Clear();
	m_ScenePixels.Clear();

	const int	CUBE_MAP_SIZE = SHProbeNetwork::CUBE_MAP_SIZE;

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
		Pixel*	pCubeMapPixels = &m_CubeMapPixels[CubeFaceIndex*SHProbeNetwork::CUBE_MAP_SIZE*SHProbeNetwork::CUBE_MAP_SIZE];

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
