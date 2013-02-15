#pragma once

template<typename> class CB;

class EffectDeferred
{
private:	// CONSTANTS

public:		// NESTED TYPES

	struct CBRender
	{
		NjFloat3	dUV;
		float		__PAD;
		NjFloat2	DeltaTime;
	};

	class	Object
	{
	public:		// NESTED TYPES

		struct	CBObject
		{
			NjFloat4x4	Local2World;
		};

	public:		// FIELDS
		NjFloat3	m_Position;
		NjFloat4	m_Rotation;

		// Animation
		float		m_AnimRotationSpeed;
		NjFloat3	m_AnimPositionCircleCenter;
		float		m_AnimPositionCircleRadius;
		float		m_AnimPositionCircleSpeed;

	protected:

		CB<CBObject>*	m_pCB_Object;
		Primitive*		m_pPrimitive;
		Texture2D*		m_pTexDiffuseAO;
		Texture2D*		m_pTexNormal;

	public:		// METHODS

		Object();
		virtual ~Object();

		virtual void	Upload() const;
		virtual void	Render() const;
	};

// 	class	Sphere : public Object
// 	{
// 	public:		// FIELDS
// 
// 
// 	protected:
// 
// 	public:		// METHODS
// 
// 		Sphere();
// 		virtual ~Sphere();
// 
// //		virtual void	Upload() const;
// //		virtual void	Render() const;
// 	};



private:	// FIELDS

	int					m_ErrorCode;

	Material*			m_pMatDepthPass;
	Material*			m_pMatFillGBuffer;
	Material*			m_pMatShading;

	Primitive*			m_pPrimSphere;

	Texture2D*			m_pRT;

public:
//	Texture2D*			m_pTexVoronoi;

	CB<CBRender>*		m_pCB_Render;


	// Params
public:


public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }

public:		// METHODS

	EffectDeferred();
	~EffectDeferred();

	void	Render( float _Time, float _DeltaTime );

protected:
	
	void	BuildVoronoiTexture( TextureBuilder& _TB, NjFloat2* _pCellCenters, VertexFormatPt4* _pVertices );
};