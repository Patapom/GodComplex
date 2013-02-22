#include "../GodComplex.h"

#define VWRITE( pVertex, P, N, T, UV )	WriteVertex( _Writer, pVertex, P, N, T, UV, _TweakVertex, _pUserData );	pVertex+=VStride;	VerticesCount--
#define IWRITE( pIndex, i )				_Writer.WriteIndex( pIndex, i );	pIndex+=IStride;	IndicesCount--


//////////////////////////////////////////////////////////////////////////
//
void	GeometryBuilder::BuildSphere( int _PhiSubdivisions, int _ThetaSubdivisions, IGeometryWriter& _Writer, const MapperBase& _Mapper, TweakVertexDelegate _TweakVertex, void* _pUserData )
{
	int	BandLength = _PhiSubdivisions;
	int	VerticesCount = (BandLength+1) * (1 + _ThetaSubdivisions + 1);	// 1 band at the top and bottom of the sphere + as many subdivisions as required

	int	BandsCount = 1 + _ThetaSubdivisions;
	int	IndicesCount = (2*(BandLength+1+1)) * BandsCount - 2;

	// Create the buffers
	void*	pVerticesArray = NULL;
	void*	pIndicesArray = NULL;
	int		VStride, IStride;

	_Writer.CreateBuffers( VerticesCount, IndicesCount, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, pVerticesArray, pIndicesArray, VStride, IStride );
	ASSERT( pVerticesArray != NULL, "Invalid vertex buffer !" );
	ASSERT( pIndicesArray != NULL, "Invalid index buffer !" );

	U8*		pVertex = (U8*) pVerticesArray;
	U8*		pIndex = (U8*) pIndicesArray;

	//////////////////////////////////////////////////////////////////////////
	// Build vertices
	NjFloat3	Position, Normal, Tangent;
	NjFloat2	UV;

	// Top band
	{
		for ( int i=0; i <= BandLength; i++ )
		{
			float	Phi = TWOPI * i / BandLength;
			Tangent.x = cosf( Phi );
			Tangent.y = 0.0f;
			Tangent.z = -sinf( Phi );

			// Create a dummy position that is slightly offseted from the top of the sphere so UVs are not all identical
			Position.y = 1.0f;
			Position.x = 0.001f * Tangent.z;
			Position.z = 0.001f * Tangent.x;
//			Position.Normalize();
			Normal = Position;	Normal.Normalize();

			// Ask for UVs
			_Mapper.Map( Position, Normal, Tangent, UV );

			// Write vertex
			VWRITE( pVertex, NjFloat3::UnitY, NjFloat3::UnitY, Tangent, UV );
		}
	}

	// Generic bands
	for ( int j=0; j < _ThetaSubdivisions; j++ )
	{
		float	Theta = PI * (1+j) / (1 + _ThetaSubdivisions);
		for ( int i=0; i <= BandLength; i++ )
		{
			float	Phi = TWOPI * i / BandLength;

			Position.x = sinf( Phi ) * sinf( Theta );
			Position.y = cosf( Theta );
			Position.z = cosf( Phi ) * sinf( Theta );

			Normal = Position;

			Tangent.x = cosf( Phi );
			Tangent.y = 0.0f;
			Tangent.z = -sinf( Phi );

			// Ask for UVs
			_Mapper.Map( Position, Normal, Tangent, UV );

			// Write vertex
			VWRITE( pVertex, Position, Normal, Tangent, UV );
		}
	}

	// Bottom band
	{
		for ( int i=0; i <= BandLength; i++ )
		{
			float	Phi = TWOPI * i / BandLength;
			Tangent.x = cosf( Phi );
			Tangent.y = 0.0f;
			Tangent.z = -sinf( Phi );

			// Create a dummy position that is slightly offseted from the bottom of the sphere so UVs are not all identical
			Position.y = -1.0f;
			Position.x = 0.001f * Tangent.z;
			Position.z = 0.001f * Tangent.x;
//			Position.Normalize();
			Normal = Position;	Normal.Normalize();

			// Ask for UVs
			_Mapper.Map( Position, Normal, Tangent, UV );

			// Write vertex
			VWRITE( pVertex, -NjFloat3::UnitY, -NjFloat3::UnitY, Tangent, UV );
		}
	}
	ASSERT( VerticesCount == 0, "Wrong contruction !" );


	//////////////////////////////////////////////////////////////////////////
	// Build indices
	for ( int j=0; j < BandsCount; j++ )
	{
		int	CurrentBandOffset = j * (BandLength+1);
		int	NextBandOffset = (j+1) * (BandLength+1);

		for ( int i=0; i <= BandLength; i++ )
		{
			IWRITE( pIndex, CurrentBandOffset + i );
			IWRITE( pIndex, NextBandOffset + i );
		}

		if ( j == BandsCount-1 )
			continue;	// Not for the last band...

		// Write 2 last degenerate indices so we smoothly transition to next band
		IWRITE( pIndex, NextBandOffset + BandLength );
		IWRITE( pIndex, NextBandOffset + BandLength+1 );
	}
	ASSERT( IndicesCount == 0, "Wrong contruction !" );

	//////////////////////////////////////////////////////////////////////////
	// Finalize
	_Writer.Finalize( pVerticesArray, pIndicesArray );
}

void	GeometryBuilder::BuildTorus( int _PhiSubdivisions, int _ThetaSubdivisions, float _LargeRadius, float _SmallRadius, IGeometryWriter& _Writer, const MapperBase& _Mapper, TweakVertexDelegate _TweakVertex, void* _pUserData )
{
	int	BandLength = _ThetaSubdivisions;
	int	BandsCount = _PhiSubdivisions;

	int	VerticesCount = BandsCount * (BandLength+1);
	int	IndicesCount = 2*(BandLength+1+1) * BandsCount - 2;

	// Create the buffers
	void*	pVerticesArray = NULL;
	void*	pIndicesArray = NULL;
	int		VStride, IStride;

	_Writer.CreateBuffers( VerticesCount, IndicesCount, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, pVerticesArray, pIndicesArray, VStride, IStride );
	ASSERT( pVerticesArray != NULL, "Invalid vertex buffer !" );
	ASSERT( pIndicesArray != NULL, "Invalid index buffer !" );

	U8*		pVertex = (U8*) pVerticesArray;
	U8*		pIndex = (U8*) pIndicesArray;

	//////////////////////////////////////////////////////////////////////////
	// Build vertices
	NjFloat3	Position, Normal, Tangent;
	NjFloat2	UV;

	for ( int j=0; j < BandsCount; j++ )
	{
		float		Phi = TWOPI * j / BandsCount;

		NjFloat3	X( cosf(Phi), sinf(Phi), 0.0f );	// Radial branch in X^Y plane at this angle
		NjFloat3	Center = _LargeRadius * X;			// Center of the small ring

		Tangent.x = -sinf(Phi);
		Tangent.y = cosf(Phi);
		Tangent.z = 0.0f;

		for ( int i=0; i <= BandLength; i++ )
		{
			float	Theta = TWOPI * i / BandLength;

			Normal = cosf(Theta) * X + sinf(Theta) * NjFloat3::UnitZ;
			Position = Center + _SmallRadius * Normal;
			_Mapper.Map( Position, Normal, Tangent, UV );

			VWRITE( pVertex, Position, Normal, Tangent, UV );
		}
	}
	ASSERT( VerticesCount == 0, "Wrong contruction !" );

	//////////////////////////////////////////////////////////////////////////
	// Build indices
	for ( int j=0; j < BandsCount; j++ )
	{
		int	CurrentBandOffset = j * (BandLength+1);
		int	NextBandOffset = ((j+1) % _PhiSubdivisions) * (BandLength+1);
		int	NextNextBandOffset = ((j+2) % _PhiSubdivisions) * (BandLength+1);

		for ( int i=0; i <= BandLength; i++ )
		{
			IWRITE( pIndex, CurrentBandOffset + i );
			IWRITE( pIndex, NextBandOffset + i );
		}

		if ( j == BandsCount-1 )
			continue;	// Not for the last band...

		// Write 2 last degenerate indices so we smoothly transition to next band
		IWRITE( pIndex, NextBandOffset + BandLength );
		IWRITE( pIndex, NextNextBandOffset );
	}
	ASSERT( IndicesCount == 0, "Wrong contruction !" );

	//////////////////////////////////////////////////////////////////////////
	// Finalize
	_Writer.Finalize( pVerticesArray, pIndicesArray );
}

void	GeometryBuilder::BuildPlane( int _SubdivisionsX, int _SubdivisionsY, const NjFloat3& _X, const NjFloat3& _Y, IGeometryWriter& _Writer, const MapperBase& _Mapper, TweakVertexDelegate _TweakVertex, void* _pUserData )
{
	ASSERT( _SubdivisionsX > 0 && _SubdivisionsY > 0, "Can't create a plane with 0 subdivision!" );

	int	VerticesCount = (_SubdivisionsX+1) * (_SubdivisionsY+1);
	int	IndicesCount = 2*(_SubdivisionsX+1+1) * _SubdivisionsY - 2;

	// Create the buffers
	void*	pVerticesArray = NULL;
	void*	pIndicesArray = NULL;
	int		VStride, IStride;

	_Writer.CreateBuffers( VerticesCount, IndicesCount, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, pVerticesArray, pIndicesArray, VStride, IStride );
	ASSERT( pVerticesArray != NULL, "Invalid vertex buffer !" );
	ASSERT( pIndicesArray != NULL, "Invalid index buffer !" );

	U8*		pVertex = (U8*) pVerticesArray;
	U8*		pIndex = (U8*) pIndicesArray;

	//////////////////////////////////////////////////////////////////////////
	// Build vertices
	NjFloat3	Tangent = _X;					Tangent.Normalize();
	NjFloat3	BiTangent = _Y;					BiTangent.Normalize();
	NjFloat3	Normal = Tangent ^ BiTangent;	Normal.Normalize();

	NjFloat3	Position;
	NjFloat2	UV;

	for ( int j=0; j <= _SubdivisionsY; j++ )
	{
		float	Y = 1.0f - 2.0f * j / _SubdivisionsY;
		for ( int i=0; i <= _SubdivisionsX; i++ )
		{
			float	X = 2.0f * i / _SubdivisionsX - 1.0f;

			Position = X * _X + Y * _Y;
			_Mapper.Map( Position, Normal, Tangent, UV );

			VWRITE( pVertex, Position, Normal, Tangent, UV );
		}
	}
	ASSERT( VerticesCount == 0, "Wrong contruction !" );

	//////////////////////////////////////////////////////////////////////////
	// Build indices
	for ( int j=0; j < _SubdivisionsY; j++ )
	{
		int	CurrentBandOffset = j * (_SubdivisionsX+1);
		int	NextBandOffset = (j+1) * (_SubdivisionsX+1);

		for ( int i=0; i <= _SubdivisionsX; i++ )
		{
			IWRITE( pIndex, CurrentBandOffset + i );
			IWRITE( pIndex, NextBandOffset + i );
		}

		if ( j == _SubdivisionsY-1 )
			continue;	// Not for the last band...

		// Write 2 last degenerate indices so we smoothly transition to next band
		IWRITE( pIndex, NextBandOffset-1 );
		IWRITE( pIndex, NextBandOffset );
	}
	ASSERT( IndicesCount == 0, "Wrong contruction !" );

	//////////////////////////////////////////////////////////////////////////
	// Finalize
	_Writer.Finalize( pVerticesArray, pIndicesArray );
}


void	GeometryBuilder::WriteVertex( IGeometryWriter& _Writer, void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV, TweakVertexDelegate _TweakVertex, void* _pUserData )
{
	if ( _TweakVertex == NULL )
	{
		_Writer.WriteVertex( _pVertex, _Position, _Normal, _Tangent, _UV );
		return;
	}

	// Ask the user to tweak the vertices first !
	NjFloat3	P = _Position;
	NjFloat3	N = _Normal;
	NjFloat3	T = _Tangent;
	NjFloat2	UV = _UV;
	(*_TweakVertex)( P, N, T, UV, _pUserData );
	_Writer.WriteVertex( _pVertex, P, N, T, UV );
}


//////////////////////////////////////////////////////////////////////////
// Spherical mapping
//
GeometryBuilder::MapperSpherical::MapperSpherical( float _WrapU, float _WrapV, const NjFloat3& _Center, const NjFloat3& _X, const NjFloat3& _Y )
	: m_WrapU( _WrapU )
	, m_WrapV( _WrapV )
	, m_Center( _Center )
	, m_X( _X )
	, m_Y( _Y )
{
	m_X.Normalize();
	m_Y.Normalize();
	m_Z = m_X ^ m_Y;
	m_Z.Normalize();
}
void	GeometryBuilder::MapperSpherical::Map( const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, NjFloat2& _UV ) const
{
	NjFloat3	Dir = _Position - m_Center;
	Dir.Normalize();
	float	X = Dir | m_X;
	float	Y = Dir | m_Y;
	float	Z = Dir | m_Z;

	float	Phi = atan2f( X, Z );		// Phi=0 at +Z and 90° at +X
	float	Theta = acosf( Y );			// Theta=0 at +Y and 180° at -Y

	_UV.x = m_WrapU * INV2PI * Phi;
	_UV.y = m_WrapV * INVPI * Theta;
}


//////////////////////////////////////////////////////////////////////////
// Planar mapping
//
GeometryBuilder::MapperPlanar::MapperPlanar( float _WrapU, float _WrapV, const NjFloat3& _Center, const NjFloat3& _Tangent, const NjFloat3& _BiTangent )
	: m_WrapU( _WrapU )
	, m_WrapV( _WrapV )
	, m_Center( _Center )
	, m_Tangent( _Tangent )
	, m_BiTangent( _BiTangent )
{
}
void	GeometryBuilder::MapperPlanar::Map( const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, NjFloat2& _UV ) const
{
	NjFloat3	Delta = _Position - m_Center;

	_UV.x = m_WrapU * (Delta | m_Tangent);
	_UV.y = m_WrapV * (Delta | m_BiTangent);
}

