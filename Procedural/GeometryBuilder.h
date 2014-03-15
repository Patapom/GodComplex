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
		virtual void	Map( const float3& _Position, const float3& _Normal, const float3& _Tangent, float2& _UV, bool _bIsBandEndVertex ) const = 0;
	};

	// Spherical mapping
	class	MapperSpherical : public MapperBase
	{
	protected:

		float		m_WrapU;
		float		m_WrapV;
		float3	m_Center, m_X, m_Y, m_Z;

	public:
		MapperSpherical( float _WrapU=2.0f, float _WrapV=1.0f, const float3& _Center=float3::Zero, const float3& _X=float3::UnitX, const float3& _Y=float3::UnitY );
		virtual void	Map( const float3& _Position, const float3& _Normal, const float3& _Tangent, float2& _UV, bool _bIsBandEndVertex ) const;
	};

	// Cylindrical mapping
	class	MapperCylindrical : public MapperBase
	{
	protected:

		float		m_WrapU;
		float		m_WrapV;
		float3	m_Center, m_X, m_Y, m_Z;

	public:
		MapperCylindrical( float _WrapU=2.0f, float _WrapV=1.0f, const float3& _Center=float3::Zero, const float3& _X=float3::UnitX, const float3& _Z=float3::UnitZ );
		virtual void	Map( const float3& _Position, const float3& _Normal, const float3& _Tangent, float2& _UV, bool _bIsBandEndVertex ) const;
	};

	// Planar mapping
	class	MapperPlanar : public MapperBase
	{
	protected:

		float		m_WrapU;
		float		m_WrapV;
		float3	m_Center, m_Tangent, m_BiTangent;

	public:
		MapperPlanar( float _WrapU=1.0f, float _WrapV=1.0f, const float3& _Center=float3::Zero, const float3& _Tangent=float3::UnitZ, const float3& _BiTangent=float3::UnitX );
		virtual void	Map( const float3& _Position, const float3& _Normal, const float3& _Tangent, float2& _UV, bool _bIsBandEndVertex ) const;
	};

	// Cube mapping
	class	MapperCube : public MapperBase
	{
	protected:

		float		m_WrapU;
		float		m_WrapV;
		float3	m_Center, m_X, m_Y, m_Z;
		float4x4	m_World2CubeMap;

	public:
		MapperCube( float _WrapU=1.0f, float _WrapV=1.0f, const float3& _Center=float3::Zero, const float3& _X=float3::UnitX, const float3& _Y=float3::UnitY, const float3& _Z=float3::UnitZ );
		virtual void	Map( const float3& _Position, const float3& _Normal, const float3& _Tangent, float2& _UV, bool _bIsBandEndVertex ) const;
	};

	class	IGeometryWriter
	{
	public:
		virtual void	CreateBuffers( int _VerticesCount, int _IndicesCount, D3D11_PRIMITIVE_TOPOLOGY _Topology, void*& _pVertices, void*& _pIndices ) = 0;
		virtual void	AppendVertex( void*& _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) = 0;
		virtual void	AppendIndex( void*& _pIndex, int _Index ) = 0;
		virtual void	Finalize( void* _pVertices, void* _pIndices ) = 0;
	};

	typedef void	(*TweakVertexDelegate)( float3& _Position, float3& _Normal, float3& _Tangent, const float3& _BiTangent, float2& _UV, void* _pUserData );

public:		// METHODS

	// Builds a uniformly subdivided sphere of radius 1 centered in 0
	static void		BuildSphere( int _PhiSubdivisions, int _ThetaSubdivisions, IGeometryWriter& _Writer, const MapperBase* _pMapper=NULL, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );

	// Builds a uniformly subdivided cylinder of radius 1, height 2, centered in 0 (so top cap is Y=+1, bottom cap is Y=-1)
	static void		BuildCylinder( int _RadialSubdivisions, int _VerticalSubdivisions, bool _bIncludeCaps, IGeometryWriter& _Writer, const MapperBase* _pMapper=NULL, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );

	// Builds a torus in the XY plane centered in 0
	static void		BuildTorus( int _PhiSubdivisions, int _ThetaSubdivisions, float _LargeRadius, float _SmallRadius, IGeometryWriter& _Writer, const MapperBase* _pMapper=NULL, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );

	// Builds a subdivided plane centered in 0
	static void		BuildPlane( int _SubdivisionsX, int _SubdivisionsY, const float3& _X, const float3& _Y, IGeometryWriter& _Writer, const MapperBase* _pMapper=NULL, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );

	// Builds a subdivided cube centered in 0 of size 2 (extents go from (-1,-1,-1) to (+1,+1,+1))
	static void		BuildCube( int _SubdivisionsX, int _SubdivisionsY, int _SubdivisionsZ, IGeometryWriter& _Writer, const MapperBase* _pMapper=NULL, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );

private:

	static void		AppendVertex( IGeometryWriter& _Writer, void*& _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV, TweakVertexDelegate _TweakVertex=NULL, void* _pUserData=NULL );
};
