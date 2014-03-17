#pragma once

template<typename> class CB;

class EffectDeferred
{
private:	// CONSTANTS

public:		// NESTED TYPES

	struct CBRender
	{
		float3	dUV;
		float		__PAD;
		float3	Ambient;
	};

	class	Object
	{
	public:		// NESTED TYPES

		struct	CBObject
		{
			float4x4	Local2World;
		};

	public:		// FIELDS
		float3	m_Position;
		float4	m_Rotation;	// Quaternion
		float3	m_Scale;

		// Animation
		float		m_AnimRotationSpeed;
		float3	m_AnimPositionCircleCenter;
		float		m_AnimPositionCircleRadius;
		float		m_AnimPositionCircleSpeed;

	protected:

		CB<CBObject>*	m_pCB_Object;
		Primitive*		m_pPrimitive;
		Texture2D*		m_pTexDiffuseSpec;
		Texture2D*		m_pTexNormalRoughnessAO;

	public:		// METHODS

		Object();
		~Object();

		void			SetPrimitive( Primitive& _Primitive, const Texture2D& _TexDiffuseAO, const Texture2D& _TexNormal );

		void			Render( bool _IsDepthPass ) const;
	};

	class	Light
	{
	protected:

		struct	CBLight
		{
			float3	Position;
			U32			Type;
			float3	Direction;
			float		__PAD1;
			float3	Color;
			float		__PAD2;
			float4	Data;
		};

	public:

		enum LIGHT_TYPE {
			OMNI,
			SPOT,
			DIRECTIONAL
		}			m_Type;
		float3	m_Position;
		float3	m_Direction;
		float3	m_Color;
		union
		{
			struct {	// OMNI
				float	RadiusHotspot;
				float	RadiusFalloff;
			};
			struct {	// DIRECTION
				float	RadiusHotspot;
				float	RadiusFalloff;
				float	Length;
			};
			struct {	// SPOT
				float	AngleHotspot;
				float	AngleFalloff;
				float	Length;
			};
		} m_Data;

	protected:

		CB<CBLight>*	m_pCBLight;

	public:

		Light();
		~Light();

		void		Upload();
	};


private:	// FIELDS

	int					m_ErrorCode;

	Material*			m_pMatDepthPass;
	Material*			m_pMatFillGBuffer;
	Material*			m_pMatShading_StencilPass;
	Material*			m_pMatShading;

	int					m_ObjectsCount;
	Object**			m_ppObjects;

	int					m_LightsCount;
	Light**				m_ppLights;

	Texture2D*			m_pRTGBuffer;
	Texture2D*			m_pRTLightAccumulation;

	Primitive*			m_pPrimCylinder;
	Primitive*			m_pPrimSphere;

public:

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
	
//	void	BuildVoronoiTexture( TextureBuilder& _TB, NjFloat2* _pCellCenters, VertexFormatPt4* _pVertices );
};