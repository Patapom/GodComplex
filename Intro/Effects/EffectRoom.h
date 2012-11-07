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
// 		NjFloat4	EmissiveColor;
// 		NjFloat4	NoiseOffset;	// XYZ=Noise Position  W=NoiseAmplitude
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
	Material*			m_pMatRenderCubeMap;	// Renders the cube map
	Material*			m_pMatTestTesselation;	// My first Domain Shader!

	// Primitives
	Primitive*			m_pPrimRoom;
	Primitive*			m_pPrimTesselatedQuad;

	// Textures
	Texture2D*			m_pTexLightMaps;		// The array texture that will contain the room's light map

	// Constant buffers
 	CB<CBObject>*		m_pCB_Object;
 	CB<CBTesselate>*	m_pCB_Tesselate;


	static MaterialDescriptor	ms_pMaterials[];

	// Params
public:
	
public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }

public:		// METHODS

	EffectRoom( Texture2D& _RTTarget );
	~EffectRoom();

	void	Render( float _Time, float _DeltaTime );

	// Light map rendering
//	void	RenderLightmap( IntroProgressDelegate& _Delegate );


protected:

	void		BuildRoom();
 
// 	void		RenderDirect( TextureBuilder& _Positions, TextureBuilder& _Normals, TextureBuilder& _Tangents, TextureBuilder** _ppLightMaps );
// 	void		RenderDirectOLD( RayTracer& _Tracer, TextureBuilder& _Positions, TextureBuilder& _Normals, TextureBuilder& _Tangents, int _RaysCount, NjFloat3* _pRays, TextureBuilder** _ppLightMaps );
// 
// 	void		RenderCubeMap( const NjFloat3& _Position, const NjFloat3& _At, const NjFloat3& _Up, float _Near, float _Far );
// 	void		ReadBack( NjFloat4** _ppTarget );
// 
// 	NjFloat2	GetLightMapAspectRatios();
// 	NjFloat2	LightUV( int _FaceIndex, const NjFloat2& _UV, bool _bBias=false );
// 	void		DrawQuad( DrawUtils& _DrawPosition, DrawUtils& _DrawNormal, DrawUtils& _DrawTangent, const NjFloat2& _TopLeft, const NjFloat2& _BottomRight, const RayTracer::Quad& _Quad );

};