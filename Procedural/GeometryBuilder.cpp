#include "../GodComplex.h"

#define VWRITE( pVertex, P, N, T, B, UV )	AppendVertex( _Writer, pVertex, P, N, T, B, UV, _TweakVertex, _pUserData );	VerticesCount--
#define IWRITE( pIndex, i )					_Writer.AppendIndex( pIndex, i );	IndicesCount--


//////////////////////////////////////////////////////////////////////////
//
void	GeometryBuilder::BuildSphere( int _PhiSubdivisions, int _ThetaSubdivisions, IGeometryWriter& _Writer, const MapperBase* _pMapper, TweakVertexDelegate _TweakVertex, void* _pUserData )
{
	int	BandLength = _PhiSubdivisions;
	int	VerticesCount = (BandLength+1) * (1 + _ThetaSubdivisions + 1);	// 1 band at the top and bottom of the sphere + as many subdivisions as required

	int	BandsCount = 1 + _ThetaSubdivisions;
	int	IndicesCount = (2*(BandLength+1+1)) * BandsCount - 2;

	// Create the buffers
	void*	pVerticesArray = NULL;
	void*	pIndicesArray = NULL;
	_Writer.CreateBuffers( VerticesCount, IndicesCount, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, pVerticesArray, pIndicesArray );
	ASSERT( pVerticesArray != NULL, "Invalid vertex buffer !" );
	ASSERT( pIndicesArray != NULL, "Invalid index buffer !" );

	void*		pVertex = pVerticesArray;
	void*		pIndex = pIndicesArray;

	//////////////////////////////////////////////////////////////////////////
	// Build vertices
	float3	Position, Normal, Tangent, BiTangent;
	float2	UV;

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
			Position.x = -0.001f * Tangent.z;
			Position.z = 0.001f * Tangent.x;
//			Position.Normalize();
			Normal = Position;	Normal.Normalize();

			BiTangent = Normal.Cross( Tangent );

			// Ask for UVs
			if ( _pMapper )
				_pMapper->Map( Position, Normal, Tangent, UV, i == BandLength );
			else
				UV.Set( 2.0f * float(i) / BandLength, 0.0f );

			Position.x = Position.z = 0.0f;

			// Write vertex
			VWRITE( pVertex, float3::UnitY, float3::UnitY, Tangent, BiTangent, UV );
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

			BiTangent = Normal.Cross( Tangent );

			// Ask for UVs
			if (_pMapper )
				_pMapper->Map( Position, Normal, Tangent, UV, i == BandLength );
			else
				UV.Set( 2.0f * float(i) / BandLength, float(j) / _ThetaSubdivisions );

			// Write vertex
			VWRITE( pVertex, Position, Normal, Tangent, BiTangent, UV );
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
			Position.x = -0.001f * Tangent.z;
			Position.z = 0.001f * Tangent.x;
//			Position.Normalize();
			Normal = Position;	Normal.Normalize();

			BiTangent = Normal.Cross( Tangent );

			// Ask for UVs
			if ( _pMapper )
				_pMapper->Map( Position, Normal, Tangent, UV, i == BandLength );
			else
				UV.Set( 2.0f * float(i) / BandLength, 1.0f );

			Position.x = Position.z = 0.0f;

			// Write vertex
			VWRITE( pVertex, -float3::UnitY, -float3::UnitY, Tangent, BiTangent, UV );
		}
	}
	ASSERT( VerticesCount == 0, "Wrong contruction!" );


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
	ASSERT( IndicesCount == 0, "Wrong contruction!" );

	//////////////////////////////////////////////////////////////////////////
	// Finalize
	_Writer.Finalize( pVerticesArray, pIndicesArray );
}

void	GeometryBuilder::BuildCylinder( int _RadialSubdivisions, int _VerticalSubdivisions, bool _bIncludeCaps, IGeometryWriter& _Writer, const MapperBase* _pMapper, TweakVertexDelegate _TweakVertex, void* _pUserData )
{
	ASSERT( _RadialSubdivisions > 1, "Can't create a cylinder with less than 2 radial subdivisions!" );
	ASSERT( _VerticalSubdivisions > 0, "Can't create a cylinder with 0 vertical subdivisions!" );

	int	BandLength = 1+_RadialSubdivisions;
	int	BandsCount = _bIncludeCaps ? 1 + (1+_VerticalSubdivisions) + 1 : 1+_VerticalSubdivisions;	// 1 band at the top and bottom for the optional caps + as many subdivisions as required
	int	VerticesCount = BandLength * BandsCount;

	int	IndicesCount = 2 * (BandLength+1) * (BandsCount-1) - 2;

	// Create the buffers
	void*	pVerticesArray = NULL;
	void*	pIndicesArray = NULL;
	_Writer.CreateBuffers( VerticesCount, IndicesCount, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, pVerticesArray, pIndicesArray );
	ASSERT( pVerticesArray != NULL, "Invalid vertex buffer !" );
	ASSERT( pIndicesArray != NULL, "Invalid index buffer !" );

	void*		pVertex = pVerticesArray;
	void*		pIndex = pIndicesArray;

	//////////////////////////////////////////////////////////////////////////
	// Build vertices
	float3	Position, Normal, Tangent, BiTangent;
	float2	UV;

	// Top vertices
	if ( _bIncludeCaps )
	{
		for ( int i=0; i <= _RadialSubdivisions; i++ )
		{
			float	Phi = TWOPI * i / _RadialSubdivisions;
			Tangent.x = cosf( Phi );
			Tangent.y = 0.0f;
			Tangent.z = -sinf( Phi );

			// Create a dummy position that is slightly offseted from the top of the sphere so UVs are not all identical
			Position.y = 1.0f;
			Position.x = -0.001f * Tangent.z;
			Position.z = 0.001f * Tangent.x;
			Normal = float3::UnitY;

			BiTangent = Normal.Cross( Tangent );

			// Ask for UVs
			if ( _pMapper )
				_pMapper->Map( Position, Normal, Tangent, UV, i == _RadialSubdivisions );
			else
				UV.Set( 2.0f * float(i) / _RadialSubdivisions, 0.0f );

			Position.x = Position.z = 0.0f;

			// Write vertex
			VWRITE( pVertex, float3::UnitY, float3::UnitY, Tangent, BiTangent, UV );
		}
	}

	// Generic bands
	for ( int j=0; j <= _VerticalSubdivisions; j++ )
	{
		float	Y = 1.0f - 2.0f * j / _VerticalSubdivisions;
		for ( int i=0; i <= _RadialSubdivisions; i++ )
		{
			float	Phi = TWOPI * i / _RadialSubdivisions;

			Position.x = sinf( Phi );
			Position.y = Y;
			Position.z = cosf( Phi );

			Normal.x = sinf( Phi );
			Normal.y = 0;
			Normal.z = cosf( Phi );

			Tangent.x = cosf( Phi );
			Tangent.y = 0.0f;
			Tangent.z = -sinf( Phi );

			BiTangent = Normal.Cross( Tangent );

			// Ask for UVs
			if ( _pMapper )
				_pMapper->Map( Position, Normal, Tangent, UV, i == _RadialSubdivisions );
			else
				UV.Set( 2.0f * float(i) / _RadialSubdivisions, float(j) / _VerticalSubdivisions );

			// Write vertex
			VWRITE( pVertex, Position, Normal, Tangent, BiTangent, UV );
		}
	}

	// Bottom band
	if ( _bIncludeCaps )
	{
		for ( int i=0; i <= _RadialSubdivisions; i++ )
		{
			float	Phi = TWOPI * i / _RadialSubdivisions;
			Tangent.x = cosf( Phi );
			Tangent.y = 0.0f;
			Tangent.z = -sinf( Phi );

			// Create a dummy position that is slightly offseted from the bottom of the sphere so UVs are not all identical
			Position.y = -1.0f;
			Position.x = -0.001f * Tangent.z;
			Position.z = 0.001f * Tangent.x;
			Normal = -float3::UnitY;

			BiTangent = Normal.Cross( Tangent );

			// Ask for UVs
			if ( _pMapper )
				_pMapper->Map( Position, Normal, Tangent, UV, i == _RadialSubdivisions );
			else
				UV.Set( 2.0f * float(i) / _RadialSubdivisions, 1.0f );

			Position.x = Position.z = 0.0f;

			// Write vertex
			VWRITE( pVertex, -float3::UnitY, -float3::UnitY, Tangent, BiTangent, UV );
		}
	}
	ASSERT( VerticesCount == 0, "Wrong contruction!" );


	//////////////////////////////////////////////////////////////////////////
	// Build indices
	for ( int j=0; j < BandsCount-1; j++ )
	{
		int	CurrentBandOffset = j * BandLength;
		int	NextBandOffset = (j+1) * BandLength;

		for ( int i=0; i < BandLength; i++ )
		{
			IWRITE( pIndex, CurrentBandOffset + i );
			IWRITE( pIndex, NextBandOffset + i );
		}

		if ( j == BandsCount-2 )
			continue;	// Not for the last band...

		// Write 2 last degenerate indices so we smoothly transition to next band
		IWRITE( pIndex, NextBandOffset + BandLength-1 );
		IWRITE( pIndex, NextBandOffset + BandLength );
	}
	ASSERT( IndicesCount == 0, "Wrong contruction!" );

	//////////////////////////////////////////////////////////////////////////
	// Finalize
	_Writer.Finalize( pVerticesArray, pIndicesArray );
}

void	GeometryBuilder::BuildTorus( int _PhiSubdivisions, int _ThetaSubdivisions, float _LargeRadius, float _SmallRadius, IGeometryWriter& _Writer, const MapperBase* _pMapper, TweakVertexDelegate _TweakVertex, void* _pUserData )
{
	int	BandLength = _ThetaSubdivisions;
	int	BandsCount = _PhiSubdivisions;

	int	VerticesCount = BandsCount * (BandLength+1);
	int	IndicesCount = 2*(BandLength+1+1) * BandsCount - 2;

	// Create the buffers
	void*	pVerticesArray = NULL;
	void*	pIndicesArray = NULL;
	_Writer.CreateBuffers( VerticesCount, IndicesCount, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, pVerticesArray, pIndicesArray );
	ASSERT( pVerticesArray != NULL, "Invalid vertex buffer !" );
	ASSERT( pIndicesArray != NULL, "Invalid index buffer !" );

	void*		pVertex = pVerticesArray;
	void*		pIndex = pIndicesArray;

	//////////////////////////////////////////////////////////////////////////
	// Build vertices
	float3	Position, Normal, Tangent, BiTangent;
	float2	UV;

	for ( int j=0; j < BandsCount; j++ )
	{
		float		Phi = TWOPI * j / BandsCount;

		float3	X( cosf(Phi), sinf(Phi), 0.0f );	// Radial branch in X^Y plane at this angle
		float3	Center = _LargeRadius * X;			// Center of the small ring

		Tangent.x = -sinf(Phi);
		Tangent.y = cosf(Phi);
		Tangent.z = 0.0f;

		for ( int i=0; i <= BandLength; i++ )
		{
			float	Theta = TWOPI * i / BandLength;

			Normal = cosf(Theta) * X + sinf(Theta) * float3::UnitZ;
			Position = Center + _SmallRadius * Normal;

			BiTangent = Normal.Cross( Tangent );

			if ( _pMapper )
				_pMapper->Map( Position, Normal, Tangent, UV, i == BandLength );
			else
				UV.Set( 4.0f * float(j) / BandsCount, float(j) / BandLength );

			VWRITE( pVertex, Position, Normal, Tangent, BiTangent, UV );
		}
	}
	ASSERT( VerticesCount == 0, "Wrong contruction!" );

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
	ASSERT( IndicesCount == 0, "Wrong contruction!" );

	//////////////////////////////////////////////////////////////////////////
	// Finalize
	_Writer.Finalize( pVerticesArray, pIndicesArray );
}

void	GeometryBuilder::BuildPlane( int _SubdivisionsX, int _SubdivisionsY, const float3& _X, const float3& _Y, IGeometryWriter& _Writer, const MapperBase* _pMapper, TweakVertexDelegate _TweakVertex, void* _pUserData )
{
	ASSERT( _SubdivisionsX > 0 && _SubdivisionsY > 0, "Can't create a plane with 0 subdivision!" );

	int	VerticesCount = (_SubdivisionsX+1) * (_SubdivisionsY+1);
	int	IndicesCount = 2*(_SubdivisionsX+1+1) * _SubdivisionsY - 2;

	// Create the buffers
	void*	pVerticesArray = NULL;
	void*	pIndicesArray = NULL;
	_Writer.CreateBuffers( VerticesCount, IndicesCount, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, pVerticesArray, pIndicesArray );
	ASSERT( pVerticesArray != NULL, "Invalid vertex buffer !" );
	ASSERT( pIndicesArray != NULL, "Invalid index buffer !" );

	void*		pVertex = pVerticesArray;
	void*		pIndex = pIndicesArray;

	//////////////////////////////////////////////////////////////////////////
	// Build vertices
	float3	Tangent = _X;							Tangent.Normalize();
	float3	BiTangent = _Y;							BiTangent.Normalize();
	float3	Normal = Tangent.Cross( BiTangent );	Normal.Normalize();

	float3	Position;
	float2	UV;

	for ( int j=0; j <= _SubdivisionsY; j++ )
	{
		float	Y = 1.0f - 2.0f * j / _SubdivisionsY;
		for ( int i=0; i <= _SubdivisionsX; i++ )
		{
			float	X = 2.0f * i / _SubdivisionsX - 1.0f;

			Position = X * _X + Y * _Y;
			if ( _pMapper )
				_pMapper->Map( Position, Normal, Tangent, UV, false );
			else
				UV.Set( float(i) / _SubdivisionsX, float(j) / _SubdivisionsY );

			VWRITE( pVertex, Position, Normal, Tangent, BiTangent, UV );
		}
	}
	ASSERT( VerticesCount == 0, "Wrong contruction!" );

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
		IWRITE( pIndex, NextBandOffset+_SubdivisionsX );
		IWRITE( pIndex, NextBandOffset );
	}
	ASSERT( IndicesCount == 0, "Wrong contruction!" );

	//////////////////////////////////////////////////////////////////////////
	// Finalize
	_Writer.Finalize( pVerticesArray, pIndicesArray );
}

void	GeometryBuilder::BuildCube( int _SubdivisionsX, int _SubdivisionsY, int _SubdivisionsZ, IGeometryWriter& _Writer, const MapperBase* _pMapper, TweakVertexDelegate _TweakVertex, void* _pUserData )
{
	ASSERT( _SubdivisionsX > 0 && _SubdivisionsY > 0 && _SubdivisionsZ > 0, "Can't create a cube with 0 subdivision!" );

	int	SizeX = _SubdivisionsX+1;
	int	SizeY = _SubdivisionsY+1;
	int	SizeZ = _SubdivisionsZ+1;

	int	VerticesCount = 2*(SizeX*SizeY + SizeX*SizeZ + SizeY*SizeZ);
	int	IndicesCount = 2*( (2*(SizeZ+1) * _SubdivisionsY - 2) + (2*(SizeX+1) * _SubdivisionsZ - 2) + (2*(SizeX+1) * _SubdivisionsZ - 2) ) + 2*5;

	// Create the buffers
	void*	pVerticesArray = NULL;
	void*	pIndicesArray = NULL;
	_Writer.CreateBuffers( VerticesCount, IndicesCount, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, pVerticesArray, pIndicesArray );
	ASSERT( pVerticesArray != NULL, "Invalid vertex buffer !" );
	ASSERT( pIndicesArray != NULL, "Invalid index buffer !" );

	void*	pVertex = pVerticesArray;
	void*	pIndex = pIndicesArray;

	//////////////////////////////////////////////////////////////////////////
	// Build vertices
	float3	pNormals[6] = {
		-float3::UnitX,
		 float3::UnitX,
		-float3::UnitY,
		 float3::UnitY,
		-float3::UnitZ,
		 float3::UnitZ,
	};
	float3	pTangents[6] = {
		 float3::UnitZ,
		-float3::UnitZ,
		 float3::UnitX,
		 float3::UnitX,
		-float3::UnitX,
		 float3::UnitX,
	};

	int			pSizesX[6] = { SizeZ, SizeZ, SizeX, SizeX, SizeX, SizeX };
	int			pSizesY[6] = { SizeY, SizeY, SizeZ, SizeZ, SizeZ, SizeZ };

	float3	Position, Normal, X, Y;
	float2	UV;

	for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
	{
		int		Sx = pSizesX[FaceIndex];
		int		Sy = pSizesY[FaceIndex];
		Normal = pNormals[FaceIndex];
		X = pTangents[FaceIndex];
		Y = Normal.Cross( X );

		for ( int j=0; j < Sy; j++ )
		{
			float	y = 1.0f - 2.0f * float(j) / (Sy-1);
			for ( int i=0; i < Sx; i++ )
			{
				float	x = 2.0f * float(i) / (Sx-1) - 1.0f;

				Position = Normal + x * X + y * Y;

				if ( _pMapper )
					_pMapper->Map( Position, Normal, X, UV, false );
				else
					UV.Set( 0.5f * (1.0f + x), 0.5f * (1.0f + y) );

				VWRITE( pVertex, Position, Normal, X, Y, UV );
			}
		}
	}
	ASSERT( VerticesCount == 0, "Wrong contruction!" );

	//////////////////////////////////////////////////////////////////////////
	// Build indices
	int		FaceOffset = 0;
	for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
	{
		int		Sx = pSizesX[FaceIndex];
		int		Sy = pSizesY[FaceIndex];

		if ( FaceIndex > 0 )
		{	// Write a first degenerate vertex for that face to make a clean junction with previous face...
 			IWRITE( pIndex, FaceOffset+0 );
		}

		for ( int j=0; j < Sy-1; j++ )
		{
			int	CurrentBandOffset = FaceOffset + j * Sx;
			int	NextBandOffset = FaceOffset + (j+1) * Sx;

			for ( int i=0; i < Sx; i++ )
			{
				IWRITE( pIndex, CurrentBandOffset + i );
				IWRITE( pIndex, NextBandOffset + i );
			}

			if ( j == _SubdivisionsY-1 )
				continue;	// Not for the last band...

			// Write 2 last degenerate indices so we smoothly transition to next band
			IWRITE( pIndex, NextBandOffset+Sx-1 );
			IWRITE( pIndex, NextBandOffset );
		}

		FaceOffset += Sx*Sy;

 		if ( FaceIndex < 5 )
 		{	// Write one last degenerate vertex for that face to make a clean junction with next face...
			IWRITE( pIndex, FaceOffset-1 );
		}
	}
	ASSERT( IndicesCount == 0, "Wrong contruction!" );

	//////////////////////////////////////////////////////////////////////////
	// Finalize
	_Writer.Finalize( pVerticesArray, pIndicesArray );
}

void	GeometryBuilder::AppendVertex( IGeometryWriter& _Writer, void*& _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV, TweakVertexDelegate _TweakVertex, void* _pUserData )
{
	if ( _TweakVertex == NULL )
	{
		_Writer.AppendVertex( _pVertex, _Position, _Normal, _Tangent, _BiTangent, _UV );
		return;
	}

	// Ask the user to tweak the vertices first !
	float3	P = _Position;
	float3	N = _Normal;
	float3	T = _Tangent;
	float3	B = _BiTangent;
	float2	UV = _UV;
	(*_TweakVertex)( P, N, T, B, UV, _pUserData );
	_Writer.AppendVertex( _pVertex, P, N, T, B, UV );
}


//////////////////////////////////////////////////////////////////////////
// Spherical mapping
//
GeometryBuilder::MapperSpherical::MapperSpherical( float _WrapU, float _WrapV, const float3& _Center, const float3& _X, const float3& _Y )
	: m_WrapU( _WrapU )
	, m_WrapV( _WrapV )
	, m_Center( _Center )
	, m_X( _X )
	, m_Y( _Y )
{
	m_X.Normalize();
	m_Y.Normalize();
	m_Z = m_X.Cross( m_Y );
	m_Z.Normalize();
}
void	GeometryBuilder::MapperSpherical::Map( const float3& _Position, const float3& _Normal, const float3& _Tangent, float2& _UV, bool _bIsBandEndVertex ) const
{
	float3	Dir = _Position - m_Center;
	Dir.Normalize();
	float	X = Dir.Dot( m_X );
	float	Y = Dir.Dot( m_Y );
	float	Z = Dir.Dot( m_Z );

	float	Phi = atan2f( X, Z );		// Phi=0 at +Z and 90° at +X
			Phi = fmodf( Phi + TWOPI, TWOPI );
			Phi += _bIsBandEndVertex ? TWOPI : 0.0f;

	float	Theta = acosf( Y );			// Theta=0 at +Y and 180° at -Y

	_UV.x = m_WrapU * INV2PI * Phi;
	_UV.y = m_WrapV * INVPI * Theta;
}


//////////////////////////////////////////////////////////////////////////
// Cylindrical mapping
//
GeometryBuilder::MapperCylindrical::MapperCylindrical( float _WrapU, float _WrapV, const float3& _Center, const float3& _X, const float3& _Z )
	: m_WrapU( _WrapU )
	, m_WrapV( _WrapV )
	, m_Center( _Center )
	, m_X( _X )
	, m_Z( _Z )
{
	m_X.Normalize();
	m_Z.Normalize();
	m_Y = m_Z.Cross( m_X );
	m_Y.Normalize();
}
void	GeometryBuilder::MapperCylindrical::Map( const float3& _Position, const float3& _Normal, const float3& _Tangent, float2& _UV, bool _bIsBandEndVertex ) const
{
	float3	Dir = _Position - m_Center;
	float	X = Dir.Dot( m_X );
	float	Y = Dir.Dot( m_Y );
	float	Z = Dir.Dot( m_Z );

	float	Phi = atan2f( X, Z );		// Phi=0 at +Z and 90° at +X
			Phi = fmodf( Phi + TWOPI, TWOPI );
			Phi += _bIsBandEndVertex ? TWOPI : 0.0f;

	_UV.x = m_WrapU * INV2PI * Phi;
	_UV.y = m_WrapV * Y;
}


//////////////////////////////////////////////////////////////////////////
// Planar mapping
//
GeometryBuilder::MapperPlanar::MapperPlanar( float _WrapU, float _WrapV, const float3& _Center, const float3& _Tangent, const float3& _BiTangent )
	: m_WrapU( _WrapU )
	, m_WrapV( _WrapV )
	, m_Center( _Center )
	, m_Tangent( _Tangent )
	, m_BiTangent( _BiTangent )
{
}
void	GeometryBuilder::MapperPlanar::Map( const float3& _Position, const float3& _Normal, const float3& _Tangent, float2& _UV, bool _bIsBandEndVertex ) const
{
	float3	Delta = _Position - m_Center;

	_UV.x = m_WrapU * Delta.Dot( m_Tangent );
	_UV.y = m_WrapV * Delta.Dot( m_BiTangent );
}


//////////////////////////////////////////////////////////////////////////
// Cube mapping
//
GeometryBuilder::MapperCube::MapperCube( float _WrapU, float _WrapV, const float3& _Center, const float3& _X, const float3& _Y, const float3& _Z )
	: m_WrapU( _WrapU )
	, m_WrapV( _WrapV )
	, m_Center( _Center )
	, m_X( _X )
	, m_Y( _Y )
	, m_Z( _Z )
{
	m_World2CubeMap.SetRow( 0, m_X, 0 );
	m_World2CubeMap.SetRow( 1, m_Y, 0 );
	m_World2CubeMap.SetRow( 2, m_Z, 0 );
	m_World2CubeMap.SetRow( 3, m_Center, 1 );
	m_World2CubeMap = m_World2CubeMap.Inverse();
}
void	GeometryBuilder::MapperCube::Map( const float3& _Position, const float3& _Normal, const float3& _Tangent, float2& _UV, bool _bIsBandEndVertex ) const
{
	float4	P = float4( _Position, 1 ) * m_World2CubeMap;
	float4	D = float4( _Normal, 0 ) * m_World2CubeMap;

	float		pHits[6];
	pHits[0] = -(1.0f + P.x) / D.x;	// -X
	pHits[1] =  (1.0f - P.x) / D.x;	// +X
	pHits[2] = -(1.0f + P.y) / D.y;	// -Y
	pHits[3] =  (1.0f - P.y) / D.y;	// +Y
	pHits[4] = -(1.0f + P.z) / D.z;	// -Z
	pHits[5] =  (1.0f - P.z) / D.z;	// +Z

	int		MinHit = -1;
	float	fMinHit = +MAX_FLOAT;
	for ( int HitIndex=0; HitIndex < 6; HitIndex++ )
		if ( pHits[HitIndex] >= 0.0 && pHits[HitIndex] < fMinHit ) {
			// New closer hit!
			MinHit = HitIndex;
			fMinHit = pHits[HitIndex];
		}
	
	float4	HitPos = P + fMinHit * D;
	switch ( MinHit ) {
	case 0:	// -X
		_UV.x = 0.5f * (1.0f + HitPos.z);
		_UV.y = 0.5f * (1.0f + HitPos.y);
		break;
	case 1:	// +X
		_UV.x = 0.5f * (1.0f - HitPos.z);
		_UV.y = 0.5f * (1.0f + HitPos.y);
		break;

	case 2:	// -Y
		_UV.x = 0.5f * (1.0f + HitPos.x);
		_UV.y = 0.5f * (1.0f - HitPos.z);
		break;
	case 3:	// +Y
		_UV.x = 0.5f * (1.0f + HitPos.x);
		_UV.y = 0.5f * (1.0f + HitPos.z);
		break;

	case 4:	// -Z
		_UV.x = 0.5f * (1.0f - HitPos.x);
		_UV.y = 0.5f * (1.0f + HitPos.y);
		break;
	case 5:	// +Z
		_UV.x = 0.5f * (1.0f + HitPos.x);
		_UV.y = 0.5f * (1.0f + HitPos.y);
		break;
	}

	_UV.x *= m_WrapU;
	_UV.y *= m_WrapV;
}
