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
		virtual void	Map( const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, NjFloat2& _UV ) const = 0;
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
		virtual void	Map( const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, NjFloat2& _UV ) const;
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
		virtual void	Map( const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, NjFloat2& _UV ) const;
	};

	class	IGeometryWriter
	{
	public:
		virtual void	CreateBuffers( int _VerticesCount, int _IndicesCount, D3D11_PRIMITIVE_TOPOLOGY _Topology, void*& _pVertices, void*& _pIndices, int& _VertexStride, int& _IndexStride ) = 0;
		virtual void	WriteVertex( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV ) = 0;
		virtual void	WriteIndex( void* _pIndex, int _Index ) = 0;
		virtual void	Finalize( void* _pVertices, void* _pIndices ) = 0;
	};

	typedef void	(*TweakVertexDelegate)( NjFloat3& _Position, NjFloat3& _Normal, NjFloat3& _Tangent, NjFloat2& _UV, void* _pUserData );

public:		// METHODS

	// Builds a uniformly subdivided sphere
	static void		BuildSphere( int _PhiSubdivisions, int _ThetaSubdivisions, IGeometryWriter& _Writer, const MapperBase& _Mapper, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );

	// Builds a torus in the XY plane
	static void		BuildTorus( int _PhiSubdivisions, int _ThetaSubdivisions, float _LargeRadius, float _SmallRadius, IGeometryWriter& _Writer, const MapperBase& _Mapper, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );

	// Builds a subdivided plane
	static void		BuildPlane( int _SubdivisionsX, int _SubdivisionsY, const NjFloat3& _X, const NjFloat3& _Y, IGeometryWriter& _Writer, const MapperBase& _Mapper, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );

private:

	static void		WriteVertex( IGeometryWriter& _Writer, void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );
};
