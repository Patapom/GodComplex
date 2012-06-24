#pragma once

#define ROOM_HEIGHT	5.0f		// The height of the room, in meters

template<typename> class CB;

class EffectRoom
{
private:	// CONSTANTS

	static const int	LIGHTMAP_SIZE = 128;	// Large size of the lightmap


public:		// NESTED TYPES

	struct CBObject
	{
		NjFloat4x4	Local2World;	// Local=>World transform to rotate the object
// 		NjFloat4	EmissiveColor;
// 		NjFloat4	NoiseOffset;	// XYZ=Noise Position  W=NoiseAmplitude
 	};

private:	// FIELDS

	int					m_ErrorCode;
	Texture2D&			m_RTTarget;

	Material*			m_pMatDisplay;		// Displays the room

	// Primitives
	Primitive*			m_pPrimRoom;

	// Textures
	Texture2D*			m_pTexLightmap;

 	CB<CBObject>*		m_pCB_Object;


	// Params
public:
	
public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }

public:		// METHODS

	EffectRoom( Texture2D& _RTTarget );
	~EffectRoom();

	void	RenderLightmap( IntroProgressDelegate& _Delegate );

	void	Render( float _Time, float _DeltaTime );

protected:

	void		BuildRoom();
	NjFloat2	GetLightMapAspectRatios();
	NjFloat2	LightUV( int _FaceIndex, const NjFloat2& _UV );

};