#pragma once

#define EFFECT_PARTICLES_COUNT	16

template<typename> class CB;

class EffectParticles
{
private:	// CONSTANTS

	static const int	TEXTURE_SIZE = 512;


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

	Primitive*			m_pPrimParticle;

	Texture2D*			m_pRTParticlePositions[3];
	Texture2D*			m_pRTParticleRotations[3];
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
	
	void	BuildVoronoiTexture( TextureBuilder& _TB, VertexFormatPt4* _pVertices );

};