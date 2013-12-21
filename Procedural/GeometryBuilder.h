//////////////////////////////////////////////////////////////////////////
// Helps to build a primitive
//
#pragma once

class	GeometryBuilder
{
protected:	// CONSTANTS

public:		// NESTED TYPES

	class	MapperBase
	{
	public:
		virtual void	Map( const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, NjFloat2& _UV, bool _bIsBandEndVertex ) const = 0;
	};

	// Spherical mapping
	class	MapperSpherical : public MapperBase
	{
	protected:

		float		m_WrapU;
		float		m_WrapV;
		NjFloat3	m_Center, m_X, m_Y, m_Z;

	public:
		MapperSpherical( float _WrapU=2.0f, float _WrapV=1.0f, const NjFloat3& _Center=NjFloat3::Zero, const NjFloat3& _X=NjFloat3::UnitX, const NjFloat3& _Y=NjFloat3::UnitY );
		virtual void	Map( const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, NjFloat2& _UV, bool _bIsBandEndVertex ) const;
	};

	// Cylindrical mapping
	class	MapperCylindrical : public MapperBase
	{
	protected:

		float		m_WrapU;
		float		m_WrapV;
		NjFloat3	m_Center, m_X, m_Y, m_Z;

	public:
		MapperCylindrical( float _WrapU=2.0f, float _WrapV=1.0f, const NjFloat3& _Center=NjFloat3::Zero, const NjFloat3& _X=NjFloat3::UnitX, const NjFloat3& _Z=NjFloat3::UnitZ );
		virtual void	Map( const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, NjFloat2& _UV, bool _bIsBandEndVertex ) const;
	};

	// Planar mapping
	class	MapperPlanar : public MapperBase
	{
	protected:

		float		m_WrapU;
		float		m_WrapV;
		NjFloat3	m_Center, m_Tangent, m_BiTangent;

	public:
		MapperPlanar( float _WrapU=1.0f, float _WrapV=1.0f, const NjFloat3& _Center=NjFloat3::Zero, const NjFloat3& _Tangent=NjFloat3::UnitZ, const NjFloat3& _BiTangent=NjFloat3::UnitX );
		virtual void	Map( const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, NjFloat2& _UV, bool _bIsBandEndVertex ) const;
	};

	// Cube mapping
	class	MapperCube : public MapperBase
	{
	protected:

		float		m_WrapU;
		float		m_WrapV;
		NjFloat3	m_Center, m_X, m_Y, m_Z;
		NjFloat4x4	m_World2CubeMap;

	public:
		MapperCube( float _WrapU=1.0f, float _WrapV=1.0f, const NjFloat3& _Center=NjFloat3::Zero, const NjFloat3& _X=NjFloat3::UnitX, const NjFloat3& _Y=NjFloat3::UnitY, const NjFloat3& _Z=NjFloat3::UnitZ );
		virtual void	Map( const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, NjFloat2& _UV, bool _bIsBandEndVertex ) const;
	};

	class	IGeometryWriter
	{
	public:
		virtual void	CreateBuffers( int _VerticesCount, int _IndicesCount, D3D11_PRIMITIVE_TOPOLOGY _Topology, void*& _pVertices, void*& _pIndices ) = 0;
		virtual void	AppendVertex( void*& _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat3& _BiTangent, const NjFloat2& _UV ) = 0;
		virtual void	AppendIndex( void*& _pIndex, int _Index ) = 0;
		virtual void	Finalize( void* _pVertices, void* _pIndices ) = 0;
	};

	typedef void	(*TweakVertexDelegate)( NjFloat3& _Position, NjFloat3& _Normal, NjFloat3& _Tangent, const NjFloat3& _BiTangent, NjFloat2& _UV, void* _pUserData );

public:		// METHODS

	// Builds a uniformly subdivided sphere of radius 1 centered in 0
	static void		BuildSphere( int _PhiSubdivisions, int _ThetaSubdivisions, IGeometryWriter& _Writer, const MapperBase* _pMapper=NULL, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );

	// Builds a uniformly subdivided cylinder of radius 1, height 2, centered in 0 (so top cap is Y=+1, bottom cap is Y=-1)
	static void		BuildCylinder( int _RadialSubdivisions, int _VerticalSubdivisions, bool _bIncludeCaps, IGeometryWriter& _Writer, const MapperBase* _pMapper=NULL, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );

	// Builds a torus in the XY plane centered in 0
	static void		BuildTorus( int _PhiSubdivisions, int _ThetaSubdivisions, float _LargeRadius, float _SmallRadius, IGeometryWriter& _Writer, const MapperBase* _pMapper=NULL, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );

	// Builds a subdivided plane centered in 0
	static void		BuildPlane( int _SubdivisionsX, int _SubdivisionsY, const NjFloat3& _X, const NjFloat3& _Y, IGeometryWriter& _Writer, const MapperBase* _pMapper=NULL, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );

	// Builds a subdivided cube centered in 0 of size 2 (extents go from (-1,-1,-1) to (+1,+1,+1))
	static void		BuildCube( int _SubdivisionsX, int _SubdivisionsY, int _SubdivisionsZ, IGeometryWriter& _Writer, const MapperBase* _pMapper=NULL, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );

private:

	static void		AppendVertex( IGeometryWriter& _Writer, void*& _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat3& _BiTangent, const NjFloat2& _UV, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );
};
