#pragma once

#define ROOM_HEIGHT	5.0f		// The height of the room, in meters
#define ROOM_SIZE	10.0f		// The size of the room, in meters

#define ROOM_BOUNCES_COUNT		4
#define ROOM_RAY_GROUPS_COUNT	1

template<typename> class CB;

class EffectRoom
{
private:	// CONSTANTS

	static const int	LIGHTMAP_SIZE = 128;		// Size of the lightmap
//	static const int	LIGHTMAP_CUBEMAP_SIZE = 32;	// Size of the cube maps rendered for each texel of the light map

protected:	// NESTED TYPES

	struct CBObject
	{
		NjFloat4x4	Local2World;	// Local=>World transform to rotate the object

		NjFloat4	LightColor0;
		NjFloat4	LightColor1;
		NjFloat4	LightColor2;
		NjFloat4	LightColor3;
 	};

	struct CBTesselate
	{
		NjFloat3	dUV;
		float		__PAD0;
		NjFloat2	TesselationFactors;
	};

	struct MaterialDescriptor 
	{
		int			LightSourceIndex;	// -1 For standard reflective materials
		NjFloat3	Color;				// Either the diffuse albedo or the emissive power depending on the emissive flag
	};

private:	// FIELDS

	int					m_ErrorCode;
	Texture2D&			m_RTTarget;

	Material*			m_pMatDisplay;			// Displays the room
	Material*			m_pMatDisplayEmissive;	// Displays the lights
//	Material*			m_pMatTestTesselation;	// My first Domain Shader!

	// Primitives
	Primitive*			m_pPrimRoom;
	Primitive*			m_pPrimRoomLights;
//	Primitive*			m_pPrimTesselatedQuad;

	// Textures
	Texture2D*			m_pTexLightMaps;		// The array texture that will contain the room's light map

	// Constant buffers
 	CB<CBObject>*		m_pCB_Object;
 	CB<CBTesselate>*	m_pCB_Tesselate;

	// Animation parameters
	NjFloat4			m_LightUpTime;
	NjFloat4			m_LightFailTimer;		// Time before the light fails
	NjFloat4			m_LightFailureTimer;	// Time since the light failed (used to animate the failure)
	NjFloat4			m_LightFailureDuration;	// Duration of the failure

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

	void		BuildRoom();
	float		AnimateFailure( float& _TimerTillFailure, float& _TimeSinceFailure, float& _FailureDuration, float _FailMinTime, float _FailDeltaTime, float _DeltaTime );

};