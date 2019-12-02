#include "../GodComplex.h"

RayTracer::RayTracer() : m_pQuads( NULL )
{
}
RayTracer::~RayTracer()
{
	ExitGeometry();
}

void	RayTracer::InitGeometry( int _QuadsCount, const Quad* _pQuads )
{
	ExitGeometry();

	m_QuadsCount = _QuadsCount;
	m_pQuads = new Quad_Internal[_QuadsCount];

	for ( int QuadIndex=0; QuadIndex < m_QuadsCount; QuadIndex++ )
	{
		const Quad&		Source = _pQuads[QuadIndex];
		Quad_Internal&	Target = m_pQuads[QuadIndex];
		memcpy( &Target, &Source, sizeof(Quad) );

		Target.Normal.Normalize();
		Target.Tangent.Normalize();
		Target.BiTangent = Target.Normal.Cross( Target.Tangent );
		Target.SizeAndInvSize.Set( 0.5f * Source.Size.x, 0.5f * Source.Size.y, 2.0f / Source.Size.x, 2.0f / Source.Size.y );
	}
}

bool	RayTracer::Trace( Ray& _Ray )
{
	_Ray.pHitQuad = NULL;
	_Ray.HitDistance = FLOAT32_MAX;	// Infinity...

	Quad_Internal*	pQuad = m_pQuads;
	for ( int QuadIndex=0; QuadIndex < m_QuadsCount; QuadIndex++, pQuad++ )
	{
		float3	ToCenter = pQuad->Center - _Ray.Position;
		float		HeightFromQuad = ToCenter.Dot( pQuad->Normal );		// Negative if above quad
		float		SlopeToQuad = _Ray.Direction.Dot( pQuad->Normal );	// Rate at which we get closer to the quad
		float		HitDistance = HeightFromQuad / SlopeToQuad;			// Distance at which we'll hit the quad's plane
		if ( HitDistance <= 0.0f || HitDistance > _Ray.HitDistance )
			continue;	// No hit, or we hit too far away from best hit...

		// Compute hit position and check we're within the quad
		float3	HitPosition = _Ray.Position + HitDistance * _Ray.Direction;	// Position within quad's plane
		float3	FromCenter = HitPosition - pQuad->Center;
		float		DistanceX = FromCenter.Dot( pQuad->Tangent );
		float		DistanceY = FromCenter.Dot( pQuad->BiTangent );
		if ( abs(DistanceX) > pQuad->SizeAndInvSize.x || abs(DistanceY) > pQuad->SizeAndInvSize.y )
			continue;	// We hit outside the quad...

		// We have a hit !
		// Now, all we need to do is to find the UVs where it happened
		_Ray.HitDistance = HitDistance;
		_Ray.pHitQuad = pQuad;
		_Ray.HitUV.Set( 0.5f + DistanceX * pQuad->SizeAndInvSize.z, 0.5f + DistanceY * pQuad->SizeAndInvSize.w );
	}

	return _Ray.pHitQuad != NULL;
}

void	RayTracer::ExitGeometry()
{
	if ( m_pQuads != NULL )
		delete[] m_pQuads;
	m_pQuads = NULL;
}

