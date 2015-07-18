#pragma once

#define ROOM_HEIGHT	5.0f		// The height of the room, in meters
#define ROOM_SIZE	10.0f		// The size of the room, in meters

#define ROOM_BOUNCES_COUNT		4
#define ROOM_RAY_GROUPS_COUNT	1

template<typename> class CB;

class EffectRoom
{
private:	// CONSTANTS

	static const int	LIGHTMAP_SIZE = 256;		// Size of the lightmap


protected:	// NESTED TYPES

	struct CBObject
	{
		float4x4	Local2World;	// Local=>World transform to rotate the object

		float4	LightColor0;
		float4	LightColor1;
		float4	LightColor2;
		float4	LightColor3;
 	};

	struct CBTesselate
	{
		float3	dUV;
		float		__PAD0;
		float2	TesselationFactors;
	};

	struct MaterialDescriptor 
	{
		int			LightSourceIndex;	// -1 For standard reflective materials
		float3	Color;				// Either the diffuse albedo or the emissive power depending on the emissive flag
	};

private:	// FIELDS

	int					m_ErrorCode;
	Texture2D&			m_RTTarget;

	Shader*			m_pMatDisplay;			// Displays the room
	Shader*			m_pMatDisplayEmissive;	// Displays the lights
//	Material*			m_pMatTestTesselation;	// My first Domain Shader!

	// Primitives
	Primitive*			m_pPrimRoom;
	Primitive*			m_pPrimRoomLights;
//	Primitive*			m_pPrimTesselatedQuad;

	// Textures
	Texture2D*			m_pTexWalls;			// The fat texture for the walls
	Texture2D*			m_pTexLightMaps;		// The array texture that will contain the room's light map
public:	Texture2D*			m_pTexVoronoi;		// Test voronoï texture

	// Constant buffers
 	CB<CBObject>*		m_pCB_Object;
 	CB<CBTesselate>*	m_pCB_Tesselate;

	// Animation parameters
	float4			m_LightUpTime;
	float4			m_LightFailTimer;		// Time before the light fails
	float4			m_LightFailureTimer;	// Time since the light failed (used to animate the failure)
	float4			m_LightFailureDuration;	// Duration of the failure

//	static MaterialDescriptor	ms_pMaterials[];

	// Params
public:
	
public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }

public:		// METHODS

	EffectRoom( Texture2D& _RTTarget );
	~EffectRoom();

	void		Render( float _Time, float _DeltaTime );


protected:

	void		BuildRoom( const TextureBuilder& _TB );
	void		BuildRoomTextures( TextureBuilder& _TB );
	void		BuildVoronoiTexture( TextureBuilder& _TB );

	float		AnimateFailure( float& _TimerTillFailure, float& _TimeSinceFailure, float& _FailureDuration, float _FailMinTime, float _FailDeltaTime, float _DeltaTime );

};