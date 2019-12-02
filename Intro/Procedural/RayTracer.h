//////////////////////////////////////////////////////////////////////////
// Helps to ray trace a bunch of rays
// We only raytrace for quads at the moment
//
#pragma once

class	RayTracer
{
protected:	// CONSTANTS

public:		// NESTED TYPES

	// The geometric quad structure
	// A quad is simply a rectangle defined by its position, normal and tangent.
	// Its bitangent will be computed as CrossProduct( Normal, Tangent )
	// The quad is mapped so :
	//	UV=(0,0) at the position [Center - 0.5 * Size.x * Tangent + 0.5 * Size.y * BiTangent]
	//	UV=(1,1) at the position [Center + 0.5 * Size.x * Tangent - 0.5 * Size.y * BiTangent]
	//
	struct	Quad
	{
		float3	Center;			// Center of the quad in WORLD space
		float3	Normal;			// Normal to the quad in WORLD space
		float3	Tangent;		// Tangent to the quad in WORLD space
		float2	Size;			// Size of the quad in WORLD space
		int			MaterialID;		// Material ID associated to the quad
	};

	struct	Ray
	{
		float3	Position;		// Ray position
		float3	Direction;		// Ray direction
		float		HitDistance;	// Distance to the hit
		float2	HitUV;			// UV of the hit within the hit quad
		Quad*		pHitQuad;		// Pointer to the quad that was hit
	};

	struct	Quad_Internal : public Quad
	{
		float3	BiTangent;
		float4	SizeAndInvSize;	// XY=0.5*Size ZW=1/(0.5*Size)
	};


protected:	// FIELDS

	int				m_QuadsCount;
	Quad_Internal*	m_pQuads;


public:		// METHODS

	RayTracer();
	~RayTracer();

	void	InitGeometry( int _QuadsCount, const Quad* _pQuads );

	// Traces a ray in the geometry
	bool	Trace( Ray& _Ray );

	void	ExitGeometry();
};
