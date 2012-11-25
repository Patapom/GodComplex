#pragma once

#define EFFECT_PARTICLES_COUNT	16

template<typename> class CB;

class EffectParticles
{
private:	// CONSTANTS

public:		// NESTED TYPES

	struct CBRender
	{
		NjFloat3	dUV;
		float		__PAD;
		NjFloat2	DeltaTime;
	};

private:	// FIELDS

	int					m_ErrorCode;

	Material*			m_pMatCompute;
	Material*			m_pMatDisplay;
	Material*			m_pMatDebugVoronoi;

	Primitive*			m_pPrimParticle;

	Texture2D*			m_ppRTParticlePositions[3];
	Texture2D*			m_ppRTParticleNormals[2];
	Texture2D*			m_ppRTParticleTangents[2];
public:	Texture2D*			m_pTexVoronoi;

	CB<CBRender>*		m_pCB_Render;


	// Params
public:
	float				m_EmissivePower;


public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }

public:		// METHODS

	EffectParticles();
	~EffectParticles();

	void	Render( float _Time, float _DeltaTime );

protected:
	
	void	BuildVoronoiTexture( TextureBuilder& _TB, NjFloat2* _pCellCenters, VertexFormatPt4* _pVertices );
};