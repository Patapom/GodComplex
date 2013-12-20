#pragma once

template<typename> class CB;

class EffectGlobalIllum : public Scene::ISceneTagger, public Scene::ISceneRenderer
{
private:	// CONSTANTS

	static const U32		CUBE_MAP_SIZE = 128;
	static const U32		MAX_NEIGHBOR_PROBES = 32;

	static const U32		MAX_LIGHTS = 1;

protected:	// NESTED TYPES

	struct CBScene
	{
		U32			LightsCount;
		U32			ProbesCount;
 	};

	struct CBObject
	{
		NjFloat4x4	Local2World;	// Local=>World transform to rotate the object
 	};

	struct CBMaterial
	{
		NjFloat3	DiffuseColor;
		bool		HasDiffuseTexture;
		NjFloat3	SpecularColor;
		bool		HasSpecularTexture;
		float		SpecularExponent;
	};

	struct CBProbe
	{
		NjFloat3	CurrentProbePosition;
		U32			NeighborProbeID;
		NjFloat3	NeighborProbePosition;
 	};

	struct CBSplat
	{
		NjFloat3	dUV;
	};

	// The probe structure
	struct	ProbeStruct
	{
		Scene::Probe*	pSceneProbe;

		NjFloat3		pSHBounce[9];			// The pre-computed SH that gives back how much the probe perceives of indirectly bounced light
		float			pSHOcclusion[9];		// The pre-computed SH taht gives back how much of the environment is perceived in a given direction
		NjFloat3		pSHLight[9];			// The radiance field surrounding the probe
		float			ProbeInfluenceDistance;	// The distance above which the probe stops being used

		int				NeighborsCount;			// The amount of neighbor probes
		struct	NeighborLink
		{
			double			SolidAngle;			// The solid angle covered by the neighbor
			float			Distance;			// The distance to the neighbor
			float			pSHLink[9];			// The "link SH" the neighbor's SH needs to be convolved with to act as a local light source for this probe
			ProbeStruct*	pNeighbor;			// The neighbor probe
		}				pNeighborLinks[MAX_NEIGHBOR_PROBES];		// The array of 32 max neighbor probes

		NjFloat3		pSHBouncedLight0[9];	// The resulting bounced irradiance (bounce * light) for current frame
		NjFloat3		pSHBouncedLight1[9];	// The resulting bounced irradiance (bounce * light) from last frame

		// Temporary counters for a specific probe to count its neighbors
		int				__TempNeighborCounter;
		double			__TempSumSolidAngle;

		// Computes the product of SHLight and SHBounce to get the SH coefficients for the bounced light
		void			ComputeLightBounce( const NjFloat3 _pSHAmbient[9] );
		void			SwapBuffers();
	};


private:	// FIELDS

	int					m_ErrorCode;
	Device&				m_Device;
	Texture2D&			m_RTTarget;
	Primitive&			m_ScreenQuad;

	Material*			m_pMatRender;				// Displays the room
	Material*			m_pMatRenderCubeMap;		// Renders the room into a cubemap
	Material*			m_pMatRenderNeighborProbe;	// Renders the neighbor probes as planes to form a 3D voronoï cell
	Material*			m_pMatPostProcess;			// Post-processes the result

	// Primitives
	Scene				m_Scene;
	bool				m_bDeleteSceneTags;

	// Textures
	Texture2D*			m_pTexWalls;

	// Constant buffers
 	CB<CBScene>*		m_pCB_Scene;
 	CB<CBObject>*		m_pCB_Object;
 	CB<CBMaterial>*		m_pCB_Material;
 	CB<CBProbe>*		m_pCB_Probe;
	CB<CBSplat>*		m_pCB_Splat;

	// Light buffer
	struct	LightStruct
	{
		NjFloat3	Position;
		NjFloat3	Color;
		float		Radius;	// Light radius to compute the solid angle for the probe injection
	};
	SB<LightStruct>*	m_pSB_Lights;

	// Runtime probes buffer
	struct RuntimeProbe 
	{
		NjFloat3	Position;
		float		ProbeInfluenceDistance;
		NjFloat3	pSHBounce[9];
		NjFloat3	pSHLight[9];
	};
	SB<RuntimeProbe>*	m_pSB_RuntimeProbes;

	// Probes
	int					m_ProbesCount;
	ProbeStruct*		m_pProbes;


	// Params
public:
	

public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }


public:		// METHODS

	EffectGlobalIllum( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera );
	~EffectGlobalIllum();

	void		Render( float _Time, float _DeltaTime );


	// ISceneTagger Implementation
	virtual void*	TagMaterial( const Scene::Material& _Material ) const override;
	virtual void*	TagTexture( const Scene::Material::Texture& _Texture ) const override;
	virtual void*	TagNode( const Scene::Node& _Node ) const override;
	virtual void*	TagPrimitive( const Scene::Mesh& _Mesh, const Scene::Mesh::Primitive& _Primitive ) const override;

	// ISceneRenderer Implementation
	virtual void	RenderMesh( const Scene::Mesh& _Mesh, Material* _pMaterialOverride ) const override;

private:

	void			BuildSHCoeffs( const NjFloat3& _Direction, double _Coeffs[9] );
	void			BuildSHCosineLobe( const NjFloat3& _Direction, double _Coeffs[9] );
	void			BuildSHCone( const NjFloat3& _Direction, float _HalfAngle, double _Coeffs[9] );
	void			BuildSHSmoothCone( const NjFloat3& _Direction, float _HalfAngle, double _Coeffs[9] );
	void			ZHRotate( const NjFloat3& _Direction, const NjFloat3& _ZHCoeffs, double _Coeffs[9] );

	void			PreComputeProbes();
};